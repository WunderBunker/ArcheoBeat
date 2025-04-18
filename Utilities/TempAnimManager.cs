using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//GESTION DES ANIMATIONS TEMPORAIRES (FX)
public class TempAnimManager
{
    List<Anim> Anims = new List<Anim>();

    public void Update()
    {
        //On maj chquae animation et on supprime celles qui sont finies
        for (int lCptAnim = Anims.Count - 1; lCptAnim >= 0; lCptAnim--)
        {
            Anims[lCptAnim].Update();
            if (Anims[lCptAnim].IsFinished) Anims.RemoveAt(lCptAnim);
        }
    }

    public void Draw()
    {
        //On dessine chaque animation
        foreach (Anim lAnim in Anims)
            lAnim.Draw();
    }

    //Creation d'une animation temporaire
    public Anim CreateTempAnim(string pImagePath, Vector2 pPosition, int pNbFrames)
    {
        Anim vNewAnim = new Anim(pImagePath, pPosition, pNbFrames);
        Anims.Add(vNewAnim);
        return vNewAnim;
    }
    public Anim CreateTempAnim(Texture2D pTexture, Vector2 pPosition, int pNbFrames)
    {
        Anim vNewAnim = new Anim(pTexture, pPosition, pNbFrames);
        Anims.Add(vNewAnim);
        return vNewAnim;
    }
}

//ANIMATION TEMPORAIRE
public class Anim : Sprite
{
    public bool IsFinished;

    public Anim(Texture2D pTexture, Vector2 pPosition, int pNbFrames) : base(pTexture, pPosition, pNbFrames)
    { }
    public Anim(string pImagePath, Vector2 pPosition, int pNbFrames) : base(pImagePath, pPosition, pNbFrames)
    { }

    //On override le update pour ne pas boucler l'animation contrairement à un sprite classique
    public new void Update()
    {
        if (_nbFrames == 1) return;

        _frameCounter += AnimSpeed * GetFrameTime();
        if (_frameCounter >= _nbFrames)
        {
            _frameCounter = 0;
            IsFinished = true;
            return;
        }

        CurrentFrame = (int)Math.Floor(_frameCounter);
    }
}