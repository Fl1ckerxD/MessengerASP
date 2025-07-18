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
using System.Runtime.Serialization.Formatters;

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

            builder.Services.AddResponseCompression(opt =>
            {
                opt.EnableForHttps = true;
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddAutoMapper(typeof(AppMappingProfile));
            builder.Services.AddSignalR();
            builder.Services.AddMemoryCache();

            // Add Scoped services to the container.
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IRequestService, RequestService>();

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

            app.UseResponseCompression();
            app.MapStaticAssets();
            app.UseStaticFiles(new StaticFileOptions()
            {
               OnPrepareResponse = ctx =>
               {
                  ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=600");
               }
            });

            app.MapHub<ChatHub>("/chatHub");

            app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapAreaControllerRoute(
                    name: "Messaging",
                    areaName: "Messaging",
                    pattern: "Messaging/Chat/{id}",
                    defaults: new { controller = "Chat", action = "Index" });        

            app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();

            app.Run();
        }
    }
}
