// WingsEmu
// 
// Developed by NosWings Team

namespace WingsEmu.Game.Specialists;

public interface ISpecialistStatsComponent
{
    int DamageMinimum { get; }
    int DamageMaximum { get; }
    int HitRate { get; }
    int CriticalLuckRate { get; }
    int CriticalRate { get; }
    int DefenceDodge { get; }
    int DistanceDefenceDodge { get; }
    int ElementRate { get; }
    int DarkResistance { get; }
    int LightResistance { get; }
    int FireResistance { get; }
    int WaterResistance { get; }
    int CriticalDodge { get; }
    int CloseDefence { get; }
    int DistanceDefence { get; }
    int MagicDefence { get; }
    int Hp { get; }
    int Mp { get; }
    int SpDamage { get; }
    int SpDefence { get; }
    int SpElement { get; }
    int SpHP { get; }
    int SpDark { get; }
    int SpFire { get; }
    int SpWater { get; }
    int SpLight { get; }
    int GetSlHit();
    int GetSlDefense();
    int GetSlElement();
    int GetSlHp();
    void RefreshSlStats();
}