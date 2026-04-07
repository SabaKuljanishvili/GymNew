using GymMembershipManagement.DAL.Repositories;
using GymMembershipManagement.DATA.Entities;
using GymMembershipManagement.SERVICE.DTOs.User;
using GymMembershipManagement.SERVICE.Interfaces;
using GymMembershipManagement.SERVICE.Services;
using Microsoft.Extensions.Logging;

namespace GymMembershipManagement.SERVICE
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IPersonRepository personRepository, IRoleRepository roleRepository, ITokenService tokenService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _personRepository = personRepository;
            _roleRepository = roleRepository;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task UserRegistration(UserRegisterModel model)
        {
            // Check if email already exists
            var existing = await _userRepository.GetByEmailAsync(model.Email);
            if (existing != null)
                throw new InvalidOperationException($"A user with email '{model.Email}' already exists.");

            var person = new Person
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                Address = model.Address
            };
            await _personRepository.AddAsync(person);

            var user = new User
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Email = model.Email,
                RegistrationDate = DateTime.UtcNow,
                PersonId = person.PersonId
            };
            await _userRepository.AddAsync(user);

            // Assign Customer role to the new user
            var customerRole = await _roleRepository.GetByRoleNameAsync("Customer");
            if (customerRole != null)
            {
                await _roleRepository.AssignRoleToUserAsync(user.UserId, customerRole.RoleId);
            }

            _logger.LogInformation("User registered successfully: {Email}", model.Email);
        }

        public async Task<LoginResponseDTO> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Email and password are required.");

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email}", email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for email: {Email}", email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            _logger.LogInformation("Login successful for: {Email}", email);

            // Get fresh user data with latest roles from database
            var freshUser = await _userRepository.GetByIdWithPersonAsync(user.UserId);

            // Generate JWT token with fresh role data
            var token = _tokenService.GenerateToken(freshUser!);

            // Ensure user has exactly one role (the primary one)
            var primaryRole = freshUser!.UserRoles?.FirstOrDefault()?.Role.RoleName ?? "Customer";

            return new LoginResponseDTO
            {
                UserId = freshUser.UserId,
                Username = freshUser.Username,
                Email = freshUser.Email,
                FirstName = freshUser.Person?.FirstName ?? "",
                LastName = freshUser.Person?.LastName ?? "",
                RegistrationDate = freshUser.RegistrationDate,
                RoleId = freshUser.UserRoles?.FirstOrDefault()?.RoleId,
                Roles = new List<string> { primaryRole },  // Only return primary role
                Token = token
            };
        }

        public async Task<UserDTO> GetProfile(int userId)
        {
            var user = await _userRepository.GetByIdWithPersonAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            return MapToDTO(user);
        }

        public async Task<UserDTO> UpdateProfile(int userId, UpdateUserModel model)
        {
            var user = await _userRepository.GetByIdWithPersonAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            bool userChanged = false;
            bool personChanged = false;

            if (!string.IsNullOrWhiteSpace(model.Username) && model.Username != user.Username)
            {
                user.Username = model.Username;
                userChanged = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
            {
                user.Email = model.Email;
                userChanged = true;
            }

            if (user.Person != null)
            {
                if (!string.IsNullOrWhiteSpace(model.FirstName) && model.FirstName != user.Person.FirstName)
                { user.Person.FirstName = model.FirstName; personChanged = true; }

                if (!string.IsNullOrWhiteSpace(model.LastName) && model.LastName != user.Person.LastName)
                { user.Person.LastName = model.LastName; personChanged = true; }

                if (!string.IsNullOrWhiteSpace(model.Phone) && model.Phone != user.Person.Phone)
                { user.Person.Phone = model.Phone; personChanged = true; }

                if (!string.IsNullOrWhiteSpace(model.Address) && model.Address != user.Person.Address)
                { user.Person.Address = model.Address; personChanged = true; }

                if (personChanged) await _personRepository.UpdateAsync(user.Person);
            }

            if (userChanged) await _userRepository.UpdateAsync(user);

            return MapToDTO(user);
        }

        public async Task DeleteProfile(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            await _userRepository.DeleteAsync(userId);
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();
            return users.Select(MapToDTO).ToList();
        }

        public void Logout()
        {
            // Client-side token invalidation (JWT stateless — token is removed on client)
        }

        private static UserDTO MapToDTO(User user) => new UserDTO
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            RegistrationDate = user.RegistrationDate,
            FirstName = user.Person?.FirstName,
            LastName = user.Person?.LastName,
            RoleId = user.UserRoles?.FirstOrDefault()?.RoleId
        };
    }
}
