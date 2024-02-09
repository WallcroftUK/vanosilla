﻿namespace WingsEmu.Game._enum;

public enum EffectType : short
{
    Respawn = 4,
    ShinyStars = 5,
    NormalLevelUp = 6, // Looks yellow
    Summon = 7,
    JobLevelUp = 8, // Looks white
    BigWhiteLines = 11, // It's like when you take pvp snack or snack who give you an hp/mp excess but much bigger
    BoostedAttack = 15,
    ShinyDust = 18,
    SpecialTeleport = 23,
    Frozen = 35,
    FireExplosion = 36,
    EquipAmulet = 39,
    DemonDignityRestore = 47,
    AngelDignityRestore = 48,
    SmallCritical = 191,
    BlueMiss = 192,
    RedMiss = 193,
    SmokePuff = 194,
    Transform = 196,
    CatchSuccess = 197,
    NormalLevelUpSubEffect = 198, // A sub effect sent with "NormalLevelUp"
    BigCritical = 199,
    PushSmoke = 212,
    SmallWhiteLines = 203, // Hp/Atk/Def/Exp/Pvp etc...
    BlueTimeSpace = 822,
    BlueRemoveTimeSpace = 823,
    TsTarget = 824,
    TsMate = 825,
    TsBonus = 826,
    OtherRaidMember = 828,
    OtherRaidLeader = 829,
    OwnRaidMember = 830,
    OwnRaidLeader = 831,
    Heart = 854,
    SmallHearth = 881,
    MediumHearth = 882,
    LargeHearth = 883,
    SpeedBoost = 885,
    UnfixedItem = 3003,
    UpgradeFail = 3004,
    UpgradeSuccess = 3005,
    PetPickupEnabled = 3007,
    YellowArrow = 3008,
    NoneFlag = 3009,
    RedFlag = 3010,
    BlueFlag = 3011,
    RedTeam = 3012,
    BlueTeam = 3013,
    MagicDefense = 3020,
    MeleeDefense = 3021,
    RangeDefense = 3022,
    MateAttackDefenceUpgrade = 3035,
    MateAttackUpgrade = 3038,
    MateDefenceUpgrade = 3039,
    IgnoreDefence = 3037,
    MovingAura = 3415,
    VehicleTeleportation = 3625,
    FairyResetBuff = 4013,
    Sp6ArcherTargetFalcon = 4269,
    MateHealByMonster = 4328,
    MeditationFirstStage = 4343,
    MeditationFinalStage = 4344,
    TalentArenaCall = 4432,
    RedCircle = 4660,
    AngelProtection = 4800,
    DemonProtection = 4801,
    Taunt = 4968,
    ArchmageTeleport = 4482,
    ArchmageTeleportSet = 4497,
    ArchmageTeleportWhiteEffect = 4498,
    ArchmageTeleportAfter = 4499,
    Targeted = 5000,
    TargetedByOthers = 5001,
    PetLove = 5002,
    PetLoveBroke = 5003,
    PetPickUp = 5004,
    PetAttack = 5005,
    MinigameQuarry = 5102,
    MinigameSawmill = 5103,
    MinigameShooting = 5105,
    MinigameFishing = 5104,
    MinigameTypewritter = 5113,
    MinigameMemory = 5112,
    Eat = 6000,
    DecreaseHp = 6004,
    DoubleChanceDrop = 7552
}