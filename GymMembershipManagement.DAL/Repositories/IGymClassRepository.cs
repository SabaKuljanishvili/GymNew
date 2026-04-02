using GymMembershipManagement.DATA;
using GymMembershipManagement.DATA.Entities;
using GymMembershipManagement.DATA.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymMembershipManagement.DAL.Repositories
{
    public interface IGymClassRepository : IBaseRepository<GymClass>
    {
        Task<IEnumerable<GymClass>> GetAllWithDetailsAsync();
    }

    public class GymClassRepository : BaseRepository<GymClass>, IGymClassRepository
    {
        private readonly GymDbContext _context;
        public GymClassRepository(GymDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GymClass>> GetAllWithDetailsAsync()
        {
            return await _context.GymClasses
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
