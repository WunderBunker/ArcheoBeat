using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

// GESTION DES IMAGES DE FOND
public class BackGroundManager
{

    List<BackGround> BackGrounds = new List<BackGround>();

    protected Vector2 _camTargetPosition;

    public BackGroundManager()
    {
        //On enregistre la position de la camera pour futur calcul du parallax
        _camTargetPosition = ServiceLocator.Get<MyCamera>().Target;
    }

    public void Update()
    {
        //On récupère la nouvelle position de la caméra et son delta avec la précédente
        Vector2 vNewCamTargetPosition = ServiceLocator.Get<MyCamera>().Target;
        Vector2 vCamMove = vNewCamTargetPosition - _camTargetPosition;
        
        //On update la posiiton de chaque niveau de fond 
        foreach (BackGround lBackGround in BackGrounds)
        {
            //On actualise la position en fonction du déplacmeent de la caméra et du parallax
            foreach (Sprite lSprite in lBackGround.Sprites)
                lSprite.Position += lBackGround.Speed * GetFrameTime() + vCamMove * lBackGround.Parallax;


            //On recentre les différents pannels constitutifs de ce niveau de fond  si besoin  :
            GameParameters vGP = ServiceLocator.Get<GameParameters>();
            //Pannel gauche en face de la caméra
            if (lBackGround.Sprites[0].Position.X >= vNewCamTargetPosition.X - vGP.Window.Size.X / 2
            //Ou pannel droit en face de la caméra : on recentre
            || lBackGround.Sprites[1].Position.X <= vNewCamTargetPosition.X - vGP.Window.Size.X / 2)
            {
                lBackGround.Sprites[0].Position = new Vector2(vNewCamTargetPosition.X - vGP.Window.Size.X / 2 - lBackGround.Sprites[0].Size.X,
                    lBackGround.Sprites[0].Position.Y);
                lBackGround.Sprites[1].Position = new Vector2(vNewCamTargetPosition.X - vGP.Window.Size.X / 2,
                    lBackGround.Sprites[1].Position.Y);
                lBackGround.Sprites[2].Position = new Vector2(vNewCamTargetPosition.X - vGP.Window.Size.X / 2 - lBackGround.Sprites[2].Size.X,
                    lBackGround.Sprites[1].Position.Y);
                lBackGround.Sprites[3].Position = new Vector2(vNewCamTargetPosition.X - vGP.Window.Size.X / 2,
                    lBackGround.Sprites[3].Position.Y);
            }

            //Pannel haut en face de la caméra
            if (lBackGround.Sprites[2].Position.Y >= vNewCamTargetPosition.Y - vGP.Window.Size.Y / 2
            //Ou pannel bas en face de la caméra : on recentre
            || lBackGround.Sprites[0].Position.Y <= vNewCamTargetPosition.Y - vGP.Window.Size.Y / 2)
            {
                lBackGround.Sprites[0].Position = new Vector2(lBackGround.Sprites[0].Position.X, vNewCamTargetPosition.Y - vGP.Window.Size.Y / 2);
                lBackGround.Sprites[1].Position = new Vector2(lBackGround.Sprites[1].Position.X, vNewCamTargetPosition.Y - vGP.Window.Size.Y / 2);
                lBackGround.Sprites[2].Position = new Vector2(lBackGround.Sprites[2].Position.X, vNewCamTargetPosition.Y - vGP.Window.Size.Y / 2 - lBackGround.Sprites[2].Size.Y);
                lBackGround.Sprites[3].Position = new Vector2(lBackGround.Sprites[3].Position.X, vNewCamTargetPosition.Y - vGP.Window.Size.Y / 2 - lBackGround.Sprites[3].Size.Y);
            }

        }

        //On actualise la dernière position de la caméra
        _camTargetPosition = vNewCamTargetPosition;
    }

    public void Draw()
    {
        //Affichage de chaque pannel de chaque fond
        foreach (BackGround lBackGround in BackGrounds)
            foreach (Sprite lSprite in lBackGround.Sprites)
                lSprite.Draw();
    }

    public void AddBackGround(string pImagePath, float pParallax, Vector2 pSpeed, Vector2 pOffset, Color pColor)
    {
        BackGround vBackGround = new BackGround(pImagePath, pParallax, pSpeed, pOffset);
        vBackGround.Color = pColor;
        BackGrounds.Add(vBackGround);
    }
}

// IMAGE DE FOND
public class BackGround
{
    public Vector2 Speed;
    public float Parallax;
    public Vector2 Offset;
    public Sprite[] Sprites = new Sprite[4];

    //La couleur commune aux qautre panneaux consitutifs du fond
    public Color? Color
    {
        get
        {
            return Sprites[0].Color;
        }
        set
        {
            foreach (Sprite lSprite in Sprites)
                lSprite.Color = value != null ? (Color)value : Raylib_cs.Color.White;
        }
    }


    public BackGround(string pImagePath, float pParallax, Vector2 pSpeed, Vector2? pOffset = null)
    {
        Speed = pSpeed;
        Parallax = pParallax;

        //Creation d'un sprite pour chauqe pannel (haut-gauche, haut-droit, bas-gauche, bas-droit)
        Sprites[0] = new Sprite(pImagePath, new Vector2(0, 0));
        Sprites[1] = new Sprite(pImagePath, new Vector2(0, 0));
        Sprites[2] = new Sprite(pImagePath, new Vector2(0, 0));
        Sprites[3] = new Sprite(pImagePath, new Vector2(0, 0));

        //On ajuste le scale pour que chaque prenne tout l'écran tout en respectant le ratio de l'image
        Vector2 vScale = Vector2.One * Tools.GetScaleRectSizeInParent_CoverAll(Sprites[0].Size, ServiceLocator.Get<GameParameters>().Window.Size);
        //Vector2 vScale = ServiceLocator.Get<GameParameters>().CurrentWindow.Size/Sprites[0].Size;
        foreach (Sprite lSprite in Sprites)
            lSprite.Scale = vScale;

        //On positionne chaque pannel en les centrant (haut-gauche, haut-droit, bas-gauche, bas-droit)
        GameParameters vGP = ServiceLocator.Get<GameParameters>();
        Sprites[0].Position = vGP.Window.Size/2 - new Vector2(vGP.Window.Size.X / 2 + Sprites[0].Size.X,
            vGP.Window.Size.Y / 2);
        Sprites[1].Position = vGP.Window.Size/2 - new Vector2(vGP.Window.Size.X / 2,
            vGP.Window.Size.Y / 2);
        Sprites[2].Position = vGP.Window.Size/2 - new Vector2(vGP.Window.Size.X / 2 + Sprites[0].Size.X,
            vGP.Window.Size.Y / 2 - Sprites[2].Size.Y);
        Sprites[3].Position = vGP.Window.Size/2 - new Vector2(vGP.Window.Size.X / 2,
            vGP.Window.Size.Y / 2 - Sprites[3].Size.Y);

        //Si le fond a un offset, on l'applique à la position de chaque pannel
        if (pOffset == null) pOffset = Vector2.Zero;
        Offset = (Vector2)pOffset;
        foreach (Sprite lSprite in Sprites)
            lSprite.Position += (Vector2)pOffset;
    }

}
