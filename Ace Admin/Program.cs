using Ace_Admin.Dto;
using Ace_Admin.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PracticeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 🔹 Configure Authentication with JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],     // "Ace_Admin"
            ValidAudience = jwtSettings["Audience"], // "CUD"
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])),
            ClockSkew = TimeSpan.FromMinutes(2) // allow minor clock drift
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT Authentication Failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine("Token received: " + context.Token);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token Validated for user: " + context.Principal.Identity.Name);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddAuthorization();

var app = builder.Build();

// 🔹 Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // ✅ authentication must come before authorization
app.UseAuthorization();
app.MapHub<ChatHub>("/chathub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}"
);
app.MapControllers();
app.Run();