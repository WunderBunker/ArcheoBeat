using System.Numerics;
using Raylib_cs;

//CELLULE D'UN BLOCK
public class Cell
{
    public int Id { get; private set; }
    public Rectangle Tile { get; private set; }
    public Vector2 CellIndexInGrid { get; private set; }
    public bool CanActivate = true;
    List<Sprite> _anims;
    int _beatTimer;



    public Cell(int pId, Vector2 pCellIndexesInGrid)
    {
        //Type de la cellule
        Id = pId;
        //Rectangle de texture dans la tilesheet pour ce type de cellule
        Tile = CellDefinition.GetTile(Id);

        CellIndexInGrid = pCellIndexesInGrid;

        Init();
    }

    //Changement du type de la cellule
    public void ChangeId(int pNewId)
    {
        Unload();
        Id = pNewId;
        Tile = CellDefinition.GetTile(Id);
        Init();
    }


    //Initilaisation spécifique au type de cellule
    public void Init()
    {
        switch (Id / 10)
        {
            case 2:
                _anims = [
                    new("images/startBackAnim.png", Grid.GetCellPosition(CellIndexInGrid),4, 10),
                    new("images/startFrontAnim.png", Grid.GetCellPosition(CellIndexInGrid),4, 10)];
                _anims[0].Color = new(1, 0.2f, 0.6f, 0.5f);
                _anims[1].Color = new(0, 0.8f, 1, 0.5f);
                Sprite.AddOrderedSprite(_anims[0], 0);
                Sprite.AddOrderedSprite(_anims[1], 1);
                break;
            case 3:
                _anims = [
                        new("images/startBackAnim.png", Grid.GetCellPosition(CellIndexInGrid),4, 10),
                        new("images/startFrontAnim.png", Grid.GetCellPosition(CellIndexInGrid),4, 10),
                        new("images/endAnim.png", Grid.GetCellPosition(CellIndexInGrid) - Vector2.UnitY*CellDefinition.Tilesize/2, 5, 10)];
                _anims[0].Color = new Color(1, 0.2f, 0.6f, 0.5f);
                _anims[1].Color = new Color(1, 0.9f, 0, 0.5f);

                Sprite.AddOrderedSprite(_anims[0], 0);
                Sprite.AddOrderedSprite(_anims[1], 1);
                break;
            default:
                return;
        }
    }

    //Dechargement spécifique au type de cellule
    public void Unload()
    {
        switch (Id / 10)
        {
            case 2:
                Sprite.RemoveOrderedSprite(_anims[0], 0);
                Sprite.RemoveOrderedSprite(_anims[1], 1);
                _anims = null;
                break;
            case 3:
                Sprite.RemoveOrderedSprite(_anims[0], 0);
                Sprite.RemoveOrderedSprite(_anims[1], 1);
                _anims = null;
                break;
            case 7:
                if (_anims == null) return;
                Sprite.RemoveOrderedSprite(_anims[0], 0);
                Sprite.RemoveOrderedSprite(_anims[1], 1);
                _anims = null;
                break;
            default:
                return;
        }
    }

    //Activation de l'effet d'un type Cellule si il y en a
    public void Activate()
    {
        if (!CanActivate) return;
        CanActivate = false;
        switch (Id / 10)
        {
            case 0:
                ServiceLocator.Get<PlayerManager>().KillPlayer();
                break;
            case 3:
                Console.WriteLine("VICTORY !!!");
                ((SceneLevel)ServiceLocator.Get<SceneManager>().CurrentScene).WinLevel();
                break;
            case 4:
                ChangeId(50 + Id % 10);
                ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.littleCrackSounds, 1f);
                _beatTimer = 4;
                break;
            case 5:
                ChangeId(0);
                ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.BigCrackSounds, 0.8f);
                ServiceLocator.Get<PlayerManager>().KillPlayer();
                _beatTimer = 4;
                break;
            case 6:
                ChangeId(10 + Id % 10);
                ServiceLocator.Get<AudioManager>().PlaySoundFx(FXSounds.ObstacleBreakSounds, 0.8f);
                break;
            case 7:
                if (_anims != null) return;

                ServiceLocator.Get<MapManager>().StartingCell = CellIndexInGrid;
                _anims = [
                    new("images/startBackAnim.png", Grid.GetCellPosition(CellIndexInGrid),4, 10),
                    new("images/startFrontAnim.png", Grid.GetCellPosition(CellIndexInGrid),4, 10)];
                _anims[0].Color = new(0.87f, 1, 0.33f, 0.5f);
                _anims[1].Color = new(0.33f, 1, 0.56f, 0.5f);
                Sprite.AddOrderedSprite(_anims[0], 0);
                Sprite.AddOrderedSprite(_anims[1], 1);
                break;
            default:
                return;
        }
    }

    public void UpdateTimer()
    {
        if (_beatTimer == 0) return;

        _beatTimer--;
        if (_beatTimer == 0)
        {
            switch (Id / 10)
            {
                //Les cases craquelées se regénèrent au bout d'un certain temps pour les blocks fixes
                case 5:
                    if (!ServiceLocator.Get<MapManager>().GetBlockFromCell(CellIndexInGrid).IsFixed) break;
                    ChangeId(40 + Id % 10);
                    break;
                default:
                    break;
            }
        }
    }

    public void UpdateAnim()
    {
        if (_anims != null)
            foreach (Sprite lSprite in _anims) lSprite.Update();
    }
    public void DrawAnim()
    {
        if (_anims != null && Id / 10 != 2)
            foreach (Sprite lSprite in _anims) lSprite.Draw();
    }
}
