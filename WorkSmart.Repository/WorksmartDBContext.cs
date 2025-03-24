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
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<ReportUser> ReportUsers { get; set; }
        public DbSet<PersonalMessage> PersonalMessages { get; set; }
        public DbSet<ReportPost> ReportPosts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NotificationJobTag> NotificationJobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", true, true);
            IConfigurationRoot config = builder.Build();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
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
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Sender)
                .WithMany(u => u.FeedbacksSent)
                .HasForeignKey(f => f.SenderID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Receiver)
                .WithMany(u => u.FeedbacksReceived)
                .HasForeignKey(f => f.ReceiverID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ReportUser>()
                .HasOne(r => r.Receiver)
                .WithMany(u => u.ReportsReceived)
                .HasForeignKey(r => r.ReceiverID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReportUser>()
                .HasOne(r => r.Sender)
                .WithMany(u => u.ReportsSent)
                .HasForeignKey(r => r.SenderID)
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
        }
    }
}
