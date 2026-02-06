using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Data;

public static class DbInitializer
{
    public const string RoleAdmin = "Admin";
    public const string RoleFuncionario = "Funcionario";

    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(RoleAdmin))
        {
            await roleManager.CreateAsync(new IdentityRole(RoleAdmin));
        }

        if (!await roleManager.RoleExistsAsync(RoleFuncionario))
        {
            await roleManager.CreateAsync(new IdentityRole(RoleFuncionario));
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = app.Configuration["AdminUser:Email"] ?? "admin@lavajato.local";
        var adminPassword = app.Configuration["AdminUser:Password"] ?? "Admin@123";

        var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NomeCompleto = "Administrador"
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleAdmin);
            }
        }

        var funcionarioEmail = app.Configuration["FuncionarioUser:Email"] ?? "funcionario@lavajato.local";
        var funcionarioPassword = app.Configuration["FuncionarioUser:Password"] ?? "Funcionario@123";

        var funcionarioUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == funcionarioEmail);
        if (funcionarioUser is null)
        {
            funcionarioUser = new ApplicationUser
            {
                UserName = funcionarioEmail,
                Email = funcionarioEmail,
                EmailConfirmed = true,
                NomeCompleto = "Funcionario"
            };

            var createResult = await userManager.CreateAsync(funcionarioUser, funcionarioPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(funcionarioUser, RoleFuncionario);
            }
        }
    }
}
