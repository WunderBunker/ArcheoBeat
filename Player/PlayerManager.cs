using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//GESTION DU PERSO JOUEUR
public class PlayerManager : IDisposable
{
    public Player Player { get; private set; }

    public PlayerManager()
    {
        //Creation du player
        Player = new Player();
    }

    public void Update()
    {
        //Maj de l'animation du sprite
        Player.Sprite.Update();
        //Maj du mouvement si déplacement en cours
        if (Player.IsMoving) Player.UpdatePlayerPosition();
    }

    public void Draw()
    {
        //Affichage du player
        Player.Sprite.Draw();
    }

    public void PushPlayer(Vector2 pDirection)
    {
        //On déplace le joueur
        Player.Move(pDirection);
    }

    public void KillPlayer()
    {
        //Le joueur meurt et reprend du début
        Player.Die();
    }

    public void Dispose()
    {
        //Clearing et désinscription aux évènement 
        Player.Dispose();
    }
}

//PERSONNAGE JOUEUR
public class Player : IDisposable
{
    public Sprite Sprite { get; private set; }
    public Vector2 CurrentCell { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsBlocked { get; private set; }

    public int Life = 5;

    public delegate void ShootEventHandler(Vector2[] pImpactedCells);
    public event ShootEventHandler ShootEvent;
    public delegate void MoveEventHandler(Vector2 pNewCell);
    public event MoveEventHandler MoveEvent;


    private float _speed = 700;
    private int _cellSpeed = 1;
    private Vector2 _nextPosition;
    private bool _hasMoved;
    private byte _gunIsReadyIn;
    private bool _isInBeatSpan;

    private Texture2D _texAnimIdle;
    private Texture2D _texAnimMoveUp;
    private Texture2D _texAnimMoveDown;
    private Texture2D _texAnimMoveRight;
    private Texture2D _texAnimMoveLeft;
    private Texture2D _texAnimShoot;
    private Texture2D _texAnimBreak;

    public Player()
    {
        //On charge les animations 
        _texAnimIdle = LoadTexture("images/PlayerAnimIdle.png");
        _texAnimMoveUp = LoadTexture("images/PlayerUpAnim.png");
        _texAnimMoveDown = LoadTexture("images/PlayerDownAnim.png");
        _texAnimMoveRight = LoadTexture("images/PlayerRightAnim.png");
        _texAnimMoveLeft = LoadTexture("images/PlayerLeftAnim.png");
        _texAnimShoot = LoadTexture("images/ShootAnim.png");
        _texAnimBreak = LoadTexture("images/PlayerBreakAnim.png");

        //On initialise le sprite avec l'animation de repos
        Sprite = new Sprite(_texAnimIdle, Vector2.Zero, 25);
        Sprite.AnimSpeed = 10;

        //Souscription aux events d'Inputs
        ServiceLocator.Get<InputController>().ArrowPressed += OnShootInput;
        ServiceLocator.Get<InputController>().ZQSDPressed += OnMoveInput;
        //Souscription à l'event du quart de tempo
        ServiceLocator.Get<AudioManager>().OnThirdOfBeat += OnThirdOfBeat;
    }

    public void InitPositionAndSize(Vector2 pStartingCell)
    {
        //On initialise le joueur sur une cellule spécifiée par la map
        CurrentCell = pStartingCell;
        Sprite.Scale = new Vector2(Grid.CellSize / Sprite.Size.X, Grid.CellSize / Sprite.Size.Y) * 0.9f;
        Sprite.Position = Grid.GetCellPosition(CurrentCell) - Vector2.UnitY * Sprite.Size.Y / 3;
    }

    //Deplacement
    public void Move(Vector2 pDirection)
    {
        //Calcul  de la nouvelle cell
        Vector2 vIncrement = pDirection * _cellSpeed;
        Vector2 vNewCell = CurrentCell + vIncrement;

        //Si les éléments de la map le permettent...
        if (ServiceLocator.Get<MapManager>().CellMoveIsAllowed(CurrentCell, vNewCell)
        //...Et pas de ciseau
        && !ServiceLocator.Get<ScissorsManager>().IsScissorInCell(vNewCell))
        {
            CurrentCell = vNewCell;
            //On indique qu'un déplacement est en cours et on détermine quel est son point d'arrivé (cf UpdatePlayerPosition)
            IsMoving = true;
            _nextPosition = Grid.GetCellPosition(CurrentCell) - Vector2.UnitY * Sprite.Size.Y / 3;
            MoveEvent?.Invoke(CurrentCell);
        }
    }

    //Chutte et restart du joueur
    public void Die()
    {
        if (IsBlocked) return;

        IsBlocked = true;
        Life--;
        ServiceLocator.Get<UIManager>().LifeUI.RemoveLife();

        //On lance l'animation de chutte
        Sprite.ChangeAnim("images/PlayerFallAnim.png", 5);
        Sprite.AnimSpeed = 10;

        //On ajoute un CallBack sur la dernière frame de l'animation de chutte pour masker le sprite
        Sprite.AddCallBack(() => { Sprite.Color = new(0, 0, 0, 0); }, 4);

        //SoundFx
        //On ajoute un CallBack à la fin du son de chutte pour lancer l'animation d'explosion
        Action vCB = () =>
        {
            ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.CrashSound);
            Sprite.ChangeAnim("images/PlayerExplodeAnim.png", 4);
            Sprite.AnimSpeed = 15;
            Sprite.Color = Color.White;
            Sprite.AddCallBack(() => { Sprite.Color = new(0, 0, 0, 0); }, 3);
            ServiceLocator.Get<MapManager>().ReInitFixedBlocks();
        };
        ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.FallSound, 1f, Sprite.Position, vCB);

        //On lance le fondu au noir et on souscrit à l'event de fin pour reload la scene
        ServiceLocator.Get<UIManager>().FadePannel.StartFadeIn();
        ServiceLocator.Get<UIManager>().FadePannel.FadeEnd += EndDeath;
        ;
    }

    //Callback appelé lorsque le fadeIn de la mort est fini 
    void EndDeath()
    {
        ServiceLocator.Get<UIManager>().FadePannel.FadeEnd -= EndDeath;

        // Si plus de vies alors on recharge la scène
        if (Life < 0)
        {
            ServiceLocator.Get<SceneManager>().ReLoadScene();
        }
        //Sinon on respawn au start
        else
        {
            InitPositionAndSize(ServiceLocator.Get<MapManager>().StartingCell);
            Sprite.ChangeAnim(_texAnimIdle, 25);
            Sprite.AnimSpeed = 10;
            Sprite.Color = Color.White;
            IsBlocked = false;
        }
    }

    //Maj de la position du joueur lorsqu'il est en mouvement
    public void UpdatePlayerPosition()
    {
        //Calcul de la distance à la posiiton d'arrivée
        float vDistanceToTarget = Vector2.Distance(Sprite.Position, _nextPosition);
        //Si on s'apprête à dépasser le point d'arrivé alors on arrête la position dessus et on met fin au mouvement
        if (vDistanceToTarget <= _speed * GetFrameTime())
        {
            Sprite.Position = _nextPosition;
            IsMoving = false;
            //On applique l'effet de la nouvelle cell si il y en a un
            ServiceLocator.Get<MapManager>().ApplyCellEffect(CurrentCell);
            return;
        }

        //On maj la position en fonction d'une vitesse nominale pondérée à la distance restant à parcourir par rapport à distance totale (taille de la cellule * nb de cellule à parcourir)
        Vector2 vDirection = Vector2.Normalize(_nextPosition - Sprite.Position);
        Sprite.Position += vDirection * _speed * GetFrameTime() * Tools.OutSine(Tools.InvLerp(0, CellDefinition.Tilesize.X * _cellSpeed, vDistanceToTarget));
    }

    //Event de l'input de tir
    void OnShootInput(Vector2 pDirection)
    {
        //Si on n'a pas encore tirer suur ce beat et  qu'on est dans les temps alors on tire
        if (_gunIsReadyIn == 0 && !IsBlocked && _isInBeatSpan) Shoot(pDirection);
    }

    //Event d'un Input de déplacement
    void OnMoveInput(Vector2 pDirection)
    {
        //Si on tente de bouger en dehors du tempo alors on recule
        if (!_isInBeatSpan)
        {
            Break(-pDirection);
            return;
        }

        //Si on ne s'est pas encore déplacé sur ce beat et qu'on est dans les temps alors on se déplace
        if (!_hasMoved && !IsBlocked)
        {
            //On déplace le joueur
            Move(pDirection);
            if (IsBlocked) return;

            //On applique une animation de déplacement
            MajMoveAnim(pDirection);
            ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.FlameBurstSounds, 0.1f);
            //On indique qu'on s'est déjà déplacé sur ce beat
            _hasMoved = true;
        }
    }

    //Event de l'Input de quart de beat
    protected void OnThirdOfBeat(int pThirdCount)
    {
        //On autorise les actions entre le 2/3 d'un beat et le 1/3 du suivant (soit sur un 2/3 du tempo)
        _isInBeatSpan = pThirdCount == 2 || pThirdCount == 0;

        //La fin du span se situe donc à la fin du premier tiers
        if (pThirdCount == 1)
        {
            //Si on n'a pas déjà bougé à la fin du span alors on active la cell (car pas encore activée)
            if (!_hasMoved && !IsBlocked) ServiceLocator.Get<MapManager>().ApplyCellEffect(CurrentCell);
            ServiceLocator.Get<MapManager>().DeblockCellsEffect();

            _hasMoved = false;
            if (_gunIsReadyIn > 0)
            {
                _gunIsReadyIn--;
                if (_gunIsReadyIn == 0) ServiceLocator.Get<UIManager>().GunUI.MakeGunUsable(true);
            }
        }
    }

    //mouvement hors tempo : le joueur casse et avance dans le sens opposé à ce qui était souhaité
    protected void Break(Vector2 pDirection)
    {
        if (IsBlocked) return;

        Move(pDirection);
        _hasMoved = true;

        //Animation player cassé
        Sprite.ChangeAnim(_texAnimBreak, 10);
        //On ajoute un callback à la fin de l'animation de déplacement pour revenir à l'animation de repos
        Sprite.AnimSpeed = 30;
        Action vCallBack = () =>
        {
            Sprite.ChangeAnim(_texAnimIdle, 25);
            Sprite.AnimSpeed = 10;
        };
        Sprite.AddCallBack(vCallBack, 9);

        //Bruit de disfonctionnement
        ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.PlayerBreakSound,0.5f);
    }

    //Application d'une animation de déplacement
    protected void MajMoveAnim(Vector2 pDirection)
    {
        //On applqieu la bonne animation en fonction de la direction du déplacement
        switch ((pDirection.X, pDirection.Y))
        {
            case (1, 0):
                Sprite.ChangeAnim(_texAnimMoveRight, 10);
                break;
            case (-1, 0):
                Sprite.ChangeAnim(_texAnimMoveLeft, 10);
                break;
            case (0, 1):
                Sprite.ChangeAnim(_texAnimMoveDown, 10);
                break;
            case (0, -1):
                Sprite.ChangeAnim(_texAnimMoveUp, 10);
                break;
            default:
                break;
        }

        //On ajoute un callback à la fin de l'animation de déplacement pour revenir à l'animation de repos
        Sprite.AnimSpeed = 30;
        Action vCallBack = () =>
        {
            Sprite.ChangeAnim(_texAnimIdle, 25);
            Sprite.AnimSpeed = 10;
        };
        Sprite.AddCallBack(vCallBack, 9);
    }

    //Tir
    protected void Shoot(Vector2 pDirection)
    {
        //Creation de l'animation temporaire de tir
        Anim vAnim = ServiceLocator.Get<TempAnimManager>().CreateTempAnim(_texAnimShoot, Sprite.Position, 9);
        //On oriente l'animation
        vAnim.Angle = pDirection.X > 0 ? 0 : (pDirection.X < 0 ? 180 : (pDirection.Y > 0 ? 90 : -90));
        //On la scale pour qu'elle fasse 2 cellules de long et une de large
        vAnim.Scale = new Vector2(Grid.CellSize * 2 / vAnim.Size.X, Grid.CellSize / vAnim.Size.Y);
        //On la positionne correctement en fonction de l'angle
        vAnim.Origine = vAnim.Size / 2;
        vAnim.Position = Sprite.Position + Sprite.Size / 2
            + Tools.RotateAround(new Vector2(Sprite.Size.X + vAnim.Size.Y / 2, 0), Vector2.Zero, vAnim.Angle);

        vAnim.Color = Color.Red;
        vAnim.AnimSpeed = 50;

        //SoundFx
        ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.LaserSounds, 0.4f);

        //On lance l'event de tir pour impacter les deux case visées
        ShootEvent?.Invoke([CurrentCell + pDirection, CurrentCell + pDirection * 2]);

        _gunIsReadyIn = 2;
        ServiceLocator.Get<UIManager>().GunUI.MakeGunUsable(false);
    }


    public void Dispose()
    {
        try
        {
            ServiceLocator.Get<InputController>().ArrowPressed -= OnShootInput;
            ServiceLocator.Get<InputController>().ZQSDPressed -= OnMoveInput;
            ServiceLocator.Get<AudioManager>().OnThirdOfBeat -= OnThirdOfBeat;
            ServiceLocator.Get<UIManager>().FadePannel.FadeEnd -= () => { ServiceLocator.Get<SceneManager>().ReLoadScene(); };

            ShootEvent = null;
            MoveEvent = null;
        }
        catch (Exception) { }
    }
}
