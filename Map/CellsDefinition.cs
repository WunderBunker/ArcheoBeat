using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

//DEFINITION DES PROPRIETES DES CELLULES DE BLOCKS
public static class CellDefinition
{
    public static Texture2D TileSheet { get; private set; }
    public static Vector2 Tilesize { get; private set; }
    public static Rectangle PreviewTile { get; private set; }
    static Rectangle[] _tiles;

    static CellDefinition()
    {
        Tilesize = new Vector2(64, 64);
        TileSheet = LoadTexture("images/tileSheet.png");
        Vector2 vSheetSizeInTiles = new Vector2(TileSheet.Width, TileSheet.Height) / Tilesize;

        //On prédécoupe la tilesheet en rectangles de la taille d'une cellule
        _tiles = new Rectangle[(int)(vSheetSizeInTiles.X * vSheetSizeInTiles.Y)];
        for (int lCptTileY = 0; lCptTileY < vSheetSizeInTiles.Y; lCptTileY++)
        {
            for (int lCptTileX = 0; lCptTileX < vSheetSizeInTiles.X; lCptTileX++)
            {
                _tiles[(int)(lCptTileX + lCptTileY * vSheetSizeInTiles.X)] = new Rectangle(lCptTileX * Tilesize.X, lCptTileY * Tilesize.Y, Tilesize.X, Tilesize.Y);
            }
        }

        PreviewTile = _tiles[51];
    }

    /* Mode d'emploi Id :
       Premier Digit - type de cellule, dans l'ordre : 
           0 - Vide ; 1- ground, 2 - Start, 3 - End ; 4- ClosedTrapDoor 5 - OppenningTrapDoor ; 6 - Wall ; 7- checkPoint
       Second Digit - Forme de la Cellule : 
           0 - Carré plein, 1 - Pont centre, 2 - Pont Gauche, 3 - Pont Droit
    */

    //Récupère le rectangle de texture correspondant à un type de cellule dans la tilesheet
    public static Rectangle GetTile(int pCellId)
    {
        switch (pCellId)
        {
            case 00:
                return _tiles[0];
            case 10:
                return _tiles[19];
            case 11:
                return _tiles[17];
            case 12:
                return _tiles[16];
            case 13:
                return _tiles[18];
            case 20:
                return _tiles[138];
            case 21:
                return _tiles[17];
            case 22:
                return _tiles[16];
            case 23:
                return _tiles[18];
            case 30:
                return _tiles[137];
            case 31:
                return _tiles[17];
            case 32:
                return _tiles[16];
            case 33:
                return _tiles[18];
            case 40:
                return _tiles[49];
            case 41:
                return _tiles[47];
            case 42:
                return _tiles[46];
            case 43:
                return _tiles[48];
            case 50:
                return _tiles[79];
            case 51:
                return _tiles[77];
            case 52:
                return _tiles[76];
            case 53:
                return _tiles[78];
            case 60:
                return _tiles[109];
            case 61:
                return _tiles[107];
            case 62:
                return _tiles[106];
            case 63:
                return _tiles[108];
            case 70:
                return _tiles[139];
            default:
                return _tiles[0];
        }
    }

}