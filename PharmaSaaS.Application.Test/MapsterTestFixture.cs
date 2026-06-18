using PharmaSaaS.Application.Mapping;

namespace PharmaSaaS.Application.Test;

public class MapsterTestFixture
{
    private static bool _initialized;

    public MapsterTestFixture()
    {
        if (_initialized) return;

        TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfiguration).Assembly);
        _initialized = true;
    }
}

