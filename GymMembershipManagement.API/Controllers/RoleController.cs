using GymMembershipManagement.SERVICE.DTOs.Role;
using GymMembershipManagement.SERVICE.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]  // All endpoints in this controller require Admin role
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // Required: Admin only - Get all roles
        [HttpGet("GetAllRoles")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        // Required: Admin only - Get role by ID
        [HttpGet("GetRoleById/{id:int}")]
        public async Task<ActionResult<RoleDto>> GetRoleById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound("Role not found.");
            return Ok(role);
        }

        // Required: Admin only - Create new role
        [HttpPost("CreateRole")]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var role = await _roleService.CreateRoleAsync(model);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.RoleId }, role);
        }

        // Required: Admin only - Update role
        [HttpPut("UpdateRole/{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _roleService.UpdateRoleAsync(id, model);
            if (!result) return NotFound("Role not found.");
            return NoContent();
        }

        // Required: Admin only - Delete role
        [HttpDelete("DeleteRole/{id:int}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result) return NotFound("Role not found.");
            return NoContent();
        }
    }
}
