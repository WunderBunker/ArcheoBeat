using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//BOUTON 
public struct Button
{
    public static Texture2D ButtonUp = LoadTexture("images/ButtonUnPressed.png");
    Texture2D _buttonUp = ButtonUp;
    public static Texture2D ButtonDown = LoadTexture("images/ButtonPressed.png");
    Texture2D _buttonDown = ButtonDown;
    public Rectangle Rectangle { get; private set; }
    public bool IsDown { get; private set; }
    public string Text { get; private set; }

    public Vector2 TextPosition { get; private set; }
    float _textScale;
    public float TextScale
    {
        get => _textScale;
        set
        {
            _textScale = value;
            TextPosition = Rectangle.Position + Rectangle.Size / 2 - MeasureTextEx(Font, Text, Font.BaseSize, 1) * TextScale / 2;
        }
    }

    public Color TextColor = Color.White;
    Color _buttonColor = Color.White;
    bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled; set
        {
            _isEnabled = value;
            if (!value) _buttonColor = Color.Gray;
            else _buttonColor = Color.White;
        }
    }
    public Font Font;

    public Button(Vector2 pPosition, Vector2 pSize, string pText)
    {
        Rectangle = new Rectangle(pPosition, pSize);
        Text = pText;
        Font = UIManager.MainFont;
        Vector2 vTextSize = MeasureTextEx(Font, Text, Font.BaseSize, 1);
        TextScale = Tools.GetScaleRectSizeInParent_InWhole(vTextSize, Rectangle.Size) * 0.8f;
        TextPosition = Rectangle.Position + Rectangle.Size / 2 - vTextSize * TextScale / 2 + Vector2.UnitY * (IsDown ? 5 : 0);
    }

    public void SetTextures(Texture2D pTexturePressed, Texture2D pTextureUnPressed)
    {
        _buttonDown = pTexturePressed;
        _buttonUp = pTextureUnPressed;
    }

    public bool ClickIfOnButton(Vector2 pPosition)
    {
        if(!IsEnabled) return false;

        if (CheckCollisionPointRec(pPosition, Rectangle))
        {
            IsDown = true;
            ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.ClickSound);
            return true;
        }
        return false;
    }
    public bool UnClickIfOnButton(Vector2 pPosition)
    {
        if(!IsEnabled) return false;

        bool vIsUnclick;
        if (IsDown && CheckCollisionPointRec(pPosition, Rectangle)) vIsUnclick = true;
        else vIsUnclick = false;

        IsDown = false;

        return vIsUnclick;
    }

    public void DrawButton()
    {
        //Dessin du bouton (en fonction de si clické ou non)
        if (IsDown)
            DrawTexturePro(_buttonDown, new Rectangle(Vector2.Zero, new Vector2(_buttonDown.Width, _buttonDown.Height)), Rectangle, Vector2.Zero, 0, _buttonColor);
        else
            DrawTexturePro(_buttonUp, new Rectangle(Vector2.Zero, new Vector2(_buttonUp.Width, _buttonUp.Height)), Rectangle, Vector2.Zero, 0, _buttonColor);

        //Texte du bouton
        DrawTextEx(Font, Text, TextPosition + Vector2.UnitY * (IsDown ? 5 : 0), Font.BaseSize * TextScale, 1, TextColor);
    }
}

//SLIDER
public class Slider
{
    public Rectangle SliderRect { get; private set; }
    Rectangle _handleRect;
    Rectangle _fillerRect;

    float _value;

    bool _isSelected;

    public Slider(Rectangle pSliderRect, float pValue)
    {
        _value = pValue;
        SliderRect = pSliderRect;
        _fillerRect = new Rectangle(SliderRect.Position, new Vector2(SliderRect.Size.X * pValue, SliderRect.Size.Y));
        _handleRect = new Rectangle(SliderRect.Position + new Vector2(SliderRect.Size.X * pValue, SliderRect.Size.Y / 2 - SliderRect.Size.Y * 1.1f / 2),
            new Vector2(SliderRect.Size.X / 50, SliderRect.Size.Y * 1.1f));
    }

    public void MoveSlider(Vector2 pDelta)
    {
        SliderRect = new Rectangle(SliderRect.Position + pDelta, SliderRect.Size);
        _fillerRect = new Rectangle(_fillerRect.Position + pDelta, _fillerRect.Size);
        _handleRect = new Rectangle(_handleRect.Position + pDelta, _handleRect.Size);
    }

    public void Draw()
    {
        DrawRectangleRec(SliderRect, Color.Gray);
        DrawRectangleRec(_fillerRect, Color.White);
        DrawRectangleRec(_handleRect, Color.Blue);
    }

    public void OnClick(Vector2 pPosition)
    {
        if (CheckCollisionPointRec(pPosition, _handleRect))
            _isSelected = true;
    }
    public void OnUnClick()
    {
        _isSelected = false;
    }

    public float? UpdateSliderValue()
    {
        if (!_isSelected) return null;

        Vector2 vNewPos = _handleRect.Position + GetMouseDelta().X * Vector2.UnitX;
        if (vNewPos.X > SliderRect.Position.X + SliderRect.Size.X) vNewPos.X = SliderRect.Position.X + SliderRect.Size.X;
        if (vNewPos.X < SliderRect.Position.X) vNewPos.X = SliderRect.Position.X;

        _handleRect.Position = vNewPos;

        _value = (_handleRect.Position.X - SliderRect.Position.X) / SliderRect.Size.X;
        _fillerRect.Size = new Vector2(SliderRect.Size.X * _value, _fillerRect.Size.Y);

        return _value;
    }

}

public class TutoPannel
{
    public event Action Quit;

    Rectangle _pannel;
    Texture2D _tutoTexture;
    float _textureScale;
    Vector2 _texturePos;
    Button _quitButton;

    public TutoPannel()
    {
        Window vWindow = ServiceLocator.Get<GameParameters>().Window;
        _pannel = new Rectangle(Vector2.Zero, new Vector2(vWindow.Size.X * 0.6f, vWindow.Size.Y * 0.6f));
        _pannel.Position = vWindow.Size / 2 - _pannel.Size / 2;

        _tutoTexture = LoadTexture("images/tutorial.png");
        _textureScale = Tools.GetScaleRectSizeInParent_InWhole(new Vector2(_tutoTexture.Width, _tutoTexture.Height), _pannel.Size) * 0.9f;
        _texturePos = _pannel.Position + _pannel.Size / 2 - new Vector2(_tutoTexture.Width, _tutoTexture.Height) * _textureScale / 2;

        _quitButton = new Button(_pannel.Position, new Vector2(40, 40), "X");

        ServiceLocator.Get<InputController>().Click += OnClick;
        ServiceLocator.Get<InputController>().UnClick += OnUnClick;
    }

    public void Draw()
    {
        DrawRectangleRec(_pannel, new(0, 0, 0, 0.75f));
        DrawTextureEx(_tutoTexture, _texturePos, 0, _textureScale, Color.White);
        _quitButton.DrawButton();
    }

    void OnClick(int pButton, Vector2 pPosition)
    {
        _quitButton.ClickIfOnButton(pPosition);
    }

    void OnUnClick(int pButton, Vector2 pPosition)
    {
        if (_quitButton.UnClickIfOnButton(pPosition))
        {
            Dispose();
            Quit?.Invoke();
            return;
        }
    }

    void Dispose()
    {
        ServiceLocator.Get<InputController>().Click -= OnClick;
        ServiceLocator.Get<InputController>().UnClick -= OnUnClick;
    }
}


public class VolumeMenu
{
    public event Action Quit;

    Rectangle _pannel;
    float _textScale = 0.75f;
    Slider _mainVolumeSlider;
    Slider _musicVolumeSlider;
    Slider _fxVolumeSlider;
    Button _quitButton;
    Font _font;
    AudioManager _audioManager;

    public VolumeMenu()
    {
        _font = UIManager.MainFont;
        _audioManager = ServiceLocator.Get<AudioManager>();

        Window vWindow = ServiceLocator.Get<GameParameters>().Window;
        _pannel = new Rectangle(Vector2.Zero, new Vector2(vWindow.Size.X * 2 / 3, vWindow.Size.X / 3));
        _pannel.Position = vWindow.Size / 2 - _pannel.Size / 2;

        _quitButton = new Button(_pannel.Position, new Vector2(40, 40), "X");

        _mainVolumeSlider = new Slider(new Rectangle(_pannel.Position + new Vector2(_pannel.Size.X / 2, 60), new Vector2(_pannel.Size.X * 0.8f, 20)), _audioManager.MainVolume);
        _mainVolumeSlider.MoveSlider(-new Vector2(_mainVolumeSlider.SliderRect.Size.X / 2, 0));
        _musicVolumeSlider = new Slider(new Rectangle(_pannel.Position + new Vector2(_pannel.Size.X / 2, 140), new Vector2(_pannel.Size.X * 0.8f, 20)), _audioManager.MusicVolume);
        _musicVolumeSlider.MoveSlider(-new Vector2(_musicVolumeSlider.SliderRect.Size.X / 2, 0));
        _fxVolumeSlider = new Slider(new Rectangle(_pannel.Position + new Vector2(_pannel.Size.X / 2, 220), new Vector2(_pannel.Size.X * 0.8f, 20)), _audioManager.FXVolume);
        _fxVolumeSlider.MoveSlider(-new Vector2(_fxVolumeSlider.SliderRect.Size.X / 2, 0));


        ServiceLocator.Get<InputController>().Click += OnClick;
        ServiceLocator.Get<InputController>().UnClick += OnUnClick;
    }

    public void Draw()
    {
        DrawRectangleRec(_pannel, new(0, 0, 0, 0.75f));

        _quitButton.DrawButton();

        DrawTextEx(_font, "Main Volume", _mainVolumeSlider.SliderRect.Position - 40 * Vector2.UnitY, _font.BaseSize * _textScale, 1, Color.White);
        _mainVolumeSlider.Draw();
        DrawTextEx(_font, "Music Volume", _musicVolumeSlider.SliderRect.Position - 40 * Vector2.UnitY, _font.BaseSize * _textScale, 1, Color.White);
        _musicVolumeSlider.Draw();
        DrawTextEx(_font, "FX Volume", _fxVolumeSlider.SliderRect.Position - 40 * Vector2.UnitY, _font.BaseSize * _textScale, 1, Color.White);
        _fxVolumeSlider.Draw();
    }

    public void Update()
    {
        float? vVolume;
        vVolume = _mainVolumeSlider.UpdateSliderValue();
        if (vVolume != null)
        {
            _audioManager.MainVolume = (float)vVolume;
            _audioManager.ChangeMusicVolume(_audioManager.MusicVolume);
        }
        vVolume = _musicVolumeSlider.UpdateSliderValue();
        if (vVolume != null) _audioManager.ChangeMusicVolume((float)vVolume);
        vVolume = _fxVolumeSlider.UpdateSliderValue();
        if (vVolume != null) _audioManager.FXVolume = (float)vVolume;
    }


    void OnClick(int pButton, Vector2 pPosition)
    {
        _quitButton.ClickIfOnButton(pPosition);

        _mainVolumeSlider.OnClick(pPosition);
        _musicVolumeSlider.OnClick(pPosition);
        _fxVolumeSlider.OnClick(pPosition);
    }
    void OnUnClick(int pButton, Vector2 pPosition)
    {
        if (_quitButton.UnClickIfOnButton(pPosition))
        {
            Dispose();
            Quit?.Invoke();
            return;
        }

        _mainVolumeSlider.OnUnClick();
        _musicVolumeSlider.OnUnClick();
        _fxVolumeSlider.OnUnClick();
    }

    void Dispose()
    {
        ServiceLocator.Get<InputController>().Click -= OnClick;
        ServiceLocator.Get<InputController>().UnClick -= OnUnClick;
    }
}

// PANNEAU DE FONDU
public class FadePannel
{
    public Rectangle Rect { get; private set; }
    public Color Color { get; private set; }
    public delegate void FadeEndEventHandle();
    public event FadeEndEventHandle FadeEnd;

    float _fadeTime = 1.5f;
    float _fadeTimer;
    bool _fadeOut;
    bool _fadeIn;

    public FadePannel(Rectangle pRect, Color pColor)
    {
        Rect = pRect;
        Color = pColor;
    }

    public void StartFadeIn()
    {
        _fadeIn = true;
        _fadeTimer = _fadeTime;
    }

    public void StartFadeOut()
    {
        _fadeOut = true;
        _fadeTimer = _fadeTime;
    }
    public void Update()
    {
        if (_fadeIn)
        {
            _fadeTimer -= GetFrameTime();
            if (_fadeTimer <= 0)
            {
                _fadeTimer = 0;
                _fadeIn = false;
                Color = new(0, 0, 0, 1);

                FadeEnd?.Invoke();
            }
            else Color = new(0, 0, 0, 1 - _fadeTimer / _fadeTime);
        }
        else if (_fadeOut)
        {
            _fadeTimer -= GetFrameTime();
            if (_fadeTimer <= 0)
            {
                _fadeTimer = 0;
                _fadeOut = false;
                Color = new(0, 0, 0, 0);
            }
            else Color = new(0, 0, 0, _fadeTimer / _fadeTime);
        }
    }

    public void Draw()
    {
        if (_fadeIn || _fadeOut)
        {
            DrawRectangleRec(new(Rect.Position, Rect.Size), Color);
        }
    }

    public void Dispose()
    {
        FadeEnd = null;
    }
}

//PANNEAU DE VICTOIRE
public class WinPannel
{
    Rectangle Pannel;
    Button MenuButton;
    Button NextLevelButton;

    Vector2 TitlePosition;
    string Title;

    public WinPannel()
    {
        Window vWindow = ServiceLocator.Get<GameParameters>().Window;
        Pannel = new Rectangle(Vector2.Zero, new Vector2(vWindow.Size.X * 2 / 3, vWindow.Size.X / 3));
        Pannel.Position = vWindow.Size / 2 - Pannel.Size / 2;

        Title = "Victory !!!";
        Vector2 vTitleSize = MeasureTextEx(UIManager.MainFont, Title, UIManager.MainFont.BaseSize, 1);
        TitlePosition = Pannel.Position + new Vector2(Pannel.Size.X / 2 - vTitleSize.X / 2, 30);

        Vector2 vButtonSize = Pannel.Size / 3;
        MenuButton = new(Pannel.Position + Pannel.Size / 2 + new Vector2(-vButtonSize.X - 20, 30), vButtonSize, "Menu");
        MenuButton.TextColor = new(200, 100, 0);
        NextLevelButton = new(Pannel.Position + Pannel.Size / 2 + new Vector2(20, 30), vButtonSize, "Next Level");
        NextLevelButton.TextColor = new(200, 100, 0);
        MenuButton.TextScale = MathF.Min(MenuButton.TextScale, NextLevelButton.TextScale);

        //Souscription aux évènhement de click
        ServiceLocator.Get<InputController>().Click += OnClick;
        ServiceLocator.Get<InputController>().UnClick += OnUnClick;
    }

    public void Draw()
    {
        DrawRectangleV(Pannel.Position, Pannel.Size, new(0, 0, 0, 0.75f));
        DrawTextEx(UIManager.MainFont, Title, TitlePosition, UIManager.MainFont.BaseSize, 1, new(0, 255, 255));
        MenuButton.DrawButton();
        NextLevelButton.DrawButton();
    }

    void OnClick(int pButton, Vector2 pPosition)
    {
        if (pButton != 1) return;

        MenuButton.ClickIfOnButton(pPosition);
        NextLevelButton.ClickIfOnButton(pPosition);
    }

    void OnUnClick(int pButton, Vector2 pPosition)
    {
        if (pButton != 1) return;

        if (MenuButton.UnClickIfOnButton(pPosition)) BackToMenu();
        if (NextLevelButton.UnClickIfOnButton(pPosition)) NextLevel();
    }

    void BackToMenu()
    {
        ServiceLocator.Get<SceneManager>().LoadScene<SceneMenu>();
    }
    void NextLevel()
    {
        ServiceLocator.Get<SceneManager>().LoadNextScene();
    }

    public void Dispose()
    {
        ServiceLocator.Get<InputController>().Click -= OnClick;
        ServiceLocator.Get<InputController>().UnClick -= OnUnClick;
    }
}

//PANNEAU DE PAUSE
public class PausePannel
{
    public event Action Quit;

    Rectangle _pannel;
    string _title;
    Vector2 _posTitle;
    Vector2 _sizeTitle;
    float _textScale = 0.75f;
    Font _font;

    Button _volumeButton;
    Button _backMenuButton;
    Button _quitButton;

    VolumeMenu _volumeMenu;

    public PausePannel()
    {

        _title = "PAUSE";
        _font = UIManager.MainFont;

        Window vWin = ServiceLocator.Get<GameParameters>().Window;
        _pannel = new Rectangle(vWin.Size / 2, vWin.Size / 2);
        _pannel.Position -= _pannel.Size / 2;

        _sizeTitle = MeasureTextEx(_font, _title, _font.BaseSize, 1) * _textScale;
        _posTitle = _pannel.Position + (_pannel.Size / 2 - _sizeTitle / 2) * Vector2.UnitX + 30 * Vector2.UnitY;

        Vector2 vButtonSize = _pannel.Size / 3;
        _volumeButton = new Button(_pannel.Position + _pannel.Size / 2 + new Vector2(-vButtonSize.X - 20, 30), vButtonSize, "Volume Options");
        _volumeButton.TextColor = new(200, 100, 0);
        _backMenuButton = new Button(_pannel.Position + _pannel.Size / 2 + new Vector2(20, 30), vButtonSize, "Back To Menu");
        _backMenuButton.TextColor = new(200, 100, 0);
        _backMenuButton.TextScale = MathF.Min(_backMenuButton.TextScale, _volumeButton.TextScale);
        _volumeButton.TextScale = _backMenuButton.TextScale;

        _quitButton = new Button(_pannel.Position, new Vector2(40, 40), "X");

        //Souscription aux évènhement de click
        ServiceLocator.Get<InputController>().Click += OnClick;
        ServiceLocator.Get<InputController>().UnClick += OnUnClick;
    }


    public void Update()
    {
        _volumeMenu?.Update();
    }

    public void Draw()
    {
        if (_volumeMenu != null) _volumeMenu.Draw();
        else
        {
            DrawRectangleRec(_pannel, new(0, 0, 0, 0.75f));
            DrawTextEx(_font, _title, _posTitle, _font.BaseSize * _textScale, 1, new(0, 255, 255));
            _volumeButton.DrawButton();
            _backMenuButton.DrawButton();
            _quitButton.DrawButton();
        }

    }

    void OnClick(int pButton, Vector2 pPosition)
    {
        Console.WriteLine("vho ");

        if (pButton != 1 || _volumeMenu != null) return;

        _quitButton.ClickIfOnButton(pPosition);
        _volumeButton.ClickIfOnButton(pPosition);
        _backMenuButton.ClickIfOnButton(pPosition);
    }

    void OnUnClick(int pButton, Vector2 pPosition)
    {
        if (pButton != 1 || _volumeMenu != null) return;

        if (_quitButton.UnClickIfOnButton(pPosition))
        {
            Quit?.Invoke();
            return;
        }

        if (_volumeButton.UnClickIfOnButton(pPosition))
        {
            _volumeMenu = new();
            _volumeMenu.Quit += QuitVolumeMenu;
        }
        if (_backMenuButton.UnClickIfOnButton(pPosition))
        {
            ServiceLocator.Get<SceneManager>().LoadScene<SceneMenu>();
        }
    }

    void QuitVolumeMenu()
    {
        _volumeMenu.Quit -= QuitVolumeMenu;
        _volumeMenu = null;
    }

    public void Dispose()
    {
        ServiceLocator.Get<InputController>().Click -= OnClick;
        ServiceLocator.Get<InputController>().UnClick -= OnUnClick;
        Quit = null;
    }
}

public class GunUI
{
    Texture2D _imageUp;
    Texture2D _imageDown;
    Vector2 _position;
    float _scale = 0.75f;
    bool _usable;

    public GunUI()
    {
        _imageUp = LoadTexture("images/gun.png");
        _imageDown = LoadTexture("images/gun_down.png");
        Window vWin = ServiceLocator.Get<GameParameters>().Window;
        _position = new Vector2(vWin.Size.X - _imageUp.Width * _scale, 0);
        _usable = true;
    }

    public void Draw()
    {
        DrawRectangleLinesEx(new Rectangle(_position, new Vector2(_imageUp.Width, _imageUp.Height) * _scale), 5, Color.Blue);
        DrawTextureEx(_usable ? _imageUp : _imageDown, _position, 0, _scale, new(1, 1, 1, 0.9f));
    }

    public void MakeGunUsable(bool pIsUsable)
    {
        _usable = pIsUsable;
    }
}

public class LifeUI
{
    int _maxLife;
    int _lifeNb;
    Texture2D _lightBulbOn;
    Texture2D _lightBulbOff;
    Vector2 _lifeBarPosition;

    public LifeUI()
    {
        _lightBulbOn = LoadTexture("images/lightBulbOn.png");
        _lightBulbOff = LoadTexture("images/lightBulbOff.png");
        _lifeBarPosition = Vector2.Zero;

        _maxLife = ServiceLocator.Get<PlayerManager>().Player.Life;
        _lifeNb = _maxLife;
    }

    public void RemoveLife()
    {
        _lifeNb--;
    }

    public void Draw()
    {
        for (int lCptLife = 0; lCptLife < _maxLife; lCptLife++)
        {
            Vector2 lPosition = _lifeBarPosition + Vector2.UnitX * _lightBulbOn.Width * lCptLife;

            if (lCptLife < _lifeNb) DrawTexture(_lightBulbOn, (int)lPosition.X, (int)lPosition.Y, Color.White);
            else DrawTexture(_lightBulbOff, (int)lPosition.X, (int)lPosition.Y, Color.White);
        }
    }

}