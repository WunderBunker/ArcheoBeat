using static Raylib_cs.Raylib;

//CLASSE MERE DES SCENES
public abstract class Scene
{
    public string LevelId { get; protected set; }

    public bool DebugMode { get; protected set; }

    public virtual void Load(string pLevelId = "")
    {
        LevelId = pLevelId;

        //Chargements des différents managers de la scène dans le service locator
        LoadManagers();

        //Chargement des images de fond
        InitBackGround();

        //Event du mode debug
        ServiceLocator.Get<InputController>().F1 += ChangeDebugMode;

    }
    public virtual void UnLoad()
    {
        ServiceLocator.Get<InputController>().F1 -= ChangeDebugMode;

        //Unload des différents managers de la scène du service locator
        UnLoadManagers();
    }

    public virtual void Update()
    {
        if (WindowShouldClose())
        {
            OnEchap();
            return;
        }
    }

    public virtual void Draw() { }

    protected virtual void LoadManagers() { }

    protected virtual void UnLoadManagers() { }

    protected virtual void InitBackGround() { }

    protected virtual void ChangeDebugMode()
    {
        DebugMode = !DebugMode;
        ServiceLocator.Get<MeasureTool>().ToolIsActive = DebugMode;
    }

    protected virtual void OnEchap(){}


}