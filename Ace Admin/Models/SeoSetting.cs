using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class SeoSetting
{
    public int Id { get; set; }

    public string PageTitle { get; set; } = null!;

    public string? PageUrl { get; set; }

    public string MetaDescription { get; set; } = null!;

    public string? MetaKeywords { get; set; }

    public string? MetaAuthor { get; set; }

    public string? OgTitle { get; set; }

    public string? OgImage { get; set; }

    public string? OgDescription { get; set; }

    public string? TwitterCard { get; set; }

    public string? TwitterSite { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? Robots { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsActive { get; set; }
}
