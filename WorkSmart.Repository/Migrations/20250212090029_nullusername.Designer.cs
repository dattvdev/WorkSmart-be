﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkSmart.Repository;

#nullable disable

namespace WorkSmart.Repository.Migrations
{
    [DbContext(typeof(WorksmartDBContext))]
    [Migration("20250212090029_nullusername")]
    partial class nullusername
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("JobTagUser", b =>
                {
                    b.Property<int>("JobTagsJobTagID")
                        .HasColumnType("int");

                    b.Property<int>("UsersUserID")
                        .HasColumnType("int");

                    b.HasKey("JobTagsJobTagID", "UsersUserID");

                    b.HasIndex("UsersUserID");

                    b.ToTable("JobTagUser");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Application", b =>
                {
                    b.Property<int>("ApplicationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ApplicationID"));

                    b.Property<int>("CVID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("JobID")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("ApplicationID");

                    b.HasIndex("CVID");

                    b.HasIndex("JobID");

                    b.HasIndex("UserID");

                    b.ToTable("Applications");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV", b =>
                {
                    b.Property<int>("CVID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CVID"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("JobPosition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Link")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Summary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("CVID");

                    b.HasIndex("UserID");

                    b.ToTable("CVs");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV_Certification", b =>
                {
                    b.Property<int>("CertificationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CertificationID"));

                    b.Property<int>("CVID")
                        .HasColumnType("int");

                    b.Property<string>("CertificateName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("datetime2");

                    b.HasKey("CertificationID");

                    b.HasIndex("CVID");

                    b.ToTable("CV_Certifications");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV_Education", b =>
                {
                    b.Property<int>("EducationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EducationID"));

                    b.Property<int>("CVID")
                        .HasColumnType("int");

                    b.Property<string>("Degree")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EndedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Major")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SchoolName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("EducationID");

                    b.HasIndex("CVID");

                    b.ToTable("CV_Educations");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV_Experience", b =>
                {
                    b.Property<int>("ExperienceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ExperienceID"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CVID")
                        .HasColumnType("int");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EndedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("JobPosition")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("ExperienceID");

                    b.HasIndex("CVID");

                    b.ToTable("CV_Experiences");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV_Skill", b =>
                {
                    b.Property<int>("SkillID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SkillID"));

                    b.Property<int>("CVID")
                        .HasColumnType("int");

                    b.Property<string>("SkillName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SkillID");

                    b.HasIndex("CVID");

                    b.ToTable("CV_Skills");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.FavoriteJob", b =>
                {
                    b.Property<int>("FavoriteJobID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("FavoriteJobID"));

                    b.Property<DateTime>("AddedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("JobID")
                        .HasColumnType("int");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("FavoriteJobID");

                    b.HasIndex("JobID");

                    b.HasIndex("UserID");

                    b.ToTable("FavoriteJobs");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Feedback", b =>
                {
                    b.Property<int>("FeedbackID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("FeedbackID"));

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.Property<int>("ReceiverID")
                        .HasColumnType("int");

                    b.Property<int>("SenderID")
                        .HasColumnType("int");

                    b.HasKey("FeedbackID");

                    b.HasIndex("ReceiverID");

                    b.HasIndex("SenderID");

                    b.ToTable("Feedbacks");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Job", b =>
                {
                    b.Property<int>("JobID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("JobID"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("JobTagID")
                        .HasColumnType("int");

                    b.Property<string>("Location")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Salary")
                        .HasColumnType("float");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("JobID");

                    b.HasIndex("JobTagID");

                    b.HasIndex("UserID");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.JobTag", b =>
                {
                    b.Property<int>("JobTagID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("JobTagID"));

                    b.Property<string>("TagName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("JobTagID");

                    b.ToTable("JobTags");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Notification", b =>
                {
                    b.Property<int>("NotificationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NotificationID"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<string>("Link")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("NotificationID");

                    b.HasIndex("UserID");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Package", b =>
                {
                    b.Property<int>("PackageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PackageID"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.HasKey("PackageID");

                    b.ToTable("Packages");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.PersonalMessage", b =>
                {
                    b.Property<int>("PersonalMessageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PersonalMessageID"));

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<int>("ReceiverID")
                        .HasColumnType("int");

                    b.Property<int>("SenderID")
                        .HasColumnType("int");

                    b.HasKey("PersonalMessageID");

                    b.HasIndex("ReceiverID");

                    b.HasIndex("SenderID");

                    b.ToTable("PersonalMessages");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.ReportPost", b =>
                {
                    b.Property<int>("ReportPostID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReportPostID"));

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("JobID")
                        .HasColumnType("int");

                    b.Property<int>("SenderID")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ReportPostID");

                    b.HasIndex("JobID");

                    b.HasIndex("SenderID");

                    b.ToTable("ReportPosts");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.ReportUser", b =>
                {
                    b.Property<int>("ReportUserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReportUserID"));

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ReceiverID")
                        .HasColumnType("int");

                    b.Property<int>("SenderID")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ReportUserID");

                    b.HasIndex("ReceiverID");

                    b.HasIndex("SenderID");

                    b.ToTable("ReportUsers");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Subscription", b =>
                {
                    b.Property<int>("SubscriptionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SubscriptionID"));

                    b.Property<DateTime>("ExpDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("PackageID")
                        .HasColumnType("int");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("SubscriptionID");

                    b.HasIndex("PackageID");

                    b.HasIndex("UserID");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Transaction", b =>
                {
                    b.Property<int>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TransactionID"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("TransactionID");

                    b.HasIndex("UserID");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserID"));

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Amount")
                        .HasColumnType("float");

                    b.Property<string>("BankName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BankNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CompanyDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CompanyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConfirmationCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Exp")
                        .HasColumnType("float");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("bit");

                    b.Property<bool>("IsEmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Skills")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WorkLocation")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("JobTagUser", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.JobTag", null)
                        .WithMany()
                        .HasForeignKey("JobTagsJobTagID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UsersUserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Application", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.CV", "CV")
                        .WithMany("Applications")
                        .HasForeignKey("CVID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.Job", "Job")
                        .WithMany("Applications")
                        .HasForeignKey("JobID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", "User")
                        .WithMany("Applications")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("CV");

                    b.Navigation("Job");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.User", "User")
                        .WithMany("CVs")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV_Certification", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.CV", "CV")
                        .WithMany("Certifications")
                        .HasForeignKey("CVID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CV");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV_Education", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.CV", "CV")
                        .WithMany("Educations")
                        .HasForeignKey("CVID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CV");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV_Experience", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.CV", "CV")
                        .WithMany("Experiences")
                        .HasForeignKey("CVID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CV");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV_Skill", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.CV", "CV")
                        .WithMany("Skills")
                        .HasForeignKey("CVID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CV");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.FavoriteJob", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.Job", "Job")
                        .WithMany("FavoriteJobs")
                        .HasForeignKey("JobID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", "User")
                        .WithMany("FavoriteJobs")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Job");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Feedback", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.User", "Receiver")
                        .WithMany("FeedbacksReceived")
                        .HasForeignKey("ReceiverID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", "Sender")
                        .WithMany("FeedbacksSent")
                        .HasForeignKey("SenderID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Job", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.JobTag", "JobTag")
                        .WithMany()
                        .HasForeignKey("JobTagID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", "User")
                        .WithMany("PostedJobs")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("JobTag");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Notification", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.User", "User")
                        .WithMany("Notifications")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.PersonalMessage", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.User", "Receiver")
                        .WithMany("MessagesReceived")
                        .HasForeignKey("ReceiverID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", "Sender")
                        .WithMany("MessagesSent")
                        .HasForeignKey("SenderID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.ReportPost", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.Job", "Job")
                        .WithMany("ReportPosts")
                        .HasForeignKey("JobID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", "Sender")
                        .WithMany("ReportPosts")
                        .HasForeignKey("SenderID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Job");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.ReportUser", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.User", "Receiver")
                        .WithMany("ReportsReceived")
                        .HasForeignKey("ReceiverID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", "Sender")
                        .WithMany("ReportsSent")
                        .HasForeignKey("SenderID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Subscription", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.Package", "Package")
                        .WithMany("Subscriptions")
                        .HasForeignKey("PackageID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WorkSmart.Core.Entity.User", "User")
                        .WithMany("Subscriptions")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Package");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Transaction", b =>
                {
                    b.HasOne("WorkSmart.Core.Entity.User", "User")
                        .WithMany("Transactions")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.CV", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("Certifications");

                    b.Navigation("Educations");

                    b.Navigation("Experiences");

                    b.Navigation("Skills");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Job", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("FavoriteJobs");

                    b.Navigation("ReportPosts");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.Package", b =>
                {
                    b.Navigation("Subscriptions");
                });

            modelBuilder.Entity("WorkSmart.Core.Entity.User", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("CVs");

                    b.Navigation("FavoriteJobs");

                    b.Navigation("FeedbacksReceived");

                    b.Navigation("FeedbacksSent");

                    b.Navigation("MessagesReceived");

                    b.Navigation("MessagesSent");

                    b.Navigation("Notifications");

                    b.Navigation("PostedJobs");

                    b.Navigation("ReportPosts");

                    b.Navigation("ReportsReceived");

                    b.Navigation("ReportsSent");

                    b.Navigation("Subscriptions");

                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
