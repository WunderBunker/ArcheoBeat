using System.Numerics;
using System.Xml.Schema;
using Raylib_cs;
using static Raylib_cs.Raylib;

//GESTION DES SCISSORS
public class ScissorsManager : IDisposable
{
    private Scissor[] _scissors;
    private MobSpecific _mobSpecifics;

    public ScissorsManager()
    {
        //On récupère la data spécifique au niveau en cours et on initialise les ciseaux avec
        _mobSpecifics = (MobSpecific)typeof(MobSpecifics).GetField(ServiceLocator.Get<SceneManager>().CurrentScene.LevelId).GetValue(null);

        Vector2[] vScissorsPosition = _mobSpecifics.ScissorsPosition;
        Vector2[] vScissorsDirection = _mobSpecifics.ScissorsDirection;
        if (vScissorsPosition != null)
        {
            _scissors = new Scissor[vScissorsPosition.Length];
            for (int lCptScissors = 0; lCptScissors < vScissorsPosition.Length; lCptScissors++)
                _scissors[lCptScissors] = new Scissor(vScissorsPosition[lCptScissors], vScissorsDirection[lCptScissors]);
        }
        else _scissors = [null];

        ServiceLocator.Get<PlayerManager>().Player.ShootEvent += OnShoot;
    }

    public void Update()
    {
        //on maj chaque ciseau
        foreach (Scissor lScissor in _scissors)
        {
            //Un ciseau mort est un ciseau null, on le passe
            if (lScissor == null) continue;

            //Si il est en train de se déplacer on maj sa position
            if (lScissor.IsMoving) lScissor.UpdatePosition();
            //Maj de l'animation du sprite
            lScissor.Sprite.Update();
        }
    }

    public void Draw()
    {
        //On dessine chaque ciseau
        foreach (Scissor lScissor in _scissors)
        {
            //Un ciseau mort est un ciseau null, on le passe
            if (lScissor == null) continue;

            lScissor.Sprite.Draw();
        }
    }

    //Vérifie si un ciseau se trouve sur une cell
    public bool IsScissorInCell(Vector2 pCellInGrid)
    {
        foreach (Scissor lScissor in _scissors)
        {
            if (lScissor == null) continue;
            if (lScissor.CurrentCell == pCellInGrid) return true;
        }
        return false;
    }

    void OnShoot(Vector2[] pImpactedCells)
    {
        for (int lCptScissor = 0; lCptScissor < _scissors.Length; lCptScissor++)
            foreach (Vector2 lCell in pImpactedCells)
                if (_scissors[lCptScissor] != null && lCell == _scissors[lCptScissor].CurrentCell) KillScissor(lCptScissor);
    }

    //Vérifie si un ciseau se trouve sur une cell et le tue
    void KillScissor(int pIndex)
    {
        if (_scissors[pIndex] == null) return;

        _scissors[pIndex].Dispose();
        ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.ObstacleBreakSounds);
        Anim vExplosion = ServiceLocator.Get<TempAnimManager>().CreateTempAnim("images/ScissorExplodeAnim.png", Vector2.Zero, 4);
        vExplosion.AnimSpeed = 10;
        vExplosion.Angle = _scissors[pIndex].Sprite.Angle;
        vExplosion.Position = _scissors[pIndex].Sprite.Position
        + Tools.RotateAround(_scissors[pIndex].Sprite.Size / 2 - vExplosion.Size / 2, Vector2.Zero, vExplosion.Angle);
        _scissors[pIndex] = null;
    }

    public void Dispose()
    {
        try { ServiceLocator.Get<PlayerManager>().Player.ShootEvent -= OnShoot; }
        catch (Exception) { }

        foreach (Scissor lScissor in _scissors)
        {
            if (lScissor == null) continue;
            lScissor.Dispose();
        }
    }
}

//CISEAU    
public class Scissor : IDisposable
{
    public Sprite Sprite { get; private set; }
    public Vector2 CurrentCell { get; private set; }
    public bool IsMoving { get; private set; }

    private static Texture2D _textureAnimIdle = LoadTexture("images/ScissorsIdle.png");
    private static Texture2D _textureAnimCut = LoadTexture("images/ScissorsCut.png");
    private Vector2 _nextPosition;
    private float _speed = 700;
    private Vector2 _direction;


    public Scissor(Vector2 pCell, Vector2 pDirection)
    {
        CurrentCell = pCell;
        _direction = pDirection;

        //Creation du sprite
        Sprite = new Sprite(_textureAnimIdle, Grid.GetCellPosition(pCell), 8);
        Sprite.AnimSpeed = 50;
        Sprite.Origine = Sprite.Size / 2;

        //Maj de l'angle du sprite en fonction de la direction
        MajSpriteAngle();

        ServiceLocator.Get<AudioManager>().PlayLoopSoundFx(FXSounds.WhirlSound, Sprite, 0.5f);

        //Souscription à l'évènement du Beat
        ServiceLocator.Get<AudioManager>().OnBeat += OnBeat;
    }

    //Maj de la position du ciseau lorsqu'il est en mouvement
    public void UpdatePosition()
    {
        //Calcul de la distance à la posiiton d'arrivée
        float vDistanceToTarget = Vector2.Distance(Sprite.Position, _nextPosition);
        //Si on s'apprête à dépasser le point d'arrivé alors on arrête la position dessus et on met fin au mouvement
        if (vDistanceToTarget <= _speed * GetFrameTime())
        {
            Sprite.Position = _nextPosition;
            IsMoving = false;
            return;
        }

        //On maj la position en fonction d'une vitesse nominale pondérée à la distance restant à parcourir par rapport à distance totale (taille d'une cellule)
        Vector2 vDirection = Vector2.Normalize(_nextPosition - Sprite.Position);
        Sprite.Position += vDirection * _speed * GetFrameTime() * Tools.OutSine(Tools.InvLerp(0, CellDefinition.Tilesize.X, vDistanceToTarget));
    }

    //désinscription, nettoyage, etc 
    public void Dispose()
    {
        try
        {
            ServiceLocator.Get<AudioManager>().OnBeat -= OnBeat;
            ServiceLocator.Get<AudioManager>().RemoveLoopSoundFx(Sprite);
        }
        catch (Exception) { }
    }

    //Event du beat
    protected void OnBeat()
    {
        //On détermine la cellule dans la quelle aller
        Vector2 vNewCell = CurrentCell + _direction;
        //Si on sort du grid alors on fait demi-tour
        if (!Grid.IsCellInGrid(vNewCell))
        {
            _direction = -_direction;
            vNewCell = CurrentCell + _direction;
            MajSpriteAngle();
        }
        CurrentCell = vNewCell;

        //Si la nouvelle cellule contien le joueur alors on le pousse
        if (ServiceLocator.Get<PlayerManager>().Player.CurrentCell == CurrentCell)
            ServiceLocator.Get<PlayerManager>().PushPlayer(_direction);

        //On calcul la position réelle de la nouvelle cellule et on enclenche le mouvement vers elle
        _nextPosition = Grid.GetCellPosition(CurrentCell) + Sprite.Origine;
        IsMoving = true;

        //On passe à l'animation de mouvement et on lui ajoute un callback pour repasser à l'animation de repos à la fin
        Action vCB = () =>
        {
            Sprite.ChangeAnim(_textureAnimIdle, 8);
            Sprite.AnimSpeed = 50;
            Sprite.Origine = Sprite.Size / 2;
        };
        Sprite.ChangeAnim(_textureAnimCut, 7);
        Sprite.AnimSpeed = 25;
        Sprite.Origine = Sprite.Size / 2;
        Sprite.AddCallBack(vCB, 6);

        //SoundFx
        ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.ScissorCutSounds, 1f, Sprite.Position);
    }


    //Maj de l'angle du sprite en fonction de la direction dde déplacement
    protected void MajSpriteAngle()
    {
        switch ((_direction.X, _direction.Y))
        {
            case (1, 0):
                Sprite.Angle = -90;
                break;
            case (-1, 0):
                Sprite.Angle = 90;
                break;
            case (0, 1):
                Sprite.Angle = 0;
                break;
            case (0, -1):
                Sprite.Angle = 180;
                break;
            default:
                break;
        }
    }

}