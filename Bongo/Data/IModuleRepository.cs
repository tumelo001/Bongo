using Bongo.Models;

namespace Bongo.Data
{
    public interface IModuleRepository
    {
        void Add(Module module);
        void Delete(Module module);
        void SaveChanges();
        IEnumerable<Module> GetAllSessions();
    }
}
