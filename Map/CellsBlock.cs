using System.Numerics;
using Raylib_cs;

//BLOCK DE CELLULES
public class CellsBlock
{
    public int TypeId { get; private set; }
    public int NextType { get; private set; }
    public Vector2 IndexesInGrid { get; private set; }
    public bool IsFixed;
    public bool IsAllwaysFixed;
    public bool IsCollapsed;
    public bool IsHectic;
    public Cell[] Cells;
    public int[] NextCells;

    public CellsBlock LeftNeighbour;
    public CellsBlock RightNeighbour;
    public CellsBlock TopNeighbour;
    public CellsBlock BottomNeighbour;

    public List<Blockade> Blockades { get; private set; } = new();

    List<int> _probabilityList;

    public CellsBlock(Vector2 pIndexesInGrid)
    {
        //Type 0 par défaut
        TypeId = 0;
        //On initialise le block dans un état "non effondré" (son type est 0 mais il n'est pas considéré comme résolu pour le moment)
        IsCollapsed = false;
        //On initialise la liste des états possibles ( types que peut prendre le block)
        _probabilityList = new List<int>([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);

        //On initialise la liste des cellules contenues dans le block
        Cells = new Cell[(int)(CellsBlocksDefinition.BlockSizeInCells.X * CellsBlocksDefinition.BlockSizeInCells.Y)];
        for (int lCptCell = 0; lCptCell < Cells.Length; lCptCell++) Cells[lCptCell] = new(13, Grid.GetCellIndexInGrid(lCptCell, pIndexesInGrid));

        NextCells = new int[Cells.Length];

        IndexesInGrid = pIndexesInGrid;
    }

    //Passe le block dans un état effondré après avoir réduit sa liste d'états possibles à un seul type
    public void ForceCollapse(int pTypeId)
    {
        _probabilityList = new List<int>([pTypeId]);
        Collapse();
    }

    //Passe le block dans un état effondré (on définit le type du block en fonction de ses états possibles et on le considère résolu)
    public void Collapse()
    {
        //Si pas d'état possible dans la liste, alors on prend le type 13 qui correspond à un block vide 
        if (_probabilityList.Count == 0) NextType = 13;
        //Si il n'y a qu'un état possible on prend celui-là
        else if (_probabilityList.Count == 1) NextType = _probabilityList[0];
        //Sinon on  en prend un aléatoirement dans la liste
        else NextType = _probabilityList[new Random().Next(0, _probabilityList.Count - 1)];

        //On récupère les types de chaque cellule du block en fonction du type qui vient d'être résolut et on les stock pour futur maj
        for (int lCptCell = 0; lCptCell < CellsBlocksDefinition.Blocks[NextType].Length; lCptCell++)
            NextCells[lCptCell] = CellsBlocksDefinition.Blocks[NextType][lCptCell];

        IsCollapsed = true;

        //On réinitialise la liste des états possibles
        _probabilityList = new List<int>([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 14, 15, 16, 17, 18, 19, 20]);
        //On retire le prochain type de la liste des proba
        int vNextTypeIndex = _probabilityList.FindIndex(_ => _ == NextType);
        if (vNextTypeIndex != -1)
            _probabilityList.RemoveAt(vNextTypeIndex);

        //On maj les blocks voisins
        AtualiseNeighboors();
    }

    //On applique le prochain type (celui qui vient d'être résolu) au block
    public void ApplyNextType()
    {
        TypeId = NextType;
        for (int lCptCell = 0; lCptCell < CellsBlocksDefinition.Blocks[TypeId].Length; lCptCell++)
        {
            Cells[lCptCell].ChangeId(NextCells[lCptCell]);
        }
        TopNeighbour?.SmoothTiles();

        if (!IsFixed) IsCollapsed = false;
    }

    //Maj des listes d'états possible des blocks voisins
    public void AtualiseNeighboors()
    {
        LeftNeighbour?.MajCompatibilities(NextType, CellsBlocksDefinition.LeftIncompatibilityMatrix, true);
        RightNeighbour?.MajCompatibilities(NextType, CellsBlocksDefinition.LeftIncompatibilityMatrix);
        TopNeighbour?.MajCompatibilities(NextType, CellsBlocksDefinition.TopIncompatibilityMatrix, true);
        BottomNeighbour?.MajCompatibilities(NextType, CellsBlocksDefinition.TopIncompatibilityMatrix);
    }

    //On vérifie si il faut  maj des cellules pour être continu avec celles du dessous
    public void SmoothTiles()
    {
        if (BottomNeighbour == null) return;

        for (int lCptCell = 6; lCptCell < Cells.Length; lCptCell++)
        {
            //Si notre cellule est vide on passe à la suivante
            if (Cells[lCptCell].Id == 0) continue;

            //Si cellule pleine en haut du block du voisin alors on passe la cellule dans sa version continue
            if (CellsBlocksDefinition.Blocks[BottomNeighbour.TypeId][(lCptCell + 3) % Cells.Length] != 00 &&
                (Cells[lCptCell].Id % 10 == 1 || Cells[lCptCell].Id % 10 == 2 || Cells[lCptCell].Id % 10 == 3))
                Cells[lCptCell].ChangeId(Cells[lCptCell].Id - Cells[lCptCell].Id % 10);
            //Sinon, si cellule vide en haut du block du voisin et notre cellule est continue, on la passe dans une version "pont"
            else if (CellsBlocksDefinition.Blocks[BottomNeighbour.TypeId][(lCptCell + 3) % Cells.Length] == 00 && Cells[lCptCell].Id % 10 == 0)
                Cells[lCptCell].ChangeId(Cells[lCptCell].Id + 1);
        }
    }

    //Maj du tableau des probabilités pour en fonction d'un nouveau type voisin et de la matrice d'incompabilité contre laquelle le tester
    public void MajCompatibilities(int pType, int[][] pIncompabilityMatrix, bool pInverserMatrice = false)
    {
        if (IsCollapsed) return;

        for (int lCptProba = _probabilityList.Count - 1; lCptProba >= 0; lCptProba--)
        {
            int lProbableType = _probabilityList[lCptProba];

            int[] lCompatibilityArray;

            if (!pInverserMatrice)
            {
                lCompatibilityArray = pIncompabilityMatrix[lProbableType];
                if (lCompatibilityArray.Contains(pType)) _probabilityList.RemoveAt(lCptProba);
            }
            //Cas où on inverse (typiquement : on utilise la matrice des voisins de gauches pour tester le type du voisin de droite )
            else
            {
                lCompatibilityArray = pIncompabilityMatrix[pType];
                if (lCompatibilityArray.Contains(lProbableType)) _probabilityList.RemoveAt(lCptProba);
            }
        }
        if (_probabilityList.Count == 1) Collapse();
    }

    //Creer une barriere qui vient blocker une direction par laquelle sortir du block
    public void PutBarrier(BlockedDirection pDirection)
    {
        PutBarrier(pDirection, false, Vector2.NaN, null);
    }
    public void PutBarrier(BlockedDirection pDirection, bool pWithBarrier, Vector2 pOrbCell, Color? pColor)
    {
        //Si déjà un blocage sur cette direction on return 
        foreach (Blockade lBlockade in Blockades)
            if (lBlockade.BlockedDirection == pDirection) return;

        //Creation d'un blocage avec barriere et orbe
        if (pWithBarrier) Blockades.Add(new(pDirection, IndexesInGrid, pOrbCell, pColor));
        else
        {
            //Dans le cas où la méthode a été appelée depuis un voisin on ne créer pas l'orbe et la barriere et s'arrête là
            Blockades.Add(new(pDirection, IndexesInGrid));
            return;
        }

        //On vient aussi appliquer un blocage au voisin de façon à ce que la barriere fonctionne dans les deux sens
        switch (pDirection)
        {
            case BlockedDirection.Up:
                TopNeighbour?.PutBarrier(BlockedDirection.Down);
                break;
            case BlockedDirection.Down:
                BottomNeighbour?.PutBarrier(BlockedDirection.Up);
                break;
            case BlockedDirection.Right:
                RightNeighbour?.PutBarrier(BlockedDirection.Left);
                break;
            case BlockedDirection.Left:
                LeftNeighbour?.PutBarrier(BlockedDirection.Right);
                break;
        }
    }

    //On retire une barriere
    public void RemoveBarrier(BlockedDirection pDirection, bool pIsRecursif = false)
    {
        //On supprime le blocage sur la direction renseignée
        for (int lCptBlockade = Blockades.Count - 1; lCptBlockade >= 0; lCptBlockade--)
            if (Blockades[lCptBlockade].BlockedDirection == pDirection)
            {
                //Destruction des éventuels éléments graphiques de la blockade
                Blockades[lCptBlockade].Destroy();

                //Suppression de la blockade
                Blockades.RemoveAt(lCptBlockade);
            }

        //On évite les appels récursifs infinis entre voisins    
        if (pIsRecursif) return;

        //On retire le blocage du voisin également
        switch (pDirection)
        {
            case BlockedDirection.Up:
                TopNeighbour?.RemoveBarrier(BlockedDirection.Down, true);
                break;
            case BlockedDirection.Down:
                BottomNeighbour?.RemoveBarrier(BlockedDirection.Up, true);
                break;
            case BlockedDirection.Right:
                RightNeighbour?.RemoveBarrier(BlockedDirection.Left, true);
                break;
            case BlockedDirection.Left:
                LeftNeighbour?.RemoveBarrier(BlockedDirection.Right, true);
                break;
        }
    }

    //Maj des timers des cellules s'il y en a
    public void UpdateTimers()
    {
        foreach (Cell lCell in Cells)
            lCell.UpdateTimer();
    }

    //Update l'animation de la barriere et son orbe si il y a  
    public void UpdateBarriers()
    {
        foreach (Blockade lBlockade in Blockades)
        {
            lBlockade.BarriereSprite?.Update();
            lBlockade.OrbSprite?.Update();
        }
    }

    //Dessine la barriere et son orbe si il y a 
    public void DrawBarriers()
    {
        foreach (Blockade lBlockade in Blockades)
        {
            lBlockade.BarriereSprite?.Draw();
            lBlockade.OrbSprite?.Draw();
        }
    }
}

public enum BlockedDirection
{
    Up, Left, Right, Down, None
}


public struct Blockade
{
    public BlockedDirection BlockedDirection;
    public Vector2 OrbCell;
    public Sprite BarriereSprite;
    public Sprite OrbSprite;
    public bool WithBarrier;

    public Blockade(BlockedDirection pDirection, Vector2 pBlkIndexInGrid) : this(pDirection, pBlkIndexInGrid, Vector2.NaN, null, false) { }
    public Blockade(BlockedDirection pDirection, Vector2 pBlkIndexInGrid, Vector2 pOrbCell, Color? pColor, bool pWithBarrier = true)
    {
        BlockedDirection = pDirection;
        WithBarrier = pWithBarrier;

        if (!pWithBarrier) return;

        //Creation du sprite de la barriere
        BarriereSprite = new("images/barrierAnim.png", Vector2.Zero, 4, 10);
        BarriereSprite.Color = (Color)pColor;

        //On définit la posiiton de la barriere dans le block 
        Vector2 vBarrierPosition = pBlkIndexInGrid * CellsBlocksDefinition.BlockSizeInCells * Grid.CellSize;
        float vAngle = 0;
        switch (pDirection)
        {
            case BlockedDirection.Up:
                vBarrierPosition += Grid.GetCellPosition(Vector2.Zero) - Vector2.UnitY * BarriereSprite.Size / 2;
                break;
            case BlockedDirection.Down:
                vBarrierPosition += Grid.GetCellPosition(new Vector2(2, 2)) + Vector2.One * Grid.CellSize - Vector2.UnitY * BarriereSprite.Size / 2;
                vAngle = 180;
                break;
            case BlockedDirection.Right:
                vBarrierPosition += Grid.GetCellPosition(new Vector2(2, 0)) + Vector2.UnitX * Grid.CellSize + Vector2.UnitX * BarriereSprite.Size.Y / 2;
                vAngle = 90;
                break;
            case BlockedDirection.Left:
                vBarrierPosition += Grid.GetCellPosition(new Vector2(0, 2)) + Vector2.UnitY * Grid.CellSize - Vector2.UnitX * BarriereSprite.Size.Y / 2;
                vAngle = -90;
                break;
        }
        BarriereSprite.Angle = vAngle;
        BarriereSprite.Position = vBarrierPosition;

        ServiceLocator.Get<AudioManager>().PlayLoopSoundFx(FXSounds.BarrierSwooshSound, BarriereSprite, 1);

        //On créé également l'orbe qui permet de détruire la barrière
        OrbSprite = new("images/orbAnim.png", Grid.GetCellPosition(pOrbCell), 4, 10);
        OrbSprite.Color = (Color)pColor;

        OrbCell = pOrbCell;
    }

    //Destruction des éventuels éléments graphiques de la blockade
    public void Destroy()
    {
        if (OrbSprite == null) return;

        //Animation explosion orbe
        Anim vOrbAnim = ServiceLocator.Get<TempAnimManager>().CreateTempAnim("images/OrbExplodeAnim.png", Vector2.Zero, 4);
        vOrbAnim.AnimSpeed = 10;
        vOrbAnim.Position = OrbSprite.Position + OrbSprite.Size / 2 - vOrbAnim.Size / 2;
        vOrbAnim.Color = OrbSprite.Color;
        //Son expl. orbe
        ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.OrbExplodeSound, 1, OrbSprite.Position);
        //Animation explosion barrière
        Anim vBarrierAnim = ServiceLocator.Get<TempAnimManager>().CreateTempAnim("images/barrierExplodeAnim.png", Vector2.Zero, 5);
        vBarrierAnim.AnimSpeed = 10;
        vBarrierAnim.Angle = BarriereSprite.Angle;
        vBarrierAnim.Color = BarriereSprite.Color;
        vBarrierAnim.Position = BarriereSprite.Position
        + Tools.RotateAround(BarriereSprite.Size / 2 - vBarrierAnim.Size / 2, Vector2.Zero, vBarrierAnim.Angle);
        //Son expl. barriere
        ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.BarrierExplodeSound, 1, BarriereSprite.Position);

        //on retire le wooshing sound de l'audioManager
        ServiceLocator.Get<AudioManager>().RemoveLoopSoundFx(BarriereSprite);
    }
}