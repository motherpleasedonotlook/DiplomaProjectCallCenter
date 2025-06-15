using CallCenterRepository;
using CallCenterRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace UserService;

public class Services(ApplicationDbContext context)
{
    //№1 получить профиль админа по id
    public async Task<AdminDto> GetAdminProfile(int adminId)
    {
        var admin = await context.Admins
            .FirstOrDefaultAsync(a => a.PkAdmin==adminId);
        if(admin==null)throw new ArgumentException($"Администратор с id {adminId} не найден.");
        var result = new AdminDto
        {
            PkAdmin = admin.PkAdmin,
            Created = admin.Created,
            IsActive = admin.IsActive,
            Password = admin.Password,
            Username = admin.Username,
            SelfOperatorProfileId = admin.SelfOperatorProfile
        };
        return result;
    }
    
    //№3 возвращает всех операторов данного админа
    public async Task<List<OperatorDto>> GetAllOperatorsByAdminAsync(int adminId)
    {
        var admin = await context.Admins.FirstOrDefaultAsync(a => a.PkAdmin == adminId);
        if (admin==null)throw new ArgumentException($"Администратор с ID {adminId} не найден.");
       
        return await context.Operators
            .Where(op => op.FkAdmin == adminId && op.PkOperator!= admin.SelfOperatorProfile)
            .Select(op => new OperatorDto
            {
                PkOperator = op.PkOperator,
                Username = op.Username,
                Password = op.Password,
                Created = op.Created,
                IsActive = op.IsActive,
                FkAdmin = op.FkAdmin
            })
            .ToListAsync();
    }
    
    //№4 возвращает оператора по id 
    public async Task<OperatorDto> GetOperatorByAdminAsync(int operatorId)
    {
        var operator_ = await context.Operators
            .FirstOrDefaultAsync(op => op.PkOperator==operatorId);
        if(operator_==null)throw new ArgumentException($"Оператор с id {operatorId} не найден.");
        return new OperatorDto
        {
            PkOperator = operator_.PkOperator,
            Username = operator_.Username,
            Password = operator_.Password,
            Created = operator_.Created,
            IsActive = operator_.IsActive,
            FkAdmin = operator_.FkAdmin
        };
    }
    
    //№5 изменяет имя оператора и возвращает профиль
    public async Task<OperatorDto> EditOperatorUsernameAsync(int operatorId, string newUsername)
    {
        var operator_ = await context.Operators
            .FirstOrDefaultAsync(op => op.PkOperator==operatorId); 
        if(operator_==null)throw new ArgumentException($"Оператор с id {operatorId} не найден.");
        var sameUsername = await context.Operators
            .AnyAsync(o => o.Username == newUsername && o.PkOperator!=operatorId);
        if (sameUsername)throw new InvalidOperationException($"Имя {newUsername} занято.");
        operator_.Username = newUsername;
        await context.SaveChangesAsync();
        return new OperatorDto
        {
            PkOperator = operator_.PkOperator,
            Username = operator_.Username,
            Password = operator_.Password,
            Created = operator_.Created,
            IsActive = operator_.IsActive,
            FkAdmin = operator_.FkAdmin
        };
    }
    //%6 переключение статуса оператора
    public async Task<OperatorDto> SwitchOperatorStatusAsync(int operatorId)
    {
        var operator_ = await context.Operators
            .FirstOrDefaultAsync(op => op.PkOperator==operatorId);
        if(operator_==null)throw new ArgumentException($"Оператор с id {operatorId} не найден.");
        operator_.IsActive = operator_.IsActive != true;
        await context.SaveChangesAsync();
        return new OperatorDto
        {
            PkOperator = operator_.PkOperator,
            Username = operator_.Username,
            Password = operator_.Password,
            Created = operator_.Created,
            IsActive = operator_.IsActive,
            FkAdmin = operator_.FkAdmin
        };
    }
    
    //№7 добавление клиентов в проект
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
    //№8 получение списка клиентов проекта
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
    
    //№9 связать операторов с проектом
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
    
    //№10 получить всех операторов, связанных с данным проектом
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
    
    //#11 все активные операторы, не связанные с проектом N
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
    
} 