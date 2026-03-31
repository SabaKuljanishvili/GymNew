using GymMembershipManagement.SERVICE.DTOs.User;
using GymMembershipManagement.SERVICE.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Admin only
        [HttpPost("AddUser")]
        public async Task<ActionResult<UserDTO>> AddUser([FromBody] UserRegisterModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _adminService.AddUser(model);
            return Ok(user);
        }

        // Admin only
        [HttpGet("GetUserById/{userId:int}")]
        public async Task<ActionResult<UserDTO>> GetUserById(int userId)
        {
            var user = await _adminService.GetUserById(userId);
            return Ok(user);
        }

        // Admin only
        [HttpPut("UpdateUser/{userId:int}")]
        public async Task<IActionResult> UpdateUserDetails(int userId, [FromBody] UpdateUserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _adminService.UpdateUserDetails(userId, model);
            return Ok("User updated successfully.");
        }

        // Admin only
        [HttpDelete("RemoveUser/{userId:int}")]
        public async Task<IActionResult> RemoveUser(int userId)
        {
            await _adminService.RemoveUser(userId);
            return Ok("User removed successfully.");
        }

        // Admin, Trainer
        [HttpGet("GetAllMembers")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllMembers()
        {
            var members = await _adminService.GetAllMembers();
            return Ok(members);
        }

        // Admin, Trainer, Member
        [HttpGet("GetAllTrainers")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllTrainers()
        {
            var trainers = await _adminService.GetAllTrainers();
            return Ok(trainers);
        }

        // Admin only
        [HttpGet("GetAllAdmins")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdmins();
            return Ok(admins);
        }

        // Admin only — Assign a role to a user (this is how a user becomes a Trainer)
        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _adminService.AssignRoleToUser(dto);
            return Ok("Role assigned successfully.");
        }

        // Admin only — Remove a role from a user
        [HttpDelete("RemoveRole")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _adminService.RemoveRoleFromUser(dto);
            return Ok("Role removed successfully.");
        }

        // Admin only — Update trainer details
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
