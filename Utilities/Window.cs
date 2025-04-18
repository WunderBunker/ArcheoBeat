using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

public struct Window
{
    public Vector2 Size;
    public RenderTexture2D Canvas;
    public float Coef => MathF.Min(GetScreenWidth() / Size.X, GetScreenHeight() / Size.Y);
    public Vector2 Offset => (new Vector2(GetRenderWidth(), GetRenderHeight()) - Size * Coef) / 2;
    public Rectangle ResizedScreen => new Rectangle(Offset, Size * Coef);

    public Window(int pWidth, int pHeight)
    {
        Size = new Vector2(pWidth, pHeight);
    }

    public void InitialiserWindow()
    {
        InitWindow((int)Size.X, (int)Size.Y, "Raylib Game");

        SetWindowSize((int)Size.X, (int)Size.Y);
        SetWindowState(ConfigFlags.ResizableWindow);

        Canvas = LoadRenderTexture((int)Size.X, (int)Size.Y);
        SetTextureFilter(Canvas.Texture, TextureFilter.Bilinear);
    }
}
