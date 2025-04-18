using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//GESTION DES UIs
public class UIManager : IDisposable
{
    static Font _titleFont;
    public static Font TitleFont
    {
        get
        {
            if (!IsFontValid(_titleFont)) _titleFont = LoadFontEx("fonts/WarriotRound-Regular.ttf", 50, null, 250);
            return _titleFont;
        }
    }
    static Font _MainFont;
    public static Font MainFont
    {
        get
        {
            if (!IsFontValid(_MainFont)) _MainFont = LoadFontEx("fonts/MERAKI.otf", 50, null, 250);
            return _MainFont;
        }
    }
    static Font _DebugFont;
    static public Font DebugFont
    {
        get
        {
            if (!IsFontValid(_DebugFont)) _DebugFont = LoadFontEx("fonts/Layn.otf", 25, null, 250);
            return _DebugFont;
        }
    }

    public FadePannel FadePannel { get; private set; }
    public GunUI GunUI { get; private set; }
    public LifeUI LifeUI { get; private set; }

    PausePannel _pausePannel;
    WinPannel _winPannel;
    TempoAnim _tempoAnim;

    public UIManager()
    {
        //Animation du Beat
        if (ServiceLocator.Get<SceneManager>().CurrentScene.GetType().IsAssignableFrom(typeof(SceneLevel)))
        {
            _tempoAnim = new();
            GunUI = new();
            LifeUI = new();
        }

        FadePannel = new(new Rectangle(Vector2.Zero, ServiceLocator.Get<GameParameters>().Window.Size), Color.Black);
        FadePannel.StartFadeOut();
    }

    public void Update()
    {
        //Maj pause menu
        if (_pausePannel != null)
        {
            _pausePannel.Update();
            return;
        }

        //Maj de l'animation du tempo
        _tempoAnim?.Update();

        //Maj fondu
        FadePannel.Update();

        return;
    }

    public void Draw()
    {
        //Animation du tempo
        _tempoAnim?.Draw();
        GunUI?.Draw();
        LifeUI?.Draw();

        //Fade Pannel
        FadePannel.Draw();

        //Pause Pannel
        _pausePannel?.Draw();

        //Win Pannel    
        if (((SceneLevel)ServiceLocator.Get<SceneManager>().CurrentScene).IsInWinState) _winPannel.Draw();

        //Données de débug
        if (ServiceLocator.Get<SceneManager>().CurrentScene.DebugMode)
        {
            Vector2 vMouseScreenPosition = GetMousePosition();
            Vector2 vMouseWorldPosition = vMouseScreenPosition;

            string vText = "Mouse : " + vMouseScreenPosition.X + "," + vMouseScreenPosition.Y + "\n" + vMouseWorldPosition.X + "," + vMouseWorldPosition.Y;
            DrawTextEx(DebugFont, vText, vMouseWorldPosition, DebugFont.BaseSize, 1, Color.Yellow);

            Vector2 vGridCell = Grid.GetCellFromPosition(vMouseWorldPosition);
            vText = "Cell : " + vGridCell.X + "," + vGridCell.Y;
            DrawTextEx(DebugFont, vText, vMouseWorldPosition + Vector2.UnitY * 75, DebugFont.BaseSize, 1, Color.Green);

            Vector2 vBlock = Grid.GetBlockIndexesFromCell(vGridCell);
            vText = "Block : " + vBlock.X + "," + vBlock.Y;
            DrawTextEx(DebugFont, vText, vMouseWorldPosition + Vector2.UnitY * 100, DebugFont.BaseSize, 1, Color.Green);

            int vCellValue = ServiceLocator.Get<MapManager>().GetCellvalue(vGridCell);
            vText = "Cell Value : " + vCellValue;
            DrawTextEx(DebugFont, vText, vMouseWorldPosition + Vector2.UnitY * 125, DebugFont.BaseSize, 1, Color.Green);
        }
    }

    public void LaunchPausePannel()
    {
        _pausePannel = new();
        _pausePannel.Quit += Unpause;
    }
    public void ClosePausePannel()
    {
        _pausePannel.Quit -= Unpause;
        _pausePannel.Dispose();
        _pausePannel = null;
    }

    void Unpause(){((SceneLevel)ServiceLocator.Get<SceneManager>().CurrentScene).UnPause();}

    public void LaunchWinPannel()
    {
        _winPannel = new();
    }

    public void Dispose()
    {
        FadePannel.Dispose();
        _winPannel?.Dispose();
    }
}
