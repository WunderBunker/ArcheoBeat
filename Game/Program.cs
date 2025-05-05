using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

public class ArcheoBeat
{
    public static int Main()
    {
        //Chargement dans le service locator des services globaux, communs à toutes les scènes
        RegisterFixedServices();

        //Initialisation de la fenêtre
        GameParameters vGP = ServiceLocator.Get<GameParameters>();
        SetTargetFPS(vGP.TargetFPS);
        SetTextLineSpacing(-20);
        vGP.Window.InitialiserWindow();

        //Chargement de la scène du menu des niveaux
        ServiceLocator.Get<SceneManager>().LoadScene<SceneMenu>("");
        

        //RAYLOOP
        while (!vGP.ShouldClose)
        {
            //UPDATES
            ServiceLocator.Get<InputController>().Update();
            ServiceLocator.Get<SceneManager>().CurrentScene.Update();

            //DRAWINGS
            //Canvas
            BeginTextureMode(vGP.Window.Canvas);
            ClearBackground(Color.DarkPurple);
            ServiceLocator.Get<SceneManager>().CurrentScene.Draw();
            EndTextureMode();
            
            //Ecran
            BeginDrawing();
            ClearBackground(Color.DarkGray);
            DrawTexturePro(vGP.Window.Canvas.Texture, new Rectangle(Vector2.Zero, new Vector2(1,-1)*vGP.Window.Size), vGP.Window.ResizedScreen, Vector2.Zero, 0, Color.White);
            EndDrawing();
        }

        CloseWindow();

        return 0;
    }

    static void RegisterFixedServices()
    {
        GameParameters vGP = new();
        vGP.Window = new Window(800, 600);
        vGP.TargetFPS = 60;
        ServiceLocator.Register(vGP);

        ServiceLocator.Register<SceneManager>();
        ServiceLocator.Register<InputController>();
        ServiceLocator.Register<AudioManager>();

        MyCamera vCamera = new();
        vCamera.Offset = vGP.Window.Size / 2;
        vCamera.Rotation = 0;
        vCamera.Zoom = 1;
        ServiceLocator.Register(vCamera);
        ServiceLocator.Register<MeasureTool>();

    }

}

//PARAMETRES GLOBAUX DU JEUX
public class GameParameters
{
    public Window Window;
    public int TargetFPS;
    public bool ShouldClose;
}