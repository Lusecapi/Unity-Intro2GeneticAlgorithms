using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum ReproductionMode
{
    ParentsByChild = 0,
    ParentsByEpoch = 1
}

public enum ParentSelectionMethod
{
    Random = 0,
    RoulleteWheel = 1,
    FittestAndRandom = 2,
    FittestAndRoulleteWheel = 3,
    RandomOfSurvivors = 4,
    RoulletteWheelOfSurvivors = 5,
    Fittest2OfPop = 6
}

public class GeneticAlgorithm : MonoBehaviour {

    Text epochsText;
    MazeGenerator mazeGenerator;
    private int[,] mazeMatrix = { 
                            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                            { 1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                            { 5, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
                            { 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1 },
                            { 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1 },
                            { 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1 },
                            { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1 },
                            { 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 8 },
                            { 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1 },
                            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };


    //size of population
    public bool allowRepathMove;
    public bool showFittestByEpoch;
    public int totalEpochs = 100;
    public int populationSize = 140;
    public ReproductionMode reproductionMode;
    public ParentSelectionMethod parentSelectionMethod;
    [Range(0.1f, 0.9f)]
    public float survivorsRange = 0.1f;
    [Range(0.01f, 0.99f)]
    public float crossoverRate = 0.7f;
    [Range(0.001f,0.99f)]
    public float mutationRate = 0.001f;
    //how many bits per chromosome
    public int chromosomesLenght = 35;

    //the population of genomes
    List<Genome> population;
    //how many bits per gene
    int genesLenght = 2;
    int fittestGenomeIndex = 0;
    List<Gene> fittestRouteGenes = new List<Gene>();
    List<Vector2> bestRoute = new List<Vector2>();
    float bestFitnessScore = 0;
    float totalFitnessScore = 0;
    int generation = 0;
    bool solutionFound;
    MazeMap maze;

    private bool isShowingPath;
    private bool fitnessScoresUpdated;


    private void Awake()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();
        epochsText = GameObject.FindGameObjectWithTag("Epoch Text").GetComponent<Text>();
    }

    private void Start()
    {
        maze = new MazeMap(mazeMatrix);
        mazeGenerator.GenerateMaze(maze);
        StartCoroutine(StartAlgorithm());
    }

    IEnumerator StartAlgorithm()
    {
        yield return new WaitForSeconds(0.3f);

        CreateStartPopulation();
        int epoch = 0;
        while (epoch < totalEpochs && !solutionFound)
        {
            generation = epoch;
            epochsText.text = string.Format("Generation: {0}", generation);
            
            //Update Fitness Scores
            StartCoroutine(UpdateFitnessScores());
            yield return new WaitUntil(() => fitnessScoresUpdated);

            if (!solutionFound)
            {
                //Reproduce genomes to create new Genertion
                ReproduceGeneration();
                epoch++;
                yield return null;
            }
            else
            {
                print("Solution Found");
            }
        }
        print("Process Completed");
    }

    /// <summary>
    /// Creates a initial random population with size = populationSize
    /// </summary>
    private void CreateStartPopulation()
    {
        population = new List<Genome>();
        for (int index = 0; index < populationSize; index++)
        {
            population.Add(Genome.GetRandom(1, chromosomesLenght, genesLenght));
        }
    }

    /// <summary>
    /// Test every genome of the population and assign its correspondent fittest value, 
    /// Also select fittest genome of population and check if its the best of all time
    /// And verify if it a solution or not
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateFitnessScores()
    {
        fitnessScoresUpdated = false;

        float epochBestFitnessScore = 0;
        int epochFittestGenomeIndex = 0;
        List<Vector2> epochBestRoute = new List<Vector2>();
        List<Gene> epochBestRouteGenes = new List<Gene>();
        totalFitnessScore = 0;

        for (int genomeIndex = 0; genomeIndex < populationSize; genomeIndex++)
        {
            Genome genome = population[genomeIndex];
            List<Gene> routeGenes;
            List<Vector2> routeCoordinates;
            maze.TestRoute(ref genome, out routeGenes, out routeCoordinates, allowRepathMove);
            totalFitnessScore += genome.Fitness;
            if (genome.Fitness > epochBestFitnessScore)
            {
                epochFittestGenomeIndex = genomeIndex;
                epochBestFitnessScore = genome.Fitness;
                epochBestRoute = routeCoordinates;
                epochBestRouteGenes = routeGenes;
            }

            if (epochBestFitnessScore == 1)
            {
                solutionFound = true;
                break;//We finish search for the fittest genome
            }
        }

        //If showFittestByEpoch = true, show epoch fittest genome route
        if (showFittestByEpoch)
        {
            StartCoroutine(ShowPath(epochBestRoute, false));
            yield return new WaitUntil(() => !isShowingPath);
        }

        if (epochBestFitnessScore > bestFitnessScore)
        {
            if (showFittestByEpoch)
                ClearPath(epochBestRoute);

            ClearPath(bestRoute);
            fittestGenomeIndex = epochFittestGenomeIndex;
            bestFitnessScore = epochBestFitnessScore;
            bestRoute = epochBestRoute;
            fittestRouteGenes = epochBestRouteGenes;
            print("Found new best in Generation/Epoch: " + generation + "\n" + population[fittestGenomeIndex]);
            StartCoroutine(ShowPath(bestRoute, true));
            yield return new WaitUntil(() => !isShowingPath);
        }

        if (!solutionFound)
        {
            if (showFittestByEpoch)
                ClearPath(epochBestRoute);
        }

        fitnessScoresUpdated = true;
    }

    /// <summary>
    /// Shows the route
    /// </summary>
    /// <param name="coordinatesRoute"></param>
    /// <param name="allTimeBest"></param>
    /// <returns></returns>
    private IEnumerator ShowPath(List<Vector2> coordinatesRoute, bool allTimeBest)
    {
        isShowingPath = true;
        Color roadColor, stopColor;
        roadColor = allTimeBest ? Color.yellow : Color.magenta;
        stopColor = Color.red;
        for (int index = 0; index < coordinatesRoute.Count; index++)
        {
            yield return new WaitForEndOfFrame();
            //Gene gene = fittestRouteGenes[index];
            Vector2 coordinate = coordinatesRoute[index];
            Color color = index == coordinatesRoute.Count - 1 ? stopColor : roadColor;
            mazeGenerator.TilesMatrix[(int)coordinate.x, (int)coordinate.y].color = color;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForEndOfFrame();
        isShowingPath = false;
    }

    /// <summary>
    /// Create breeds for the new population
    /// </summary>
    private void ReproduceGeneration()
    {
        int newBabies = 0;
        List<Genome> babiesPopulation = new List<Genome>();
        Genome mom = new Genome(1, chromosomesLenght);
        Genome dad = new Genome(1, chromosomesLenght);
        if(reproductionMode == ReproductionMode.ParentsByChild)
        {
            while (newBabies < populationSize)
            {
                GetParents(ref mom, ref dad);
                Genome baby1;
                Genome baby2;
                Crossover(mom, dad, out baby1, out baby2);
                Mutate(ref baby1);
                Mutate(ref baby2);
                babiesPopulation.Add(baby1);
                babiesPopulation.Add(baby2);
                newBabies += 2;
            }
        }
        else
            if(reproductionMode == ReproductionMode.ParentsByEpoch)
        {
            GetParents(ref mom, ref dad);
            while (newBabies < populationSize)
            {
                Genome baby1;
                Genome baby2;
                Crossover(mom, dad, out baby1, out baby2);
                Mutate(ref baby1);
                Mutate(ref baby2);
                babiesPopulation.Add(baby1);
                babiesPopulation.Add(baby2);
                newBabies += 2;
            }
        }

        //List<Genome> allPopulation = new List<Genome>();
        //allPopulation.AddRange(babiesPopulation);
        //List<Genome> sortedList = allPopulation.OrderByDescending(genome => genome.Fitness).ToList();
        population.Clear();
        //population.AddRange(sortedList);
        //population.RemoveRange(sortedList.Count, population.Count - sortedList.Count);
        population = babiesPopulation;

    }

    /// <summary>
    /// Get the parents to reproduces based on the ReproductionMethod selected
    /// </summary>
    /// <param name="mom"></param>
    /// <param name="dad"></param>
    private void GetParents(ref Genome mom, ref Genome dad)
    {
        Genome referent;
        int survivorsLenght;
        List<Genome> sortedPopulation = new List<Genome>();
        switch (parentSelectionMethod)
        {
            case ParentSelectionMethod.Random:
                mom = population[Random.Range(0, population.Count)];
                dad = population[Random.Range(0, population.Count)];
                break;
            case ParentSelectionMethod.RoulleteWheel:
                mom = RoulleteWheelSelection(population, populationSize);
                dad = RoulleteWheelSelection(population, populationSize);
                break;
            case ParentSelectionMethod.FittestAndRandom:
                referent = Random.Range(0, 2) == 0 ? mom : dad;
                if (referent.Equals(mom))
                {
                    mom = population[fittestGenomeIndex];
                    dad = population[Random.Range(0, population.Count)];
                }
                else
                {
                    dad = population[fittestGenomeIndex];
                    mom = population[Random.Range(0, population.Count)];
                }
                break;
            case ParentSelectionMethod.FittestAndRoulleteWheel:
                referent = Random.Range(0, 2) == 0 ? mom : dad;
                if (referent.Equals(mom))
                {
                    mom = population[fittestGenomeIndex];
                    dad = RoulleteWheelSelection(population, populationSize);
                }
                else
                {
                    dad = population[fittestGenomeIndex];
                    mom = RoulleteWheelSelection(population, populationSize);
                }
                break;
            case ParentSelectionMethod.RandomOfSurvivors:
                sortedPopulation = population.OrderByDescending(genome => genome.Fitness).ToList();
                survivorsLenght = (int)(populationSize * survivorsRange);
                mom = sortedPopulation[Random.Range(0, survivorsLenght)];
                dad = sortedPopulation[Random.Range(0, survivorsLenght)];
                break;
            case ParentSelectionMethod.RoulletteWheelOfSurvivors:
                sortedPopulation = population.OrderByDescending(genome => genome.Fitness).ToList();
                survivorsLenght = (int)(populationSize * survivorsRange);
                mom = RoulleteWheelSelection(sortedPopulation, survivorsLenght);
                dad = RoulleteWheelSelection(sortedPopulation, survivorsLenght);
                break;
            case ParentSelectionMethod.Fittest2OfPop:
                sortedPopulation = population.OrderByDescending(genome => genome.Fitness).ToList();
                referent = Random.Range(0, 2) == 0 ? mom : dad;
                if (referent.Equals(mom))
                {
                    mom = sortedPopulation[0];
                    dad = sortedPopulation[1];
                }
                else
                {
                    dad = sortedPopulation[0];
                    mom = sortedPopulation[1];
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// A kind of randomly selection method
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private Genome RoulleteWheelSelection(List<Genome> collection, int length)
    {
        int selectedGenome = 0;
        if(collection != population)
        {
            totalFitnessScore = 0;
            for (int index = 0; index < length; index++)
            {
                totalFitnessScore += collection[index].Fitness;
            }
        }
        float slice = Random.Range(0, totalFitnessScore);
        float total = 0;
        for (int index = 0; index < length; index++)
        {
            total = collection[index].Fitness;
            if(total > slice)
            {
                selectedGenome = index;
                break;
            }
        }
        return collection[selectedGenome];
    }

    /// <summary>
    /// Cross two genomes for a new genome, in this case, two new babies
    /// </summary>
    /// <param name="mom"></param>
    /// <param name="dad"></param>
    /// <param name="baby1"></param>
    /// <param name="baby2"></param>
    private void Crossover(Genome mom, Genome dad, out Genome baby1, out Genome baby2)
    {
        //print("mom\n" + mom);
        //print("dad\n" + dad);
        //Genome referent = Random.Range(0, 2) == 0 ? mom : dad;
        //print("referent1\n"+referent);
        if (Random.Range(0,1f) > crossoverRate || mom.Equals(dad))
        {
            //print("no hay cruce");
            baby1 = mom;
            baby2 = dad;
            //baby1 = referent;
            //baby2 = referent.Equals(mom) ? dad : mom;
            //print("baby\n"+baby);
            return;
        }
        //print("hay cruce");
        //A random point is chosen along the length of the chromosome to split the chromosomes
        int splitPoint = Random.Range(0, chromosomesLenght);
        //print("split at: " + splitPoint);
        baby1 = new Genome(1, chromosomesLenght);
        baby2 = new Genome(1, chromosomesLenght);
        //print("unboorned baby:\n" + baby);
        for (int index = 0; index < splitPoint; index++)
        {
            //baby1.Chromosomes[0].Genes[index] = referent.Chromosomes[0].Genes[index];
            baby1.Chromosomes[0].Genes[index] = mom.Chromosomes[0].Genes[index];
            baby2.Chromosomes[0].Genes[index] = dad.Chromosomes[0].Genes[index];
        }
        //referent = referent.Equals(mom) ? dad : mom;
        //print("referent2\n" + referent);
        for (int index = splitPoint; index < chromosomesLenght; index++)
        {
            baby1.Chromosomes[0].Genes[index] = dad.Chromosomes[0].Genes[index];
            baby2.Chromosomes[0].Genes[index] = mom.Chromosomes[0].Genes[index];
        }
        //print("baby\n" + baby);
    }

    /// <summary>
    /// Mutate a genome
    /// </summary>
    /// <param name="baby"></param>
    private void Mutate(ref Genome baby)
    {
        for (int gene = 0; gene < baby.Chromosomes[0].Lenght; gene++)
        {
            for (int bit = 0; bit < baby.Chromosomes[0].Genes[gene].Lenght; bit++)
            {
                if(Random.Range(0,1f) < mutationRate)
                {
                    baby.Chromosomes[0].Genes[gene].Nucleotides[bit] = baby.Chromosomes[0].Genes[gene].Nucleotides[bit] == 0 ? 1 : 0;
                }
            }
        }
    }

    /// <summary>
    /// Clear a path previously shown
    /// </summary>
    /// <param name="route"></param>
    private void ClearPath(List<Vector2> route)
    {
        for (int index = 0; index < route.Count; index++)
        {
            //Gene gene = fittestRouteGenes[index];
            Vector2 coordinate = route[index];
            mazeGenerator.TilesMatrix[(int)coordinate.x, (int)coordinate.y].color = Color.white;
        }
    }
}
