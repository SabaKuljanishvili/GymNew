using Microsoft.AspNetCore.Authorization;

namespace GymMembershipManagement.API.Attributes
{
    /// <summary>
    /// Custom attribute for role-based authorization.
    /// Usage: [AuthorizeRole("Admin", "Trainer")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeRoleAttribute(params string[] roles)
        {
            if (roles.Any())
            {
                Roles = string.Join(",", roles);
            }
        }
    }
}
