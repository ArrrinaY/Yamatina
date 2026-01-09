using InternshipService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InternshipService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<VacancyTag> VacancyTags => Set<VacancyTag>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
 
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime))
                {
                    if (property.CurrentValue is DateTime dateTime && dateTime.Kind != DateTimeKind.Utc)
                    {
                        property.CurrentValue = dateTime.Kind == DateTimeKind.Unspecified
                            ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                            : dateTime.ToUniversalTime();
                    }
                }
                else if (property.Metadata.ClrType == typeof(DateTime?))
                {
                    if (property.CurrentValue is DateTime dateTime && dateTime.Kind != DateTimeKind.Utc)
                    {
                        property.CurrentValue = dateTime.Kind == DateTimeKind.Unspecified
                            ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                            : dateTime.ToUniversalTime();
                    }
                }
            }
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Username).HasColumnName("username").IsRequired();
            entity.Property(u => u.Email).HasColumnName("email").IsRequired();
            entity.Property(u => u.Password).HasColumnName("password").IsRequired();
            entity.Property(u => u.Role).HasColumnName("role").IsRequired();
            entity.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.Name).HasColumnName("name").IsRequired();
            entity.Property(c => c.Description).HasColumnName("description");
            entity.Property(c => c.Website).HasColumnName("website");
            entity.Property(c => c.Email).HasColumnName("email").IsRequired();
            entity.Property(c => c.Phone).HasColumnName("phone");
            entity.Property(c => c.CreatedDate).HasColumnName("created_date").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        
        modelBuilder.Entity<Vacancy>(entity =>
        {
            entity.ToTable("vacancies");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Id).HasColumnName("id");
            entity.Property(v => v.Title).HasColumnName("title").IsRequired();
            entity.Property(v => v.Description).HasColumnName("description");
            entity.Property(v => v.Requirements).HasColumnName("requirements");
            entity.Property(v => v.SalaryRange).HasColumnName("salary_range");
            entity.Property(v => v.Type).HasColumnName("type").IsRequired();
            entity.Property(v => v.Location).HasColumnName("location");
            entity.Property(v => v.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(v => v.CreatedDate).HasColumnName("created_date").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(v => v.CompanyId).HasColumnName("company_id");
            
            entity.HasOne(v => v.Company)
                .WithMany(c => c.Vacancies)
                .HasForeignKey(v => v.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.ToTable("candidates");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.FirstName).HasColumnName("first_name").IsRequired();
            entity.Property(c => c.LastName).HasColumnName("last_name").IsRequired();
            entity.Property(c => c.Email).HasColumnName("email").IsRequired();
            entity.Property(c => c.Phone).HasColumnName("phone");
            entity.Property(c => c.ResumeUrl).HasColumnName("resume_url");
            entity.Property(c => c.Skills).HasColumnName("skills");
            entity.Property(c => c.ExperienceLevel).HasColumnName("experience_level");
        });
        
        modelBuilder.Entity<Application>(entity =>
        {
            entity.ToTable("applications");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.CandidateId).HasColumnName("candidate_id");
            entity.Property(a => a.VacancyId).HasColumnName("vacancy_id");
            entity.Property(a => a.Status).HasColumnName("status").IsRequired();
            entity.Property(a => a.AppliedDate).HasColumnName("applied_date").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(a => a.CoverLetter).HasColumnName("cover_letter");
            
            entity.HasOne(a => a.Candidate)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(a => a.Vacancy)
                .WithMany(v => v.Applications)
                .HasForeignKey(a => a.VacancyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.Name).HasColumnName("name").IsRequired();
            entity.Property(t => t.Category).HasColumnName("category").IsRequired();
        });
        
        modelBuilder.Entity<VacancyTag>(entity =>
        {
            entity.ToTable("vacancy_tags");
            entity.HasKey(vt => new { vt.VacancyId, vt.TagId });
            entity.Property(vt => vt.VacancyId).HasColumnName("vacancy_id");
            entity.Property(vt => vt.TagId).HasColumnName("tag_id");
            
            entity.HasOne(vt => vt.Vacancy)
                .WithMany(v => v.VacancyTags)
                .HasForeignKey(vt => vt.VacancyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(vt => vt.Tag)
                .WithMany(t => t.VacancyTags)
                .HasForeignKey(vt => vt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.ToTable("api_keys");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.Key).HasColumnName("key").IsRequired();
            entity.HasIndex(a => a.Key).IsUnique();
            entity.Property(a => a.Expiration).HasColumnName("expiration").IsRequired();
            entity.Property(a => a.IsActive).HasColumnName("is_active").IsRequired();
        });
    }
}

