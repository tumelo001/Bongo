using Bongo.Models;
using System.Xml;

namespace Bongo.Data
{
    public class TimetableRepository : RepositoryBase<Timetable>, ITimetableRepository
    {
        public TimetableRepository(AppDbContext appDbContext) : base(appDbContext)
        { }

        public Timetable GetUserTimetable(string username)
        {
            return _appDbContext.Timetables.FirstOrDefault(t => t.Username == username);
        }
    }
}
