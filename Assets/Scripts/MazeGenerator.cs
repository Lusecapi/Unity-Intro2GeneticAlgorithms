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

    private int[,] defaultMaze = { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                            { 1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                            { 8, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                            { 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1 },
                            { 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1 },
                            { 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1 },
                            { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1 },
                            { 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 5 },
                            { 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 },
                            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };

    private void Start()
    {
        GenerateMaze(defaultMaze);
    }

    public void GenerateMazeFromEditor()
    {
        GenerateMaze(defaultMaze, true);
    }


    public void GenerateMaze(int[,] maze, bool calledFromEditor = false)
    {
        string holderName = "Maze";
        if (GameObject.Find(holderName))
            DestroyImmediate(GameObject.Find(holderName));

        Renderer tileRender = tilePrefab.GetComponent<Renderer>();
        Vector2 tileSize = new Vector2(tileRender.bounds.size.x, tileRender.bounds.size.z);
        Vector2 mazeSize = new Vector2(maze.GetLength(0), maze.GetLength(1));
        GameObject mazeGameObject = new GameObject(holderName);

        for (int row = 0; row < mazeSize.x; row++)
        {
            for (int colum = 0; colum < mazeSize.y; colum++)
            {
                Vector3 tilePosition = new Vector3(-mazeSize.x / 2 + (row * tileSize.x), 0, -mazeSize.y / 2 +(colum * tileSize.y));
                Transform tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tile.localScale = tile.localScale * (1 - outlinePercet);
                tile.SetParent(mazeGameObject.transform);

                if(!calledFromEditor)
                    tile.GetComponent<Renderer>().material.color = GetTileColor(maze[row, colum]);
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
