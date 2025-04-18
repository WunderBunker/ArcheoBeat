
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//SCENE DE MENU 
public class SceneMenu : Scene
{
    Vector2 _buttonSize = new Vector2(180, 50);
    LevelButton[] _buttons;

    Texture2D _rock;
    Rectangle _rockRect;
    Rectangle _rockRectLayout;

    Texture2D _littleRock;
    Rectangle _littleRockRect;

    Button _volumeButton;
    VolumeMenu _volumeMenu;
    TutoPannel _tutoPannel;

    string _titleText;
    float _titleScale;
    Vector2 _titlePos;

    public override void Load(string pLevelId = "")
    {
        ServiceLocator.Get<AudioManager>().SetLoopMusic("audio/SongMenu.mp3");

        base.Load(pLevelId);

        //Initialisation de l'image de rocher
        InitRockImage();
        InitLittleRockImage();

        //Calcul de la taille des voutons pour qu'ils rentrent tous dans le layout (moitier à gauche, moitier à droite)
        _buttonSize = new Vector2(
            _rockRectLayout.Size.X / 2 * 0.8f,
            _rockRectLayout.Size.Y / (ServiceLocator.Get<SceneManager>().Levels.Count / 2) * 0.8f);

        //Titre
        InitTitle();

        //Initialisation des boutons de sélection de niveaux
        InitSceneButtons();


        //Menu des parametres sonores
        _volumeButton = new Button(_rockRectLayout.Position + new Vector2(_rockRectLayout.Size.X, _rockRectLayout.Size.Y / 2 - 32), new Vector2(64, 64), "");
        _volumeButton.SetTextures(LoadTexture("images/AudioButtonPressed.png"), LoadTexture("images/AudioButtonUnpressed.png"));

        //Si la partie vient d'être lancée, affiche Tuto
        int vEnabledCount = 0;
        foreach (KeyValuePair<string, LevelInfo> lLI in ServiceLocator.Get<SceneManager>().Levels)
            if (!lLI.Value.IsBlocked) vEnabledCount++;
        if (vEnabledCount <= 1)
        {
            _tutoPannel = new();
            _tutoPannel.Quit += QuitTutoPannel;
        }

        //Souscription aux évènhement de click
        ServiceLocator.Get<InputController>().Click += OnClick;
        ServiceLocator.Get<InputController>().UnClick += OnUnClick;

    }

    public override void UnLoad()
    {
        base.UnLoad();
        //Désouscription aux évènhement de click
        ServiceLocator.Get<InputController>().Click -= OnClick;
        ServiceLocator.Get<InputController>().UnClick -= OnUnClick;
    }

    public override void Update()
    {
        base.Update();

        if (_volumeMenu != null)
        {
            _volumeMenu?.Update();
            return;
        }

        ServiceLocator.Get<MyCamera>().Target = ServiceLocator.Get<GameParameters>().Window.Size / 2;
        ServiceLocator.Get<BackGroundManager>().Update();
        ServiceLocator.Get<AudioManager>().Update();
    }

    public override void Draw()
    {
        base.Draw();

        ServiceLocator.Get<BackGroundManager>().Draw();

        //Rock
        DrawTexturePro(_rock, new Rectangle(Vector2.Zero, new Vector2(_rock.Width, _rock.Height)), _rockRect, Vector2.Zero, 0, Color.White);
        DrawTexturePro(_littleRock, new Rectangle(Vector2.Zero, new Vector2(_littleRock.Width, _littleRock.Height)), _littleRockRect, Vector2.Zero, 0, Color.White);

        //Titre
        DrawTextEx(UIManager.TitleFont, _titleText, _titlePos, UIManager.TitleFont.BaseSize * _titleScale, 1, new(0, 255, 255));

        //Buttons
        foreach (LevelButton lButton in _buttons)
            lButton.Button.DrawButton();

        _volumeButton.DrawButton();
        //Menu des parametres sonores
        _volumeMenu?.Draw();

        //Affiche Tuto
        _tutoPannel?.Draw();
    }

    protected override void LoadManagers()
    {
        ServiceLocator.Register<BackGroundManager>();
        ServiceLocator.Register<UIManager>();
    }

    protected override void UnLoadManagers()
    {
        ServiceLocator.UnRegister<BackGroundManager>();
        ServiceLocator.UnRegister<UIManager>();
    }

    protected override void InitBackGround()
    {
        base.InitBackGround();
        BackGroundManager vBGManager = ServiceLocator.Get<BackGroundManager>();
        vBGManager.AddBackGround("Images/background.png", 1, Vector2.Zero, Vector2.Zero, new(142, 98, 204));
        vBGManager.AddBackGround("Images/fog.png", 0.8f, Vector2.UnitX * 50, Vector2.Zero, new(142, 98, 204, 0.5f));
        vBGManager.AddBackGround("Images/MenuBG.png", 1, Vector2.Zero, Vector2.Zero, new(242, 94, 209));
        vBGManager.AddBackGround("Images/MenuBG.png", 1, Vector2.Zero, Vector2.One * 100, new(94, 175, 242));
        vBGManager.AddBackGround("Images/MenuBGBeam.png", 1, Vector2.Zero, Vector2.Zero, Color.White);
    }

    protected override void OnEchap()
    {
        base.OnEchap();
        ServiceLocator.Get<GameParameters>().ShouldClose = true;
    }

    //Event de click
    void OnClick(int pButton, Vector2 pPosition)
    {
        if (pButton != 1 || _volumeMenu != null || _tutoPannel != null) return;

        for (int lCptButton = 0; lCptButton < _buttons.Length; lCptButton++)
            _buttons[lCptButton].Button.ClickIfOnButton(pPosition);

        _volumeButton.ClickIfOnButton(pPosition);
    }

    //Event de relachement de click
    void OnUnClick(int pButton, Vector2 pPosition)
    {
        if (pButton != 1 || _volumeMenu != null || _tutoPannel != null) return;

        for (int lCptButton = 0; lCptButton < _buttons.Length; lCptButton++)
        {
            if (_buttons[lCptButton].Button.UnClickIfOnButton(pPosition))
            {
                ServiceLocator.Get<SceneManager>().LoadScene<SceneLevel>(_buttons[lCptButton].LevelId);
                return;
            }
        }

        if (_volumeButton.UnClickIfOnButton(pPosition))
        {
            _volumeMenu = new();
            _volumeMenu.Quit += QuitVolumeMenu;
        }
    }
    void QuitVolumeMenu()
    {
        _volumeMenu.Quit -= QuitVolumeMenu;
        _volumeMenu = null;
    }
    void QuitTutoPannel()
    {
        _tutoPannel.Quit -= QuitTutoPannel;
        _tutoPannel = null;
    }

    //Initialisation des boutons de sélection de niveaux
    void InitSceneButtons()
    {
        //Premier bouton sera en hut à guache du layout (+ un petit spacing)
        Vector2 vOriginPosition = _rockRectLayout.Position + new Vector2(10, 10);

        //Ecart vertical entre bouton
        float vSpacing = 10;

        int vCptSceneInfo = 0;

        _buttons = new LevelButton[ServiceLocator.Get<SceneManager>().Levels.Count];
        foreach (KeyValuePair<string, LevelInfo> lSceneInfo in ServiceLocator.Get<SceneManager>().Levels)
        {
            Vector2 lPosition = vOriginPosition;
            //Impairs à gauche
            if (vCptSceneInfo % 2 == 0)
                lPosition += new Vector2(0, (vSpacing + _buttonSize.Y) * vCptSceneInfo / 2);
            //Pairs à droite
            else
                lPosition += new Vector2(_rockRectLayout.Size.X / 2, (vSpacing + _buttonSize.Y) * MathF.Floor(vCptSceneInfo / 2));

            _buttons[vCptSceneInfo] = new LevelButton(lPosition, _buttonSize, lSceneInfo.Value.LevelID, lSceneInfo.Key);
            _buttons[vCptSceneInfo].Button.TextColor = new(255, 70, 0);
            _buttons[vCptSceneInfo].Button.IsEnabled = !lSceneInfo.Value.IsBlocked;
            vCptSceneInfo++;
        }
    }

    void InitRockImage()
    {
        var vWindow = ServiceLocator.Get<GameParameters>().Window;
        //Images du rock
        _rock = LoadTexture("images/menuRock.png");
        _rockRect = new Rectangle(Vector2.Zero, new Vector2(_rock.Width, _rock.Height));
        _rockRect.Size *= Tools.GetScaleRectSizeInParent_InWhole(_rockRect.Size, vWindow.Size) * 0.95f;
        _rockRect.Position = Vector2.UnitX * (vWindow.Size.X - _rockRect.Size.X);

        //Zone de disposition des boutons sur le rock
        _rockRectLayout = _rockRect;
        _rockRectLayout.Size = _rockRect.Size * new Vector2(0.63f, 0.44f);
        _rockRectLayout.Position += new Vector2(82, 181);
    }

    void InitLittleRockImage()
    {
        //Images du rock
        _littleRock = LoadTexture("images/menuLittleRock.png");
        _littleRockRect = new Rectangle(Vector2.Zero, new Vector2(_littleRock.Width, _littleRock.Height));
        //Même hauteur que le grand rock
        _littleRockRect.Size *= _rockRect.Size.Y / _littleRockRect.Size.Y * 0.9f;
        _littleRockRect.Position = new Vector2(_rockRect.Position.X - _littleRockRect.Size.X * 0.85f, _rockRect.Position.Y + _rockRect.Size.Y / 2 - _littleRockRect.Size.Y / 2 + 50);
    }

    void InitTitle()
    {
        _titleText = "A  B\nr  e\nc  a\nh  t\ne\no";
        _titleScale = 1.2f;
        _titlePos = _littleRockRect.Position + _littleRockRect.Size / 2
            - MeasureTextEx(UIManager.TitleFont, _titleText, UIManager.TitleFont.BaseSize, 1) * _titleScale / 2
            + new Vector2(10, 10);
    }
}

//BOUTON DE CHARGEMENT DE NIVEAU
public struct LevelButton
{
    public Button Button;
    public string LevelId;

    public LevelButton(Vector2 pPosition, Vector2 pSize, string pName, string pLevelId)
    {
        Button = new(pPosition, pSize, pName);
        LevelId = pLevelId;
    }
}
