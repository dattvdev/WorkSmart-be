using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WorkSmart.Core.Entity;

namespace WorkSmart.Repository
{
    public class WorksmartDBContext : DbContext
    {
        public WorksmartDBContext()
        {
        }

        public WorksmartDBContext(DbContextOptions<WorksmartDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<CV_Experience> CV_Experiences { get; set; }
        public DbSet<CV_Education> CV_Educations { get; set; }
        public DbSet<CV_Certification> CV_Certifications { get; set; }
        public DbSet<CV_Skill> CV_Skills { get; set; }
        public DbSet<CV_Template> CVTemplates { get; set; }
        public DbSet<FavoriteJob> FavoriteJobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PersonalMessage> PersonalMessages { get; set; }
        public DbSet<ReportPost> ReportPosts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NotificationJobTag> NotificationJobTags { get; set; }
        public DbSet<NotificationSetting> NotificationSettings { get; set; }
        public DbSet<CVEmbedding> CVEmbeddings { get; set; }
        public DbSet<JobEmbedding> JobEmbeddings { get; set; }
        public DbSet<JobAlert> JobAlerts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CV>()
            .HasOne(cv => cv.CVTemplate)
            .WithMany(template => template.CVs)
            .HasForeignKey(cv => cv.CVTemplateId)
            .OnDelete(DeleteBehavior.Restrict); //Do not delete template when CV still exists

            modelBuilder.Entity<PersonalMessage>()
                .HasOne(pm => pm.Sender)
                .WithMany(u => u.MessagesSent)
                .HasForeignKey(pm => pm.SenderID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PersonalMessage>()
                .HasOne(pm => pm.Receiver)
                .WithMany(u => u.MessagesReceived)
                .HasForeignKey(pm => pm.ReceiverID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.JobID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.CV)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.CVID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FavoriteJob>()
                .HasOne(f => f.User)
                .WithMany(u => u.FavoriteJobs)
                .HasForeignKey(f => f.UserID)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<FavoriteJob>()
                .HasOne(f => f.Job)
                .WithMany(j => j.FavoriteJobs)
                .HasForeignKey(f => f.JobID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ReportPost>()
                .HasOne(rp => rp.Sender)  
                .WithMany(u => u.ReportPosts) 
                .HasForeignKey(rp => rp.SenderID)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<ReportPost>()
                .HasOne(rp => rp.Job) 
                .WithMany(j => j.ReportPosts)  
                .HasForeignKey(rp => rp.JobID)  
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>()
               .HasOne(u => u.NotificationSetting) // User có một NotificationSetting
               .WithOne(ns => ns.User) // NotificationSetting có một User
               .HasForeignKey<NotificationSetting>(ns => ns.UserID) // Khóa ngoại ở NotificationSetting
               .OnDelete(DeleteBehavior.Cascade); // Có thể chọn hành vi xóa (Cascade khi User bị xóa)
            modelBuilder.Entity<CVEmbedding>()
            .HasKey(e => e.CVID);

            modelBuilder.Entity<CVEmbedding>()
                .HasOne(e => e.CV)
                .WithOne(cv => cv.Embedding) // nếu bạn có property ngược lại
                .HasForeignKey<CVEmbedding>(e => e.CVID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<JobEmbedding>()
            .HasKey(e => e.JobID);

            modelBuilder.Entity<JobEmbedding>()
                .HasOne(e => e.Job)
                .WithOne(j => j.Embedding)
                .HasForeignKey<JobEmbedding>(e => e.JobID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
