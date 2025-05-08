using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
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
                options.ViewLocationFormats.Add("/Web/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
                options.ViewLocationFormats.Add("/Web/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
            });

            var conString = builder.Configuration.GetConnectionString("CorpNetMessenger") ??
                throw new InvalidOperationException("Connection string 'CorpNetMessenger' not found.");
            builder.Services.AddDbContext<MessengerContext>(options => options.UseSqlServer(conString));

            builder.Services.AddIdentity<User, IdentityRole<string>>()
                .AddEntityFrameworkStores<MessengerContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

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
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
