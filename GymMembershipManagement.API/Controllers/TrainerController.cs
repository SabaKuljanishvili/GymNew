using GymMembershipManagement.SERVICE.DTOs.Schedule;
using GymMembershipManagement.SERVICE.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymMembershipManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainerController : ControllerBase
    {
        private readonly ITrainerService _trainerService;

        public TrainerController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        // Required: Admin only - Assign trainer to schedule
        [HttpPost("AssignSchedule")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> AssignSchedule([FromBody] AssignScheduleDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _trainerService.AssignSchedule(dto);
            return Ok(result);
        }

        // Required: Admin, Trainer - Get trainer's schedules
        [HttpGet("Schedules/{trainerId:int}")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult<IEnumerable<ScheduleDTO>>> GetSchedulesByTrainer(int trainerId)
        {
            var schedules = await _trainerService.GetSchedulesByTrainer(trainerId);
            return Ok(schedules);
        }

        // Required: Admin only - Get all schedules
        [HttpGet("AllSchedules")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ScheduleDTO>>> GetAllSchedules()
        {
            var schedules = await _trainerService.GetAllSchedules();
            return Ok(schedules);
        }

        // Required: Admin, Trainer - Update schedule
        [HttpPut("UpdateSchedule")]
        [Authorize(Roles = "Admin,Trainer")]
        public async Task<ActionResult<bool>> UpdateSchedule([FromBody] UpdateScheduleDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _trainerService.UpdateSchedule(dto);
            if (!result) return NotFound("Schedule not found");
            return Ok(result);
        }

        // Required: Admin only - Delete schedule
        [HttpDelete("DeleteSchedule/{scheduleId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> DeleteSchedule(int scheduleId)
        {
            var result = await _trainerService.DeleteSchedule(scheduleId);
            if (!result) return NotFound("Schedule not found");
            return Ok(result);
        }
    }
}
