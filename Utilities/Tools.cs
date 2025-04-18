using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

public static class Tools
{
    public static void DrawTexctureAt(Texture2D pTexture, Vector2 pPosition, Vector2? pOrigine = null, Vector2? pScale = null, float pRotation = 0, Color? pColor = null)
    {
        if (pColor == null) pColor = Color.White;
        if (pScale == null) pScale = Vector2.One;
        if (pOrigine == null) pOrigine = new Vector2(pTexture.Width, pTexture.Height) / 2;
        Vector2 vScale = (Vector2)pScale;


        DrawTexturePro(pTexture, new Rectangle(0, 0, pTexture.Width, pTexture.Height),
            new Rectangle(pPosition.X, pPosition.Y, pTexture.Width * vScale.X, pTexture.Height * vScale.Y),
            (Vector2)pOrigine * vScale, pRotation, (Color)pColor);

    }

    //Projeté de Vector1 sur Vector2
    public static Vector2 Project(Vector2 pVector1, Vector2 pVector2)
    {
        Vector2 vProduit = pVector1 * pVector2;
        float vScalaire = vProduit.X + vProduit.Y;
        float vAmpV2 = pVector2.Length();

        if (vAmpV2 == 0) return Vector2.Zero;
        return pVector2 * vScalaire / (vAmpV2 * vAmpV2);
    }

    public static float InvLerp(float a, float b, float t)
    {
        return (t - a) / (b - a);
    }

    public static Vector2 FloorVector2(Vector2 pVector)
    {
        return new Vector2((float)Math.Floor(pVector.X), (float)Math.Floor(pVector.Y));
    }

    public static Vector2 RotateAround(Vector2 pVect, Vector2 pOrigine, float pAngle)
    {
        //translation vers le système de coordonnées de l'origine spécifiée
        Vector2 vOrigineToVect = pVect - pOrigine;

        //rotation
        Vector2 vRotatedVect = new Vector2(
            vOrigineToVect.X * MathF.Cos(pAngle * DEG2RAD) - vOrigineToVect.Y * MathF.Sin(pAngle * DEG2RAD),
            vOrigineToVect.X * MathF.Sin(pAngle * DEG2RAD) + vOrigineToVect.Y * MathF.Cos(pAngle * DEG2RAD)
        );

        // translation inversée vers le système de coordonnées de base
        return vRotatedVect + pOrigine;
    }

    public static float OutSine(float t)
    {
        return MathF.Sin(t * MathF.PI / 2);
    }

    public static float OutQuad(float t)
    {
        return t * (2 - t);
    }

    public static float InOutQuad(float t)
    {
        return (t < 0.5f)
          ? (2 * t * t)
          : (1 - MathF.Pow(-2 * t + 2, 2) / 2);
    }

    public static float InSine(float t)
    {
        return 1 - MathF.Cos(t * MathF.PI / 2);
    }

    public static float InExpo(float t)
    {
        return (t == 0)
          ? 0
          : MathF.Pow(2, 10 * (t - 1));
    }

    public static float OutExpo(float t)
    {
        return (t == 1)
          ? 1
          : 1 - MathF.Pow(2, -10 * t);
    }

    public static float GetScaleRectSizeInParent_InWhole(Vector2 pRectSize, Vector2 pParentRectSize)
    {
        float vScale = 1;
        if (pRectSize.X > pParentRectSize.X && pRectSize.Y > pParentRectSize.Y)
            vScale = MathF.Min(pParentRectSize.X / pRectSize.X, pParentRectSize.Y / pRectSize.Y);
        else if (pRectSize.X > pParentRectSize.X) vScale = pParentRectSize.X / pRectSize.X;
        else if (pRectSize.Y > pParentRectSize.Y) vScale = pParentRectSize.Y / pRectSize.Y;

        return vScale;
    }

    public static float GetScaleRectSizeInParent_CoverAll(Vector2 pRectSize, Vector2 pParentRectSize)
    {
        float vScale = 1;
        if (pRectSize.X > pParentRectSize.X && pRectSize.Y > pParentRectSize.Y)
            vScale = MathF.Max(pParentRectSize.X / pRectSize.X, pParentRectSize.Y / pRectSize.Y);
        else if (pRectSize.X < pParentRectSize.X && pRectSize.Y < pParentRectSize.Y)
            vScale = MathF.Max(pParentRectSize.X / pRectSize.X, pParentRectSize.Y / pRectSize.Y);
        else if (pRectSize.X < pParentRectSize.X) vScale = pParentRectSize.X / pRectSize.X;
        else if (pRectSize.Y < pParentRectSize.Y) vScale = pParentRectSize.Y / pRectSize.Y;

        return vScale;
    }
}
