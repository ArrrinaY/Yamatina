using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using InternshipService.Auth.models;

namespace InternshipService.Auth;

public class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public int UserId
    {
        get
        {
            bool? isApi = httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-API-KEY", out _);
            if (isApi.HasValue && isApi.Value)
            {
                return 0;
            }
            
            var stringUserId = httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;
            
            if (int.TryParse(stringUserId, out var userId))
                return userId;

            throw new KeyNotFoundException("User not found");
        }
    }
    
    public List<string> UserPermissions
    {
        get
        {
            bool? isApi = httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-API-KEY", out _);
            if (isApi.HasValue && isApi.Value)
            {
                return [Permissions.Read, Permissions.Create, Permissions.Update, Permissions.Delete];
            }
            
            var userPermissions = httpContextAccessor.HttpContext?
                .User?
                .FindAll("permissions")?
                .Select(permission => permission.Value)
                .ToList();
            
            if (userPermissions is not null)
                return userPermissions;

            throw new KeyNotFoundException("User not found");
        }
    }
}

