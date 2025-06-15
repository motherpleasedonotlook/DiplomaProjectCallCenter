using CallCenterRepository;
using CallCenterRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectService;

public class Services(ApplicationDbContext context)
{
    //№1 добавление клиентов в проект
    public async Task<int> AddClientsToProject(
    int fkProject, 
    List<Dictionary<string, string>> clientData)
    {
        if (clientData == null || clientData.Count == 0)
            throw new ArgumentException("Client data list cannot be null or empty.", nameof(clientData));

        // Проверяем существование проекта
        var projectExists = await context.Projects.AnyAsync(p => p.PkProject == fkProject);
        if (!projectExists)
            throw new ArgumentException($"Project with id {fkProject} not found.");

        // Получаем существующие номера для проекта (для проверки дубликатов)
        var existingNumbers = await context.Clients
            .Where(c => c.FkProject == fkProject)
            .Select(c => c.PhoneNumber)
            .ToListAsync();

        var clients = new List<Client>();
        foreach (var data in clientData)
        {
            try
            {
                var phoneNumber = data["PhoneNumber"].Trim();
                var name = data["Name"].Trim();
                
                if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 12)
                    throw new ArgumentException("Invalid phone number format.");
                
                // проверка на дубликат в текущем проекте
                if (existingNumbers.Contains(phoneNumber)) continue; //пропускаем его

                var client = new Client(
                    phoneNumber: phoneNumber,
                    name: name ?? throw new InvalidOperationException("Name field is empty."),
                    fkProject: fkProject
                );
                clients.Add(client);
            }
            catch (KeyNotFoundException ex)
            {
                throw new ArgumentException($"Missing required field: {ex.Message}");
            }
        }

        if (clients.Count == 0)
            return 0;

        // Сохраняем в БД в транзакции
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await context.Clients.AddRangeAsync(clients);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return clients.Count;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    //№2 получение списка клиентов проекта
    public async Task<List<ClientInfoDto>> GetProjectClients(int projectId)
    {
        // Проверяем существование проекта
        var projectExists = await context.Projects
            .AnyAsync(p => p.PkProject == projectId);
    
        if (!projectExists)
        {
            throw new ArgumentException($"Проект с id {projectId} не найден.");
        }

        // Получаем клиентов проекта
        var clients = await context.Clients
            .Where(cl => cl.Project.PkProject == projectId)
            .Select(cl => new ClientInfoDto
            {
                ClientId = cl.PkClient,
                PhoneNumber = cl.PhoneNumber,
                Name = cl.Name,
                ProcessingStatus = cl.Status.ToString()
            })
            .ToListAsync();

        return clients;
    }
    
    //№3 связать операторов с проектом
    public async Task<int> AddOperatorsToProjectAsync(int projectId, List<int> operatorIds)
    {
        // проект есть
        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.PkProject == projectId);
    
        if (project == null)
            throw new ArgumentException($"Проект с ID {projectId} не найден.");

        // проверка операторов
        var existingOperators = await context.Operators
            .Where(o => operatorIds.Contains(o.PkOperator)) 
            .Where(o => o.FkAdmin == project.FkAdmin) //тот же админ
            .Select(o => o.PkOperator)
            .ToListAsync();
        
        var missingOperators = operatorIds.Except(existingOperators).ToList();
        if (missingOperators.Any())
        {
            throw new ArgumentException(
                $"Операторы не найдены или не принадлежат администратору: {string.Join(", ", missingOperators)}", 
                nameof(operatorIds));
        }

        // кто из списка уже связан с проектом
        var existingLinks = await context.OperatorProjects
            .Where(op => op.FkProject == projectId)
            .Select(op => op.FkOperator)
            .ToListAsync();
        // кого нет в проекте
        var operatorsToAdd = operatorIds.Except(existingLinks).ToList();

        // добавляем ток тех, кого не было
        foreach (var operatorId in operatorsToAdd)
        {
            context.OperatorProjects.Add(new OperatorProject
            {
                FkOperator = operatorId,
                FkProject = projectId,
                AssignedDate = DateTime.UtcNow
            });
        }
        project.LastUpdate = DateTime.UtcNow;

        // Сохраняем изменения и возвращаем количество добавленных операторов
        await context.SaveChangesAsync();
        return operatorsToAdd.Count;
    }
    
    //№4 создать проект
    public async Task<ProjectDto> CreateProjectAsync(
    string projectName,
    string? scriptText,
    int callInterval,
    TimeOnly startTime,
    TimeOnly endTime,
    int timeZoneOffset,
    int adminId,
    int selfOperatorProfileId)
    {
        // Проверка существования администратора
        var adminExists = await context.Admins.AnyAsync(a => a.PkAdmin == adminId);
        if (!adminExists)
        {
            throw new ArgumentException("Администратор не найден", nameof(adminId));
        }
        // создание нового проекта
        var project = new Project(
            projectName: projectName,
            callInterval: callInterval,
            startTime: startTime,
            endTime: endTime,
            timeZoneOffset: timeZoneOffset,
            fkAdmin: adminId)
        {
            ScriptText = scriptText,
            LastUpdate = DateTime.UtcNow,
            Created = DateTime.UtcNow,
            ProjectStatus = false,
            Closed = null // по умолчанию проект не закрыт
        };

        // добавление проекта в контекст
        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();
        try
        {
            await AddOperatorsToProjectAsync(project.PkProject, [selfOperatorProfileId]);
        }
        catch(Exception exception)
        {
            Console.WriteLine(exception.Message);
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
        }
        // получение созданного проекта с деталями для возврата
        try
        {
            return await GetProjectByIdAsync(project.PkProject);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException(exception.Message+
                $" Проект {project.ProjectName} не был создан");
        }
    }
 
    // №5 получение проекта
    public async Task<ProjectDto> GetProjectByIdAsync(int projectId)
    {
        var result = await context.Projects
            .Where(p => p.PkProject == projectId)
            .Select(p => new ProjectDto()
            {
                ProjectId = p.PkProject,
                ProjectName = p.ProjectName,
                ProjectCreated = p.Created,
                ProjectLastUpdate = p.LastUpdate,
                IsProjActive = p.ProjectStatus,
                ProjectScriptText = p.ScriptText,
                ProjectCallInterval = p.CallInterval,
                ProjectStartsAt = p.StartTime,
                ProjectEndsAt = p.EndTime,
                ProjectTimeOffset = p.TimeZoneOffset,
                ProjectClosedAt = p.Closed
            })
            .FirstOrDefaultAsync();
        if (result == null) throw new ArgumentException($"Проект с id {projectId} не найден.");
        return result;
    }
  
    //№6 получить всех операторов, связанных с данным проектом
    public async Task<List<OperatorDto>> GetOperatorsByProjectIdAsync(int projectId)
    {
        if (!await context.Projects.AnyAsync(p => p.PkProject == projectId))
            throw new ArgumentException($"Проект с ID {projectId} не найден");

        return await context.OperatorProjects
            .Where(op => op.FkProject == projectId)
            .Include(op => op.Operator)
            .Select(op => new OperatorDto
            {
                PkOperator = op.Operator.PkOperator,
                Username = op.Operator.Username,
                Password = op.Operator.Password,
                Created = op.Operator.Created,
                IsActive = op.Operator.IsActive,
                FkAdmin = op.Operator.FkAdmin
            })
            .ToListAsync();
    }
    
    //№7 редактирование проекта
    public async Task<ProjectDto> EditProjectAsync(
        int projectId,
        string? newName = null,
        int? newCallInterval = null,
        TimeOnly? newStartTime = null,
        TimeOnly? newEndTime = null,
        int? newTimeZoneOffset = null)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.PkProject == projectId);
    
        if (project == null)
            throw new ArgumentException($"Проект с ID {projectId} не найден");

        if (newName != null) project.ProjectName = newName;
        if (newCallInterval != null) project.CallInterval = newCallInterval.Value;
        if (newStartTime != null) project.StartTime = newStartTime.Value;
        if (newEndTime != null) project.EndTime = newEndTime.Value;
        if (newTimeZoneOffset != null) project.TimeZoneOffset = newTimeZoneOffset.Value;

        project.LastUpdate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return await GetProjectByIdAsync(projectId);
    }
    
    //№8 Редактировать скрипт диалога
    public async Task<string> EditProjectScriptAsync(int projectId, string newScript)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.PkProject == projectId);
    
        if (project == null)
            throw new ArgumentException($"Проект с ID {projectId} не найден");
        project.ScriptText = newScript;
        await context.SaveChangesAsync();

        return project.ScriptText;
    }
    
    //№9 переключить статус проекта
    public async Task<ProjectDto> SwitchProjectStatusAsync(int projectId)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.PkProject == projectId);
    
        if (project == null)
            throw new ArgumentException($"Проект с ID {projectId} не найден");

        project.ProjectStatus = !project.ProjectStatus;
        project.LastUpdate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return await GetProjectByIdAsync(projectId);
    }
    
    //№10 закрыть проект
    public async Task<ProjectDto> CloseProjectAsync(int projectId)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.PkProject == projectId);
    
        if (project == null)
            throw new ArgumentException($"Проект с ID {projectId} не найден");

        project.Closed = DateTime.UtcNow;
        project.ProjectStatus = false;
        project.LastUpdate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return await GetProjectByIdAsync(projectId);
    }
    
    // №11 снять операторов с проекта
    public async Task<int> RemoveOperatorsFromProjectAsync(int projectId, List<int> operatorIds)
    {
        // Проверяем существование проекта
        var projectExists = await context.Projects.AnyAsync(p => p.PkProject == projectId);
        if (!projectExists)
        {
            throw new ArgumentException($"Проект с ID {projectId} не найден");
        }

        // Получаем существующие связи операторов с проектом
        var linksToRemove = await context.OperatorProjects
            .Where(op => op.FkProject == projectId && operatorIds.Contains(op.FkOperator))
            .ToListAsync();

        if (!linksToRemove.Any())
        {
            return 0; // Нет операторов для открепления
        }

        // Открепляем операторов
        context.OperatorProjects.RemoveRange(linksToRemove);

        // Обновляем дату последнего изменения проекта
        var project = await context.Projects.FindAsync(projectId);
        if (project != null)
        {
            project.LastUpdate = DateTime.UtcNow;
        }

        // Сохраняем изменения
        await context.SaveChangesAsync();

        return linksToRemove.Count;
    }
    
    // №12 получение проектов администратора
    public async Task<List<ProjectDto>> GetProjects(int adminId)
    {
        var result = await context.Projects
            .Where(p => p.FkAdmin == adminId)
            .Select(p => new ProjectDto()
            {
                ProjectId = p.PkProject,
                ProjectName = p.ProjectName,
                ProjectCreated = p.Created,
                ProjectLastUpdate = p.LastUpdate,
                IsProjActive = p.ProjectStatus,
                ProjectScriptText = p.ScriptText,
                ProjectCallInterval = p.CallInterval,
                ProjectStartsAt = p.StartTime,
                ProjectEndsAt = p.EndTime,
                ProjectTimeOffset = p.TimeZoneOffset,
                ProjectClosedAt = p.Closed
            })
            .ToListAsync();
        if (result == null) throw new ArgumentException($"Администратор с id {adminId} не найден.");
        return result;
    }
    // №13 актуальные проекты оператора
    public async Task<List<ProjectDto>> GetOperatorProjectsAsync(int operatorId)
    {
        if (!await context.Operators.AnyAsync(o => o.PkOperator == operatorId))
        {
            throw new ArgumentException($"Оператор с id {operatorId} не найден.");
        }
        return await context.OperatorProjects
            .Where(op => op.FkOperator == operatorId)
            .Include(op => op.Project) // связанный проект
            .Select(op => new ProjectDto
            {
                ProjectId = op.Project.PkProject,
                ProjectName = op.Project.ProjectName,
                ProjectCreated = op.Project.Created,
                ProjectLastUpdate = op.Project.LastUpdate,
                IsProjActive = op.Project.ProjectStatus,
                ProjectScriptText = op.Project.ScriptText,
                ProjectCallInterval = op.Project.CallInterval,
                ProjectStartsAt = op.Project.StartTime,
                ProjectEndsAt = op.Project.EndTime,
                ProjectTimeOffset = op.Project.TimeZoneOffset,
                ProjectClosedAt = op.Project.Closed
            })
            .ToListAsync();
    }
    
    //№14 все активные операторы, не связанные с проектом N
    public async Task<List<OperatorDto>> GetAvailableOperatorsForProjectAsync(int adminId, int projectId)
    {
        return await context.Operators
            .Where(o => o.FkAdmin == adminId && o.IsActive)
            .Where(o => !context.OperatorProjects
                .Any(op => op.FkProject == projectId && op.FkOperator == o.PkOperator))
            .Select(o => new OperatorDto
            {
                PkOperator = o.PkOperator,
                Username = o.Username,
                Password = o.Password,
                Created = o.Created,
                IsActive = o.IsActive,
                FkAdmin = o.FkAdmin
            })
            .ToListAsync();
    } 
    //№15 Получить все звонки оператора по проекту за интервал времени
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

    //№16 Создать группу статусов вместе со статусами
    public async Task<StatusGroupDto> CreateStatusGroupAsync(
        string groupName,
        int projectId,
        List<string>? statusNames)
    {
        // Проверяем существование проекта
        var projectExists = await context.Projects.AnyAsync(p => p.PkProject == projectId);
        if (!projectExists)
        {
            throw new ArgumentException("Project not found", nameof(projectId));
        }

        // Создаем группу статусов
        var group = new ProjectStatusGroup
        {
            StatusGroupName = groupName,
            FkProject = projectId
        };

        await context.ProjectStatusGroups.AddAsync(group);
        await context.SaveChangesAsync(); // Сохраняем, чтобы получить PkStatusGroup

        // Создаем список для всех статусов (включая Undefined)
        var allStatuses = new List<ProjectStatus>();

        // Добавляем Undefined
        allStatuses.Add(new ProjectStatus
        {
            StatusName = "Undefined",
            FkStatusGroup = group.PkStatusGroup
        });

        // Добавляем статусы из списка statusNames (если они есть)
        if (statusNames != null && statusNames.Any())
        {
            allStatuses.AddRange(from statusName in statusNames where !string.IsNullOrWhiteSpace(statusName) select new ProjectStatus { StatusName = statusName.Trim(), FkStatusGroup = @group.PkStatusGroup });
        }

        // Добавляем все статусы в контекст
        await context.ProjectStatuses.AddRangeAsync(allStatuses);
        await context.SaveChangesAsync();

        // Получаем все статусы группы из базы (включая ID)
        var savedStatuses = await context.ProjectStatuses
            .Where(s => s.FkStatusGroup == group.PkStatusGroup)
            .ToListAsync();

        // Возвращаем данные группы статусов со списком статусов
        return new StatusGroupDto
        {
            StatusGroupId = group.PkStatusGroup,
            StatusGroupName = group.StatusGroupName,
            Statuses = savedStatuses.Select(s => new StatusDto
            {
                StatusId = s.PkProjectStatus,
                StatusName = s.StatusName
            }).ToList()
        };
    }
    
    //№17 получить список статус-групп проекта и их статусов
    public async Task<List<StatusGroupDto>> GetStatusGroupsAsync(int projectId)
    {
        var projectExists = await context.Projects.AnyAsync(p => p.PkProject == projectId);
        if (!projectExists)
        {
            throw new ArgumentException("Project not found", nameof(projectId));
        }
        var statusGroups = await context.ProjectStatusGroups
            .Where(sg => sg.FkProject == projectId)
            .Include(sg => sg.Statuses)
            .Select(sg => new StatusGroupDto
            {
                StatusGroupId = sg.PkStatusGroup,
                StatusGroupName = sg.StatusGroupName,
                Statuses = sg.Statuses.Select(s => new StatusDto
                {
                    StatusId = s.PkProjectStatus,
                    StatusName = s.StatusName
                }).ToList()
            })
            .ToListAsync();
        return statusGroups;
    }
    
    //№18 создать новый статус и добавить его в существующую группу
    public async Task<StatusDto> CreateStatusInGroupAsync(int groupId, string statusName)
    {
        var groupExists = await context.ProjectStatusGroups.AnyAsync(g => g.PkStatusGroup == groupId);
        if (!groupExists)
        {
            throw new ArgumentException("Status group not found", nameof(groupId));
        }

        var status = new ProjectStatus
        {
            StatusName = statusName,
            FkStatusGroup = groupId
        };

        await context.ProjectStatuses.AddAsync(status);
        await context.SaveChangesAsync();

        return new StatusDto
        {
            StatusId = status.PkProjectStatus,
            StatusName = status.StatusName
        };
    }
    
    //№19 открепить статус от группы 
    public async Task<bool> DetachStatusFromGroupAsync(int statusId)
    {
        var status = await context.ProjectStatuses
            .FirstOrDefaultAsync(s => s.PkProjectStatus == statusId);
    
        if (status == null)
        {
            throw new ArgumentException("Status not found", nameof(statusId));
        }

        // Отключаем трекинг навигационного свойства
        context.Entry(status).Reference(s => s.StatusGroup).CurrentValue = null;
        status.FkStatusGroup = null;
    
        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            // Логируем полную ошибку
            Console.WriteLine($"Database update error: {ex.InnerException?.Message ?? ex.Message}");
            throw; // Перебрасываем исключение для обработки в контроллере
        }
    }
    
    //№20 получить список всех свободных статусов звонка
    public async Task<List<string>> GetFreeStatusNamesByCallIdAsync(int callId)
    {
        return await context.ConversationStatuses
            .Where(cs => cs.FkTalk == callId) 
            .Select(cs => cs.ProjectStatus) 
            .Where(ps => ps.FkStatusGroup == null) // Берём только свободные статусы (без группы)
            .Select(ps => ps.StatusName) 
            .ToListAsync();
    }
    //№21 найти и добавить в группу свободный статус
    public async Task<bool> AttachFreeStatusToGroupAsync(int statusId, int groupId)
    {
        var status = await context.ProjectStatuses.FindAsync(statusId);
        if (status == null)
        {
            throw new ArgumentException("Status not found", nameof(statusId));
        }

        if (status.FkStatusGroup != null)
        {
            throw new InvalidOperationException("Status is not free");
        }

        var groupExists = await context.ProjectStatusGroups.AnyAsync(g => g.PkStatusGroup == groupId);
        if (!groupExists)
        {
            throw new ArgumentException("Status group not found", nameof(groupId));
        }

        status.FkStatusGroup = groupId;
        await context.SaveChangesAsync();
        return true;
    }
    
    //№22 удаляем группу статусов (открепляя статусы, тк они пригодятся для корректного отображения звонков)
    public async Task<bool> DetachStatusGroupFromProjectAsync(int groupId, int projectId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
    
        try
        {
            var group = await context.ProjectStatusGroups
                .Include(g => g.Statuses)
                .FirstOrDefaultAsync(g => g.PkStatusGroup == groupId);
        
            if (group == null)
            {
                throw new ArgumentException("Status group not found", nameof(groupId));
            }

            if (group.FkProject != projectId)
            {
                throw new InvalidOperationException("Status group does not belong to this project");
            }

            // Обновляем статусы одним запросом без загрузки в память
            await context.ProjectStatuses
                .Where(s => s.FkStatusGroup == groupId && s.StatusName != "Undefined")
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.FkStatusGroup, (int?)null));
        
            context.ProjectStatusGroups.Remove(group);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    // №23 переименовываем группу
    public async Task<string> RenameGroupAsync(int groupId, string newName)
    {
        //находим группу
        var group = await context.ProjectStatusGroups.FindAsync(groupId);
        if (group == null)
        {
            throw new ArgumentException("Status group not found", nameof(groupId));
        }

        group.StatusGroupName = newName;
        await context.SaveChangesAsync();
        return group.StatusGroupName;
    }
    
    //№24 переименовываем статус
    public async Task<string> RenameStatusAsync(int statusId, string newName)
    {
        //находим группу
        var status = await context.ProjectStatuses.FindAsync(statusId);
        if (status == null)
        {
            throw new ArgumentException("Status group not found", nameof(statusId));
        }

        status.StatusName = newName;
        await context.SaveChangesAsync();
        return status.StatusName;
    }
    
} 