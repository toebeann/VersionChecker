using System.Threading.Tasks;

namespace Straitjacket.Subnautica.Mods.VersionChecker
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
