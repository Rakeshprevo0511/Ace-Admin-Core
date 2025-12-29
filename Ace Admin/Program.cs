using System.Net;
using System.Security.Cryptography;
using Ace_Admin.Dto;
using Ace_Admin.Jobs;
using Ace_Admin.Mappings;
using Ace_Admin.Models;
using Ace_Admin.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PracticeDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
   
    var config = builder.Configuration["Redis:ConnectionString"];
    if (string.IsNullOrEmpty(config) || string.IsNullOrEmpty(config))
    {
        throw new InvalidOperationException("JWT key paths are not configured properly.");
    }
    return ConnectionMultiplexer.Connect(config);
});

// 🔹 Generate RSA keys if missing
var privateKeyPath = config["JwtSettings:PrivateKeyPath"]; var publicKeyPath = config["JwtSettings:PublicKeyPath"];
if (string.IsNullOrEmpty(privateKeyPath) || string.IsNullOrEmpty(publicKeyPath))
{
    throw new InvalidOperationException("JWT key paths are not configured properly.");
}
// 🔹 Load public key for verification
var publicKey = File.ReadAllText(publicKeyPath);
var rsa = RSA.Create();
rsa.ImportFromPem(publicKey);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["JwtSettings:Issuer"],
        ValidAudience = config["JwtSettings:Audience"],
        IssuerSigningKey = new RsaSecurityKey(rsa),
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT Failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var user = context.Principal;
            var type = user?.FindFirst("tokenType")?.Value;

            if (type != "accessToken")
            {
                context.Fail("Invalid token type");
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DailyEmployeeJob");

    q.AddJob<PunchIn>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DailyEmployeeJob-trigger")
        .WithCronSchedule("0 * * * * ?")
    );
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "text/plain", "text/css", "application/javascript", "text/html", "application/json" };
});
builder.Services.AddScoped<ISeoService,SeoService>();
builder.Services.AddScoped<SeoActionFilter>();
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("default", limiter =>
    {
        limiter.PermitLimit = 100; // 100 requests
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 5;
        limiter.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});
builder.Services.AddHttpClient("NSE")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        }
    );
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddOutputCache();
builder.Services.AddSingleton<RedisService>();
var app = builder.Build();

// 🔹 Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000";
    }
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.UseRateLimiter();
app.MapHub<ChatHub>("/chathub");
app.MapGet("/", () => Results.Redirect("/login"));
app.UseSerilogRequestLogging();
app.MapControllers();
app.Run();
