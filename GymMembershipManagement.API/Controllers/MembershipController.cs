using GymMembershipManagement.SERVICE.DTOs.Membership;
using GymMembershipManagement.SERVICE.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        // Required: Admin only - Register new membership
        [HttpPost("Register")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MembershipDTO>> RegisterMembership([FromBody] RegisterMembershipDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _membershipService.RegisterMembership(dto);
            return Ok(result);
        }

        // Required: Admin only - Renew membership
        [HttpPut("Renew/{membershipId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> RenewMembership(int membershipId)
        {
            var result = await _membershipService.RenewMembership(membershipId);
            if (!result) return NotFound("Membership not found.");
            return Ok(result);
        }

        // Required: Admin only - Update membership details
        [HttpPut("Update/{membershipId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMembership(int membershipId, [FromBody] UpdateMembershipDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _membershipService.UpdateMembership(membershipId, dto);
            if (!result) return NotFound("Membership not found.");
            return Ok("Membership updated successfully.");
        }

        // Required: Admin only - Delete membership
        [HttpDelete("Delete/{membershipId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMembership(int membershipId)
        {
            var result = await _membershipService.DeleteMembership(membershipId);
            if (!result) return NotFound("Membership not found.");
            return Ok("Membership deleted successfully.");
        }

        // Required: Any authenticated user - Check own membership status
        [HttpGet("Status/{customerId:int}")]
        [Authorize(Roles = "Member,Trainer,Admin")]
        public async Task<ActionResult<MembershipStatusDTO>> GetMembershipStatus(int customerId)
        {
            var status = await _membershipService.GetMembershipStatus(customerId);
            return Ok(status);
        }

        // Required: Admin, Trainer - Get user's memberships
        [HttpGet("ByUser/{userId:int}")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult<IEnumerable<MembershipDTO>>> GetMembershipsByUser(int userId)
        {
            var memberships = await _membershipService.GetMembershipsByUser(userId);
            return Ok(memberships);
        }

        // Required: Admin, Trainer - Get active memberships
        [HttpGet("Active")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult<IEnumerable<MembershipDTO>>> GetMembershipsByStatus()
        {
            var memberships = await _membershipService.GetMembershipsByStatus();
            return Ok(memberships);
        }
    }
}
