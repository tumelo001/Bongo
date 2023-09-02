using Bongo.Models;

namespace Bongo.Data
{
    public interface ITimetableRepository : IRepositoryBase<Timetable>
    {
        public Timetable GetUserTimetable(string username);
    }
}
