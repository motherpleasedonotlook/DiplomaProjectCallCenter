using CallCenterRepository;
using CallCenterRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace RatingService;

public class Services(ApplicationDbContext context)
{
    //#1 поставить оценку диалогу
    public async Task<List<GradesDto>> PutGrade(int conversationId, int gradeTypeValue, int grade)
    {
        if (!Enum.IsDefined(typeof(ConversationGradeType), gradeTypeValue))
            throw new ArgumentOutOfRangeException(nameof(gradeTypeValue), $"Значение '{gradeTypeValue}' не является допустимым значением для ConversationGradeType.");
        var type = (ConversationGradeType)gradeTypeValue;
        
        bool exists = await context.ConversationGrades
            .AnyAsync(g => g.FkConversation == conversationId && g.GradeType == type);
        
        if (exists)throw new InvalidOperationException($"Оценка типа '{type}' для разговора с ID '{conversationId}' уже существует.");
        
        var conversationGrade = new ConversationGrade(type, grade, conversationId);
        await context.ConversationGrades.AddAsync(conversationGrade);
        await context.SaveChangesAsync();
        // возвращается список всех оценок для данного разговора
        return await GetGrade(conversationId);
    }
    
    //#2 получить оценку по id диалога
    public async Task<List<GradesDto>> GetGrade(int conversationId)
    {
        return await context.ConversationGrades
            .Where(g => g.FkConversation == conversationId)
            .Select(g => new GradesDto
            {
                PkGrade = g.PkGrade,
                GradeType = g.GradeType.ToString(),
                Score = g.Score,
                FkConversation = g.FkConversation
            })
            .ToListAsync();
    }
    
    //#3 редактировать оценку
    public async Task<List<GradesDto>> EditGrade(int conversationId, int gradeTypeValue, int newGrade)
    {
        if (!Enum.IsDefined(typeof(ConversationGradeType), gradeTypeValue))
            throw new ArgumentOutOfRangeException(nameof(gradeTypeValue), $"Значение '{gradeTypeValue}' не является допустимым значением для ConversationGradeType.");
        var type = (ConversationGradeType)gradeTypeValue;
        
        var unit = await context.ConversationGrades
            .FirstOrDefaultAsync(g => g.FkConversation == conversationId && g.GradeType == type);
        
        if (unit==null)throw new ArgumentException($"Оценка типа '{type}' для разговора с ID '{conversationId}' не найдена.");

        unit.Score = newGrade;
        await context.SaveChangesAsync();
        // возвращается список всех оценок для данного разговора
        return await GetGrade(conversationId);
    }

    //#4 получить список рейтингов всех операторов данного администратора
    public async Task<List<OperatorRatingsDto>> GetActiveOperatorsRatingByAdminAsync(int adminId, bool isActive=true)
    {
        var adminExists = await context.Admins.AnyAsync(a => a.PkAdmin == adminId);
        if (!adminExists)throw new ArgumentException($"Администратор с ID {adminId} не найден.");
        
        var operators = await context.Operators
            .Where(o => o.FkAdmin == adminId && o.IsActive==isActive&& o.PkOperator!=adminId)
            .ToListAsync();
        
        var operatorRatings = new List<OperatorRatingsDto>();
        
        foreach (var operatorItem in operators)
        {
            var rating = await GetOperatorRatingFull(operatorItem.PkOperator);
            operatorRatings.Add(rating);
        }
        return operatorRatings;
    }

    //#5 получить общий рейтинг оператора
    public async Task<OperatorRatingsDto> GetOperatorRatingFull(int operatorId)
    {
        var operatorEntity = await context.Operators
            .FirstOrDefaultAsync(o => o.PkOperator == operatorId);

        if (operatorEntity == null)throw new ArgumentException("Operator not found.", nameof(operatorId));

        //оценки, связанные с разговорами этого оператора
        var grades = await context.ConversationGrades
            //.Include(g => g.Conversation) //включить разговор
            .Where(g => g.Conversation.FkOperator == operatorId) //ConversationGrade ссылается на оператора через Conversation
            .ToListAsync();

        //записываем средние значения каждого вида оценки
        var ratings = grades.GroupBy(g => g.GradeType.ToString())
            .ToDictionary(
                group => group.Key,
                group => group.Average(g => g.Score)
            );

        //общая оценка
        var fullScore = grades.Any() ? grades.Average(g => g.Score) : 0;
        
        var result = new OperatorRatingsDto
        {
            OperatorId = operatorEntity.PkOperator,
            OperatorName = operatorEntity.Username,
            FullScore = fullScore,
            Ratings = ratings
        };
        return result;
    }
} 