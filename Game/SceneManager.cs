
//GESTION DES SCENES    
using Newtonsoft.Json;

public class SceneManager
{
    public Dictionary<string, LevelInfo> Levels { get; private set; }

    public Scene CurrentScene { get; private set; }

    public SceneManager()
    {
        LoadGame();
    }

    public void ReLoadScene()
    {
        Type vTypeScene = CurrentScene.GetType();
        string vLevelId = CurrentScene.LevelId;

        UnLoadCurrentScene();

        CurrentScene = (Scene)Activator.CreateInstance(vTypeScene);
        CurrentScene.Load(vLevelId);
    }

    public void LoadScene<SceneType>(string pLevelId = "") where SceneType : Scene, new()
    {
        UnLoadCurrentScene();
        CurrentScene = new SceneType();
        CurrentScene.Load(pLevelId);
    }

    void UnLoadCurrentScene()
    {
        if (CurrentScene == null) return;

        CurrentScene.UnLoad();
        CurrentScene = null;
        ServiceLocator.Get<AudioManager>().Dispose();
        Sprite.CleanOrderedSprites();

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public void LoadNextScene()
    {
        if (Levels[CurrentScene.LevelId].NextLevelID != "")
            LoadScene<SceneLevel>(Levels[CurrentScene.LevelId].NextLevelID);
        else LoadScene<SceneMenu>();
    }

    public void UnblockNextLevel()
    {
        string vNextLevelId = Levels[CurrentScene.LevelId].NextLevelID;
        if(vNextLevelId=="")return;
        LevelInfo vNextLevel = Levels[vNextLevelId];
        vNextLevel.IsBlocked = false;
        Levels[vNextLevelId] = vNextLevel;
    }

    public void SaveGame()
    {
        string vPath = Path.Combine(SaveUtilities.GetSaveFolder(), "savegame.json");
        string vJsonSave = JsonConvert.SerializeObject(Levels, Formatting.Indented);
        File.WriteAllText(vPath, vJsonSave);
    }

    void LoadGame()
    {
        string vPath = Path.Combine(SaveUtilities.GetSaveFolder(), "savegame.json");
        if(!File.Exists(vPath)) InitLevelsInfos();
        else
        {
            string vJsonSave = File.ReadAllText(vPath);
            Levels = JsonConvert.DeserializeObject<Dictionary<string, LevelInfo>>(vJsonSave);
        }
    }


    void InitLevelsInfos()
    {
        Levels = new() {
        { "L1", new("L1","Level 1", "L2",false)},
        { "L2", new("L2","Level 2", "L3",true)},
        { "L3", new("L3","Level 3", "L4",true)},
        { "L4", new("L4","Level 4", "L5",true)},
        { "L5", new("L5","Level 5", "L6",true)},
        { "L6", new("L6","Level 6", "L7",true)},
        { "L7", new("L7","Level 7", "L8",true)},
        { "L8", new("L8","Level 8", "",  true)}};
    }
}

public struct LevelInfo
{
    public string LevelID ;
    public string Name ;
    public string NextLevelID ;
    public bool IsBlocked;

    public LevelInfo(string pLevelId, string pName, string pNextLevelID, bool pIsBlocked)
    {
        LevelID = pLevelId;
        Name = pName;
        NextLevelID = pNextLevelID;
        IsBlocked = pIsBlocked;
    }
}

public static class SaveUtilities
{
    public static string GetSaveFolder()
    {
        string vSavePath = "";

        if (OperatingSystem.IsWindows())
            vSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        else if (OperatingSystem.IsLinux())
            vSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config");
        else if (OperatingSystem.IsMacOS())
            vSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support");

        vSavePath = Path.Combine(vSavePath, "ArcheoBeat");
        Directory.CreateDirectory(vSavePath);
        return vSavePath;
    }
}