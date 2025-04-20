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
            [-Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY, Vector2.UnitX, Vector2.UnitY]);
    public static MobSpecific L4 =
    new(
        [new Vector2(9, 0), new Vector2(15, 6), new Vector2(21, 0)],
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
        [new Vector2(1, 0), new Vector2(6, 0), new Vector2(9, 0), new Vector2(13, 0), new Vector2(3, 4), new Vector2(7, 4), new Vector2(10, 4), new Vector2(12, 4),
        new Vector2(2, 8), new Vector2(4, 8), new Vector2(7, 8), new Vector2(11, 8),new Vector2(5, 11), new Vector2(6, 11), new Vector2(8, 11), new Vector2(10, 11),
        new Vector2(1, 14), new Vector2(3, 14), new Vector2(6, 14), new Vector2(9, 14)],
    [Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, Vector2.UnitY, Vector2.UnitX, Vector2.UnitX,Vector2.UnitX, -Vector2.UnitY, Vector2.UnitY, Vector2.UnitX,Vector2.UnitX, -Vector2.UnitY,
    -Vector2.UnitX,Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY,-Vector2.UnitY,Vector2.UnitY, Vector2.UnitX, -Vector2.UnitX]
    );
    public static MobSpecific L8 =
    new(
        [new Vector2(1, 0),  new Vector2(9, 0),  new Vector2(3, 4),  new Vector2(10, 4), new Vector2(2, 8),  new Vector2(7, 8), new Vector2(5, 11), new Vector2(8, 11),
        new Vector2(1, 14), new Vector2(6, 14), ],
        [Vector2.UnitX,  Vector2.UnitY,  Vector2.UnitX, Vector2.UnitX, Vector2.UnitY,Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY,
        -Vector2.UnitY, Vector2.UnitX, ]
    );
}

