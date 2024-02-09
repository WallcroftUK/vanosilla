namespace WingsEmu.Packets.Enums
{
    public enum WindowType : byte
    {
        PARTNER_EXCHANGE = 0,
        UPGRADE_ITEM_NPC = 1,
        MERGE_JEWELRY = 3,
        UPGRADE_ITEM_RARITY = 7,
        CONNECT_BOOTS_GLOVES = 8,
        UPGRADE_SP = 9,
        UPGRADE_ITEM_PLUS_EQ_SCROLL = 20,
        UPGRADE_ITEM_RARITY_EQ_SCROLL = 21,
        DECONSTRUCTION_ITEM_TO_MATERIAL = 23, // not supported on official server, but it's in the client
        CELLON_UPGRADING = 24, // not supported on official server, but it's in the client
        UPGRADE_SP_BLUE_SCROLL = 25,
        UPGRADE_SP_RED_SCROLL = 26,
        CRAFTING_RANDOM_ITEMS_RARITY = 27,
        CRAFTING_ITEMS = 28,
        RELIC_RESEARCH = 29,
        UNKNOWN_EXCHANGE = 30, // not used
        NOSBAZAAR = 32,
        CHICKEN_FREE_SCROLL = 35,
        FAMILY_UPGRADE = 37,
        PAJAMA_FREE_SCROLL = 38,
        SP_PERFECTION = 41,
        PIRATE_FREE_SCROLL = 42,
        UPGRADE_ITEM_PLUS_GOLD_EQ_SCROLL = 43,
        GOLDEN_SP_CARD_HOLDER = 44,
        FAIRY_UPGRADE_ZENAS = 50,
        FAIRY_UPGRADE_ERENIA = 51,
        FAIRY_UPGRADE_FERNON = 52,
        COSTUME_MERGE = 53,
        CLOUSE_UI = 99 // sending c_close 0 packet
    }
}