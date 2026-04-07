using GymMembershipManagement.SERVICE;
using GymMembershipManagement.SERVICE.DTOs.User;
using GymMembershipManagement.SERVICE.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Unauthorized - Public endpoint
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _userService.UserRegistration(model);
            return Ok("User registered successfully.");
        }

        // Unauthorized - Public endpoint
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var response = await _userService.Login(model.Email, model.Password);
            return Ok(response);
        }

        // Required: Any authenticated user (Member, Trainer, Admin)
        [HttpGet("GetProfile/{userId:int}")]
        [Authorize(Roles = "Member,Trainer,Admin")]
        public async Task<ActionResult<UserDTO>> GetProfile(int userId)
        {
            var user = await _userService.GetProfile(userId);
            return Ok(user);
        }

        // Required: Admin only
        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        // Required: Any authenticated user (can update own profile)
        [HttpPut("UpdateProfile/{userId:int}")]
        [Authorize(Roles = "Member,Trainer,Admin")]
        public async Task<ActionResult<UserDTO>> UpdateProfile(int userId, [FromBody] UpdateUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updatedUser = await _userService.UpdateProfile(userId, model);
            return Ok(updatedUser);
        }

        // Required: Any authenticated user (can delete own profile)
        [HttpDelete("DeleteProfile/{userId:int}")]
        [Authorize(Roles = "Member,Trainer,Admin")]
        public async Task<IActionResult> DeleteProfile(int userId)
        {
            await _userService.DeleteProfile(userId);
            return Ok("User deleted successfully.");
        }

        // Required: Any authenticated user
        [HttpPost("Logout")]
        [Authorize(Roles = "Member,Trainer,Admin")]
        public IActionResult Logout()
        {
            _userService.Logout();
            return Ok("Logged out successfully.");
        }
    }
}
