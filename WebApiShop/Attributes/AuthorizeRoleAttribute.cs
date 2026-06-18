using Microsoft.AspNetCore.Authorization;

namespace WebApiShop.Attributes
{
    /// <summary>
    /// Custom authorization attribute for role-based access control.
    /// Supports a single role or multiple comma-separated roles.
    /// Usage: [AuthorizeRole("Admin")] or [AuthorizeRole("Admin", "Manager")]
    /// </summary>
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeRoleAttribute(string role) : base()
        {
            Roles = role;
        }

        public AuthorizeRoleAttribute(params string[] roles) : base()
        {
            Roles = string.Join(",", roles);
        }
    }
}
