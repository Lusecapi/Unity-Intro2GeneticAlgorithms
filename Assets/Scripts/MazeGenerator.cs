using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    public bool generateFromEditor;
    public Transform tilePrefab;
    [Range(0,1)]
    public float outlinePercet = 0.1f;

    private Color openSpaceColor = Color.white;
    private Color wallColor = Color.black;
    private Color startPointColor = Color.blue;
    private Color exitPointColor = Color.green;

    public Material[,] TilesMatrix { get; set; }

    //--------Solo de prueba
    private MazeMap maze;
    private int[,] mazeMatrix = { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                            { 1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                            { 8, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                            { 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1 },
                            { 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1 },
                            { 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1 },
                            { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1 },
                            { 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 5 },
                            { 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 },
                            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };
    public void GenerateMazeFromEditor()
    {
        GenerateMaze(maze, true);
    }
    //-------------------------


    public void GenerateMaze(MazeMap maze, bool calledFromEditor = false)
    {
        string holderName = "Maze";
        if (GameObject.Find(holderName))
            DestroyImmediate(GameObject.Find(holderName));

        Renderer tileRender = tilePrefab.GetComponent<Renderer>();
        Vector2 tileSize = new Vector2(tileRender.bounds.size.x, tileRender.bounds.size.z);
        GameObject mazeGameObject = new GameObject(holderName);
        if(!calledFromEditor)
            TilesMatrix = new Material[(int)maze.Size.x, (int)maze.Size.y];
        for (int row = 0; row < maze.Size.x; row++)
        {
            for (int colum = 0; colum < maze.Size.y; colum++)
            {
                Vector3 tilePosition = new Vector3(-maze.Size.x / 2 + (row * tileSize.x), 0, -maze.Size.y / 2 +(colum * tileSize.y));
                Transform tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.localScale = tile.localScale * (1 - outlinePercet);
                tile.SetParent(mazeGameObject.transform);

                Material tileMat;
                if (!calledFromEditor)
                {
                    tileMat = tile.GetComponent<Renderer>().material;
                    TilesMatrix[row, colum] = tileMat;
                    tileMat.color = GetTileColor(maze.MazeMatrix[row, colum]);
                }
            }
        }
    }

    private Color GetTileColor(int tileValue)
    {
        switch (tileValue)
        {
            case 1:
                return wallColor;
            case 5:
                return startPointColor;
            case 8:
                return exitPointColor;
            default:
                return openSpaceColor;
        }
    }


}
