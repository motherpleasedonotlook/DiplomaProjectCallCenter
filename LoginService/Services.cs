using CallCenterRepository;
using CallCenterRepository.Models;
using Microsoft.EntityFrameworkCore;
namespace LoginService;

public class Services(ApplicationDbContext context)
{
    //регистрация администратора + создание связанного профиля оператора
    public async Task<AdminDto> RegisterAdminAsync(string username, string password)
    {
        // Проверяем существующего администратора
        if (await context.Admins.AnyAsync(a => a.Username == username))
        {
            throw new InvalidOperationException($"Администратор с именем {username} уже существует.");
        }

        // Создаём администратора, но НЕ сохраняем его сразу
        var newAdmin = new Admin(username, password,-1);
        context.Admins.Add(newAdmin);

        OperatorDto selfOperatorProfile;
        try
        {
            // сохраняем администратора, чтобы получить его id
            await context.SaveChangesAsync();
            //создаём оператора
            selfOperatorProfile = await AddOperatorAsync(newAdmin.PkAdmin, username, password);
            //делаем указание на него в профиле админа
            var admin = await context.Admins
                .FirstOrDefaultAsync(a => a.PkAdmin==newAdmin.PkAdmin);
            if(admin==null)throw new ArgumentException($"Администратор с id {newAdmin.PkAdmin} не найден.");
            admin.SelfOperatorProfile = selfOperatorProfile.PkOperator;
            
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // отменяем изменения, если связанный профиль не создался
            context.Admins.Remove(newAdmin);
            await context.SaveChangesAsync();

            throw new InvalidOperationException(
                $"Не удалось создать профиль оператора: {ex.Message}");
        }

        return new AdminDto
        {
            PkAdmin = newAdmin.PkAdmin,
            Created = newAdmin.Created,
            IsActive = newAdmin.IsActive,
            Password = newAdmin.Password,
            Username = newAdmin.Username,
            SelfOperatorProfileId = selfOperatorProfile.PkOperator
        };
    }
    
    //возвращает профиль администратора с заданныи именем и паролем
    public async Task<AdminDto> CheckAdminAsync(string username, string password)
    {
        // существует ли администратор с таким юзернеймом и паролем
        var existingAdmin = await context.Admins
            .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);
        
        if (existingAdmin == null)throw new ArgumentException("Администратор с такими данными не найден.");
        return new AdminDto
        {
            PkAdmin = existingAdmin.PkAdmin,
            Created = existingAdmin.Created,
            IsActive = existingAdmin.IsActive,
            Password = existingAdmin.Password,
            Username = existingAdmin.Username,
            SelfOperatorProfileId = existingAdmin.SelfOperatorProfile
        };
    }
    
    public async Task<OperatorDto> CheckOperatorAsync(string username, string password)
    {
        // существует ли администратор с таким юзернеймом и паролем
        var existingOperator = await context.Operators
            .FirstOrDefaultAsync(op => op.Username == username && op.Password == password);
        
        if (existingOperator == null)throw new ArgumentException("Администратор с такими данными не найден.");
        return new OperatorDto
        {
            PkOperator = existingOperator.PkOperator,
            Created = existingOperator.Created,
            IsActive = existingOperator.IsActive,
            Password = existingOperator.Password,
            Username = existingOperator.Username,
            FkAdmin = existingOperator.FkAdmin
        };
    }
    
    //добавляет оператора и возвращает его профиль
    public async Task<OperatorDto> AddOperatorAsync(int adminId, string username, string password)
    {
        var admin = await context.Admins.FindAsync(adminId);
        if (admin == null)throw new ArgumentException($"Администратор с id {adminId} не найден.");
        var existingOperator = await context.Operators
            .AnyAsync(o => o.Username == username);
        if (existingOperator)throw new InvalidOperationException($"Оператор с именем {username} уже существует.");
        
        var newOperator = new Operator(username, password, adminId);
        await context.Operators.AddAsync(newOperator);
        await context.SaveChangesAsync();
        return new OperatorDto
        {
            PkOperator = newOperator.PkOperator,
            Username = newOperator.Username,
            Password = newOperator.Password,
            Created = newOperator.Created,
            IsActive = newOperator.IsActive,
            FkAdmin = newOperator.FkAdmin
        };
    }
    
    //сменить пароль админа
    public async Task<string> EditAdminsPassword(int idAdmin, string oldPassword, string newPassword)
    {
        // существует ли администратор с таким id
        var admin = await context.Admins
            .FirstOrDefaultAsync(a => a.PkAdmin == idAdmin);
        if (admin == null)throw new ArgumentException($"Администратор с id {idAdmin} не найден.");
        var selfOperatorProfile =
            await context.Operators.FirstOrDefaultAsync(o => o.PkOperator == admin.SelfOperatorProfile);
        if (selfOperatorProfile == null)throw new ArgumentException($"Не найден связанный профиль для администратора с id {idAdmin}.");
        if (admin.Password == oldPassword || selfOperatorProfile.Password == oldPassword)
        {
            admin.Password = newPassword;
            selfOperatorProfile.Password = newPassword;
            await context.SaveChangesAsync();
        }else throw new InvalidOperationException($"Неверный пароль для администратора {admin.Username} id {idAdmin}.");

        return admin.Password;
    }
    
}