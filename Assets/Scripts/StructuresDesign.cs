using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MazeElement
{
    OpenSpace = 0,
    Wall = 1,
    StartPoint = 5,
    ExitPoint = 8
}

public class MazeMap
{
    public int[,] MazeMatrix { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 StartPoint { get; set; }
    public Vector2 ExitPoint { get; set; }

    public MazeMap(int[,] mapMatrix)
    {
        bool startPointFound = false, exitPointFound = false;
        MazeMatrix = mapMatrix;
        Size = new Vector2(mapMatrix.GetLength(0), mapMatrix.GetLength(1));
        for (int row = 0; row < Size.x; row++)
        {
            for (int colum = 0; colum < Size.y; colum++)
            {
                int element = mapMatrix[row, colum];
                switch ((MazeElement)element)
                {
                    case MazeElement.StartPoint:
                        StartPoint = new Vector2(row, colum);
                        break;
                    case MazeElement.ExitPoint:
                        ExitPoint = new Vector2(row, colum);
                        break;
                    default:
                        break;
                }

                if (startPointFound && exitPointFound)
                    break;
            }

            if (startPointFound && exitPointFound)
                break;
        }
    }
}

public struct Gene
{
    /// <summary>
    /// Nucleotides are the minium unit, can be 0 or 1;
    /// </summary>
    public List<int> Nucleotides { get; set; }
    public int Lenght { get { return Nucleotides.Count; } }

    public static Gene GetRandom(int geneSize)
    {
        Gene gene = new Gene
        {
            Nucleotides = new List<int>()
        };
        for (int index = 0; index < geneSize; index++)
        {
            int nucleotide = UnityEngine.Random.Range(0, 2);
            gene.Nucleotides.Add(nucleotide);
        }

        return gene;
    }

    public int Decode()
    {
        string binary = this.ToString();
        int code = Convert.ToInt32(binary, 2);
        return code;
    }

    public override string ToString()
    {
        string gen = string.Empty;
        for (int index = 0; index < Nucleotides.Count; index++)
        {
            gen += Nucleotides[index].ToString();
        }
        return gen;
    }
}

public struct Chromosome
{
    public List<Gene> Genes { get;set; }
    public int Lenght { get { return Genes.Count; } }

    public static Chromosome GetRandom(int chromoLenght, int genesLenght)
    {
        Chromosome chromosome = new Chromosome
        {
            Genes = new List<Gene>()
        };
        for (int index = 0; index < chromoLenght; index++)
        {
            chromosome.Genes.Add(Gene.GetRandom(genesLenght));
        }
        return chromosome;
    }

    public override string ToString()
    {
        string genes = string.Empty;
        for (int index = 0; index < Genes.Count; index++)
        {
            genes += Genes[index].ToString();
        }
        return genes;
    }
}

public struct Genome
{
    public List<Chromosome> Chromosomes { get; set; }
    public int Lenght { get { return Chromosomes.Count; } }

    public static Genome GetRandom(int genomeLeght = 1, int chromosLeght = 70, int genesLenght = 2)
    {
        Genome genome = new Genome
        {
            Chromosomes = new List<Chromosome>()
        };
        for (int index = 0; index < genomeLeght; index++)
        {
            genome.Chromosomes.Add(Chromosome.GetRandom(chromosLeght, genesLenght));
        }
        return genome;
    }

    public override string ToString()
    {
        string genome = string.Empty;
        for (int index = 0; index < Chromosomes.Count; index++)
        {
            genome = Chromosomes[index].ToString();
        }
        return genome;
    }
}
