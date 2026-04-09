using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        /// <summary>
        /// Decodes and displays JWT token claims for debugging
        /// This helps verify token structure and role claims
        /// 
        /// Usage:
        /// POST /api/debug/VerifyToken
        /// Body: { "token": "eyJ..." }
        /// </summary>
        [HttpPost("VerifyToken")]
        [AllowAnonymous]
        public IActionResult VerifyToken([FromBody] TokenDebugRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
                return BadRequest(new { error = "Token is required" });

            try
            {
                var handler = new JwtSecurityTokenHandler();
                
                // Don't validate signature - just decode
                var token = handler.ReadJwtToken(request.Token);

                var claims = token.Claims.Select(c => new
                {
                    type = c.Type,
                    value = c.Value
                }).ToList();

                var roleClaimKey = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
                var roleClaim = token.Claims.FirstOrDefault(c => c.Type == roleClaimKey);

                return Ok(new
                {
                    success = true,
                    message = "Token decoded successfully",
                    tokenInfo = new
                    {
                        issuer = token.Issuer,
                        audience = token.Audiences.FirstOrDefault(),
                        expiresAt = token.ValidTo,
                        isExpired = token.ValidTo < DateTime.UtcNow,
                        issuedAt = token.IssuedAt
                    },
                    claims = claims,
                    roleClaimFound = roleClaim != null,
                    roleValue = roleClaim?.Value ?? "NOT FOUND",
                    summary = new
                    {
                        userId = token.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value ?? "NOT FOUND",
                        email = token.Claims.FirstOrDefault(c => c.Type.Contains("emailaddress"))?.Value ?? "NOT FOUND",
                        username = token.Claims.FirstOrDefault(c => c.Type.Contains("name") && !c.Type.Contains("identifier"))?.Value ?? "NOT FOUND",
                        role = roleClaim?.Value ?? "NOT FOUND"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Invalid token format",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Checks if user is authenticated and shows their current claims
        /// </summary>
        [HttpGet("WhoAmI")]
        [Authorize]
        public IActionResult WhoAmI()
        {
            var claims = User.Claims.Select(c => new
            {
                type = c.Type,
                value = c.Value
            }).ToList();

            var roleClaimKey = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == roleClaimKey);

            return Ok(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                identity = new
                {
                    authenticationType = User.Identity?.AuthenticationType,
                    name = User.Identity?.Name
                },
                claims = claims,
                roleValue = roleClaim?.Value ?? "NO ROLE",
                hasRole = roleClaim != null,
                allRoles = User.Claims
                    .Where(c => c.Type == roleClaimKey)
                    .Select(c => c.Value)
                    .ToList()
            });
        }

        /// <summary>
        /// Tests if user has specific role
        /// </summary>
        [HttpPost("CheckRole")]
        [Authorize]
        public IActionResult CheckRole([FromBody] CheckRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RoleName))
                return BadRequest(new { error = "RoleName is required" });

            var hasRole = User.IsInRole(request.RoleName);

            return Ok(new
            {
                requestedRole = request.RoleName,
                hasRole = hasRole,
                userRoles = User.Claims
                    .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    .Select(c => c.Value)
                    .ToList(),
                userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value,
                email = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
            });
        }
    }

    public class TokenDebugRequest
    {
        public string? Token { get; set; }
    }

    public class CheckRoleRequest
    {
        public string? RoleName { get; set; }
    }
}
