using CallCenterRepository;
using CallCenterRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace CallService;

public class Services(ApplicationDbContext context)
{
    //№1 Получить все звонки оператора по проекту за интервал времени
    public async Task<List<ConversationDto>> GetOperatorProjectConversationsAsync(
        int operatorId,
        DateTime startDate, 
        DateTime endDate)
    {
        return await context.Conversations
            .Where(c => c.FkOperator == operatorId &&
                       c.TimeStarted >= startDate && 
                       c.TimeEnded <= endDate)
            .Select(c => new ConversationDto()
            {
                CallId = c.PkTalk,
                ProjectId = c.FkProject,
                OperatorId = c.FkOperator,
                ClientId = c.FkClient,
                CallStartedAt = c.TimeStarted,
                CallEndedAt = c.TimeEnded,
                PathToCallRecord = c.PathToAudio,
            })
            .ToListAsync();
    }

    //№2 Изменить статус звонка
    public async Task<CallStatusDto> ChangeConversationStatusAsync(
    int conversationId, 
    int newStatusId)
    {
        var conversation = await context.Conversations
            .Include(c => c.SelectedStatuses)
                .ThenInclude(cs => cs.ProjectStatus)
                    .ThenInclude(ps => ps.StatusGroup)
            .FirstOrDefaultAsync(c => c.PkTalk == conversationId);

        if (conversation == null)
        {
            throw new ArgumentException("Conversation not found", nameof(conversationId));
        }

        var newStatus = await context.ProjectStatuses
            .Include(ps => ps.StatusGroup)
            .FirstOrDefaultAsync(ps => ps.PkProjectStatus == newStatusId);
        
        if (newStatus == null)
        {
            throw new ArgumentException("Status not found", nameof(newStatusId));
        }

        // Удаляем старые статусы той же группы
        var statusesToRemove = conversation.SelectedStatuses
            .Where(cs => cs.ProjectStatus.StatusGroup.PkStatusGroup == 
                        newStatus.StatusGroup.PkStatusGroup)
            .ToList();

        context.ConversationStatuses.RemoveRange(statusesToRemove);

        var conversationStatus = new ConversationStatus
        {
            FkTalk = conversationId,
            FkProjectStatus = newStatusId,
            SelectedTime = DateTime.UtcNow
        };

        await context.ConversationStatuses.AddAsync(conversationStatus);
        await context.SaveChangesAsync();

        // Загружаем обновленные данные для возврата
        var updatedStatus = await context.ConversationStatuses
            .Include(cs => cs.ProjectStatus)
                .ThenInclude(ps => ps.StatusGroup)
            .FirstAsync(cs => cs.PkConversationStatus == conversationStatus.PkConversationStatus);

        return new CallStatusDto
        {
            StatusGroupId = updatedStatus.ProjectStatus.StatusGroup.PkStatusGroup,
            StatusGroupName = updatedStatus.ProjectStatus.StatusGroup.StatusGroupName,
            StatusId = updatedStatus.ProjectStatus.PkProjectStatus,
            StatusName = updatedStatus.ProjectStatus.StatusName,
            TimeStatusSelected = updatedStatus.SelectedTime
        };
    }

    //№3 Сохранить звонок
    public async Task<ConversationDto> SaveConversationAsync(
    DateTime timeStarted,
    DateTime timeEnded,
    string pathToAudio,
    int operatorId,
    int clientId,
    int projectId,
    string clientProcessingStatus,
    List<int> statusIds)
    {
        // Проверка оператора, клиента и проекта
        if (!await context.Operators.AnyAsync(o => o.PkOperator == operatorId))
            throw new ArgumentException("Operator not found");

        if (!await context.Clients.AnyAsync(c => c.PkClient == clientId))
            throw new ArgumentException("Client not found");

        if (!await context.Projects.AnyAsync(p => p.PkProject == projectId))
            throw new ArgumentException("Project not found");

        // Проверка статусов и их групп
        var existingStatuses = await context.ProjectStatuses
            .Include(ps => ps.StatusGroup) // Добавляем загрузку групп
            .Where(ps => statusIds.Contains(ps.PkProjectStatus))
            .ToListAsync();

        if (existingStatuses.Count != statusIds.Count)
            throw new ArgumentException("Some statuses not found");

        // Проверка на дублирование групп статусов
        var statusGroups = existingStatuses
            .Select(s => s.StatusGroup.PkStatusGroup)
            .ToList();

        if (statusGroups.Distinct().Count() != statusGroups.Count)
        {
            // Находим дублирующиеся группы для информативного сообщения
            var duplicateGroups = statusGroups
                .GroupBy(g => g)
                .Where(g => g.Count() > 1)
                .Select(g => existingStatuses.First(s => s.StatusGroup.PkStatusGroup == g.Key).StatusGroup.StatusGroupName)
                .ToList();

            throw new ArgumentException(
                $"Conversation can have only one status per group. Duplicate groups found: {string.Join(", ", duplicateGroups)}");
        }

        // Создаём звонок
        var conversation = new Conversation(
            timeStarted,
            timeEnded,
            pathToAudio,
            operatorId,
            clientId,
            projectId);

        // Добавляем статусы (загруженные из БД!)
        foreach (var status in existingStatuses)
        {
            conversation.AddStatus(status);
        }

        await UpdateClientStatusAsync(clientId, clientProcessingStatus);
        
        await context.Conversations.AddAsync(conversation);
        await context.SaveChangesAsync();

        return new ConversationDto
        {
            CallId = conversation.PkTalk,
            ProjectId = conversation.FkProject,
            OperatorId = conversation.FkOperator,
            ClientId = conversation.FkClient,
            CallStartedAt = conversation.TimeStarted,
            CallEndedAt = conversation.TimeEnded,
            PathToCallRecord = conversation.PathToAudio
        };
    }
    
    //№4 Получить все статусы звонка по id звонка (если не найден, то просто вернет пустой список)
    public async Task<List<CallStatusDto>> GetCallStatusesAsync(int callId)
    {
        return await context.ConversationStatuses
            .Where(cs => cs.Conversation.PkTalk == callId)
            .Include(cs => cs.ProjectStatus)
            .ThenInclude(ps => ps.StatusGroup)
            .Select(cs => new CallStatusDto
            {
                StatusGroupId = cs.ProjectStatus.StatusGroup.PkStatusGroup,
                StatusGroupName = cs.ProjectStatus.StatusGroup.StatusGroupName,
                StatusId = cs.ProjectStatus.PkProjectStatus,
                StatusName = cs.ProjectStatus.StatusName,
                TimeStatusSelected = cs.SelectedTime
            })
            .ToListAsync();
    }
    
    //№5 Получить заметку по id звонка
    public async Task<ClientNoteDto> GetNoteByCallIdAsync(int callId)
    {
        try
        {
            var note = await context.ClientNotes
                .FirstOrDefaultAsync(n => n.FkConversation == callId);
            return new ClientNoteDto
            {
                NoteId = note.PkNote,
                Text = note.Text,
                DateWritten = note.DateWritten
            };
        }
        catch (Exception)
        {
            return new ClientNoteDto
            {
                NoteId = 0,
                Text = null,
                DateWritten = null
            };
        }
        
    }
    // №6 получить клиента по id
    public async Task<ClientInfoDto> GetClientByCallIdAsync(int clientId)
    {
        var client = await context.Clients.FirstOrDefaultAsync(cl => cl.PkClient == clientId);

        if (client == null)throw new ArgumentException($"Клиент с id {clientId} не найден.");

        return new ClientInfoDto
        {
            ClientId = client.PkClient,
            Name = client.Name,
            PhoneNumber = client.PhoneNumber,
            ProcessingStatus = client.Status.ToString()
        };
    }
    //№7 Получить все звонки в рамках проекта за интервал времени
    public async Task<List<ConversationDto>> GetProjectConversationsAsync(
        int projectId, 
        DateTime startDate, 
        DateTime endDate)
    {
        return await context.Conversations
            .Where(c => c.FkProject == projectId &&
                        c.TimeStarted >= startDate && 
                        c.TimeEnded <= endDate)
            .Select(c => new ConversationDto()
            {
                CallId = c.PkTalk,
                ProjectId = c.FkProject,
                OperatorId = c.FkOperator,
                ClientId = c.FkClient,
                CallStartedAt = c.TimeStarted,
                CallEndedAt = c.TimeEnded,
                PathToCallRecord = c.PathToAudio,
            })
            .ToListAsync();
    }
    //#8 Изменить статус клиента
    public async Task<ClientInfoDto> UpdateClientStatusAsync(int clientId, string statusString)
    {
        // Пытаемся преобразовать строку в enum
        if (!Enum.TryParse<ClientStatus>(statusString, true, out var status))
        {
            throw new ArgumentException($"Invalid status value: {statusString}. " +
                                        $"Allowed values: {string.Join(", ", Enum.GetNames(typeof(ClientStatus)))}");
        }

        // Находим клиента
        var client = await context.Clients
            .FirstOrDefaultAsync(c => c.PkClient == clientId);

        if (client == null)
        {
            throw new ArgumentException($"Client with ID {clientId} not found");
        }

        // Обновляем статус
        client.Status = status;
        await context.SaveChangesAsync();

        return new ClientInfoDto()
        {
            ClientId = client.PkClient,
            Name = client.Name,
            PhoneNumber = client.PhoneNumber,
            ProcessingStatus = client.Status.ToString()
        };
    }
    
    //№9 оставить заметку о звонке
    public async Task<bool> LeaveNoteAsync(int callId, string note)
    {
        var callExists = await context.Conversations.AnyAsync(c => c.PkTalk == callId);
        if (!callExists)
        {
            throw new ArgumentException($"Звонок с id{callId} не найден");
        }
        var newNote = new ClientNote(DateTime.UtcNow, note, callId);
        await context.ClientNotes.AddAsync(newNote);
        await context.SaveChangesAsync();
        return true;
    }
    //№ 10 редактировать заметку
    public async Task<bool> EditNoteAsync(int callId, string text)
    {
        var call = await context.Conversations.FindAsync(callId);
        if (call == null)
        {
            throw new ArgumentException($"Звонок с id {callId} не найден" );
        }
        var note = await context.ClientNotes.FirstOrDefaultAsync(n => n.FkConversation == call.PkTalk);
        if (note == null)
        {
            throw new ArgumentException($"Заметка для звонка с id {callId} не найдена" );
        }

        note.Text = text;
        note.DateWritten = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }
} 