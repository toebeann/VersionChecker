using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker.Interface
{
    internal interface IUpdatable
    {
        void Update();
    }

    internal interface IUpdatableAsync
    {
        Task UpdateAsync();
    }
}
