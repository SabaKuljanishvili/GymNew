using GymMembershipManagement.DAL.Repositories;
using GymMembershipManagement.DATA.Entities;
using GymMembershipManagement.SERVICE.DTOs.Schedule;
using GymMembershipManagement.SERVICE.Interfaces;

namespace GymMembershipManagement.SERVICE
{
    public class TrainerService : ITrainerService
    {
        private readonly IScheduleRepository _scheduleRepository;

        public TrainerService(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task<bool> AssignSchedule(AssignScheduleDTO dto)
        {
            var schedule = new Schedule
            {
                UserId = dto.UserId,
                ScheduledDateTime = dto.ScheduledDateTime,
                Duration = dto.Duration,
                GymClassId = dto.GymClassId
            };
            await _scheduleRepository.AddAsync(schedule);
            return true;
        }

        public async Task<IEnumerable<ScheduleDTO>> GetSchedulesByTrainer(int trainerId)
        {
            var schedules = await _scheduleRepository.GetByTrainerIdAsync(trainerId);
            return schedules.Select(s => new ScheduleDTO
            {
                Id = s.Id,
                UserId = s.UserId,
                ScheduledDateTime = s.ScheduledDateTime,
                Duration = s.Duration,
                GymClassId = s.GymClassId,
                GymClassName = s.GymClass?.GymClassName
            });
        }

        public async Task<IEnumerable<ScheduleDTO>> GetAllSchedules()
        {
            var schedules = await _scheduleRepository.GetAllWithDetailsAsync();
            return schedules.Select(s => new ScheduleDTO
            {
                Id = s.Id,
                UserId = s.UserId,
                ScheduledDateTime = s.ScheduledDateTime,
                Duration = s.Duration,
                GymClassId = s.GymClassId,
                GymClassName = s.GymClass?.GymClassName
            });
        }

        public async Task<bool> UpdateSchedule(UpdateScheduleDTO dto)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(dto.Id);
            if (schedule == null) return false;

            schedule.UserId = dto.UserId;
            schedule.ScheduledDateTime = dto.ScheduledDateTime;
            schedule.Duration = dto.Duration;
            schedule.GymClassId = dto.GymClassId;

            await _scheduleRepository.UpdateAsync(schedule);
            return true;
        }

        public async Task<bool> DeleteSchedule(int scheduleId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null) return false;

            await _scheduleRepository.DeleteAsync(scheduleId);
            return true;
        }
    }
}
