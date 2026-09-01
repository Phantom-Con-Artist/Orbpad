using Orb.Engine.Graph;

namespace Orbpad.Orbis.Services;

public sealed class OrbEngineService
{
    public OrbEntity CreateEntity(string name, string type)
    {
        return new OrbEntity
        {
            Name = name,
            Type = type
        };
    }
}