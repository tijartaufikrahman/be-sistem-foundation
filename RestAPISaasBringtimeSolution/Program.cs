using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestAPISaasBringtimeSolution.Data;
using RestAPISaasBringtimeSolution.Interfaces;
using RestAPISaasBringtimeSolution.Repositories;
using RestAPISaasBringtimeSolution.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container with JSON cycle ignore.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Configure CORS for Frontend (SaasBringtimeSolution)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// SQL Server Database DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories & Services (Interface / Repository Pattern)
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IMenuGroupRepository, MenuGroupRepository>();
builder.Services.AddScoped<IMenuRoleRepository, MenuRoleRepository>();
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IApprovalRoleRepository, ApprovalRoleRepository>();
builder.Services.AddScoped<IFoundationRepository, FoundationRepository>();
builder.Services.AddScoped<INewsPostRepository, NewsPostRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<IDonationRepository, DonationRepository>();

// Configure JWT Bearer Authentication (1-Day Expiry)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "BringtimeSolution_SuperSecretJwtSecurityKey_2026_SecureKey_MustBeLongEnough!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BringtimeSolution";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BringtimeSolutionAudience";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Swagger/OpenAPI with unique schema IDs and JWT Bearer support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName ?? type.Name);
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Masukkan token JWT dengan format: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Migrate & Seed Database on Startup (Seed only if database is empty)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    if (!await db.Users.AnyAsync(u => u.Email == "admin@softwarehouse.com"))
    {
        await DbSeeder.ResetAndSeedAsync(db);
    }
    else
    {
        // Pastikan slug yayasan di DB bersih dan konsisten (peduliummat & berkahyatim)
        var f1 = await db.Foundations.FirstOrDefaultAsync(f => f.Domain == "peduliummat.org" || f.Slug == "yayasan-peduli-ummat");
        if (f1 != null && f1.Slug != "peduliummat")
        {
            f1.Slug = "peduliummat";
            db.Foundations.Update(f1);
        }

        var f2 = await db.Foundations.FirstOrDefaultAsync(f => f.Domain == "berkahyatim.org" || f.Slug == "yayasan-berkah-yatim");
        if (f2 != null && f2.Slug != "berkahyatim")
        {
            f2.Slug = "berkahyatim";
            db.Foundations.Update(f2);
        }

        // Pastikan kolom Note ada di tabel menus
        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID('menus') AND name = 'Note'
                )
                BEGIN
                    ALTER TABLE menus ADD Note NVARCHAR(MAX) NULL;
                END

                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID('menu_roles') AND name = 'IsActive'
                )
                BEGIN
                    ALTER TABLE menu_roles ADD IsActive BIT NOT NULL CONSTRAINT DF_menu_roles_IsActive DEFAULT 1;
                END
            ");

            // Isi nilai default Note untuk menu yang masih kosong
            await db.Database.ExecuteSqlRawAsync(@"
                UPDATE menus SET Note = 'ini untuk super admin' 
                WHERE (Note IS NULL OR Note = '') AND RouteUrl LIKE '/Admin/%';

                UPDATE menus SET Note = 'ini untuk foundation' 
                WHERE (Note IS NULL OR Note = '') AND RouteUrl LIKE '/Foundation/%';

                UPDATE menus SET Note = 'ini untuk donor' 
                WHERE (Note IS NULL OR Note = '') AND RouteUrl LIKE '/Donor/%';
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Auto-migration note error: " + ex.Message);
        }

        // Auto-create & seed navbar tables based on schema.dbml
        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'navbar_groups')
                BEGIN
                    CREATE TABLE navbar_groups (
                        id BIGINT IDENTITY(1,1) PRIMARY KEY,
                        name NVARCHAR(100) NOT NULL,
                        icon_class NVARCHAR(50) NULL,
                        is_active BIT NOT NULL DEFAULT 1,
                        created_at DATETIME2 NULL DEFAULT GETUTCDATE(),
                        created_by BIGINT NULL,
                        updated_at DATETIME2 NULL,
                        updated_by BIGINT NULL
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'navbars')
                BEGIN
                    CREATE TABLE navbars (
                        id BIGINT IDENTITY(1,1) PRIMARY KEY,
                        navbar_group_id BIGINT NULL FOREIGN KEY REFERENCES navbar_groups(id) ON DELETE SET NULL,
                        title NVARCHAR(100) NOT NULL,
                        url NVARCHAR(255) NOT NULL,
                        icon_class NVARCHAR(50) NULL,
                        target NVARCHAR(10) NOT NULL DEFAULT '_self',
                        platform NVARCHAR(20) NOT NULL DEFAULT 'all',
                        order_index INT NOT NULL DEFAULT 0,
                        is_active BIT NOT NULL DEFAULT 1,
                        created_at DATETIME2 NULL DEFAULT GETUTCDATE(),
                        created_by BIGINT NULL,
                        updated_at DATETIME2 NULL,
                        updated_by BIGINT NULL
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'foundation_navbars')
                BEGIN
                    CREATE TABLE foundation_navbars (
                        id BIGINT IDENTITY(1,1) PRIMARY KEY,
                        foundation_id BIGINT NOT NULL FOREIGN KEY REFERENCES foundations(id) ON DELETE CASCADE,
                        navbar_id BIGINT NULL FOREIGN KEY REFERENCES navbars(id) ON DELETE SET NULL,
                        navbar_group_id BIGINT NULL FOREIGN KEY REFERENCES navbar_groups(id) ON DELETE SET NULL,
                        order_index INT NOT NULL DEFAULT 0,
                        is_active BIT NOT NULL DEFAULT 1,
                        created_at DATETIME2 NULL DEFAULT GETUTCDATE(),
                        created_by BIGINT NULL,
                        updated_at DATETIME2 NULL,
                        updated_by BIGINT NULL
                    );
                END
            ");

            // Clean any legacy {tenant} or {slug} tokens from navbars
            await db.Database.ExecuteSqlRawAsync(@"
                UPDATE navbars SET url = '/' WHERE url = '/{tenant}' OR url = '{tenant}' OR url = '/{slug}' OR url = '{slug}' OR url = '' OR url IS NULL;
                UPDATE navbars SET url = REPLACE(REPLACE(REPLACE(REPLACE(url, '/{tenant}', ''), '{tenant}', ''), '/{slug}', ''), '{slug}', '') WHERE url LIKE '%{tenant}%' OR url LIKE '%{slug}%';
                UPDATE navbars SET url = '/' WHERE url = '' OR url IS NULL;
            ");

            // Seed default master navbars & groups if navbars table is empty
            if (!await db.Navbars.AnyAsync())
            {
                var mBeranda = new RestAPISaasBringtimeSolution.Models.Entities.Navbar { Title = "Beranda", Url = "/", IconClass = "fas fa-home", OrderIndex = 1, IsActive = true, Created_at = DateTime.UtcNow };
                var mProfile = new RestAPISaasBringtimeSolution.Models.Entities.Navbar { Title = "Profile", Url = "/profile", IconClass = "fas fa-building", OrderIndex = 2, IsActive = true, Created_at = DateTime.UtcNow };
                var mDonasi = new RestAPISaasBringtimeSolution.Models.Entities.Navbar { Title = "Program Donasi", Url = "/campaigns", IconClass = "fas fa-hand-holding-heart", OrderIndex = 3, IsActive = true, Created_at = DateTime.UtcNow };
                var mDoa = new RestAPISaasBringtimeSolution.Models.Entities.Navbar { Title = "Untaian Doa", Url = "/prayers", IconClass = "fas fa-hands-praying", OrderIndex = 4, IsActive = true, Created_at = DateTime.UtcNow };
                var mFaq = new RestAPISaasBringtimeSolution.Models.Entities.Navbar { Title = "Tanya Jawab (FAQ)", Url = "/faq", IconClass = "fas fa-circle-question", OrderIndex = 5, IsActive = true, Created_at = DateTime.UtcNow };

                await db.Navbars.AddRangeAsync(mBeranda, mProfile, mDonasi, mDoa, mFaq);
                await db.SaveChangesAsync();

                // Example Dropdown Group: "Tentang Kami"
                var grpTentang = new RestAPISaasBringtimeSolution.Models.Entities.NavbarGroup { Name = "Tentang Kami", IconClass = "fas fa-circle-info", IsActive = true, Created_at = DateTime.UtcNow };
                await db.NavbarGroups.AddAsync(grpTentang);
                await db.SaveChangesAsync();

                var mTentangProfil = new RestAPISaasBringtimeSolution.Models.Entities.Navbar { NavbarGroupId = grpTentang.Id, Title = "Profil Lembaga", Url = "/profile", IconClass = "fas fa-id-card", OrderIndex = 1, IsActive = true, Created_at = DateTime.UtcNow };
                var mTentangLegal = new RestAPISaasBringtimeSolution.Models.Entities.Navbar { NavbarGroupId = grpTentang.Id, Title = "Legalitas & Izin", Url = "/profile#legalitas", IconClass = "fas fa-certificate", OrderIndex = 2, IsActive = true, Created_at = DateTime.UtcNow };
                var mTentangRek = new RestAPISaasBringtimeSolution.Models.Entities.Navbar { NavbarGroupId = grpTentang.Id, Title = "Rekening Resmi", Url = "/profile#rekening", IconClass = "fas fa-credit-card", OrderIndex = 3, IsActive = true, Created_at = DateTime.UtcNow };

                await db.Navbars.AddRangeAsync(mTentangProfil, mTentangLegal, mTentangRek);
                await db.SaveChangesAsync();
            }

            // Seed default foundation_navbars for all active foundations if empty
            var foundations = await db.Foundations.Where(f => f.IsActive).ToListAsync();
            var defaultNavbars = await db.Navbars.Where(n => n.NavbarGroupId == null).OrderBy(n => n.OrderIndex).ToListAsync();

            foreach (var f in foundations)
            {
                if (!await db.FoundationNavbars.AnyAsync(fn => fn.FoundationId == f.Id))
                {
                    var order = 1;
                    foreach (var nav in defaultNavbars)
                    {
                        await db.FoundationNavbars.AddAsync(new RestAPISaasBringtimeSolution.Models.Entities.FoundationNavbar
                        {
                            FoundationId = f.Id,
                            NavbarId = nav.Id,
                            NavbarGroupId = null,
                            OrderIndex = order++,
                            IsActive = true,
                            Created_at = DateTime.UtcNow
                        });
                    }
                }
            }
            await db.SaveChangesAsync();

            // Seed Super Admin Menu Group: "Manajemen Navbar"
            var superAdminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "super_admin");
            if (superAdminRole != null)
            {
                // Ensure Group 'Manajemen Navbar' exists
                var navGroup = await db.Groups.FirstOrDefaultAsync(g => g.Name == "Manajemen Navbar");
                if (navGroup == null)
                {
                    navGroup = new RestAPISaasBringtimeSolution.Models.Entities.Group
                    {
                        Name = "Manajemen Navbar",
                        IconClass = "fas fa-compass",
                        IsActive = true,
                        Created_at = DateTime.UtcNow
                    };
                    await db.Groups.AddAsync(navGroup);
                    await db.SaveChangesAsync();
                }

                // 3 Menus
                var mNav = await db.Menus.FirstOrDefaultAsync(m => m.RouteUrl == "/Navbars" || m.RouteUrl == "/Admin/Navbars");
                if (mNav == null)
                {
                    mNav = new RestAPISaasBringtimeSolution.Models.Entities.Menu { Label = "Kelola Navbar - (Master)", RouteUrl = "/Navbars", IconClass = "fas fa-bars-staggered", OrderIndex = 1, Note = "ini untuk super admin", IsActive = true, Created_at = DateTime.UtcNow };
                    await db.Menus.AddAsync(mNav);
                    await db.SaveChangesAsync();
                }

                var mNavGroup = await db.Menus.FirstOrDefaultAsync(m => m.RouteUrl == "/NavbarGroups" || m.RouteUrl == "/Admin/NavbarGroups");
                if (mNavGroup == null)
                {
                    mNavGroup = new RestAPISaasBringtimeSolution.Models.Entities.Menu { Label = "Kelola Grup Navbar - (Master)", RouteUrl = "/NavbarGroups", IconClass = "fas fa-folder-tree", OrderIndex = 2, Note = "ini untuk super admin", IsActive = true, Created_at = DateTime.UtcNow };
                    await db.Menus.AddAsync(mNavGroup);
                    await db.SaveChangesAsync();
                }

                var mFoundNav = await db.Menus.FirstOrDefaultAsync(m => m.RouteUrl == "/FoundationNavbars" || m.RouteUrl == "/Admin/FoundationNavbars");
                if (mFoundNav == null)
                {
                    mFoundNav = new RestAPISaasBringtimeSolution.Models.Entities.Menu { Label = "Foundation Navbar", RouteUrl = "/FoundationNavbars", IconClass = "fas fa-link", OrderIndex = 3, Note = "ini untuk super admin", IsActive = true, Created_at = DateTime.UtcNow };
                    await db.Menus.AddAsync(mFoundNav);
                    await db.SaveChangesAsync();
                }

                // Link to MenuGroup
                if (!await db.MenuGroups.AnyAsync(mg => mg.GroupId == navGroup.Id && mg.MenuId == mNav.Id))
                {
                    await db.MenuGroups.AddAsync(new RestAPISaasBringtimeSolution.Models.Entities.MenuGroup { GroupId = navGroup.Id, MenuId = mNav.Id, OrderIndex = 1, IsActive = true, Created_at = DateTime.UtcNow });
                }
                if (!await db.MenuGroups.AnyAsync(mg => mg.GroupId == navGroup.Id && mg.MenuId == mNavGroup.Id))
                {
                    await db.MenuGroups.AddAsync(new RestAPISaasBringtimeSolution.Models.Entities.MenuGroup { GroupId = navGroup.Id, MenuId = mNavGroup.Id, OrderIndex = 2, IsActive = true, Created_at = DateTime.UtcNow });
                }
                if (!await db.MenuGroups.AnyAsync(mg => mg.GroupId == navGroup.Id && mg.MenuId == mFoundNav.Id))
                {
                    await db.MenuGroups.AddAsync(new RestAPISaasBringtimeSolution.Models.Entities.MenuGroup { GroupId = navGroup.Id, MenuId = mFoundNav.Id, OrderIndex = 3, IsActive = true, Created_at = DateTime.UtcNow });
                }
                await db.SaveChangesAsync();

                // Link Group to Role 4
                if (!await db.MenuRoles.AnyAsync(mr => mr.GroupId == navGroup.Id && mr.RoleId == superAdminRole.Id))
                {
                    await db.MenuRoles.AddAsync(new RestAPISaasBringtimeSolution.Models.Entities.MenuRole
                    {
                        GroupId = navGroup.Id,
                        RoleId = superAdminRole.Id,
                        OrderIndex = 4,
                        IsActive = true,
                        Created_at = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Navbar table auto-create/seed error: " + ex.Message);
        }

        await db.SaveChangesAsync();
    }
}

// Forwarded Headers for Linux Nginx Reverse Proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
