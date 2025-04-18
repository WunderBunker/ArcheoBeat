using System.IO.Compression;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

public class MeasureTool
{
    public bool ToolIsActive = false;

    List<Vector2> _points = new List<Vector2>();

    public MeasureTool()
    {
        ServiceLocator.Get<InputController>().Click += OnClick;
        ServiceLocator.Get<InputController>().Entry += OnEntryPressed;
    }

    void OnClick(int pButton, Vector2 pPosition)
    {
        if (!ToolIsActive) return;

        if (pButton == 2)
            _points.Add(pPosition + ServiceLocator.Get<MyCamera>().Target - ServiceLocator.Get<MyCamera>().Offset);
    }

    void OnEntryPressed()
    {
        if (!ToolIsActive || _points.Count == 0) return;

        string vToPrint = "";
        Vector2 vOrigine = _points[0];
        Vector2 vMax = vOrigine;

        foreach (Vector2 lPoint in _points)
        {
            if (lPoint.X < vOrigine.X) vOrigine.X = lPoint.X;
            if (lPoint.X > vMax.X) vMax.X = lPoint.X;
            if (lPoint.Y < vOrigine.Y) vOrigine.Y = lPoint.Y;
            if (lPoint.Y > vMax.Y) vMax.Y = lPoint.Y;
        }

        //On affiche les points soit relatif à leur origine (point le plus en haut à gauche) soit par rapport à leur milieu

        //  LCrtl => On transforme l'origine pour qu'elle se situe au milieu du polygone
        if (IsKeyDown(KeyboardKey.LeftControl))
        {
            vOrigine = (vMax - vOrigine) / 2;

            /*  for (int lCptPoint = 0; lCptPoint < _points.Count; lCptPoint++)
             {
                 Vector2 lPoint = _points[lCptPoint];
                 lPoint = lPoint - vOrigine;
                 _points[lCptPoint] = lPoint;
             } */
        }

        foreach (Vector2 lPoint in _points)
            vToPrint = vToPrint + ",{ x = " + (lPoint.X - vOrigine.X) + ", Y = " + (lPoint.Y - vOrigine.Y) + "}";
        Console.WriteLine("Points : " + vToPrint);
        Console.WriteLine("Origine : { x = " + vOrigine.X + " , y = " + vOrigine.Y + "}");

        _points.Clear();
    }

    ~MeasureTool()
    {
        ServiceLocator.Get<InputController>().Click -= OnClick;
        ServiceLocator.Get<InputController>().Entry -= OnEntryPressed;
    }

}