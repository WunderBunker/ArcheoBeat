using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//SPRITE (ANIME OU NON)
public class Sprite : IListenable
{
    public Vector2 Position { get; set; }
    public int CurrentFrame = 0;
    public float AnimSpeed = 1;
    public Vector2 Size
    {
        get;
        protected set;
    }
    private Vector2 _scale = Vector2.One;
    public Vector2 Scale
    {
        get => _scale;
        set
        {
            //On scale la size de façon a avoir toujours la taille réelle du sprite
            Size = Size / _scale;
            Size = Size * value;

            _scale = value;
        }
    }

    public float Angle = 0;
    public Vector2 Origine = Vector2.Zero;
    public Color Color = Color.White;

    protected Action[] _callBacks;

    protected Texture2D _texture;
    protected float _nbFrames;
    protected float _frameCounter;
    protected Rectangle[] _frames;

    static List<Sprite>[] _orderedSprites = new List<Sprite>[4];

    public static void AddOrderedSprite(Sprite pSprite, int pOrder)
    {
        if (_orderedSprites[pOrder] == null || pSprite == null) _orderedSprites[pOrder] = new();
        _orderedSprites[pOrder].Add(pSprite);
    }

    public static void RemoveOrderedSprite(Sprite pSprite, int pOrder)
    {
        if (_orderedSprites[pOrder] == null) return;
        int vIndex = _orderedSprites[pOrder].FindIndex(_ => _ == pSprite);
        if (vIndex != -1) _orderedSprites[pOrder].RemoveAt(vIndex);
    }

    public static void DrawOrderedSprites(int pOrder)
    {
        foreach (Sprite lSprite in _orderedSprites[pOrder]) lSprite.Draw();
    }

    public static void CleanOrderedSprites()
    {
        _orderedSprites = new List<Sprite>[4];
    }

    public Sprite(string pImagePath, Vector2 pPosition, int pNbFrames = 1, float pSpeed = 1) : this(LoadTexture(pImagePath), pPosition, pNbFrames, pSpeed) { }
    public Sprite(Texture2D pTexture, Vector2 pPosition, int pNbFrames = 1, float pSpeed = 1)
    {
        //On Construit le sprite avec les paramètres de position, texture et animation
        Position = pPosition;
        _nbFrames = pNbFrames;
        _texture = pTexture;
        _frames = new Rectangle[pNbFrames];
        Size = new Vector2(_texture.Width / pNbFrames, _texture.Height);
        AnimSpeed = pSpeed;

        //On prédécoupe chaque frame de l'animation dans la texture
        for (int lCptFrame = 0; lCptFrame < pNbFrames; lCptFrame++)
            _frames[lCptFrame] = new Rectangle(Size.X * lCptFrame, 0, Size.X, Size.Y);
    }

    public void Update()
    {
        //On maj la frame en cours pour faire évoluer l'animation en boucle
        if (_nbFrames == 1) return;

        _frameCounter += AnimSpeed * GetFrameTime();
        if (_frameCounter >= _nbFrames) _frameCounter = 0;

        //Changement de Frame + on a des callbacks
        if (_callBacks != null && _callBacks.Length > 0 && CurrentFrame != (int)Math.Floor(_frameCounter))
        {
            //Si il y a un callback sur cette frame on l'éxécute
            if (_callBacks[(int)Math.Floor(_frameCounter)] != null)
                _callBacks[(int)Math.Floor(_frameCounter)]();
        }

        CurrentFrame = (int)Math.Floor(_frameCounter);
    }

    public void Draw()
    {
        //On dessine a frame en cours
        DrawTexturePro(_texture, _frames[CurrentFrame],
                    new Rectangle(Position.X, Position.Y, Size.X, Size.Y),
                    Origine, Angle, Color);
    }

    //Changement de l'animation du sprite
    public void ChangeAnim(string pImagePath, int pNbFrames = 1)
    {
        ChangeAnim(LoadTexture(pImagePath), pNbFrames);
    }
    public void ChangeAnim(Texture2D pTexture, int pNbFrames = 1)
    {
        _nbFrames = pNbFrames;
        _texture = pTexture;
        _frames = new Rectangle[pNbFrames];
        Size = new Vector2(_texture.Width / pNbFrames, _texture.Height);

        for (int lCptFrame = 0; lCptFrame < pNbFrames; lCptFrame++)
            _frames[lCptFrame] = new Rectangle(Size.X * lCptFrame, 0, Size.X, Size.Y);

        _callBacks = null;
        CurrentFrame = 0;
        AnimSpeed = 1;
    }

    //Ajout d'une fonction de callback à éxécuter sur une frame de l'animation
    public void AddCallBack(Action pCallBack, int pNoFrame)
    {
        _callBacks = new Action[_frames.Length];
        _callBacks[pNoFrame] = pCallBack;
    }
}


public interface IListenable
{
    public Vector2 Position { get; set; }

}