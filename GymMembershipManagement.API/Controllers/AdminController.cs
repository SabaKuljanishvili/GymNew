using GymMembershipManagement.SERVICE.DTOs.User;
using GymMembershipManagement.SERVICE.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]  // All endpoints in this controller require Admin role
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Required: Admin only - Create new user
        [HttpPost("AddUser")]
        public async Task<ActionResult<UserDTO>> AddUser([FromBody] UserRegisterModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _adminService.AddUser(model);
            return Ok(user);
        }

        // Required: Admin only - Get user details
        [HttpGet("GetUserById/{userId:int}")]
        public async Task<ActionResult<UserDTO>> GetUserById(int userId)
        {
            var user = await _adminService.GetUserById(userId);
            return Ok(user);
        }

        // Required: Admin only - Update user details
        [HttpPut("UpdateUser/{userId:int}")]
        public async Task<IActionResult> UpdateUserDetails(int userId, [FromBody] UpdateUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _adminService.UpdateUserDetails(userId, model);
            return Ok("User updated successfully.");
        }

        // Required: Admin only - Delete user
        [HttpDelete("RemoveUser/{userId:int}")]
        public async Task<IActionResult> RemoveUser(int userId)
        {
            await _adminService.RemoveUser(userId);
            return Ok("User removed successfully.");
        }

        // Required: Admin only - Get all members
        [HttpGet("GetAllMembers")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllMembers()
        {
            var members = await _adminService.GetAllMembers();
            return Ok(members);
        }

        // Required: Admin only - Get all trainers
        [HttpGet("GetAllTrainers")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllTrainers()
        {
            var trainers = await _adminService.GetAllTrainers();
            return Ok(trainers);
        }

        // Required: Admin only - Get all admins
        [HttpGet("GetAllAdmins")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdmins();
            return Ok(admins);
        }

        // Required: Admin only - Assign a role to a user (promote/change roles)
        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _adminService.AssignRoleToUser(dto);
            return Ok("Role assigned successfully.");
        }

        // Required: Admin only - Remove a role from a user
        [HttpDelete("RemoveRole")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _adminService.RemoveRoleFromUser(dto);
            return Ok("Role removed successfully.");
        }

        // Required: Admin only - Update trainer details
        [HttpPut("UpdateTrainer/{userId:int}")]
        public async Task<IActionResult> UpdateTrainer(int userId, [FromBody] UpdateUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _adminService.UpdateTrainer(userId, model);
            return Ok("Trainer updated successfully.");
        }

        // Admin only — Delete a trainer
        [HttpDelete("DeleteTrainer/{userId:int}")]
        public async Task<IActionResult> DeleteTrainer(int userId)
        {
            await _adminService.DeleteTrainer(userId);
            return Ok("Trainer deleted successfully.");
        }
    }
}
