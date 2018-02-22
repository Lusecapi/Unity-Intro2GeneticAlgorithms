# Unity-Intro2GeneticAlgorithms
An Unity Project that shows the using of genetic algorithms for finding paths of mazes

## Quick biology class

**Chromosomes** are made up of blocks of **genes** constructed by nucleotides (A, G, C, T). Genes determine certain features of the organism
such as hair color, or the shape of the ears. The different configurations that can have for example (brown, blond, black hair) are known
as **alleles** and their position in the DNA chain is called **locus**.

The collection of chromosomes within a cell contains all the information necessary to reproduce that organism. This collection of chromosomes
is known as the **genome** of the organism. The state of the alleles in a particular genome is known as the **genotype** of that organism.
These are the instructions that can be applied to the real organism itself, which are called **phenotype**.

## Representation in this project

- **Genome:** just one Chromosome (the route)
- **Chromosomes:** has *chromosomesLenght* amount of genes with the instructions (i.e 111010010101011000...)
- **Gene:** is the particular instruction
- **Nucleotide:** 0 or 1

### Chromosomes representation

1001101001110000010111010100101111010010111001...

### Alleles
the alleles in this project represent the directions

00 => 0 = North

01 => 1 = East

10 => 2 = South

11 => 3 = West

## How Algorithm Works

Create the initial population randomly.
*Loop until solution is found*

  1. Test each chromosome to see how good it is at solving the problem and assign 
  fitness score acordingly.
  2. Select two members from the current population. The selection method depends on the
  selected method
  3 Dependent on the *crossover rate*, crossover the genes from each chosen chromosome at
  a randomly chosen point
  4. Step trhough the chosen chromosome's bits (Nucleotides) and flip dependent on the
  *mutation rate*
  5. Repeat steps 2,3 and 4 until a new population of *population size*  has been created
  
  *End Loop*
  
  The algorithm is executed until the loop reaches the amount of epochs or a solution is found.

## Algorithm Variables

* **allowRepahtMove : bool**

  When true, it allows the algorithm to repeat positions when creating/searchin path, walk by positions that it previously walked

* s**howFittestByEpoch : bool**

  When true, algorithm will show the fittest genome(route) by each generation, the path will has magenta color.
  also shows the all time fittest in yellow color.
  
* **totalEpochs : int**

  The maximun number of times/cycles that algorithm will be executed or will try to find a solution
  
* **populationSize : int**

  The amount of genomes by each iteration (epoch)
  
* **reproductionMode : enum**
  
  - *parentsByEpoch*
  
    The two selected parents creates all the new population (this was just for test purposes, not recomended)
    
  - *parentsByChild*
  
    For every child (in this project we create two childs per parents) we select new parents (could be the same, depends on the parent selection method)

* **parentSelectionMethod : enum**

  - *Random*
  
    Parents are random genomes picked from the population
    
   - *Roulette Wheel*
   
    Parents are selected with RouletteWheelSelection method (algorithm extracted from the book) from the population
    
   - *FittestAndRandom*
   
    One parent is the fittest genome of the generation and the other is selected randomly from the population
    
   - *FittestAndRouletteWheel*
   
    One parent is the fittest genome of generation and the other is selected using RouletteWheelSelection method from the population
    
    - *RandomOfSurvivors*
    
      Parents are selected randomly from the survivors population (the amount of survivors depends by epoch on de survivorsRate variable)

    - *RouletteWheelOfSurvivors*
    
      Parents are selected with RouletteWheelSelection method from the survivors population
      
     - *Fittest2OfPop*
     
      Parents are the two fittest genomes (was only for test purposes, not recomended)

* **survivorsRange : float**

  The percentage of the population that survive and are able to be selected as parents (only used when is selected a parent selection mode that involve survivors population)

* **crossoverRate : float**

  The probability that babies results from the parent crossover
  
* **mutationRate : float**

  The proability that a nucleotide (bit) mutates, resulting in new allele (or a gene mutates)
  
* **chromosomesLenght : int**

  The amount of genes per chromosome


### Some conclusions of project

* The algorithm sometimes never found a solution, It happens when it reach a point very close to the maze exit point and then, a significant number of generations pass without finding the solution. The new population begin to be less fittest and degenerates with time (The complete species dies/extinct)

* *ParentByGeneration* is a bad method to use, always use *ParentsByChild* method. Genomes degenerate fast and there is not heritage between generation.

* The best results always of reproduction mode were RandomOfSurvivors and Random (still don't know why :smile:). The survivors range at 0.3 (fittest 30% of the population), and with a population at Range [150 - 500]. Greater the populations is, greater probability of finding a solution.

* Chromosomes lenght at 30 or 35 are good values. Greater chromosomes lenght is, greater probability of finding a solution.
