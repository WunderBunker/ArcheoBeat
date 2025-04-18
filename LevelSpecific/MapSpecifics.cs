using System.Numerics;

public struct MapSpecific
{
    public Vector2 GridSizeInBlocks;
    public (Vector2, int)[] FixedBlocks;
    public (Vector2, BlockedDirection, Vector2)[] BlockedBlocks;
    public Vector2[] HecticBlocks;


    //- pGridSizeInBlocks : matrice contentant {numéro du type de block si fixe, sinon -1}
    //- pFixedBlocksMatrix : matrice contentant {(1,2,3 ou 4 pour la direction, Vector2 pour les indexes de la cellules où se trouve l'orbe)}
    //- pHecticBlocksMatrix : matrice contentant {true si hectic}
    public MapSpecific(Vector2 pGridSizeInBlocks, int[,] pFixedBlocksMatrix, (byte, Vector2)?[,] pBlockedBlocksMatrix = null, bool[,] pHecticBlocksMatrix = null)
    {
        List<(Vector2, int)> vFixedBlocks = new();
        for (int lCptY = 0; lCptY < pFixedBlocksMatrix.GetLength(0); lCptY++)
            for (int lCptX = 0; lCptX < pFixedBlocksMatrix.GetLength(1); lCptX++)
                if (pFixedBlocksMatrix[lCptY, lCptX] != -1) vFixedBlocks.Add((new Vector2(lCptX, lCptY), pFixedBlocksMatrix[lCptY, lCptX]));

        List<Vector2> vHecticBlocks = new();
        if (pHecticBlocksMatrix != null)
            for (int lCptY = 0; lCptY < pHecticBlocksMatrix.GetLength(0); lCptY++)
                for (int lCptX = 0; lCptX < pHecticBlocksMatrix.GetLength(1); lCptX++)
                    if (pHecticBlocksMatrix[lCptY, lCptX]) vHecticBlocks.Add(new Vector2(lCptX, lCptY));

        List<(Vector2, BlockedDirection, Vector2)> vBlockedBlocks = new();
        if (pBlockedBlocksMatrix != null)
            for (int lCptY = 0; lCptY < pBlockedBlocksMatrix.GetLength(0); lCptY++)
                for (int lCptX = 0; lCptX < pBlockedBlocksMatrix.GetLength(1); lCptX++)
                    if (pBlockedBlocksMatrix[lCptY, lCptX] != null)
                        switch (pBlockedBlocksMatrix[lCptY, lCptX]?.Item1)
                        {
                            case 1:
                                vBlockedBlocks.Add((new Vector2(lCptX, lCptY), BlockedDirection.Right, (Vector2)(pBlockedBlocksMatrix[lCptY, lCptX]?.Item2)));
                                break;
                            case 2:
                                vBlockedBlocks.Add((new Vector2(lCptX, lCptY), BlockedDirection.Down, (Vector2)(pBlockedBlocksMatrix[lCptY, lCptX]?.Item2)));
                                break;
                            case 3:
                                vBlockedBlocks.Add((new Vector2(lCptX, lCptY), BlockedDirection.Left, (Vector2)(pBlockedBlocksMatrix[lCptY, lCptX]?.Item2)));
                                break;
                            case 4:
                                vBlockedBlocks.Add((new Vector2(lCptX, lCptY), BlockedDirection.Up, (Vector2)(pBlockedBlocksMatrix[lCptY, lCptX]?.Item2)));
                                break;
                            default:
                                break;
                        }

        GridSizeInBlocks = pGridSizeInBlocks;
        FixedBlocks = vFixedBlocks.ToArray();
        BlockedBlocks = vBlockedBlocks.ToArray();
        HecticBlocks = vHecticBlocks.ToArray();
    }

}
public static class MapSpecifics
{

    public static MapSpecific L1 = new
    (
        new Vector2(5, 5),
        new int[,]{
        {11, 0,  3,  13, 13},
        {13, -1, 4,  13, 13},
        {13, 14, -1, 13, 13},
        {13, 18, 13, 13, 13},
        {13, 2, -1, -1, 12}},
        null,
        null
    );


    public static MapSpecific L2 = new
    (
        new Vector2(8, 8),
        new int[,]{
        {11, 14, 18, 13, 13, 13,13},
        {13, 13, 2,  13, 13, 13,13},
        {13, -1, 9,  -1, 13, 13,13},
        {13, 21, -1, -1, 0,  -1,13},
        {13, 3,  13, 2,  13, -1,13},
        {13, -1, -1, -1, 8,  10,13},
        {13, 13, 13, 13, 13, -1,13},
        {13, 13, 13, 13, 12, -1,13},
        },
        null,
        new bool[,]{
        {false, false, false, false, false,false,false,false},
        {false, false, false, false, false,false,false,false},
        {false, false, false, false, false,false,false,false},
        {false, false, false, false, false,false,false,false},
        {false, false, false, false, false,true,false,false},
        {false, false, false, true, false,false,false,false},
        {false, false, false, false, false,false,false,false},
        {false, false, false, false, false,false,true,false},}
    );

    public static MapSpecific L3 = new
    (
        new Vector2(7, 5),
        new int[,]{
        {13, 13, 13, 13, 2,  14, -1},
        {13, 13, 13, 13, 17, 13,  12},
        {11, 0,  3,  13, 0,  4,  13},
        {13, 13, 4,  21,  -1, 14, 13},
        {13, 13, 13, -1, -1, -1, 13},
        },
        null,
        new bool[,]{
        {false, false, false, false, false,false,false},
        {false, false, false, false, false,false,false},
        {false, false, false, false, false,false,false},
        {false, false, false, false, true,false,false},
        {false, false, false, false, false,false,false},
        {false, false, false, false, false,false,false},}
    );

    public static MapSpecific L4 = new
    (
        new Vector2(10, 3),
        new int[,]{
        {13, 13, 13, 13, 13, 5,  0,  13, 13, 13},
        {11, -1, 20, 19, -1, 20, 21, -1, -1, 12},
        {13, 13, 2,  1,  13, 13, 13, -1, 13, 13}},
        new (byte, Vector2)?[,]{
        {null, null, null,                  null, null, null,                 null ,null, null,                  null},
        {null, null, (1,new Vector2(12,8)), null, null, (1,new Vector2(22,0)), null ,null, (1,new Vector2(24,7)),null},
        {null, null, null,                  null, null, null,                 null ,null, null,                  null}},
        null
    );
    public static MapSpecific L5 = new
    (
        new Vector2(8, 8),
        new int[,]{
        {13, 13, 13, 11, 13,13,13,13},
        {13, 13, 13, -1, 13,13,13,13},
        {13, -1, -1, 10, -1,-1,13,13},
        {13, -1, 13, 14, 13,-1,-1,14},
        {13, 14, 13, 17, 13,13,13,13},
        {13, 13, 13, 18, 13,13,13,13},
        {13, 13, 13, 14, -1, -1,3 ,13},
        {13, 13, 13, 12, 13,13,13,13},},
        new (byte, Vector2)?[,]{
        {null, null, null, null, null, null, null, null},
        {null, null, null, null, null, null, null, null},
        {null, null, null, (2,new Vector2(22,10)), (3,new Vector2(4,13)), null, null, null},
        {null, null, null, null, null, null, null, null},
        {null, null, null, null, null, null, null, null},
        {null, null, null, null, null, null, null, null},
        {null, null, null, (2,new Vector2(20,20)), null, null, null, null},
        {null, null, null, null, null, null, null, null},
        },
        new bool[,]{
        {false, false, false, false, false,false,false,false},
        {false, false, false, false, false,false,false,false},
        {false, false, false, false, false,false,false,false},
        {false, false, false, false, false,false,true,false},
        {false, false, false, false, false,false,false, false},
        {false, false, false, false,  false,false,false, false},
        {false, false, false, false, false,true,false,false},
        {false, false, false, false, false,false,false, false},}
    );
    public static MapSpecific L6 = new
    (
        new Vector2(5, 5),
        new int[,]{
        {11, -1, -1, -1, -1},
        {-1, -1, 10, -1, -1},
        {-1, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1},
        {-1, -1, -1, -1, 12}},
        new (byte, Vector2)?[,]{
        {null, (1,new Vector2(2,4)), null,                 null, null},
        {null, (3,new Vector2(1,3)), null,                 null, null},
        {null, (2,new Vector2(5,3)), (4,new Vector2(6,6)), null, null},
        {null, null,                 null,                 null, null},
        {null, null,                 null,                 null, null}},
        new bool[,]{
        {false, false, false, false, false},
        {true, false, true, false, false},
        {false, false, false, false, false},
        {false, false, false, false, false},
        {false, false, false, false, false}}
    );
    public static MapSpecific L7 = new
    (
        new Vector2(5, 5),
        new int[,]{
        {11, -1, -1, -1, -1},
        {-1, -1, 10, -1, -1},
        {-1, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1},
        {-1, -1, -1, -1, 12}},
        new (byte, Vector2)?[,]{
        {null, (1,new Vector2(2,4)), null,                 null, null},
        {null, (3,new Vector2(1,3)), null,                 null, null},
        {null, (2,new Vector2(5,3)), (4,new Vector2(6,6)), null, null},
        {null, null,                 null,                 null, null},
        {null, null,                 null,                 null, null}},
        new bool[,]{
        {false, false, false, false, false},
        {true, false, true, false, false},
        {false, false, false, false, false},
        {false, false, false, false, false},
        {false, false, false, false, false}}
    );
    public static MapSpecific L8 = new
    (
        new Vector2(5, 5),
        new int[,]{
        {11, -1, -1, -1, -1},
        {-1, -1, 10, -1, -1},
        {-1, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1},
        {-1, -1, -1, -1, 12}},
        new (byte, Vector2)?[,]{
        {null, (1,new Vector2(2,4)), null,                 null, null},
        {null, (3,new Vector2(1,3)), null,                 null, null},
        {null, (2,new Vector2(5,3)), (4,new Vector2(6,6)), null, null},
        {null, null,                 null,                 null, null},
        {null, null,                 null,                 null, null}},
        new bool[,]{
        {false, false, false, false, false},
        {true, false, true, false, false},
        {false, false, false, false, false},
        {false, false, false, false, false},
        {false, false, false, false, false}}
    );
}
