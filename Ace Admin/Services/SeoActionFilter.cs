using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ace_Admin.Services
{
    public class SeoActionFilter : IAsyncActionFilter
    {

        private readonly ISeoService _seoService;

        public SeoActionFilter(ISeoService seoService)
        {
            _seoService = seoService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                var path = context.HttpContext?.Request?.Path.Value?? string.Empty;
                var seoData = await _seoService.GetSeoByPageUrlAsync(path);

                controller.ViewBag.PageTitle = seoData.PageTitle;
                controller.ViewBag.MetaDescription = seoData.MetaDescription;
                controller.ViewBag.MetaKeywords = seoData.MetaKeywords;
                controller.ViewBag.MetaAuthor = seoData.MetaAuthor;
                controller.ViewBag.OgTitle = seoData.OgTitle ?? seoData.PageTitle;
                controller.ViewBag.OgDescription = seoData.OgDescription ?? seoData.MetaDescription;
                controller.ViewBag.OgImage = seoData.OgImage;
                controller.ViewBag.TwitterCard = seoData.TwitterCard ?? "summary";
                controller.ViewBag.TwitterSite = seoData.TwitterSite;
                controller.ViewBag.CanonicalUrl = seoData.CanonicalUrl;
                controller.ViewBag.Robots = seoData.Robots ?? "index, follow";
            }

            await next();
        }

    }
}
