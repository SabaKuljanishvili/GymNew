using GymMembershipManagement.DAL.Repositories;
using GymMembershipManagement.DATA;

namespace GymMembershipManagement.API.Services
{
    /// <summary>
    /// Service to fix and maintain database integrity for roles
    /// Ensures every user has exactly one role
    /// </summary>
    public interface IRoleIntegrityService
    {
        Task<int> FixUserRolesAsync();
    }

    public class RoleIntegrityService : IRoleIntegrityService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly GymDbContext _context;
        private readonly ILogger<RoleIntegrityService> _logger;

        public RoleIntegrityService(IUserRepository userRepository, IRoleRepository roleRepository, GymDbContext context, ILogger<RoleIntegrityService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Fixes database integrity by ensuring every user has exactly one role
        /// - Users with no roles get assigned "Customer" role
        /// - Users with multiple roles keep the first one, others are removed
        /// </summary>
        public async Task<int> FixUserRolesAsync()
        {
            int fixedCount = 0;
            var allUsers = await _userRepository.GetAllAsync();

            foreach (var user in allUsers)
            {
                var userWithRoles = await _userRepository.GetByIdWithPersonAsync(user.UserId);
                
                if (userWithRoles?.UserRoles == null || !userWithRoles.UserRoles.Any())
                {
                    // User has no role - assign Customer role
                    var customerRole = await _roleRepository.GetByRoleNameAsync("Customer");
                    if (customerRole != null)
                    {
                        await _roleRepository.AssignRoleToUserAsync(user.UserId, customerRole.RoleId);
                        _logger.LogInformation($"Assigned Customer role to user {user.UserId} ({user.Email})");
                        fixedCount++;
                    }
                }
                else if (userWithRoles.UserRoles.Count > 1)
                {
                    // User has multiple roles - keep first, remove others
                    var firstRole = userWithRoles.UserRoles.FirstOrDefault();
                    var rolesToRemove = userWithRoles.UserRoles.Skip(1).ToList();

                    foreach (var roleToRemove in rolesToRemove)
                    {
                        await _roleRepository.RemoveRoleFromUserAsync(user.UserId, roleToRemove.RoleId);
                        _logger.LogInformation($"Removed role {roleToRemove.Role.RoleName} from user {user.UserId}");
                        fixedCount++;
                    }
                }
            }

            _logger.LogInformation($"Role integrity check completed. Fixed {fixedCount} issues.");
            return fixedCount;
        }
    }
}
