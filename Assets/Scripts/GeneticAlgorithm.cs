using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour {

    MazeGenerator mazeGenerator;
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


    private void Awake()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();
    }

    private void Start()
    {
        maze = new MazeMap(mazeMatrix);
        mazeGenerator.GenerateMaze(maze);
        Genome genome = Genome.GetRandom();
        print(genome);
        Gene gene = genome.Chromosomes[0].Genes[1];
        print(gene);
        print(gene.Decode());

    }

    IEnumerator StartAlgorithm()
    {
        yield return new WaitForSeconds(1);
    }
}
