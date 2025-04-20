using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Raylib_cs;
using static Raylib_cs.Raylib;

//GESTION DE LA MAP (GRILLE / BLOCKS / CELLULES)
public class MapManager : IDisposable
{
    public Vector2 StartingCell { get; set; }
    MapSpecific _mapSpecifics;

    CellsBlock[,] _cellsBlocks;
    int _beatWaited = 0;
    bool _isInPreviewSpan;
    float _previewAlpha;
    Vector2[] _shotCells;
    Vector2 _playerCell;

    public MapManager()
    {

        //Recuperation de la data spécifique au niveau courrant
        _mapSpecifics = (MapSpecific)typeof(MapSpecifics).GetField(ServiceLocator.Get<SceneManager>().CurrentScene.LevelId).GetValue(null);

        //Initialisation de la grille
        Grid.InitGridParameters(_mapSpecifics.GridSizeInBlocks, 64);

        //Souscription à l'event du beat
        ServiceLocator.Get<AudioManager>().OnBeat += OnBeat;
        ServiceLocator.Get<PlayerManager>().Player.ShootEvent += OnPlayerShoot;
        ServiceLocator.Get<PlayerManager>().Player.MoveEvent += OnPlayerMove;

        //Initialisation des blocks sur la grille
        InitBlocks();

        ServiceLocator.Get<PlayerManager>().Player.InitPositionAndSize(StartingCell);
    }

    public void Update()
    {
        //Maj de l'alpha de la preview
        _previewAlpha -= GetFrameTime() * 2;
        if (_previewAlpha <= 0) _isInPreviewSpan = false;

        foreach (CellsBlock lCellBlk in _cellsBlocks)
        {
            //Maj des barrieres s'il y a
            if (lCellBlk.Blockades.Count > 0)
            {
                lCellBlk.UpdateBarriers();
                if (_shotCells != null)
                    for (int lCptBlockade = lCellBlk.Blockades.Count - 1; lCptBlockade >= 0; lCptBlockade--)
                    {
                        Blockade lBlockade = lCellBlk.Blockades[lCptBlockade];
                        if (_shotCells.Contains(lBlockade.OrbCell)) lCellBlk.RemoveBarrier(lBlockade.BlockedDirection);
                    }
            }

            for (int lCptCell = 0; lCptCell < lCellBlk.Cells.Length; lCptCell++)
            {
                //Maj des animation de cellule
                lCellBlk.Cells[lCptCell].UpdateAnim();

                //Si il y a eue un tir du joueur sur cette cellule, on regarde si elle doit être impactée
                if (_shotCells != null)
                {
                    if (_shotCells.Contains(lCellBlk.Cells[lCptCell].CellIndexInGrid))
                    {
                        if (lCellBlk.Cells[lCptCell].Id / 10 == 6)
                        {
                            ApplyCellEffect(lCellBlk.Cells[lCptCell].CellIndexInGrid);
                            if (lCellBlk.IsFixed) lCellBlk.NextCells[lCptCell] = lCellBlk.Cells[lCptCell].Id;
                        }
                    }
                }
            }
        }

        //Reset des cellules impactées par tir du joueur
        _shotCells = null;
    }

    public void Draw()
    {
        //Contours blocks en dessous
        for (int lCptY = 0; lCptY < _cellsBlocks.GetLength(0); lCptY++)
        {
            for (int lCptX = 0; lCptX < _cellsBlocks.GetLength(1); lCptX++)
            {
                Vector2 lBlockPos = new Vector2(lCptX * CellsBlocksDefinition.BlockSizeInCells.X, lCptY * CellsBlocksDefinition.BlockSizeInCells.Y) * Grid.CellSize;
                Rectangle vCadreTexture = (_cellsBlocks[lCptY, lCptX].IsFixed && _cellsBlocks[lCptY, lCptX].TypeId == 13) ? CellsBlocksDefinition.BrokenBlockFrameDownRectTex
                    : CellsBlocksDefinition.BlockFrameDownRectTex;
                DrawTexturePro(CellDefinition.TileSheet, vCadreTexture,
                    new Rectangle(lBlockPos, Vector2.One * Grid.CellSize * CellsBlocksDefinition.BlockSizeInCells),
                    Vector2.Zero, 0, Color.White);
            }
        }

        //Cells
        for (int lCptY = 0; lCptY < _cellsBlocks.GetLength(0); lCptY++)
        {
            for (int lCptX = 0; lCptX < _cellsBlocks.GetLength(1); lCptX++)
            {
                CellsBlock lBlock = _cellsBlocks[lCptY, lCptX];
                Vector2 lBlockPos = new Vector2(lCptX * CellsBlocksDefinition.BlockSizeInCells.X, lCptY * CellsBlocksDefinition.BlockSizeInCells.Y) * Grid.CellSize;
                for (int lCptCell = 0; lCptCell < lBlock.Cells.Length; lCptCell++)
                {
                    //Cellule actuelle
                    Vector2 lCellPos = lBlockPos + new Vector2(lCptCell % CellsBlocksDefinition.BlockSizeInCells.X, (int)(lCptCell / CellsBlocksDefinition.BlockSizeInCells.Y)) * Grid.CellSize;
                    DrawTexturePro(CellDefinition.TileSheet, lBlock.Cells[lCptCell].Tile,
                    new Rectangle(lCellPos.X, lCellPos.Y, lBlock.Cells[lCptCell].Tile.Size.X, lBlock.Cells[lCptCell].Tile.Size.Y),
                    Vector2.Zero, 0, lBlock.IsFixed ? Color.White : lBlock.IsHectic ? new(255, 153, 204) : new Color(189, 255, 254));

                    //Preview de la prochaine config
                    if ((_isInPreviewSpan || lBlock.IsHectic) && !lBlock.IsFixed && lBlock.NextCells[lCptCell] != 0)
                        DrawTexturePro(CellDefinition.TileSheet, CellDefinition.PreviewTile,
                        new Rectangle(lCellPos.X, lCellPos.Y, CellDefinition.PreviewTile.Size.X, CellDefinition.PreviewTile.Size.Y),
                        Vector2.Zero, 0, lBlock.IsHectic ? new(255, 10, 0, _previewAlpha) : new(0, 200, 255, _previewAlpha));

                    //Animation de la cellule
                    lBlock.Cells[lCptCell].DrawAnim();
                }
            }
        }

        //Contours blocks au dessus
        for (int lCptY = 0; lCptY < _cellsBlocks.GetLength(0); lCptY++)
        {
            for (int lCptX = 0; lCptX < _cellsBlocks.GetLength(1); lCptX++)
            {
                //Barriere (on le fait ici pour ne pas que l'orbe soit dessinée en dessous d'un block)
                if (_cellsBlocks[lCptY, lCptX].Blockades.Count > 0) _cellsBlocks[lCptY, lCptX].DrawBarriers();

                //Contour block
                Vector2 lBlockPos = new Vector2(lCptX * CellsBlocksDefinition.BlockSizeInCells.X, lCptY * CellsBlocksDefinition.BlockSizeInCells.Y) * Grid.CellSize;
                Rectangle vCadreTexture = (_cellsBlocks[lCptY, lCptX].IsFixed && _cellsBlocks[lCptY, lCptX].TypeId == 13) ? CellsBlocksDefinition.BrokenBlockFrameUpRectTex
                    : CellsBlocksDefinition.BlockFrameUpRectTex;
                DrawTexturePro(CellDefinition.TileSheet, vCadreTexture,
                    new Rectangle(lBlockPos - Vector2.UnitY * Grid.CellSize, CellsBlocksDefinition.BlockFrameUpRectTex.Size),
                    Vector2.Zero, 0, Color.White);
            }
        }
    }

    //Exetution de l'effet d'une cellule à partir de son index dans le grid
    public void ApplyCellEffect(Vector2 pCellIndexesInGrid)
    {
        if (!Grid.IsCellInGrid(pCellIndexesInGrid)) return;

        CellsBlock lBlock = GetBlockFromCell(pCellIndexesInGrid);
        int vCellIndexesInBlock = Grid.GetCellIndexInBlock(pCellIndexesInGrid);

        int vCellId = lBlock.Cells[vCellIndexesInBlock].Id;
        lBlock.Cells[vCellIndexesInBlock].Activate();

        //Si la tuile a changée et qu'il s'agit d'un block fixe, alors on maj aussi la next cell 
        //pour ne pas avoir de reset sur les tuiles modifiées lors de la prochaine maj
        if (lBlock.Cells[vCellIndexesInBlock].Id != vCellId && lBlock.IsFixed)
            lBlock.NextCells[vCellIndexesInBlock] = lBlock.Cells[vCellIndexesInBlock].Id;
    }

    //Permet à toutes les cellules de pouvoir appliquer leur effet (appelé par le joueur quand il ne peut plus bouger pour ce beat)
    public void DeblockCellsEffect()
    {
        foreach (CellsBlock lCB in _cellsBlocks)
            foreach (Cell lCell in lCB.Cells)
                lCell.CanActivate = true;
    }

    //Vérifie si le joueur peut passer d'une cellule à une autre
    public bool CellMoveIsAllowed(Vector2 pCurrentIndexes, Vector2 pNextIndexes)
    {
        bool vIsAllowed;

        //On vérivie si pas d'obstacle
        vIsAllowed = MathF.Floor(GetCellvalue(pNextIndexes) / 10) != 6;
        if (!vIsAllowed) return false;

        //Si changement de block alors on regarde si pas de barriere entre les deux
        Vector2 vCurrentBlockPos = Grid.GetBlockIndexesFromCell(pCurrentIndexes);
        CellsBlock vCurrentBlock = _cellsBlocks[(int)vCurrentBlockPos.Y, (int)vCurrentBlockPos.X];

        if (vCurrentBlockPos != Grid.GetBlockIndexesFromCell(pNextIndexes) && vCurrentBlock.Blockades.Count > 0)
            foreach (Blockade lBlockade in vCurrentBlock.Blockades)
            {
                switch (lBlockade.BlockedDirection)
                {
                    case BlockedDirection.Up:
                        vIsAllowed = pNextIndexes.Y >= pCurrentIndexes.Y;
                        break;
                    case BlockedDirection.Down:
                        vIsAllowed = pNextIndexes.Y <= pCurrentIndexes.Y;
                        break;
                    case BlockedDirection.Right:
                        vIsAllowed = pNextIndexes.X <= pCurrentIndexes.X;
                        break;
                    case BlockedDirection.Left:
                        vIsAllowed = pNextIndexes.X >= pCurrentIndexes.X;
                        break;
                }
                if (!vIsAllowed) break;
            }


        return vIsAllowed;
    }

    public void ReInitFixedBlocks()
    {
        foreach (CellsBlock lBlock in _cellsBlocks)
            if (lBlock.IsFixed)
            {
                lBlock.ForceCollapse(lBlock.TypeId);
                lBlock.ApplyNextType();
            }
    }

    //Nettoyage, désouscription, etc 
    public void Dispose()
    {
        try
        {
            ServiceLocator.Get<AudioManager>().OnBeat -= OnBeat;
            ServiceLocator.Get<PlayerManager>().Player.ShootEvent -= OnPlayerShoot;
            ServiceLocator.Get<PlayerManager>().Player.MoveEvent -= OnPlayerMove;
        }
        catch (Exception) { }
    }

    //Récupération de valeur d'une cellule à partir de son index dans le grid
    public int GetCellvalue(Vector2 pCellIndexesInGrid)
    {
        Vector2 lBlockIndexes = Grid.GetBlockIndexesFromCell(pCellIndexesInGrid);
        if (!Grid.IsCellInGrid(pCellIndexesInGrid)) return 0;

        CellsBlock lBlock = _cellsBlocks[(int)lBlockIndexes.Y, (int)lBlockIndexes.X];
        int vCellValue = lBlock.Cells[Grid.GetCellIndexInBlock(pCellIndexesInGrid)].Id;
        return vCellValue;
    }

    public CellsBlock GetBlockFromCell(Vector2 pCellIndexesInGrid)
    {
        return _cellsBlocks[(int)Grid.GetBlockIndexesFromCell(pCellIndexesInGrid).Y, (int)Grid.GetBlockIndexesFromCell(pCellIndexesInGrid).X];
    }

    //Event du beat
    protected void OnBeat()
    {
        //Si le bon nombre de beat est passé alors on fait évoluer la map
        if (_beatWaited == 0)
        {
            //On collapse le reste des block
            CollapseAllBlocks();
            _beatWaited++;
            _isInPreviewSpan = true;
            _previewAlpha = 1;
        }
        else if (_beatWaited == 1)
        {
            //On applique l'état précédemment déterminé à tous les blocks
            ApplyNextTypes();
            _beatWaited = 0;
            //On applique l'effet de la cellule sur laquelle se trouve le joueur
            Player vPlayer = ServiceLocator.Get<PlayerManager>().Player;
            if (!vPlayer.IsBlocked) ApplyCellEffect(vPlayer.CurrentCell);
            _previewAlpha = 1;
        }
    }

    //Event de tir du joueur
    protected void OnPlayerShoot(Vector2[] pImpactedCells)
    {
        //On stock les cellules impactées pour considération dans l'update
        _shotCells = pImpactedCells;
    }

    //Event de deplacement du joueur
    protected void OnPlayerMove(Vector2 pNewCell)
    {
        if (!Grid.IsCellInGrid(pNewCell))
        {
            new Cell(0, Vector2.Zero).Activate();
            return;
        }

        CellsBlock vPlayerBlock = GetBlockFromCell(_playerCell);
        if (!vPlayerBlock.IsAllwaysFixed) vPlayerBlock.IsFixed = false;
        GetBlockFromCell(pNewCell).IsFixed = true;
        _playerCell = pNewCell;
    }

    //Initialisation des blocks et de leurs cellules sur la grille
    protected void InitBlocks()
    {
        _cellsBlocks = new CellsBlock[(int)Grid.GridSizeInBlocks.Y, (int)Grid.GridSizeInBlocks.X];
        //On initialise chaque block
        for (int lCptBlocksX = 0; lCptBlocksX < Grid.GridSizeInBlocks.X; lCptBlocksX++)
        {
            for (int lCptBlocksY = 0; lCptBlocksY < Grid.GridSizeInBlocks.Y; lCptBlocksY++)
            {
                CellsBlock lNewCellBlock = new CellsBlock(new Vector2(lCptBlocksX, lCptBlocksY));

                //On renseigne les différents voisins afin de pouvoir propager les restrictions d'états possibles lors du collapse des blocks
                if (lCptBlocksX > 0)
                {
                    lNewCellBlock.LeftNeighbour = _cellsBlocks[lCptBlocksY, lCptBlocksX - 1];
                    _cellsBlocks[lCptBlocksY, lCptBlocksX - 1].RightNeighbour = lNewCellBlock;
                }
                if (lCptBlocksY > 0)
                {
                    lNewCellBlock.TopNeighbour = _cellsBlocks[lCptBlocksY - 1, lCptBlocksX];
                    _cellsBlocks[lCptBlocksY - 1, lCptBlocksX].BottomNeighbour = lNewCellBlock;
                }

                _cellsBlocks[lCptBlocksY, lCptBlocksX] = lNewCellBlock;
            }
        }

        //On définit les blocks qui ne seront pas impactés par le collapse et auront donc un état fixe
        (Vector2, int)[] vFixedBlocks = _mapSpecifics.FixedBlocks;
        for (int lCptFixedBlock = 0; lCptFixedBlock < vFixedBlocks.Length; lCptFixedBlock++)
        {
            Vector2 vPos = vFixedBlocks[lCptFixedBlock].Item1;
            int vType = vFixedBlocks[lCptFixedBlock].Item2;
            _cellsBlocks[(int)vPos.Y, (int)vPos.X].IsFixed = true;
            _cellsBlocks[(int)vPos.Y, (int)vPos.X].IsAllwaysFixed = true;
            _cellsBlocks[(int)vPos.Y, (int)vPos.X].ForceCollapse(vType);
            _cellsBlocks[(int)vPos.Y, (int)vPos.X].ApplyNextType();
            if (vType == 11) StartingCell = Grid.GetCellIndexInGrid(4, vPos);
        }

        //On initialise les barrieres de block si il y en a
        (Vector2, BlockedDirection, Vector2)[] vBlockedBlocks = _mapSpecifics.BlockedBlocks;
        if (vBlockedBlocks != null)
        {
            Color[] vBlocadesColor = [new(255, 51, 153), new(51, 153, 255), new(0, 255, 153), new(255, 204, 0)];
            for (int lCptBlockedBlock = 0; lCptBlockedBlock < vBlockedBlocks.Length; lCptBlockedBlock++)
            {
                CellsBlock lBlockedBlock = _cellsBlocks[(int)vBlockedBlocks[lCptBlockedBlock].Item1.Y, (int)vBlockedBlocks[lCptBlockedBlock].Item1.X];
                Color lColor = vBlocadesColor[lCptBlockedBlock % vBlocadesColor.Length];
                lBlockedBlock.PutBarrier(vBlockedBlocks[lCptBlockedBlock].Item2, true, vBlockedBlocks[lCptBlockedBlock].Item3, lColor);
            }
        }

        //Blocs hectic (cad qui change à chaque beat)
        Vector2[] vHecticBlocks = _mapSpecifics.HecticBlocks;
        if (vHecticBlocks != null)
            for (int lCptHecticBlock = 0; lCptHecticBlock < vHecticBlocks.Length; lCptHecticBlock++)
            {
                Vector2 vPos = vHecticBlocks[lCptHecticBlock];
                _cellsBlocks[(int)vPos.Y, (int)vPos.X].IsHectic = true;
            }

        //On collapse tous les block (cad on détermine leurs états si non déterminés)
        CollapseAllBlocks();
        //On applique les états déterminés
        ApplyNextTypes();
    }

    //Passage des blocks dans état déterminé
    protected void CollapseAllBlocks()
    {
        // On communique les blocks fixes à leur voisinnage (pour restriction des types)
        foreach (var lBlock in _cellsBlocks)
            if (lBlock.IsFixed) lBlock.AtualiseNeighboors();

        foreach (var lBlock in _cellsBlocks)
        {
            //Maj des timers des cellules s'il y en a
            lBlock.UpdateTimers();
            if (lBlock.IsFixed) continue;

            //Les block hectics changent à chaque beat
            if (lBlock.IsHectic)
            {
                //On applique le type déterminé lors du dernier ApplyNextType
                lBlock.ApplyNextType();
                //Et on on smooth les tiles (car le voisin du dessous ne s'en chargera pas si non hectic )
                lBlock.SmoothTiles();
            }

            //On détermine le prochain type de tous les blocks non fixes 
            lBlock.Collapse();
        }
    }

    //Applique l'état déterminé à chaque block
    protected void ApplyNextTypes()
    {
        //On applique le type déterminé dans le Collapse précédent à tous les blocks non fixes
        foreach (CellsBlock lBlock in _cellsBlocks)
        {
            //Maj des timers des cellules s'il y en a
            lBlock.UpdateTimers();
            if (lBlock.IsFixed) continue;

            lBlock.ApplyNextType();
            //Les types hectics changent à chaque beat, on détermine donc ici le prochain état qui sera appliqué dans le prochain collapse
            if (lBlock.IsHectic) lBlock.Collapse();
        }

        //Les blocks fixes n'ont pas été modifiés donc leur voisins du dessus n'ont pas été smoothed automatiquement
        foreach (CellsBlock lBlock in _cellsBlocks)
            if (lBlock.IsFixed) lBlock.TopNeighbour?.SmoothTiles();
    }
}

//GRILLE
public static class Grid
{
    public static Vector2 GridSizeInBlocks;
    public static Vector2 GridSizeInCells;
    public static float CellSize;

    public static void InitGridParameters(Vector2 pGridSizeInBlocks, float pCellSize)
    {
        GridSizeInBlocks = pGridSizeInBlocks;
        GridSizeInCells = GridSizeInBlocks * CellsBlocksDefinition.BlockSizeInCells;
        CellSize = pCellSize;
    }


    public static Vector2 GetCellPosition(Vector2 pCell)
    {
        pCell = Tools.FloorVector2(pCell);
        return pCell * CellSize;
    }

    public static Vector2 GetCellFromPosition(Vector2 pPosition)
    {
        Vector2 vCell = pPosition / CellSize;
        return Tools.FloorVector2(vCell);
    }

    public static Vector2 GetBlockIndexesFromCell(Vector2 pCellIndexesInGrid)
    {
        Vector2 vBlock = Tools.FloorVector2(pCellIndexesInGrid / CellsBlocksDefinition.BlockSizeInCells);
        return vBlock;
    }

    public static int GetCellIndexInBlock(Vector2 pCellIndexesInGrid)
    {
        return (int)(pCellIndexesInGrid.X % CellsBlocksDefinition.BlockSizeInCells.X
            + pCellIndexesInGrid.Y % CellsBlocksDefinition.BlockSizeInCells.Y * CellsBlocksDefinition.BlockSizeInCells.X);
    }
    public static Vector2 GetCellIndexInGrid(int pCellIndexInBlock, Vector2 pBlockIndexes)
    {
        return pBlockIndexes * CellsBlocksDefinition.BlockSizeInCells
        + new Vector2(pCellIndexInBlock % CellsBlocksDefinition.BlockSizeInCells.X, MathF.Floor(pCellIndexInBlock / CellsBlocksDefinition.BlockSizeInCells.X));
    }

    public static bool IsCellInGrid(Vector2 pCell)
    {
        bool vIsInCell = (int)pCell.Y >= 0 && (int)pCell.Y < GridSizeInCells.Y && (int)pCell.X >= 0 && (int)pCell.X < GridSizeInCells.X;
        return vIsInCell;
    }

}