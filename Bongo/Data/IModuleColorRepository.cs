using Bongo.Models;

namespace Bongo.Data
{
    public interface IModuleColorRepository : IRepositoryBase<ModuleColor>
    {
        ModuleColor GetModuleColorWithColorDetails(string username, string moduleCode);
    }
}
