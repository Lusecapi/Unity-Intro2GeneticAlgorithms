using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MazeElement
{
    OpenSpace = 0,
    Wall = 1,
    StartPoint = 5,
    ExitPoint = 8,
    Path = 3
}

public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public class MazeMap
{
    public int[,] MazeMatrix { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 StartPoint { get; set; }
    public Vector2 ExitPoint { get; set; }

    private int[,] pathMemory;

    public MazeMap(int[,] mapMatrix)
    {
        bool startPointFound = false, exitPointFound = false;
        MazeMatrix = mapMatrix;
        Size = new Vector2(mapMatrix.GetLength(0), mapMatrix.GetLength(1));
        for (int row = 0; row < Size.x; row++)
        {
            for (int colum = 0; colum < Size.y; colum++)
            {
                int element = MazeMatrix[row, colum];
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

    public void TestRoute(ref Genome genome, out List<Gene> routeGenes, out List<Vector2> routeCoordinates, bool allowMoveOverPath)
    {
        pathMemory = new int[(int)Size.x, (int)Size.y];
        Vector2 actualPos = StartPoint;
        //Genome.Extraction genomeExtraction = new Genome.Extraction();
        routeCoordinates = new List<Vector2>();
        routeGenes = new List<Gene>();

        for (int index = 0; index < genome.Chromosomes[0].Lenght; index++)
        {
            //Gene gene = genome.GetNextGene(ref genomeExtraction);
            Gene gene = genome.Chromosomes[0].Genes[index];
            Direction dir = (Direction)gene.Decode();
            Vector2 movement = GetMovement(dir);
            Vector2 newPos;
            TryToMove(actualPos, movement, out newPos, allowMoveOverPath);
            if (actualPos != newPos)
            {
                routeGenes.Add(gene);
                actualPos = newPos;
                pathMemory[(int)newPos.x, (int)newPos.y] = (int)MazeElement.Path;
                routeCoordinates.Add(actualPos);
            }
        }

        //set the fitnees score
        Vector2 diff = new Vector2(Mathf.Abs(actualPos.x - ExitPoint.x), Mathf.Abs(actualPos.y - ExitPoint.y));
        genome.Fitness = 1 / (diff.x + diff.y + 1);
    }

    private Vector2 GetMovement(Direction direction)
    {
        Vector2 movement = Vector2.zero;
        switch (direction)
        {
            case Direction.North:
                movement = new Vector2(-1, 0);
                break;
            case Direction.East:
                movement = new Vector2(0, 1);
                break;
            case Direction.South:
                movement = new Vector2(1, 0);
                break;
            case Direction.West:
                movement = new Vector2(0, -1);
                break;
            default:
                break;
        }
        return movement;
    }

    private void TryToMove(Vector2 actualPos, Vector2 movement, out Vector2 newPos, bool allowMoveOverPath)
    {
        Vector2 initialPos = actualPos;
        newPos = actualPos + movement;
        //Verify that new pos is inside matrix
        if ((newPos.x > -1 && newPos.x < MazeMatrix.GetLength(0)) && (newPos.y > 0 && newPos.y < MazeMatrix.GetLength(1)))
        {
            //verify that newPos is a wall
            if(MazeMatrix[(int)newPos.x, (int)newPos.y] == (int)MazeElement.Wall)
            {
                newPos = initialPos;
            }

            if (!allowMoveOverPath)
            {
                if (pathMemory[(int)newPos.x, (int)newPos.y] == (int)MazeElement.Path)
                {
                    newPos = initialPos;
                }
            }
        }
        else
        {
            newPos = initialPos;
        }
    }
}

public class Gene
{
    /// <summary>
    /// Nucleotides are the minium unit, can be 0 or 1;
    /// </summary>
    public List<int> Nucleotides { get; set; }
    public int Lenght { get { return Nucleotides.Count; } }
    //public static Gene Null = new Gene { Nucleotides = new List<int>() };

    public Gene() { }

    public Gene(int totNucleotides)
    {
        Nucleotides = new List<int>();
        for (int index = 0; index < totNucleotides; index++)
        {
            Nucleotides.Add(new int());
        }
    }

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

    public override bool Equals(object obj)
    {
        Gene other = (Gene)obj;
        if (other == null)
            return false;
        
        if(Lenght != other.Lenght)
            return false;

        for (int index = 0; index < Lenght; index++)
        {
            if (Nucleotides[index] != other.Nucleotides[index])
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class Chromosome
{
    public List<Gene> Genes { get;set; }
    public int Lenght { get { return Genes.Count; } }

    public Chromosome() { }

    public Chromosome(int totGenes)
    {
        Genes = new List<Gene>();
        for (int index = 0; index < totGenes; index++)
        {
            Genes.Add(new Gene(2));
        }
    }

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

    public override bool Equals(object obj)
    {
        Chromosome other = (Chromosome)obj;
        if (other == null)
            return false;

        if (Lenght != other.Lenght)
            return false;

        for (int index = 0; index < Lenght; index++)
        {
            if (!Genes[index].Equals(other.Genes[index]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class Genome
{
    public List<Chromosome> Chromosomes { get; set; }
    public float Fitness { get; set; }
    public int Lenght { get { return Chromosomes.Count; } }
    /*public static Genome Null = new Genome
    {
        Chromosomes = new List<Chromosome>(),
        Fitness = -1
    };*/
    public Genome() { }

    public Genome(int totChromos, int totGenes)
    {
        Chromosomes = new List<Chromosome>();
        for (int index = 0; index < totChromos; index++)
        {
            Chromosomes.Add(new Chromosome(totGenes));
        }
        Fitness = 0;
    }

    public static Genome GetRandom(int genomeLeght = 1, int chromosLeght = 35, int genesLenght = 2)
    {
        Genome genome = new Genome
        {
            Fitness = 0,
            Chromosomes = new List<Chromosome>()
        };
        for (int index = 0; index < genomeLeght; index++)
        {
            genome.Chromosomes.Add(Chromosome.GetRandom(chromosLeght, genesLenght));
        }
        return genome;
    }

    public Gene GetNextGene(ref Genome.Extraction extrationInfo)
    {
        if (extrationInfo.CanStract)
        {
            Gene g = Chromosomes[extrationInfo.ChromosomeIndex].Genes[extrationInfo.GeneIndex];
            extrationInfo.GeneIndex++;
            if (extrationInfo.GeneIndex == Chromosomes[extrationInfo.ChromosomeIndex].Lenght)
            {
                extrationInfo.ChromosomeIndex++;
                if (extrationInfo.ChromosomeIndex == Lenght)
                {
                    extrationInfo.ChromosomeIndex = -1;
                    extrationInfo.GeneIndex = -1;
                    extrationInfo.CanStract = false;
                }
                else
                {
                    //Continue with extraction
                    extrationInfo.GeneIndex = 0;
                }
            }
            return g;
        }
        else
        {
            return null;
        }
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

    public override bool Equals(object obj)
    {
        Genome other = (Genome)obj;
        if (other == null)
            return false;

        if (Lenght != other.Lenght)
            return false;

        for (int index = 0; index < Lenght; index++)
        {
            if (!Chromosomes[index].Equals(other.Chromosomes[index]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public class Extraction
    {
        public int ChromosomeIndex { get; set; }
        public int GeneIndex { get; set; }
        public bool CanStract { get; set; }

        public Extraction()
        {
            ChromosomeIndex = 0;
            GeneIndex = 0;
            CanStract = true;
        }
    }
}
