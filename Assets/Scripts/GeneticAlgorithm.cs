using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlgorithm : MonoBehaviour {

    public Text generationText;
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
    public bool allowMoveOverPath;
    public bool showFittestByEpoch;
    public int totalEpochs = 100;
    public int populationSize = 140;
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
    int fittestGenomeIndex;
    List<Gene> fittestRouteGenes = new List<Gene>();
    List<Vector2> bestRoute = new List<Vector2>();
    float bestFitnessScore = 0;
    float totalFitnessScore = 0;
    int generation = 0;
    MazeMap maze;

    private bool isShowingPath;


    private void Awake()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();
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
        yield return new WaitForEndOfFrame();
        //Epoch
        bool soulutionFound = false;
        int epoch = 0;
        while(epoch < totalEpochs && !soulutionFound)
        {
            float generationBestFitnessScore = 0;
            int generationFittesGenome = 0;
            List<Vector2> generationBestRoute = new List<Vector2>();
            List<Gene> generationFittestRouteGenes = new List<Gene>();
            totalFitnessScore = 0;
            generation = epoch;
            generationText.text = string.Format("Generation: {0}", generation);
            //UpdateFitnessScore
            for (int genomeIndex = 0; genomeIndex < populationSize; genomeIndex++)
            {
                Genome genome = population[genomeIndex];
                List<Gene> routeGenes;
                List<Vector2> routeCoordinates;
                maze.TestRoute(ref genome, out routeGenes, out routeCoordinates, allowMoveOverPath);
                totalFitnessScore += genome.Fitness;
                if(genome.Fitness > generationBestFitnessScore)
                {
                    generationFittesGenome = genomeIndex;
                    generationBestFitnessScore = genome.Fitness;
                    generationBestRoute = routeCoordinates;
                    generationFittestRouteGenes = routeGenes;
                }

                /*if(genome.Fitness > bestFitnessScore)
                {
                    
                }*/
                //yield return null;

                if (bestFitnessScore == 1) { soulutionFound = true; break; }
            }

            if (showFittestByEpoch)
            {
                StartCoroutine(ShowPath(generationBestRoute, false));
                yield return new WaitUntil(() => !isShowingPath);
                yield return new WaitForEndOfFrame();
            }

            if (generationBestFitnessScore > bestFitnessScore)
            {
                fittestGenomeIndex = generationFittesGenome;
                bestFitnessScore = generationBestFitnessScore;
                bestRoute = generationBestRoute;
                fittestRouteGenes = generationFittestRouteGenes;
                print("Found new best in Generation/Epoch: "+generation+"\n" + population[fittestGenomeIndex]);
                ClearPath(bestRoute);
                StartCoroutine(ShowPath(bestRoute, true));
                yield return new WaitUntil(() => !isShowingPath);
                yield return new WaitForEndOfFrame();
            }

            if (!soulutionFound)
            {
                //Reproduction
                int newBabies = 0;
                List<Genome> babiesPopulation = new List<Genome>();
                //Genome mom = population[fittestGenomeIndex];
                Genome mom;
                Genome dad;
                while (newBabies < populationSize)
                {
                    mom = RoulleteWheelSelection();
                    dad = RoulleteWheelSelection();
                    Genome baby;
                    Crossover(mom, dad, out baby);
                    Mutate(ref baby);
                    babiesPopulation.Add(baby);
                    newBabies++;
                }
                population = babiesPopulation;

                epoch++;
                yield return null;

                if (showFittestByEpoch)
                    ClearPath(generationBestRoute);
            }
            else
            {
                print("Solution Found");
            }
        }
        print("Process Completed");
    }

    void CreateStartPopulation()
    {
        population = new List<Genome>();
        for (int index = 0; index < populationSize; index++)
        {
            population.Add(Genome.GetRandom(1, chromosomesLenght, genesLenght));
        }
    }

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

    private Genome RoulleteWheelSelection()
    {
        int selectedGenome = 0;
        float slice = Random.Range(0, totalFitnessScore);
        float total = 0;
        for (int index = 0; index < populationSize; index++)
        {
            total = population[index].Fitness;
            if(total > slice)
            {
                selectedGenome = index;
                break;
            }
        }
        return population[selectedGenome];
    }

    private void Crossover(Genome mom, Genome dad, out Genome baby)
    {
        //print("mom\n" + mom);
        //print("dad\n" + dad);
        Genome referent = Random.Range(0, 2) == 0 ? mom : dad;
        //print("referent1\n"+referent);
        if (Random.Range(0,1f) > crossoverRate || mom.Equals(dad))
        {
            //print("no hay cruce");
            baby = referent;
            //print("baby\n"+baby);
            return;
        }
        //print("hay cruce");
        //A random point is chosen along the length of the chromosome to split the chromosomes
        int splitPoint = Random.Range(0, chromosomesLenght);
        //print("split at: " + splitPoint);
        baby = new Genome(1, chromosomesLenght);
        //print("unboorned baby:\n" + baby);
        for (int index = 0; index < splitPoint; index++)
        {
            baby.Chromosomes[0].Genes[index] = referent.Chromosomes[0].Genes[index];
        }
        referent = referent.Equals(mom) ? dad : mom;
        //print("referent2\n" + referent);
        for (int index = splitPoint; index < chromosomesLenght; index++)
        {
            baby.Chromosomes[0].Genes[index] = referent.Chromosomes[0].Genes[index];
        }
        //print("baby\n" + baby);
    }

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
