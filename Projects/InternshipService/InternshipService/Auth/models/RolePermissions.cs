using System.Collections.Generic;

namespace InternshipService.Auth.models;

public static class RolePermissions
{
    public static readonly Dictionary<string, List<string>> Map =
        new()
        {
            [Roles.Admin] = new()
            {
                Permissions.Read,
                Permissions.Create,
                Permissions.Update,
                Permissions.Delete
            },
            [Roles.Company] = new()
            {
                Permissions.Read,
                Permissions.Create,
                Permissions.Update
            },
            [Roles.Candidate] = new()
            {
                Permissions.Read,
                Permissions.Create,
            }
        };
}

