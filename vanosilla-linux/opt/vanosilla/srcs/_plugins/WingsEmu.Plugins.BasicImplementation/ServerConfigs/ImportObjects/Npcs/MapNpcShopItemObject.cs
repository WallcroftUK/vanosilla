using YamlDotNet.Serialization;

namespace WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Npcs;

public class MapNpcShopItemObject
{
    [YamlMember(Alias = "itemVnum", ApplyNamingConventions = true)]
    public int ItemVnum { get; set; }

    [YamlMember(Alias = "design", ApplyNamingConventions = true)]
    public byte Design { get; set; }

    [YamlMember(Alias = "rarity", ApplyNamingConventions = true)]
    public short Rarity { get; set; }

    [YamlMember(Alias = "upgrade", ApplyNamingConventions = true)]
    public byte Upgrade { get; set; }

    [YamlMember(Alias = "price", ApplyNamingConventions = true)]
    public int? Price { get; set; }
}