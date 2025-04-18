using System.Numerics;
using Raylib_cs;

public class MyCamera
{
    private Camera2D _camera = new();
    public Camera2D Camera
    {
        get => _camera;
        private set
        {
            _camera = value;
        }
    }

    public Vector2 Target
    {
        get => _camera.Target;
        set
        {
            _camera.Target = value;
        }
    }

    public Vector2 Offset
    {
        get => _camera.Offset;
        set
        {
            _camera.Offset = value;
        }
    }

    public float Zoom
    {
        get => _camera.Zoom;
        set
        {
            _camera.Zoom = value;
        }
    }
    public float Rotation
    {
        get => _camera.Rotation;
        set
        {
            _camera.Rotation = value;
        }
    }

    public Vector2 TransformVectForUI(Vector2 pVect)
    {
        return pVect + Target - Offset / Zoom;
    }

    public Rectangle TransformRectForUI(Rectangle pRect)
    {
        pRect.Position +=Target - Offset / Zoom;
        return pRect;
    }
}