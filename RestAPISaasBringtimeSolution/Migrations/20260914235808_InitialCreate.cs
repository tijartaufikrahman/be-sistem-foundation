using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestAPISaasBringtimeSolution.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "approvals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approvals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "menus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RouteUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menus_menus_ParentId",
                        column: x => x.ParentId,
                        principalTable: "menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AuthProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ResetToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ResetTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "approval_roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    StepLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approval_roles_approvals_ApprovalId",
                        column: x => x.ApprovalId,
                        principalTable: "approvals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_approval_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_roles_menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "foundations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BannerUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsappNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LegalPermitNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankAccountHolder = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_foundations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_foundations_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FoundationId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BannerUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_campaigns_foundations_FoundationId",
                        column: x => x.FoundationId,
                        principalTable: "foundations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "news_posts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FoundationId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_news_posts_foundations_FoundationId",
                        column: x => x.FoundationId,
                        principalTable: "foundations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "donations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DonorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DonorEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DonorPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    PlatformTip = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MidtransOrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MidtransTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrayerMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AamiinCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_donations_campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_donations_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "approvals",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1L, "zakat_distribution", null, true, "Penyaluran Zakat" },
                    { 2L, "mustahik_aid", null, true, "Bantuan Mustahik" },
                    { 3L, "fund_outflow", null, true, "Pengeluaran Dana" },
                    { 4L, "zakat_program", null, true, "Program Zakat" }
                });

            migrationBuilder.InsertData(
                table: "menus",
                columns: new[] { "Id", "IconClass", "IsActive", "Label", "OrderIndex", "ParentId", "RouteUrl" },
                values: new object[,]
                {
                    { 1L, null, true, "Dashboard", 1, null, "/dashboard" },
                    { 2L, null, true, "Users", 2, null, "/users" },
                    { 3L, null, true, "Roles", 3, null, "/roles" },
                    { 4L, null, true, "UserRoles", 4, null, "/user-roles" },
                    { 5L, null, true, "Menus", 5, null, "/menus" },
                    { 6L, null, true, "MenuRoles", 6, null, "/menu-roles" },
                    { 7L, null, true, "Approvals", 7, null, "/approvals" },
                    { 8L, null, true, "Foundations", 8, null, "/foundations" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Description", "DisplayName", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1L, null, "Super Admin", true, "super_admin" },
                    { 2L, null, "Foundation Admin", true, "foundation" },
                    { 3L, null, "Donatur", true, "donor" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "AuthProvider", "AvatarUrl", "Email", "IsActive", "Name", "PasswordHash", "PhoneNumber", "ProviderId", "ResetToken", "ResetTokenExpiresAt" },
                values: new object[] { 1L, "local", null, "admin@example.com", true, "Admin", "$2a$11$fF3PMihBbUK82SkvC5pgk.3WXufcJku2Kl/Tr.UWhGYCG4kXHcqXm", null, null, null, null });

            migrationBuilder.InsertData(
                table: "menu_roles",
                columns: new[] { "Id", "IsActive", "MenuId", "RoleId" },
                values: new object[,]
                {
                    { 1L, true, 1L, 1L },
                    { 2L, true, 2L, 1L },
                    { 3L, true, 3L, 1L },
                    { 4L, true, 4L, 1L },
                    { 5L, true, 5L, 1L },
                    { 6L, true, 6L, 1L },
                    { 7L, true, 7L, 1L },
                    { 8L, true, 8L, 1L }
                });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[] { 1L, 1L, 1L });

            migrationBuilder.CreateIndex(
                name: "IX_approval_roles_ApprovalId",
                table: "approval_roles",
                column: "ApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_approval_roles_RoleId",
                table: "approval_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_approvals_Code",
                table: "approvals",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_FoundationId",
                table: "campaigns",
                column: "FoundationId");

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_Slug",
                table: "campaigns",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_donations_CampaignId",
                table: "donations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_donations_InvoiceNumber_MidtransOrderId",
                table: "donations",
                columns: new[] { "InvoiceNumber", "MidtransOrderId" },
                unique: true,
                filter: "[MidtransOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_donations_UserId",
                table: "donations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_foundations_Slug",
                table: "foundations",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_foundations_UserId",
                table: "foundations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_roles_MenuId",
                table: "menu_roles",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_roles_RoleId",
                table: "menu_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_menus_ParentId",
                table: "menus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_news_posts_FoundationId",
                table: "news_posts",
                column: "FoundationId");

            migrationBuilder.CreateIndex(
                name: "IX_news_posts_Slug",
                table: "news_posts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_UserId",
                table: "user_roles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_roles");

            migrationBuilder.DropTable(
                name: "donations");

            migrationBuilder.DropTable(
                name: "menu_roles");

            migrationBuilder.DropTable(
                name: "news_posts");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "approvals");

            migrationBuilder.DropTable(
                name: "campaigns");

            migrationBuilder.DropTable(
                name: "menus");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "foundations");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
