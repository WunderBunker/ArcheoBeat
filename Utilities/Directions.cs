public struct Direction
{
    int X;
    int Y;

    public Direction(int pX, int pY)
    {
        X = pX;
        Y = pY;
    }

    public static Direction Left => new(1, 0);
    public static Direction Right => new(-1, 0);
    public static Direction Top => new(0, -1);
    public static Direction Down => new(0, 1);

    //Surcharge des opérateurs
    public static Direction operator *(int pScalar, Direction pDirection)
        => new(pScalar * pDirection.X, pScalar * pDirection.Y);
    public static Direction operator *(Direction pDirection, int pScalar)
        => new(pScalar * pDirection.X, pScalar * pDirection.Y);
    public static Direction operator *(Direction pDirection1, Direction pDirection2)
        => new(pDirection1.X * pDirection2.X, pDirection1.Y * pDirection2.Y);
    public static Direction operator +(Direction pDirection1, Direction pDirection2)
        => new(pDirection1.X + pDirection2.X, pDirection1.Y + pDirection2.Y);
    public static Direction operator -(Direction pDirection1, Direction pDirection2)
        => new(pDirection1.X - pDirection2.X, pDirection1.Y - pDirection2.Y);
    public static Direction operator /(Direction pDirection1, Direction pDirection2)
        => new(pDirection1.X / pDirection2.X, pDirection1.Y / pDirection2.Y);
    public static Direction operator /(Direction pDirection, int pScalar)
        => new(pDirection.X / pScalar, pDirection.Y / pScalar);

    public static bool operator ==(Direction pDirection1, Direction pDirection2)
        => pDirection1.Equals(pDirection2);
    public static bool operator !=(Direction pDirection1, Direction pDirection2)
        => !pDirection1.Equals(pDirection2);

    //Override des propriétés de la classe bojet
    public override bool Equals(object pOtherObject)
        => pOtherObject is Direction pOtherDirection && pOtherDirection.X == X && pOtherDirection.Y == Y;
    public override int GetHashCode() => base.GetHashCode();
}