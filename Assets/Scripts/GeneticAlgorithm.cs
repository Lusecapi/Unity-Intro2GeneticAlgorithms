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


    //the population of genomes
    List<Genome> population;
    //size of population
    public int totalEpochs = 100;
    public int populationSize = 140;
    public float crossoverRate = 0.7f;
    public float mutationRate = 0.001f;
    //how many bits per chromosome
    int chromosomesLenght = 35;
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
        generationText.text = string.Format("Generation: {0}", generation);
        maze = new MazeMap(mazeMatrix);
        mazeGenerator.GenerateMaze(maze);
        StartCoroutine(StartAlgorithm());
    }

    void CreateStartPopulation()
    {
        population = new List<Genome>();
        for (int index = 0; index < populationSize; index++)
        {
            population.Add(Genome.GetRandom(1, chromosomesLenght, genesLenght));
        }
    }

    private IEnumerator ShowPath(List<Vector2> route)
    {
        isShowingPath = true;
        for (int index = 0; index < route.Count; index++)
        {
            yield return new WaitForEndOfFrame();
            Gene gene = fittestRouteGenes[index];
            Vector2 coordinate = route[index];
            mazeGenerator.TilesMatrix[(int)coordinate.x, (int)coordinate.y].color = Color.yellow;
            yield return new WaitForSeconds(0.3f);
        }
        print("Finish");
        isShowingPath = false;
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
                maze.TestRoute(ref genome, out routeGenes, out routeCoordinates);
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

            if(generationBestFitnessScore > bestFitnessScore)
            {
                ClearPath(bestRoute);
                fittestGenomeIndex = generationFittesGenome;
                bestFitnessScore = generationBestFitnessScore;
                bestRoute = generationBestRoute;
                fittestRouteGenes = generationFittestRouteGenes;
                StartCoroutine(ShowPath(bestRoute));
                yield return new WaitUntil(() => !isShowingPath);
            }

            if (!soulutionFound)
            {
                //Reproduction
                int newBabies = 0;
                List<Genome> babiesPopulation = new List<Genome>();
                while (newBabies < populationSize)
                {
                    Genome mom = RoulleteWheelSelection();
                    Genome dad = RoulleteWheelSelection();
                    Genome baby;
                    Crossover(mom, dad, out baby);
                    Mutate(ref baby);
                    babiesPopulation.Add(baby);
                    newBabies++;
                }
                population = babiesPopulation;

                epoch++;
                yield return 1;
            }
            else
            {
                print("solution found");
            }
        }
        print("Algoritmo terminado");
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
        Genome referent = (int)Random.Range(0, 2) == 0 ? mom : dad;
        if (Random.Range(0,1f) > crossoverRate || mom.Equals(dad))
        {
            baby = referent;
            return;
        }

        //A random point is chosen along the length of the chromosome to split the chromosomes
        int splitPoint = Random.Range(0, chromosomesLenght);
        baby = new Genome(1, chromosomesLenght);
        for (int index = 0; index < splitPoint; index++)
        {
            baby.Chromosomes[0].Genes.Add(referent.Chromosomes[0].Genes[index]);
        }
        referent = referent.Equals(dad) ? mom : dad;
        for (int index = splitPoint; index < chromosomesLenght; index++)
        {
            baby.Chromosomes[0].Genes.Add(referent.Chromosomes[0].Genes[index]);
        }
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
            Gene gene = fittestRouteGenes[index];
            Vector2 coordinate = route[index];
            mazeGenerator.TilesMatrix[(int)coordinate.x, (int)coordinate.y].color = Color.white;
        }
    }
}
