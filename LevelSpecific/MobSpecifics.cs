using System.Numerics;

public struct MobSpecific
{
    public Vector2[] ScissorsPosition;
    public Vector2[] ScissorsDirection;

    public MobSpecific() { }
    public MobSpecific(Vector2[] pScissorsPosition = null, Vector2[] pScissorsDirection = null)
    {
        ScissorsPosition = pScissorsPosition;
        ScissorsDirection = pScissorsDirection;
    }
}

public static class MobSpecifics
{
    public static MobSpecific L1 =
        new();
    public static MobSpecific L2 =
        new();
    public static MobSpecific L3 =
        new(
            [new Vector2(16, 10), new Vector2(7, 4), new Vector2(16, 13), new Vector2(7, 4), new Vector2(10, 0)],
            [-Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY,Vector2.UnitX, Vector2.UnitY]);
    public static MobSpecific L4 =
    new(
        [new Vector2(9, 0), new Vector2(15, 6), new Vector2(21,0)],
        [Vector2.UnitY, -Vector2.UnitY, Vector2.UnitY]
    );
    public static MobSpecific L5 =
    new(
        [new Vector2(3, 3), new Vector2(3, 10), new Vector2(10, 5)],
    [Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY]
    );
    public static MobSpecific L6 =
    new(
        [new Vector2(3, 3), new Vector2(3, 10), new Vector2(10, 5)],
    [Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY]
    );
    public static MobSpecific L7 =
    new(
        [new Vector2(3, 3), new Vector2(3, 10), new Vector2(10, 5)],
    [Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY]
    );
    public static MobSpecific L8 =
    new(
        [new Vector2(3, 3), new Vector2(3, 10), new Vector2(10, 5)],
    [Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY]
    );
}

