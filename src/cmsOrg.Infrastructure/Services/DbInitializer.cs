using cmsOrg.Domain.Entities;
using cmsOrg.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace cmsOrg.Infrastructure.Services;

public static class DbInitializer
{
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var p in Permission.DefaultPermissions)
        {
            if (!await context.Permissions.AnyAsync(x => x.Id == p.Id))
            {
                context.Permissions.Add(p);
            }
        }
        await context.SaveChangesAsync();
    }
}
