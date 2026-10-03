using Microsoft.EntityFrameworkCore;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Data
{
    public static class DbSeeder
    {
        public static async Task ResetAndSeedAsync(AppDbContext context)
        {
            // 1. CLEAN WIPE IN REVERSE FK ORDER
            // Remove transactions and children first
            context.Donations.RemoveRange(context.Donations);
            context.NewsPosts.RemoveRange(context.NewsPosts);
            context.Campaigns.RemoveRange(context.Campaigns);
            context.FoundationUsers.RemoveRange(context.FoundationUsers);
            context.Foundations.RemoveRange(context.Foundations);
            context.ApprovalRoles.RemoveRange(context.ApprovalRoles);
            context.Approvals.RemoveRange(context.Approvals);
            context.MenuRoles.RemoveRange(context.MenuRoles);
            context.Menus.RemoveRange(context.Menus);
            context.UserRoles.RemoveRange(context.UserRoles);
            context.Users.RemoveRange(context.Users);
            context.Roles.RemoveRange(context.Roles);
            await context.SaveChangesAsync();

            // 2. ROLES (3 roles)
            var roleSuperAdmin = new Role
            {
                Name = "super_admin",
                DisplayName = "Super Admin",
                Description = "Administrator Software House / Provider Platform SaaS",
                IsActive = true
            };
            var roleFoundation = new Role
            {
                Name = "foundation",
                DisplayName = "Foundation Admin",
                Description = "Administrator Lembaga / Yayasan Pengelola Donasi",
                IsActive = true
            };
            var roleDonor = new Role
            {
                Name = "donor",
                DisplayName = "Donatur",
                Description = "Pengguna Donatur Publik Terdaftar",
                IsActive = true
            };

            await context.Roles.AddRangeAsync(roleSuperAdmin, roleFoundation, roleDonor);
            await context.SaveChangesAsync();

            // 3. USERS (1 Software House, 1 Yayasan, 1 Donatur)
            var defaultHash = BCrypt.Net.BCrypt.HashPassword("123");

            // User 1: Software House / Super Admin
            var userSoftwareHouse = new User
            {
                Name = "Software House Admin",
                Email = "admin@softwarehouse.com",
                PasswordHash = defaultHash,
                PhoneNumber = "081122334455",
                AuthProvider = "local",
                IsActive = true
            };

            // User 1b (Alias admin@example.com for dev compatibility)
            var userAdminAlias = new User
            {
                Name = "Super Admin Platform",
                Email = "admin@example.com",
                PasswordHash = defaultHash,
                PhoneNumber = "081299990001",
                AuthProvider = "local",
                IsActive = true
            };

            // User 2: Yayasan 1 Admin
            var userYayasan = new User
            {
                Name = "Ustadz H. Hilman Fauzi",
                Email = "yayasan@peduliummat.org",
                PasswordHash = defaultHash,
                PhoneNumber = "081234567890",
                AuthProvider = "local",
                IsActive = true
            };

            // User 2b: Yayasan 2 Admin (Yayasan Berkah Yatim & Dhuafa)
            var userYayasan2 = new User
            {
                Name = "Hj. Nurhayati",
                Email = "admin@berkahyatim.org",
                PasswordHash = defaultHash,
                PhoneNumber = "081345678901",
                AuthProvider = "local",
                IsActive = true
            };

            // User 3: Donatur 1
            var userDonatur = new User
            {
                Name = "Ahmad Donatur Syariah",
                Email = "donatur@gmail.com",
                PasswordHash = defaultHash,
                PhoneNumber = "085678901234",
                AuthProvider = "local",
                IsActive = true
            };

            // User 4: Donatur 2
            var userDonatur2 = new User
            {
                Name = "Dewi Lestari",
                Email = "dewi@gmail.com",
                PasswordHash = defaultHash,
                PhoneNumber = "085712345678",
                AuthProvider = "local",
                IsActive = true
            };

            await context.Users.AddRangeAsync(userSoftwareHouse, userAdminAlias, userYayasan, userYayasan2, userDonatur, userDonatur2);
            await context.SaveChangesAsync();

            // 4. USER ROLES PIVOT
            await context.UserRoles.AddRangeAsync(
                new UserRole { UserId = userSoftwareHouse.Id, RoleId = roleSuperAdmin.Id },
                new UserRole { UserId = userAdminAlias.Id, RoleId = roleSuperAdmin.Id },
                new UserRole { UserId = userYayasan.Id, RoleId = roleFoundation.Id },
                new UserRole { UserId = userYayasan2.Id, RoleId = roleFoundation.Id },
                new UserRole { UserId = userDonatur.Id, RoleId = roleDonor.Id },
                new UserRole { UserId = userDonatur2.Id, RoleId = roleDonor.Id }
            );
            await context.SaveChangesAsync();

            // 5. YAYASAN 1: Yayasan Peduli Ummat Nusantara
            var foundation1 = new Foundation
            {
                UserId = userYayasan.Id,
                Name = "Yayasan Peduli Ummat Nusantara",
                Slug = "peduliummat",
                Domain = "peduliummat.org",
                Bio = "Lembaga filantropi Islam dan amil zakat terpercaya berfokus pada program pangan, beasiswa santri dhuafa, dan penyediaan air bersih pelosok.",
                Address = "Jl. R.E. Martadinata No. 108, Cihapit, Kota Bandung, Jawa Barat",
                WhatsappNumber = "081234567890",
                LegalPermitNumber = "AHU-0018923.AH.01.04.Tahun 2021",
                BankName = "Bank Syariah Indonesia (BSI)",
                BankAccountNumber = "7123456789",
                BankAccountHolder = "Yayasan Peduli Ummat Nusantara",
                LogoUrl = "https://images.unsplash.com/photo-1532629345422-7515f3d16bb6?w=200",
                BannerUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=1200",
                IsActive = true
            };

            // YAYASAN 2: Yayasan Berkah Yatim & Dhuafa
            var foundation2 = new Foundation
            {
                UserId = userYayasan2.Id,
                Name = "Yayasan Berkah Yatim & Dhuafa",
                Slug = "berkahyatim",
                Domain = "berkahyatim.org",
                Bio = "Fokus pada santunan, pendidikan gratis, dan pembinaan karakter untuk 500+ anak yatim piatu dan dhuafa di Jabodetabek.",
                Address = "Jl. Margonda Raya No. 45, Beji, Kota Depok, Jawa Barat",
                WhatsappNumber = "081345678901",
                LegalPermitNumber = "AHU-0034512.AH.01.04.Tahun 2022",
                BankName = "Bank Mandiri",
                BankAccountNumber = "1570008899112",
                BankAccountHolder = "Yayasan Berkah Yatim Dhuafa",
                LogoUrl = "https://images.unsplash.com/photo-1509099836639-18ba1795216d?w=200",
                BannerUrl = "https://images.unsplash.com/photo-1511632765486-a01980e01a18?w=1200",
                IsActive = true
            };

            await context.Foundations.AddRangeAsync(foundation1, foundation2);
            await context.SaveChangesAsync();

            // 5b. PIVOT: FOUNDATION USERS (Many-to-Many)
            var fu1 = new FoundationUser { FoundationId = foundation1.Id, UserId = userYayasan.Id, IsActive = true, Created_at = DateTime.UtcNow };
            var fu1b = new FoundationUser { FoundationId = foundation1.Id, UserId = userSoftwareHouse.Id, IsActive = true, Created_at = DateTime.UtcNow };
            var fu1_donor = new FoundationUser { FoundationId = foundation1.Id, UserId = userDonatur.Id, IsActive = true, Created_at = DateTime.UtcNow };
            var fu2 = new FoundationUser { FoundationId = foundation2.Id, UserId = userYayasan2.Id, IsActive = true, Created_at = DateTime.UtcNow };
            var fu2b = new FoundationUser { FoundationId = foundation2.Id, UserId = userAdminAlias.Id, IsActive = true, Created_at = DateTime.UtcNow };
            var fu2_donor = new FoundationUser { FoundationId = foundation2.Id, UserId = userDonatur2.Id, IsActive = true, Created_at = DateTime.UtcNow };
            await context.FoundationUsers.AddRangeAsync(fu1, fu1b, fu1_donor, fu2, fu2b, fu2_donor);
            await context.SaveChangesAsync();

            // 6. CAMPAIGNS FOR YAYASAN 1
            var c1 = new Campaign
            {
                FoundationId = foundation1.Id,
                Title = "Sedekah Pangan Lansia & Yatim Dhuafa",
                Slug = "sedekah-pangan-lansia-yatim",
                Category = "infaq",
                TargetAmount = 35000000m,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(11),
                BannerUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800",
                Description = "Mari bantu penuhi kebutuhan beras dan sembako bergizi bagi 200 lansia dhuafa sebatang kara dan anak yatim binaan Yayasan Peduli Ummat.",
                IsActive = true
            };

            var c2 = new Campaign
            {
                FoundationId = foundation1.Id,
                Title = "Zakat Maal: Berdayakan Ekonomi Mandiri Mustahik",
                Slug = "zakat-maal-berdayakan-mustahik",
                Category = "zakat_maal",
                TargetAmount = 50000000m,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(11),
                BannerUrl = "https://images.unsplash.com/photo-1532629345422-7515f3d16bb6?w=800",
                Description = "Tunaikan kewajiban zakat maal Anda untuk disalurkan dalam bentuk gerobak usaha dan modal bergulir tanpa bunga untuk keluarga prasejahtera.",
                IsActive = true
            };

            var c3 = new Campaign
            {
                FoundationId = foundation1.Id,
                Title = "Wakaf Sumur Bor Air Bersih di Desa Kekeringan",
                Slug = "wakaf-sumur-bor-air-bersih",
                Category = "waqf",
                TargetAmount = 45000000m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(9),
                BannerUrl = "https://images.unsplash.com/photo-1509099836639-18ba1795216d?w=800",
                Description = "Pembangunan sumur bor sedalam 60 meter dan toren air komunal untuk lebih dari 300 kepala keluarga di pelosok yang mengalami krisis air bersih.",
                IsActive = true
            };

            // CAMPAIGNS FOR YAYASAN 2
            var c4 = new Campaign
            {
                FoundationId = foundation2.Id,
                Title = "Beasiswa Pendidikan 100 Anak Yatim Penghafal Al-Qur'an",
                Slug = "beasiswa-yatim-penghafal-quran",
                Category = "charity",
                TargetAmount = 60000000m,
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(10),
                BannerUrl = "https://images.unsplash.com/photo-1511632765486-a01980e01a18?w=800",
                Description = "Bantuan biaya sekolah, seragam, perlengkapan belajar, dan asrama gratis untuk 100 anak yatim berprestasi calon hafizh Qur'an.",
                IsActive = true
            };

            var c5 = new Campaign
            {
                FoundationId = foundation2.Id,
                Title = "Santunan Biaya Pengobatan Balita Dhuafa Sakit Kritis",
                Slug = "santunan-pengobatan-balita-dhuafa",
                Category = "infaq",
                TargetAmount = 25000000m,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(8),
                BannerUrl = "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?w=800",
                Description = "Dukungan darurat untuk biaya obat-obatan, rawat inap, dan transportasi rumah sakit bagi balita keluarga prasejahtera yang menderita penyakit kronis.",
                IsActive = true
            };

            await context.Campaigns.AddRangeAsync(c1, c2, c3, c4, c5);
            await context.SaveChangesAsync();

            // 7. NEWS POSTS (Articles for both foundations)
            var n1 = new NewsPost
            {
                FoundationId = foundation1.Id,
                Title = "Alhamdulillah! 120 Paket Sembako Sukses Disalurkan ke Warga Dhuafa",
                Slug = "120-paket-sembako-sukses-disalurkan",
                ThumbnailUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=600",
                Category = "distribution_report",
                Content = "Pada hari Ahad kemarin, tim relawan Yayasan Peduli Ummat Nusantara berhasil menyalurkan 120 paket sembako nutrisi seimbang untuk lansia dan janda dhuafa. Terima kasih atas sedekah para donatur.",
                ViewCount = 215,
                IsActive = true
            };

            var n2 = new NewsPost
            {
                FoundationId = foundation1.Id,
                Title = "Keutamaan dan Berkah Sedekah di Waktu Subuh",
                Slug = "keutamaan-dan-berkah-sedekah-subuh",
                ThumbnailUrl = "https://images.unsplash.com/photo-1509099836639-18ba1795216d?w=600",
                Category = "news",
                Content = "Sedekah di awal hari mendatangkan doa khusus dari malaikat untuk kelapangan rezeki dan perlindungan dari marabahaya. Mari rutinkan sedekah setiap pagi walau sedikit.",
                ViewCount = 430,
                IsActive = true
            };

            var n3 = new NewsPost
            {
                FoundationId = foundation2.Id,
                Title = "Penyaluran Perlengkapan Sekolah untuk 85 Santri Yatim Depok",
                Slug = "penyaluran-perlengkapan-sekolah-santri-yatim",
                ThumbnailUrl = "https://images.unsplash.com/photo-1511632765486-a01980e01a18?w=600",
                Category = "distribution_report",
                Content = "Alhamdulillah tas, seragam baru, dan buku pelajaran telah diserahkan langsung kepada 85 anak asuh Yayasan Berkah Yatim. Senyum bahagia mereka adalah doa untuk kita semua.",
                ViewCount = 180,
                IsActive = true
            };

            await context.NewsPosts.AddRangeAsync(n1, n2, n3);
            await context.SaveChangesAsync();

            // 8. DONATIONS (Transactions across both foundations with logged-in donors and guests)
            var d1 = new Donation
            {
                CampaignId = c1.Id,
                UserId = userDonatur.Id,
                InvoiceNumber = "INV-20260910-0001",
                DonorName = "Ahmad Donatur Syariah",
                DonorEmail = "donatur@gmail.com",
                DonorPhone = "085678901234",
                IsAnonymous = false,
                Amount = 250000m,
                PlatformTip = 5000m,
                TotalAmount = 255000m,
                PaymentMethod = "qris",
                PaymentStatus = "paid",
                PaidAt = DateTime.UtcNow.AddDays(-5),
                PrayerMessage = "Bismillah semoga berkah untuk keluarga kami dan dilapangkan rezeki orang tua kami. Aamiin.",
                AamiinCount = 15
            };

            var d2 = new Donation
            {
                CampaignId = c2.Id,
                UserId = userDonatur.Id,
                InvoiceNumber = "INV-20260912-0002",
                DonorName = "Ahmad Donatur Syariah",
                DonorEmail = "donatur@gmail.com",
                DonorPhone = "085678901234",
                IsAnonymous = false,
                Amount = 1000000m,
                PlatformTip = 15000m,
                TotalAmount = 1015000m,
                PaymentMethod = "bca_va",
                PaymentStatus = "paid",
                PaidAt = DateTime.UtcNow.AddDays(-3),
                PrayerMessage = "Kewajiban zakat maal perniagaan tahun ini. Semoga membersihkan harta dan membawa keberkahan.",
                AamiinCount = 28
            };

            var d3 = new Donation
            {
                CampaignId = c3.Id,
                UserId = null,
                InvoiceNumber = "INV-20260913-0003",
                DonorName = "Hamba Allah",
                DonorEmail = null,
                DonorPhone = null,
                IsAnonymous = true,
                Amount = 500000m,
                PlatformTip = 10000m,
                TotalAmount = 510000m,
                PaymentMethod = "qris",
                PaymentStatus = "paid",
                PaidAt = DateTime.UtcNow.AddDays(-1),
                PrayerMessage = "Wakaf sumur atas nama almarhum ayahanda tercinta. Semoga pahalanya mengalir tiada henti.",
                AamiinCount = 42
            };

            var d4 = new Donation
            {
                CampaignId = c4.Id,
                UserId = userDonatur2.Id,
                InvoiceNumber = "INV-20260914-0004",
                DonorName = "Dewi Lestari",
                DonorEmail = "dewi@gmail.com",
                DonorPhone = "085712345678",
                IsAnonymous = false,
                Amount = 500000m,
                PlatformTip = 10000m,
                TotalAmount = 510000m,
                PaymentMethod = "bni_va",
                PaymentStatus = "paid",
                PaidAt = DateTime.UtcNow.AddDays(-2),
                PrayerMessage = "Semoga anak-anak yatim penghafal Qur'an selalu dilindungi Allah dan tercapai cita-citanya.",
                AamiinCount = 19
            };

            var d5 = new Donation
            {
                CampaignId = c5.Id,
                UserId = null,
                InvoiceNumber = "INV-20260915-0005",
                DonorName = "Siti Rahayu",
                DonorEmail = "siti.rahayu@gmail.com",
                DonorPhone = "081999888777",
                IsAnonymous = false,
                Amount = 150000m,
                PlatformTip = 5000m,
                TotalAmount = 155000m,
                PaymentMethod = "gopay",
                PaymentStatus = "paid",
                PaidAt = DateTime.UtcNow.AddHours(-12),
                PrayerMessage = "Lekas sembuh untuk adik-adik pejuang kesembuhan. Aamiin yaa Rabbal alamin.",
                AamiinCount = 8
            };

            var d6 = new Donation
            {
                CampaignId = c1.Id,
                UserId = null,
                InvoiceNumber = "INV-20260915-0006",
                DonorName = "Budi Hartono",
                DonorEmail = "budi.h@gmail.com",
                DonorPhone = "081233445566",
                IsAnonymous = false,
                Amount = 100000m,
                PlatformTip = 2000m,
                TotalAmount = 102000m,
                PaymentMethod = "qris",
                PaymentStatus = "pending",
                PaidAt = null,
                PrayerMessage = "Semoga berkah untuk semua.",
                AamiinCount = 1
            };

            await context.Donations.AddRangeAsync(d1, d2, d3, d4, d5, d6);
            await context.SaveChangesAsync();

            // 9. APPROVALS (Temporarily disabled as requested)
            // var app1 = new Approval { Name = "Penyaluran Dana Zakat", Code = "zakat_distribution", Description = "Persetujuan untuk pencairan dana zakat kepada mustahik yang terverifikasi", IsActive = true };
            // var app2 = new Approval { Name = "Verifikasi Mustahik & Beasiswa", Code = "mustahik_aid", Description = "Persetujuan calon penerima beasiswa dan bantuan langsung", IsActive = true };
            // var app3 = new Approval { Name = "Pengeluaran Biaya Operasional", Code = "fund_outflow", Description = "Persetujuan pencairan anggaran operasional dan logistik penyaluran", IsActive = true };
            // await context.Approvals.AddRangeAsync(app1, app2, app3);
            // await context.SaveChangesAsync();

            // 10. APPROVAL ROLES (Temporarily disabled as requested)
            // await context.ApprovalRoles.AddRangeAsync(
            //     new ApprovalRole { ApprovalId = app1.Id, RoleId = roleSuperAdmin.Id, StepLevel = 1, IsActive = true },
            //     new ApprovalRole { ApprovalId = app1.Id, RoleId = roleFoundation.Id, StepLevel = 2, IsActive = true },
            //     new ApprovalRole { ApprovalId = app2.Id, RoleId = roleFoundation.Id, StepLevel = 1, IsActive = true },
            //     new ApprovalRole { ApprovalId = app3.Id, RoleId = roleSuperAdmin.Id, StepLevel = 1, IsActive = true }
            // );
            // await context.SaveChangesAsync();

            // 11. MENUS & MENU ROLES (All dynamic menus matching controllers)
            var menuItems = new List<(string Label, string RouteUrl, string Icon, int Order, Role Role, string Note)>
            {
                // Super Admin Menus
                ("Dashboard", "/Dashboard", "fas fa-tachometer-alt", 1, roleSuperAdmin, "ini untuk super admin"),
                ("Kelola Users", "/Users", "fas fa-users", 2, roleSuperAdmin, "ini untuk super admin"),
                ("Kelola Roles", "/Roles", "fas fa-user-shield", 3, roleSuperAdmin, "ini untuk super admin"),
                ("User Roles", "/UserRoles", "fas fa-id-badge", 4, roleSuperAdmin, "ini untuk super admin"),
                ("Kelola Menu", "/Menus", "fas fa-bars", 5, roleSuperAdmin, "ini untuk super admin"),
                ("Menu Roles", "/MenuRoles", "fas fa-user-tag", 6, roleSuperAdmin, "ini untuk super admin"),
                ("Kelola Mitra", "/Foundations", "fas fa-handshake", 7, roleSuperAdmin, "ini untuk super admin"),
                ("Kelola Navbar - (Master)", "/Navbars", "fas fa-bars-staggered", 8, roleSuperAdmin, "ini untuk super admin"),
                ("Kelola Grup Navbar - (Master)", "/NavbarGroups", "fas fa-folder-tree", 9, roleSuperAdmin, "ini untuk super admin"),
                ("Foundation Navbar", "/FoundationNavbars", "fas fa-link", 10, roleSuperAdmin, "ini untuk super admin"),

                // Foundation Admin Menus
                ("Dashboard Yayasan", "/Foundation/Dashboard", "fas fa-tachometer-alt", 1, roleFoundation, "ini untuk foundation"),
                ("Kelola Campaign", "/Foundation/Campaigns", "fas fa-hand-holding-heart", 2, roleFoundation, "ini untuk foundation"),
                ("Berita & Artikel", "/Foundation/NewsPosts", "fas fa-newspaper", 3, roleFoundation, "ini untuk foundation"),
                ("Daftar Donasi", "/Foundation/Donations", "fas fa-donate", 4, roleFoundation, "ini untuk foundation"),
                ("Pengaturan Profil", "/Foundation/Settings", "fas fa-cog", 5, roleFoundation, "ini untuk foundation"),

                // Donor Menus
                ("Dashboard Donatur", "/Donor/Dashboard", "fas fa-tachometer-alt", 1, roleDonor, "ini untuk donor"),
                ("Riwayat Donasi", "/Donor/History", "fas fa-history", 2, roleDonor, "ini untuk donor"),
                ("Profil Saya", "/Donor/Profile", "fas fa-user-circle", 3, roleDonor, "ini untuk donor")
            };

            foreach (var item in menuItems)
            {
                var m = new Menu
                {
                    Label = item.Label,
                    RouteUrl = item.RouteUrl,
                    IconClass = item.Icon,
                    OrderIndex = item.Order,
                    Note = item.Note,
                    IsActive = true
                };
                await context.Menus.AddAsync(m);
                await context.SaveChangesAsync();

                await context.MenuRoles.AddAsync(new MenuRole
                {
                    MenuId = m.Id,
                    RoleId = item.Role.Id,
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
