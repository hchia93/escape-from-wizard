namespace EscapeFromWizard
{
    public enum TileType
    {
        WALL_STONE1 = 01,
        WALL_STONE2 = 02,
        WALL_STONE3 = 03,
        WALL_STONE4 = 04,
        WALL_DIRT1 = 06,
        WALL_DIRT2 = 07,
        WALL_DIRT3 = 08,
        WALL_DIRT4 = 09,
        WALL_SAND1 = 11,
        WALL_SAND2 = 12,
        WALL_SAND3 = 13,
        WALL_SAND4 = 14,
        WALL_SNOW1 = 16,
        WALL_SNOW2 = 17,
        WALL_SNOW3 = 18,
        WALL_SNOW4 = 19,

        BOX1 = 21,
        BOX2 = 22,
        BOX3 = 23,

        RED_BRICK = 24,

        METAL1 = 26,
        METAL2 = 27,
        METAL3 = 28,

        GRASS = 29,

        WATER1 = 30,
        WATER2 = 31,
        WATER3 = 35,
        WATER4 = 36,

        WATER_TL = 32,
        WATER_T = 33,
        WATER_TR = 34,
        WATER_ML = 37,
        WATER_M = 38,
        WATER_MR = 39,
        WATER_BL = 42,
        WATER_B = 43,
        WATER_BR = 44,

        PATH = 00,
        HIDE_TILE = 05,
        START_TILE = 10,
        EXIT_SIGN = 15,
        LEFT_SIGN = 20,
        RIGHT_SIGN = 25,
        DUNGEON = 40,
        FENCE = 41,
        WEIGHT = 45,
    }

    public enum SpellItems
    {
        STAR = 9,
        HP_POTION = 21,
        QUEST_POTION = 22,
        UNLOOTED_QUEST_ITEM = 24,
    }

    public enum LootItemType
    {
        STAR,
        QUEST_POTION,
        RED_KEY,
        YELLOW_KEY,
        BLUE_KEY,
        GREEN_KEY,
    }

    public enum Color
    {
        RED = 0,
        YELLOW = 1,
        GREEN = 2,
        BLUE = 3,
    }

    public enum ObjectIcon
    {
        RED_LOCK = 15,
        YELLOW_LOCK = 16,
        BLUE_LOCK = 17,
        GREEN_LOCK = 18,
    }

    public enum HUDIcon
    {
        RED_KEY_TAKEN = 17,
        YELLOW_KEY_TAKEN = 18,
        BLUE_KEY_TAKEN = 15,
        GREEN_KEY_TAKEN = 21,
        
        RED_KEY_NONE = 22,
        YELLOW_KEY_NONE = 23,
        BLUE_KEY_NONE = 20,
        GREEN_KEY_NONE = 16,

        EMPTY_HEART = 10,
        FULL_HEART = 11,
        HALF_HEART = 12,

    }

    public enum MovingObject
    {
        WIZARD = 14,
        GHOST_FACING_LEFT = 19,
        GHOST_FACING_RIGHT = 20,
    }
}
