using Microsoft.EntityFrameworkCore;
using CallCenterRepository.Models;

namespace CallCenterRepository;


public class ApplicationDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectStatusGroup> ProjectStatusGroups { get; set; }
    public DbSet<ConversationStatus> ConversationStatuses { get; set; }
    public DbSet<ProjectStatus> ProjectStatuses { get; set; }
    public DbSet<OperatorProject> OperatorProjects { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Operator> Operators { get; set; }
    public DbSet<Client> Clients { get; set; }
    public virtual DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationGrade> ConversationGrades { get; set; }
    public DbSet<ClientNote> ClientNotes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка связи многие-ко-многим между Operator и Project
        modelBuilder.Entity<OperatorProject>()
            .HasKey(op => new { op.FkOperator, op.FkProject });

        modelBuilder.Entity<OperatorProject>()
            .HasOne(op => op.Operator)
            .WithMany()
            .HasForeignKey(op => op.FkOperator);

        modelBuilder.Entity<OperatorProject>()
            .HasOne(op => op.Project)
            .WithMany(p => p.OperatorProjects)
            .HasForeignKey(op => op.FkProject);
        //при удалении
        modelBuilder.Entity<Operator>()
            .HasOne(o => o.Admin)
            .WithMany()
            .HasForeignKey(o => o.FkAdmin)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.Operator)
            .WithMany()
            .HasForeignKey(c => c.FkOperator)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.FkClient)
            .OnDelete(DeleteBehavior.Cascade);
 
        modelBuilder.Entity<ConversationGrade>()
            .HasOne(cg => cg.Conversation)
            .WithMany()
            .HasForeignKey(cg => cg.FkConversation)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClientNote>()
            .HasOne(cn => cn.Conversation)
            .WithMany(c => c.Notes) // навигационное свойство в Conversation
            .HasForeignKey(cn => cn.FkConversation)
            .OnDelete(DeleteBehavior.Cascade);
        
        //уникальность имен пользователей
        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Username)
            .IsUnique();
        modelBuilder.Entity<Operator>()
            .HasIndex(op => op.Username)
            .IsUnique();
        
        modelBuilder.Entity<ConversationStatus>()
            .HasKey(cs => new { cs.FkTalk, cs.FkProjectStatus }); // Составной ключ

        modelBuilder.Entity<ConversationStatus>()
            .HasOne(cs => cs.Conversation)
            .WithMany(c => c.SelectedStatuses)
            .HasForeignKey(cs => cs.FkTalk);

        modelBuilder.Entity<ConversationStatus>()
            .HasOne(cs => cs.ProjectStatus)
            .WithMany()
            .HasForeignKey(cs => cs.FkProjectStatus);
        
        base.OnModelCreating(modelBuilder);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("" +
                                 "Host=localhost;" +
                                 "Port=5432;" +
                                 "Database=CallCenterDB;" +
                                 "Username=postgres;" +
                                 "Password=zsxdcf"
                                 );
    }

    


    
    
}