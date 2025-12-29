using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Ace_Admin.Models;

public partial class PracticeDbContext : DbContext
{
    public PracticeDbContext()
    {
    }

    public PracticeDbContext(DbContextOptions<PracticeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<EmpleaveMst> EmpleaveMsts { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeCourse> EmployeeCourses { get; set; }

    public virtual DbSet<Instrument> Instruments { get; set; }

    public virtual DbSet<OtpRecord> OtpRecords { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<PayoutMethod> PayoutMethods { get; set; }

    public virtual DbSet<ProductMaster> ProductMasters { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SeoSetting> SeoSettings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<UserMessage> UserMessages { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmpleaveMst>(entity =>
        {
            entity.ToTable("EMPLEAVE_MST");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Addeddate)
                .HasColumnType("datetime")
                .HasColumnName("addeddate");
            entity.Property(e => e.EmpId).HasColumnName("Emp_Id");
            entity.Property(e => e.IsProbation).HasColumnName("Is_probation");
            entity.Property(e => e.LeaveDate)
                .HasMaxLength(100)
                .HasColumnName("leave_date");
            entity.Property(e => e.LeaveType)
                .HasMaxLength(100)
                .HasColumnName("Leave_type");
            entity.Property(e => e.Modifydate)
                .HasColumnType("datetime")
                .HasColumnName("modifydate");
            entity.Property(e => e.Reason)
                .HasMaxLength(1000)
                .HasColumnName("reason");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.Property(e => e.Email).HasDefaultValue("");
            entity.Property(e => e.EmpCode)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.FilePathPic)
                .HasMaxLength(400)
                .HasColumnName("FilePath_pic");
            entity.Property(e => e.JoiningDate).HasColumnType("datetime");
            entity.Property(e => e.Password).HasDefaultValue("");
            entity.Property(e => e.PhoneNumber).HasDefaultValue("");
            entity.Property(e => e.Username).HasDefaultValue("");
        });

        modelBuilder.Entity<EmployeeCourse>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_EmployeeCourses_CourseId");

            entity.HasIndex(e => e.EmployeeId, "IX_EmployeeCourses_EmployeeId");

            entity.HasOne(d => d.Course).WithMany(p => p.EmployeeCourses).HasForeignKey(d => d.CourseId);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeCourses).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Instrument>(entity =>
        {
            entity.ToTable("Instrument");

            entity.Property(e => e.Isin).HasColumnName("ISIN");
            entity.Property(e => e.TickSize).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<OtpRecord>(entity =>
        {
            entity.Property(e => e.IsExpired).HasComputedColumnSql("(case when getdate()>[ExpiryTime] then (1) else (0) end)", false);
            entity.Property(e => e.Otp).HasColumnName("OTP");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentT__3214EC07639EEFB3");

            entity.ToTable("PaymentTransaction");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.OrderId).HasMaxLength(100);
            entity.Property(e => e.PaymentMode).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
        });

        modelBuilder.Entity<ProductMaster>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__ProductM__B40CC6ED900C2A1F");

            entity.ToTable("ProductMaster");

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ChildId).HasColumnName("Child_Id");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.ParentId).HasColumnName("Parent_Id");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A35DC1CA0");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160E92DE2A3").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<SeoSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SeoSetti__3214EC07C233283F");

            entity.Property(e => e.CanonicalUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MetaAuthor).HasMaxLength(100);
            entity.Property(e => e.MetaDescription).HasMaxLength(160);
            entity.Property(e => e.MetaKeywords).HasMaxLength(500);
            entity.Property(e => e.OgDescription).HasMaxLength(200);
            entity.Property(e => e.OgImage).HasMaxLength(500);
            entity.Property(e => e.OgTitle).HasMaxLength(100);
            entity.Property(e => e.PageTitle).HasMaxLength(60);
            entity.Property(e => e.PageUrl).HasMaxLength(500);
            entity.Property(e => e.Robots).HasMaxLength(50);
            entity.Property(e => e.TwitterCard).HasMaxLength(50);
            entity.Property(e => e.TwitterSite).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("password");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Accid).HasName("PK__UserAcco__DF8AE1AC0DEB995F");

            entity.ToTable("UserAccount");

            entity.HasIndex(e => e.RoleId, "IX_UserAccount_RoleID");

            entity.HasIndex(e => e.Username, "UQ__UserAcco__536C85E4D72AE6A2").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__UserAcco__A9D10534569DD7A3").IsUnique();

            entity.Property(e => e.Accid).HasColumnName("ACCID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Empid).HasColumnName("EMPID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserAccou__RoleI__797309D9");
        });

        modelBuilder.Entity<UserMessage>(entity =>
        {
            entity.Property(e => e.DeliveredAt).HasColumnType("datetime");
            entity.Property(e => e.MessageType).HasMaxLength(50);
            entity.Property(e => e.ReceiverId).HasMaxLength(450);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_tok__3213E83FAD9F8953");

            entity.ToTable("user_tokens");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Wallets_UserId").IsUnique();

            entity.HasOne(d => d.User).WithOne(p => p.Wallet).HasForeignKey<Wallet>(d => d.UserId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
