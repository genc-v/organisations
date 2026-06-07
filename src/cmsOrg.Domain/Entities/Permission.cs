namespace cmsOrg.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<UserOrganisationPermission> UserPermissions { get; set; } = [];

    public static readonly List<Permission> DefaultPermissions = 
    [
        new() { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), 
            Name = "Admin", 
            Description = "Full access to everything including organisation settings." 
        },
        new() { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), 
            Name = "Editor", 
            Description = "Can edit tags, categories, entries, and assets. Cannot edit organisation." 
        },
        new() { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), 
            Name = "Viewer", 
            Description = "Can only view things in the organisation." 
        }
    ];
}
