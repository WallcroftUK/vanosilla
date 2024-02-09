using WingsEmu.Game.Characters;
using WingsEmu.Plugins.BasicImplementations.Warehouse;

namespace WingsEmu.Game.Warehouse;

public class WarehouseFactory : IWarehouseFactory
{
    public IWarehouse Create(IPlayerEntity entity) => new Warehouse(entity);
}