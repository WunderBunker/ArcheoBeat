using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

public class TempoAnim
{
    private int _nbFrames = 14;
    private float _frameCounter = 0;
    private int _currentFrame = 0;
    private Rectangle[] _frames;
    private Vector2 _frameSize;
    private Texture2D _texture;
    private float _speed;
    private Vector2 _position;
    private Color _color;

    public TempoAnim()
    {
        //Initialisation des données de l'animation de tempo
        _texture = LoadTexture("images/TempoAnim.png");
        _frames = new Rectangle[_nbFrames];
        _frameSize = new Vector2(_texture.Width / _nbFrames, _texture.Height);
        //On calle la vitesse de l'animation de façon à ce qu'elle dure pile un beat
        _speed = _nbFrames / ServiceLocator.Get<AudioManager>().MainTempo;
        //On décale le compteur pour que le centre l'animation tombe sur le beat
        _frameCounter = _nbFrames / 2 - 1;
        _currentFrame = (int)_frameCounter;

        for (int lCptFrame = 0; lCptFrame < _nbFrames; lCptFrame++)
            _frames[lCptFrame] = new Rectangle(_frameSize.X * lCptFrame, 0, _frameSize.X, _frameSize.Y);

        Window vCurrentWindow = ServiceLocator.Get<GameParameters>().Window;
        _position = new Vector2(vCurrentWindow.Size.X / 2, vCurrentWindow.Size.Y - _frameSize.Y / 2);
    }

    public void Update()
    {
        //Maj de l'animation du tempo
        _frameCounter += _speed;
        if (_frameCounter >= _nbFrames) _frameCounter = 0;

        _currentFrame = (int)Math.Floor(_frameCounter);
        
        float vFrameRatio = _currentFrame/((float)_nbFrames-1);
        _color = new Color(MathF.Sin(vFrameRatio*MathF.PI), MathF.Max(MathF.Cos(vFrameRatio*2*MathF.PI +MathF.PI),0), MathF.Sin(vFrameRatio*MathF.PI), 1);
    }

    public void Draw()
    {
        //Animation du tempo
        DrawTexturePro(_texture, _frames[_currentFrame],
                    new Rectangle(_position.X, _position.Y, _frameSize.X, _frameSize.Y),
                    _frameSize / 2, 0, _color);
    }

}