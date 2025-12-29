using Ace_Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace Ace_Admin.Services {
    public interface ISeoService {
        Task<SeoSetting> GetSeoByPageUrlAsync(string pageUrl);
    }
    public class SeoService:ISeoService {
        private readonly PracticeDbContext _context;

        public SeoService(PracticeDbContext context) {
            _context = context;
        }

        public async Task<SeoSetting> GetSeoByPageUrlAsync(string pageUrl) {
            var seoSetting = await _context.SeoSettings
            .Where(s => s.IsActive == true && s.PageUrl == pageUrl)
            .FirstOrDefaultAsync();

            return seoSetting ?? new SeoSetting {
                PageTitle = "Ace Portal | Secure Management Dashboard",
                MetaDescription = "Access the Ace Portal to manage accounts, view reports, track activities, and control system operations securely.",
                MetaKeywords = "portal, dashboard, management system, admin portal, user portal, analytics",
                MetaAuthor = "Ace Technologies",
                Robots = "noindex, nofollow",   // 🔒 SAFE DEFAULT
                CanonicalUrl = "https://yourdomain.com",
                OgTitle = "Ace Portal | Secure Management Dashboard",
                OgDescription = "A secure portal to manage users, reports, and system operations efficiently.",
                OgImage = "https://yourdomain.com/assets/images/default-og.png",
                TwitterCard = "summary_large_image",
                TwitterSite = "@acetech",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

        }
    }
}
