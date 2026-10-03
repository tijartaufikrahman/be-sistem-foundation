using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Menu> Menus { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<MenuGroup> MenuGroups { get; set; } = null!;
        public DbSet<MenuRole> MenuRoles { get; set; } = null!;
        public DbSet<Approval> Approvals { get; set; } = null!;
        public DbSet<ApprovalRole> ApprovalRoles { get; set; } = null!;
        public DbSet<Foundation> Foundations { get; set; } = null!;
        public DbSet<FoundationUser> FoundationUsers { get; set; } = null!;
        public DbSet<NewsPost> NewsPosts { get; set; } = null!;
        public DbSet<Campaign> Campaigns { get; set; } = null!;
        public DbSet<Donation> Donations { get; set; } = null!;
        public DbSet<NavbarGroup> NavbarGroups { get; set; } = null!;
        public DbSet<Navbar> Navbars { get; set; } = null!;
        public DbSet<FoundationNavbar> FoundationNavbars { get; set; } = null!;
        public DbSet<FoundationType> FoundationTypes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Indexes
            modelBuilder.Entity<FoundationType>().HasIndex(ft => ft.Code).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Foundation>().HasIndex(f => f.Slug).IsUnique();
            modelBuilder.Entity<Campaign>().HasIndex(c => c.Slug).IsUnique();
            modelBuilder.Entity<NewsPost>().HasIndex(n => n.Slug).IsUnique();
            modelBuilder.Entity<Approval>().HasIndex(a => a.Code).IsUnique();
            modelBuilder.Entity<Donation>().HasIndex(d => new { d.InvoiceNumber, d.MidtransOrderId }).IsUnique();

            // FoundationUser Relationships
            modelBuilder.Entity<FoundationUser>()
                .HasIndex(fu => new { fu.FoundationId, fu.UserId })
                .IsUnique();
            // Decimal Precision
            modelBuilder.Entity<Campaign>()
                .Property(c => c.TargetAmount)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<Donation>()
                .Property(d => d.Amount)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<Donation>()
                .Property(d => d.PlatformTip)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<Donation>()
                .Property(d => d.TotalAmount)
                .HasColumnType("decimal(15,2)");

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "super_admin", DisplayName = "Super Admin", IsActive = true },
                new Role { Id = 2, Name = "foundation", DisplayName = "Foundation Admin", IsActive = true },
                new Role { Id = 3, Name = "donor", DisplayName = "Donatur", IsActive = true }
            );

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    AuthProvider = "local",
                    IsActive = true
                }
            );

            // Seed UserRole for Admin
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1 }
            );

            // Seed Menus
            modelBuilder.Entity<Menu>().HasData(
                new Menu { Id = 1, Label = "Dashboard", RouteUrl = "/dashboard", OrderIndex = 1, IsActive = true },
                new Menu { Id = 2, Label = "Users", RouteUrl = "/users", OrderIndex = 2, IsActive = true },
                new Menu { Id = 3, Label = "Roles", RouteUrl = "/roles", OrderIndex = 3, IsActive = true },
                new Menu { Id = 4, Label = "UserRoles", RouteUrl = "/user-roles", OrderIndex = 4, IsActive = true },
                new Menu { Id = 5, Label = "Menus", RouteUrl = "/menus", OrderIndex = 5, IsActive = true },
                new Menu { Id = 6, Label = "MenuRoles", RouteUrl = "/menu-roles", OrderIndex = 6, IsActive = true },
                new Menu { Id = 7, Label = "Approvals", RouteUrl = "/approvals", OrderIndex = 7, IsActive = true },
                new Menu { Id = 8, Label = "Foundations", RouteUrl = "/foundations", OrderIndex = 8, IsActive = true }
            );

            // Seed MenuRoles
            modelBuilder.Entity<MenuRole>().HasData(
                new MenuRole { Id = 1, MenuId = 1, RoleId = 1, IsActive = true },
                new MenuRole { Id = 2, MenuId = 2, RoleId = 1, IsActive = true },
                new MenuRole { Id = 3, MenuId = 3, RoleId = 1, IsActive = true },
                new MenuRole { Id = 4, MenuId = 4, RoleId = 1, IsActive = true },
                new MenuRole { Id = 5, MenuId = 5, RoleId = 1, IsActive = true },
                new MenuRole { Id = 6, MenuId = 6, RoleId = 1, IsActive = true },
                new MenuRole { Id = 7, MenuId = 7, RoleId = 1, IsActive = true },
                new MenuRole { Id = 8, MenuId = 8, RoleId = 1, IsActive = true }
            );

            // Seed Approvals
            modelBuilder.Entity<Approval>().HasData(
                new Approval { Id = 1, Name = "Penyaluran Zakat", Code = "zakat_distribution", IsActive = true },
                new Approval { Id = 2, Name = "Bantuan Mustahik", Code = "mustahik_aid", IsActive = true },
                new Approval { Id = 3, Name = "Pengeluaran Dana", Code = "fund_outflow", IsActive = true },
                new Approval { Id = 4, Name = "Program Zakat", Code = "zakat_program", IsActive = true }
            );
        }
    }
}
