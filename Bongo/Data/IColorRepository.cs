using Bongo.Models;

namespace Bongo.Data
{
    public interface IColorRepository : IRepositoryBase<Color>
    {
        public Color GetByName(string name);
    }
}
