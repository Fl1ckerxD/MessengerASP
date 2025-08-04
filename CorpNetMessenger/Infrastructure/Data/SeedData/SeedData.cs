using System.Text.Json;
using CorpNetMessenger.Application.Configs;
using CorpNetMessenger.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CorpNetMessenger.Infrastructure.Data.SeedData
{
    public static class SeedData
    {
        public static async Task SeedAsync(MessengerContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger logger, int retry = 0)
        {
            var retryForAvailability = retry;
            try
            {
                if (context.Database.IsSqlServer())
                    context.Database.Migrate();

                if (!await context.Statuses.AnyAsync())
                {
                    await context.Statuses.AddRangeAsync(GetPreconfiguredStatuses());
                    await context.SaveChangesAsync();
                }

                if (!await context.Posts.AnyAsync())
                {
                    await context.Posts.AddRangeAsync(GetPreconfiguredPosts());
                    await context.SaveChangesAsync();
                }

                if (!await context.Departments.AnyAsync())
                {
                    await context.Departments.AddRangeAsync(GetPreconfiguredDepartments());
                    await context.SaveChangesAsync();

                    var department = await context.Departments.FirstOrDefaultAsync();
                    if (department != null)
                        context.Departments.Remove(department);

                    await context.SaveChangesAsync();
                }

                if (!await context.DepartmentPost.AnyAsync())
                {
                    await context.DepartmentPost.AddRangeAsync(GetPreconfiguredDepartmentPost());
                    await context.SaveChangesAsync();
                }

                if (!await context.Chats.AnyAsync())
                {
                    await context.Chats.AddRangeAsync(GetPreconfiguredChats());
                    await context.SaveChangesAsync();
                }

                if (!await context.Roles.AnyAsync())
                    await CreatePreconfiguredRoles(roleManager);

                if (!await context.Users.AnyAsync())
                    await CreateDefaultUsers(userManager);

                if (!await context.ChatUsers.AnyAsync())
                {
                    await context.ChatUsers.AddRangeAsync(await GetPreconfiguredChatUser(context));
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability >= 0) throw;

                retryForAvailability++;

                logger.LogError(ex.Message);
                await SeedAsync(context, userManager, roleManager, logger, retryForAvailability);
                throw;
            }
        }

        static IEnumerable<Status> GetPreconfiguredStatuses()
        {
            return new List<Status>
            {
                new(){Title = "Ожидание"},
                new(){Title = "Активно"},
                new(){Title = "Отклонено"},
                new(){Title = "Удалено"}
            };
        }

        static async Task<IEnumerable<ChatUser>> GetPreconfiguredChatUser(MessengerContext context)
        {
            List<ChatUser> chatUser = new();
            var users = await context.Users.Select(u => new { UserId = u.Id, DepartmentId = u.DepartmentId }).ToListAsync();
            foreach (var user in users)
            {
                var chat = await context.Chats.FirstOrDefaultAsync(c => c.DepartmentId == user.DepartmentId);
                chatUser.Add(new() { UserId = user.UserId, ChatId = chat.Id });
            }
            return chatUser;
        }

        static IEnumerable<Post> GetPreconfiguredPosts()
        {
            var jsonData = File.ReadAllText("Infrastructure/Data/SeedData/Posts.json");
            return JsonSerializer.Deserialize<List<Post>>(jsonData);
        }

        static IEnumerable<Department> GetPreconfiguredDepartments()
        {
            var jsonData = File.ReadAllText("Infrastructure/Data/SeedData/Departments.json");
            return JsonSerializer.Deserialize<List<Department>>(jsonData);
        }

        static IEnumerable<DepartmentPost> GetPreconfiguredDepartmentPost()
        {
            var jsonData = File.ReadAllText("Infrastructure/Data/SeedData/DepartmentPost.json");
            return JsonSerializer.Deserialize<List<DepartmentPost>>(jsonData);
        }

        static IEnumerable<Chat> GetPreconfiguredChats()
        {
            var jsonData = File.ReadAllText("Infrastructure/Data/SeedData/Chats.json");
            return JsonSerializer.Deserialize<List<Chat>>(jsonData);
        }

        static async Task CreatePreconfiguredRoles(RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole { Name = "User" });
            await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            await roleManager.CreateAsync(new IdentityRole { Name = "Mod" });
        }

        static async Task CreateDefaultUsers(UserManager<User> userManager)
        {
            var defaultUser = new User
            {
                UserName = "user",
                Email = "user@microsoft.com",
                LastName = "Любимов",
                Name = "Николай",
                Patronymic = "Константинович",
                PostId = 35,
                DepartmentId = 9,
                StatusId = StatusTypes.Active
            };
            await userManager.CreateAsync(defaultUser, "123456");

            var defaultAdmin = new User
            {
                UserName = "admin",
                Email = "admin@microsoft.com",
                LastName = "Власов",
                Name = "Адам",
                Patronymic = "Иванович",
                PostId = 34,
                DepartmentId = 9,
                StatusId = StatusTypes.Active
            };
            await userManager.CreateAsync(defaultAdmin, "123456");

            defaultAdmin = await userManager.FindByNameAsync("admin");
            if (defaultAdmin != null)
            {
                await userManager.AddToRoleAsync(defaultAdmin, "Admin");
            }
        }
    }
}