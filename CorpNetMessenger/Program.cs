using CorpNetMessenger.Application;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Domain.MappingProfiles;
using CorpNetMessenger.Infrastructure.Data;
using CorpNetMessenger.Infrastructure.Repositories;
using CorpNetMessenger.Infrastructure.Services;
using CorpNetMessenger.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MessengerASP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorOptions(options =>
            {
                options.ViewLocationExpanders.Add(new CustomViewLocationExpander());
            });

            var conString = builder.Configuration.GetConnectionString("CorpNetMessenger") ??
                throw new InvalidOperationException("Connection string 'CorpNetMessenger' not found.");
            builder.Services.AddDbContext<MessengerContext>(options => options.UseSqlServer(conString));

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
            })
                .AddEntityFrameworkStores<MessengerContext>()
                .AddDefaultTokenProviders()
                .AddRoles<IdentityRole>();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.ReturnUrlParameter = "243523";
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddAutoMapper(typeof(AppMappingProfile));
            builder.Services.AddSignalR();

            // Add Scoped services to the container.
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IChatService, ChatService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapHub<ChatHub>("/chatHub");

            app.MapControllerRoute(
                name: "chat",
                pattern: "chat/{id}",
                defaults: new { controller = "Chat", action = "Index" });

            app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
