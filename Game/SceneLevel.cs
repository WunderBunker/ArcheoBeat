
using System.Numerics;
using static Raylib_cs.Raylib;

//SCENE DE NIVEAU
public class SceneLevel : Scene
{
    public bool IsInPause { get; protected set; }
    public bool IsInWinState { get; protected set; }

    public override void Load(string pLevelId = "")
    {
        ServiceLocator.Get<AudioManager>().SetIntroAndLoopMusic("audio/SongIntro.mp3", "audio/SongLoop.mp3");

        base.Load(pLevelId);
    }

    public override void UnLoad()
    {
        base.UnLoad();
    }

    public override void Update()
    {
        base.Update();

        ServiceLocator.Get<UIManager>().Update();
        if (IsInPause || IsInWinState) return;

        ServiceLocator.Get<PlayerManager>().Update();
        ServiceLocator.Get<MyCamera>().Target = ServiceLocator.Get<PlayerManager>().Player.Sprite.Position;
        ServiceLocator.Get<ScissorsManager>().Update();
        ServiceLocator.Get<MapManager>().Update();
        ServiceLocator.Get<BackGroundManager>().Update();
        ServiceLocator.Get<TempAnimManager>().Update();
        ServiceLocator.Get<AudioManager>().Update();
    }

    public override void Draw()
    {
        base.Draw();

        BeginMode2D(ServiceLocator.Get<MyCamera>().Camera);

        ServiceLocator.Get<BackGroundManager>().Draw();
        ServiceLocator.Get<MapManager>().Draw();
        Sprite.DrawOrderedSprites(0);
        ServiceLocator.Get<PlayerManager>().Draw();
        Sprite.DrawOrderedSprites(1);
        ServiceLocator.Get<ScissorsManager>().Draw();
        ServiceLocator.Get<TempAnimManager>().Draw();
        ServiceLocator.Get<BackGroundManager>().DrawFrontGround();

        EndMode2D();

        ServiceLocator.Get<UIManager>().Draw();
    }

    public void UnPause()
    {
        IsInPause = false;
        ServiceLocator.Get<UIManager>().ClosePausePannel();
    }

    protected override void LoadManagers()
    {
        base.LoadManagers();
        ServiceLocator.Register<BackGroundManager>();
        ServiceLocator.Register<PlayerManager>();
        ServiceLocator.Register<MapManager>();
        ServiceLocator.Register<ScissorsManager>();
        ServiceLocator.Register<TempAnimManager>();
        ServiceLocator.Register<UIManager>();
    }

    protected override void UnLoadManagers()
    {
        base.UnLoadManagers();
        ServiceLocator.UnRegister<BackGroundManager>();
        ServiceLocator.UnRegister<MapManager>();
        ServiceLocator.UnRegister<PlayerManager>();
        ServiceLocator.UnRegister<ScissorsManager>();
        ServiceLocator.UnRegister<TempAnimManager>();
        ServiceLocator.UnRegister<UIManager>();
    }

    protected override void InitBackGround()
    {
        base.InitBackGround();
        BackGroundManager vBGManager = ServiceLocator.Get<BackGroundManager>();
        vBGManager.AddBackGround("Images/background.png", 1, Vector2.Zero, Vector2.Zero, new(142, 98, 204));
        vBGManager.AddBackGround("Images/fog.png", 0.8f, Vector2.UnitX * 15, Vector2.Zero, new(142, 98, 204, 0.5f));
        vBGManager.AddBackGround("Images/background2.png", 0.8f, Vector2.Zero, Vector2.Zero, new(242, 94, 209));
        vBGManager.AddBackGround("Images/background2.png", 0.3f, Vector2.Zero, Vector2.One * 100, new(94, 175, 242));
        vBGManager.AddFrontGround("Images/FrontSky.png", -0.1f, Vector2.UnitX * 30, Vector2.One * 100, new(0.37f, 0.69f, 0.95f, 0.75f));
    }

    protected override void OnEchap()
    {
        base.OnEchap();
        if (!IsInWinState)
        {
            //Lancement du panneau de Pause
            IsInPause = !IsInPause;
            if (IsInPause) ServiceLocator.Get<UIManager>().LaunchPausePannel();
            else ServiceLocator.Get<UIManager>().ClosePausePannel();
        }
    }

    //Gestion de la victoire 
    public void WinLevel()
    {
        ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.WinJingleSound, 1f);
        //Lancement du panneau de victoire
        ServiceLocator.Get<UIManager>().LaunchWinPannel();
        ServiceLocator.Get<SceneManager>().UnblockNextLevel();
        ServiceLocator.Get<SceneManager>().SaveGame();
        IsInWinState = true;
    }
}
