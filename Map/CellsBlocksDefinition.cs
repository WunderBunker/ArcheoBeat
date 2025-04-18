using System.Numerics;
using Raylib_cs;

//DEFINITION DES STRUCTURES ET PROPRIETES DES BLOCKS
public static class CellsBlocksDefinition
{
    //Taille d'un block en nombre de cellules
    public static Vector2 BlockSizeInCells = new Vector2(3, 3);

    //Rectangle de Texture correspondant au dessous d'un block (cf tileSheet dans CellDefinition)
    public static Rectangle BlockFrameDownRectTex = new Rectangle(new Vector2(8, 1) * CellDefinition.Tilesize, BlockSizeInCells * CellDefinition.Tilesize);
    //Rectangle de Texture correspondant au dessus d'un block (cf tileSheet dans CellDefinition)
    public static Rectangle BlockFrameUpRectTex = new Rectangle(new Vector2(8, 4.1f) * CellDefinition.Tilesize, (BlockSizeInCells + Vector2.UnitY) * CellDefinition.Tilesize);

    //Matrices de compatibilité des types de block (comprendre block de type i peut avoir pour voisin de gauche les types dans LeftCompatibilityMatrix[i] )
    public static int[][] LeftIncompatibilityMatrix =
    {
        //0
        [1,3,6,8,10,18,20],
        //1
        [0,3,5,8,10,17,19],
        //2
        [0,1,5,6,7,8,9,10,14,15,16,17,18,19,20],
        //3
        [],
        //4
        [0,1,3,6,8,10,18,20],
        //5
        [],
        //6
        [],
        //7
        [0,3,5,8,10,17,19],
        //8
        [],
        //9
        [0,1,3,5,6,17,19,20],
        //10
        [0,1,5,6,15,17,18,20],
        //11
        [],
        //12
        [],
        //13
        [],
        //14
        [3],
        //15
        [3],
        //16
        [3],
        //17
        [0,3,5,8,10,19],
        //18
        [1,3,6,20],
        //19
        [0,3,5,140,17,19,20],
        //20
        [1,3,6,8,10,18],
        //21
        []
    };
    public static int[][] TopIncompatibilityMatrix =
    {
        //0
        [1],
        //1
        [2,3,6,7,8,9,10,14,16,17,18,19,20],
        //2
        [0,3,6,8,10,17,19],
        //3
        [0,2,4,5,9,10,18,20],
        //4
        [],
        //5
        [],
        //6
        [0,2,4,9,10,18,20],
        //7
        [0,2,4,9,10,17,19],
        //8
        [0,2,4,9,10,18,20],
        //9
        [0,2,4,9,10,17,19],
        //10
        [0,2,3,4,5,8,9,16,17,18],
        //11
        [],
        //12
        [],
        //13
        [],
        //14
        [0],
        //15
        [],
        //16
        [0,10],
        //17
        [0,3,5,8,10,19],
        //18
        [0,2,4,9,10,20],
        //19
        [0,3,5,8,17],
        //20
        [0,2,4,9,18],
        //21
        []
    };



    //Definition des différents types de blocks et des cellules qui les composent
    /* Mode d'emploi Cells Id :
        Premier Digit - type de cellule, dans l'ordre : 
            0 - Vide ; 1- ground, 2 - Start, 3 - End ; 4- ClosedTrapDoor 5 - OppenningTrapDoor ; 6 - obstacles 
        Second Digit - Forme de la Cellule : 
            0 - Carré plein, 1 - Pont centre, 2 - Pont Gauche, 3 - Pont Droit
    */
    public static int[][] Blocks =
    {
        //0
        [
        12,11,13,
        00,00,00,
        00,00,00
        ],
        //1
        [
        00,00,00,
        00,00,00,
        12,11,13
        ],
        //2
        [
        00,00,10,
        00,00,10,
        00,00,60
        ],
        //3
        [
        10,00,00,
        40,00,00,
        60,00,00
        ],
        //4
        [
        62,13,40,
        00,00,10,
        00,00,10
        ],
        //5
        [
        60,12,13,
        10,00,00,
        10,00,00
        ],
        //6
        [
        10,00,00,
        10,00,00,
        61,41,13
        ],
        //7
        [
        00,00,10,
        00,00,10,
        12,11,43
        ],
        //8
        [
        10,00,00,
        60,62,13,
        10,00,00
        ],
        //9
        [
        00,00,10,
        12,43,10,
        00,00,10
        ],
        //10
        [
        00,10,00,
        12,60,13,
        00,10,00
        ],
         //11 - SPECIAL : Start
        [
        10,10,10,
        10,20,10,
        12,11,13
        ],
         //12 - SPECIAL : End
        [
        10,10,10,
        10,30,10,
        12,11,13
        ],
        //13 - SPECIAL : pas de possibilité
        [
        00,00,00,
        00,00,00,
        00,00,00
        ],
        //14
        [
        10,41,60,
        10,00,40,
        42,11,63
        ],
        //15
        [
        62,11,13,
        00,00,00,
        12,41,13
        ],
        //16
        [
        10,00,10,
        40,00,60,
        12,00,13
        ],
        //17
        [
        00,00,40,
        60,11,13,
        12,00,00
        ],
        //18
        [
        60,00,00,
        12,41,10,
        00,00,13
        ],
        //19
        [
        00,10,43,
        00,40,00,
        12,13,00
        ],
        //20
        [
        42,10,00,
        00,60,00,
        00,12,13
        ],
        //21
        [
        10,10,10,
        10,70,10,
        12,11,13
        ],
    };

}