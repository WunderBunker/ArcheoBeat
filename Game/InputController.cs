using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//GESTIONNAIRE DES INPUTS
public class InputController
{
    public delegate void ZQSDEventHandler(Vector2 pDirection);
    public event ZQSDEventHandler ZQSDPressed;

    public delegate void ArrowEventHandler(Vector2 pDirection);
    public event ArrowEventHandler ArrowPressed;

    public delegate void ClickEventHandler(int pButton, Vector2 pPosition);
    public event ClickEventHandler Click;
    public delegate void UnClickEventHandler(int pButton, Vector2 pPosition);
    public event UnClickEventHandler UnClick;
    public delegate void EntryEventHandler();
    public event EntryEventHandler Entry;
    public delegate void F1EventHandler();
    public event F1EventHandler F1;

    public delegate void EchapEventHandler();
    public event EchapEventHandler Echap;
    public delegate void TabEventHandler();
    public event EchapEventHandler Tab;

    Window Window => ServiceLocator.Get<GameParameters>().Window;

    public void Update()
    {
        if (IsMouseButtonPressed(MouseButton.Left))
            Click?.Invoke(1, (GetMousePosition() - Window.Offset) / Window.Coef);
        if (IsMouseButtonPressed(MouseButton.Right))
            Click?.Invoke(2, (GetMousePosition() - Window.Offset) / Window.Coef);
        if (IsMouseButtonReleased(MouseButton.Left))
            UnClick?.Invoke(1, (GetMousePosition() - Window.Offset) / Window.Coef);
        if (IsMouseButtonReleased(MouseButton.Right))
            UnClick?.Invoke(2, (GetMousePosition() - Window.Offset) / Window.Coef);

        if (IsKeyPressed(KeyboardKey.W))
            ZQSDPressed?.Invoke(-Vector2.UnitY);
        if (IsKeyPressed(KeyboardKey.S))
            ZQSDPressed?.Invoke(Vector2.UnitY);
        if (IsKeyPressed(KeyboardKey.D))
            ZQSDPressed?.Invoke(Vector2.UnitX);
        if (IsKeyPressed(KeyboardKey.A))
            ZQSDPressed?.Invoke(-Vector2.UnitX);

        if (IsKeyPressed(KeyboardKey.Up))
            ArrowPressed?.Invoke(-Vector2.UnitY);
        if (IsKeyPressed(KeyboardKey.Down))
            ArrowPressed?.Invoke(Vector2.UnitY);
        if (IsKeyPressed(KeyboardKey.Right))
            ArrowPressed?.Invoke(Vector2.UnitX);
        if (IsKeyPressed(KeyboardKey.Left))
            ArrowPressed?.Invoke(-Vector2.UnitX);

        if (IsKeyPressed(KeyboardKey.Enter))
            Entry?.Invoke();
        if (IsKeyPressed(KeyboardKey.F1))
            F1?.Invoke();

        if (IsKeyReleased(KeyboardKey.Escape))
            Echap?.Invoke();
        if (IsKeyReleased(KeyboardKey.Tab))
            Tab?.Invoke();

    }

    public Vector2 GetDirectionInput()
    {
        Vector2 vDirection = Vector2.Zero;
        if (IsKeyDown(KeyboardKey.A)) vDirection += new Vector2(-1, 0);
        if (IsKeyDown(KeyboardKey.D)) vDirection += new Vector2(1, 0);
        if (IsKeyDown(KeyboardKey.W)) vDirection += new Vector2(0, -1);
        if (IsKeyDown(KeyboardKey.S)) vDirection += new Vector2(0, 1);
        return vDirection;
    }

    public void ResetSubscriptions()
    {
        ZQSDPressed = null;
        ArrowPressed = null;
        Click = null;
        UnClick = null;
        Entry = null;
        F1 = null;
        Echap = null;
    }
}