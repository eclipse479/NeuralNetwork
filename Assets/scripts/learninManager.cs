using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class learninManager : MonoBehaviour
{
    private GameObject menu;
    private mainMenu1 menuScript;
    //buttons for saving/loading
    public Button saveButton;
    public Button loadButton;

    //Gameobject prefabs
    public GameObject carPrefab;// the car
    public GameObject startingPos; // where the car spawns(should be an empy object)

    //time untill next generation starts
    public Text timerText; // text to display the timer
    private string timeLeftString; // allows getting the time left in 2 decimal places
    private float timeLeft;// actual time left
    //population
    public int populationSize = 50; // size of the population
    private int newPopSize;
    private int tenthOfPopulation; // a number that is 10% of the population size
    //Population text
    public Text populationSizeText;

    //generation
    public Text generation; // text that displays the generation number
    private int generationNumber = 0;//what number generation this generation is
    public float generationLength; //Time a generation lasts for
    public Text generationLengthText;
    //network
    private List<network> neuralNetworkList; // list of the neural networks
    private int[] layers =  new int[] {3, 4, 5, 4, 3, 2}; // how many layers/neurons in each layer(no bias neurons)
    //
    private List<carNetwork> carList; //list of the cars generated based on the population size
    private bool training = false; // is the current generation training

    //total fitness stuff for parent finding
    private bool topFitnessFound = false;
    private float totalTopFitness;

    private bool previousChampionLoaded = false;

    // Start is called before the first frame update

    private void Awake()
    {
        menu = GameObject.FindWithTag("menus"); 
        menuScript = menu.GetComponent<mainMenu1>();
        populationSize = menuScript.pop;
        generationLength = menuScript.genLength;
    }
    void Start()
    {
        populationSizeText.text = "Population: " + populationSize;
        generationLengthText.text = "Generation length: " + generationLength;
        newPopSize = populationSize;
        totalTopFitness = 0;
        if(populationSize < 20) // minimum population size
        {
            populationSize = 20;
        }
        //forces the population to be a multiple of 10
        while (populationSize % 10 != 0)
        {
            populationSize++;//increase pop size until it is a multiple of 10
        }
        tenthOfPopulation = populationSize / 10; // gets 10% of the population(will be used for 'breeding')
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!training)
        {
            if(newPopSize > populationSize)
            {
                //adds to the back of the network list
                populationSize = newPopSize;
                tenthOfPopulation = populationSize / 10;
                initCarsNetwork();
            }
            else if(newPopSize < populationSize)
            {
                //removes from the front of the network
                for(int i = populationSize; i > newPopSize; i--)
                {
                neuralNetworkList.RemoveAt(0); // removes the first element of the array
                }
                newPopSize = populationSize;
                tenthOfPopulation = populationSize / 10;
            }
            if (generationNumber == 0)
            {
                initCarsNetwork(); //create the network the 
            }
            else
            {
                neuralNetworkList[neuralNetworkList.Count - 1].champ = false; // turn off previous champion
                neuralNetworkList.Sort(); // neuralNetworkList[start] -> worst ----- neuralNetworkList[end] -> best
                //top 3 of the generation
                Debug.Log("Champion of generation " + generationNumber + " Fitness: " + neuralNetworkList[neuralNetworkList.Count - 1].getFitness());
                Debug.Log("Runner up of generation " + generationNumber + " Fitness: " + neuralNetworkList[neuralNetworkList.Count - 2].getFitness());
                Debug.Log("Third place of generation " + generationNumber + " Fitness: " + neuralNetworkList[neuralNetworkList.Count - 3].getFitness());
                if(!topFitnessFound)
                {
                    totalFitness();
                }
                for (int i = 0; i < populationSize * 0.9f; i++) //for the bottom 90%
                {
                    // top 10% was not changed
                    //creates new networks with the bottom 90% of cars
                    if(i < tenthOfPopulation) //makes the first 10% of cars have random values
                    {
                        neuralNetworkList[i] = new network(layers);
                        neuralNetworkList[i].israndomStart(); //make red
                    }
                    else //for the 10% - 90% of population create children from 2 parents in the top 10%
                    {
                        network parent1 = new network(layers);
                        network parent2 = new network(layers);
                        parent1 = findRandomParent();//finds a random parent
                        parent2 = findRandomParent();//finds another random parent
                        neuralNetworkList[i] = parent1.createChild(parent2); //creates new by splicing 2 networks together
                        //mutate the new networks
                        neuralNetworkList[i].mutate(); // add in mutation
                    }
                }
                //resets top 10% of cars
                for (int i = tenthOfPopulation * 9; i < populationSize; i++)
                {
                    neuralNetworkList[i] = new network(neuralNetworkList[i]);//copies self so as to reset the network
                }
                // resets the fitness of the networks
                for (int i = 0; i < populationSize; i++) 
                {
                    neuralNetworkList[i].setFitness(0.0f);
                }
                //set last neuron after sort to be the champion (to change colours)
                neuralNetworkList[neuralNetworkList.Count - 1].makeChamp();
                //if a previous network was loaded
                if(previousChampionLoaded)
                {
                    readText();
                    neuralNetworkList[0].wasLoaded();
                }
                previousChampionLoaded = false;
                topFitnessFound = false; // set to false for the next generation
            }
            //generation tracking
            generationNumber++;
            generation.text = "Generation: " + generationNumber;
            Debug.Log("Generation " + generationNumber + " start");
            generationLengthText.text = "Generation length: " + generationLength;

            //resets total fitness
            totalTopFitness = 0;
            //time until next generation is started
            training = true;
            timeLeft = generationLength;

            //creates the cars
            createCars();

            //waits 'generationLength' seconds then does the "timer" function
            Invoke("timer", generationLength); 
        }

        //keeps track of the time left for the user to view
        timeLeft -= Time.deltaTime;
        timeLeftString = timeLeft.ToString("F2");
        timerText.text = "Time left: " + timeLeftString;
        checkAllDead();

    }

    /// <summary>
    /// gets the toatl fitness of the top 50% of the networks
    /// </summary>
    private void totalFitness() // will be called if all agents have crashed or when time is up -> finds top 50% networks total fitness
    {
        for(int i = neuralNetworkList.Count - 1; i > populationSize * 0.5f; i--)
        {
            totalTopFitness += neuralNetworkList[i].getFitness();
        }
        topFitnessFound = true;
    }
    // total fitness() && findRandomParent() are used in conjunction
    /// <summary>
    /// chooses a random network using the total fitness found
    /// </summary>
    /// <returns></returns>
    private network findRandomParent()//finds a random network to be a parent form the networks list with larger fitness values having a higher chance
    {
        float rand = Random.Range(0, totalTopFitness);
        network foundParent = new network(layers);//created placeholder for parent

        // goes through top 50% of networks and reduces total fitness equivelent until it is 0 then chooses the parent 
        for (int i = neuralNetworkList.Count - 1; i > populationSize * 0.5f; i--) 
        {
            rand -= neuralNetworkList[i].getFitness();
            if (rand < 0)
            {
                foundParent = neuralNetworkList[i]; //parent has been found
                i = 0; // ends for loop early
            }
        }
        return foundParent; //returns the parent found
    }
    /// <summary>
    /// checks if all cars have died and will start the next generation earlier if thye are all dead
    /// </summary>
    void checkAllDead() // ends a generation early if all population members have crashed
    { 
            float howManyCrashed = 0;
        for (int i = 0; i < populationSize; i++)
        {
            if(neuralNetworkList[i].hasCrashed)//checks all networks to see if they have crashed
            {
                howManyCrashed++;
            }
            else if(!neuralNetworkList[i].hasCrashed) 
            {
                i = populationSize;//ends loop early if it finds a network that has not crashed
            }
            if(howManyCrashed == neuralNetworkList.Count)
            {
                if(timeLeft > 1) // will not change timer if the timer is already below a certain time limit
                {
                    CancelInvoke(); // stops the previous time so it can be set to a shorter timer
                    timeLeft = 1;
                    Invoke("timer", 1);
                    totalFitness();
                }
            }
        }
    }
    /// <summary>
    /// sets training to false which will allow the next generation to be created
    /// </summary>
    void timer()
    {
        //signifies the end of the current generation
        training = false;
    }

    /// <summary>
    /// creates random neural networks as a base
    /// </summary>
    private void initCarsNetwork()
    {
        //create all of the networks for the cars
        neuralNetworkList = new List<network>();
        for (int i = 0; i < populationSize; i++)
        {
            network network = new network(layers);
            network.mutate();
            network.israndomStart();
            neuralNetworkList.Add(network);
        }
    }
    /// <summary>
    /// removes all cars then recreates them
    /// </summary>
    private void createCars()
    {
        //delete all of the cars then create new ones
        if (carList != null)
        {
            for (int i = 0; i < carList.Count; i++)
            {
                Destroy(carList[i].gameObject);
            }
        }

        carList = new List<carNetwork>();
        //creates/recreates the cars
        for (int i = 0; i < populationSize; i++)
        {
            //create the car then asign it a neural network
            carNetwork car = ((GameObject)Instantiate(carPrefab, startingPos.transform.position, startingPos.transform.rotation)).GetComponent<carNetwork>();
            car.Init(neuralNetworkList[i]);
            carList.Add(car);
        }
    }
    /// <summary>
    /// writes the currents champions wights to a text document
    /// </summary>
    public void writeToText()
    {
        // creates folder if it doesn't exist
        if (!Directory.Exists(Application.dataPath + "\\previousCars"))
        {
        System.IO.Directory.CreateDirectory(Application.dataPath + "\\previousCars");
        }
        string path = Application.dataPath + "\\previousCars/savedWeights.txt";

        File.WriteAllText(path, "");//create file or wipe if already existing
        for (int i = 0; i < neuralNetworkList[neuralNetworkList.Count - 1].weights.Length; i++)
        {
            for (int j = 0; j < neuralNetworkList[neuralNetworkList.Count - 1].weights[i].Length; j++)
            {
                for (int k = 0; k < neuralNetworkList[neuralNetworkList.Count - 1].weights[i][j].Length; k++)
                {
                    string weightToSave = neuralNetworkList[neuralNetworkList.Count - 1].weights[i][j][k].ToString(); // saves the current champion's weights
                    createText(path, weightToSave);
                }
            }
        }
    }
    /// <summary>
    /// writes an individual weight to a text document, also create it if it does not exist
    /// </summary>
    /// <param name="path"></param>
    /// <param name="whatToWrite"></param>
    private void createText(string path, string whatToWrite)
    {
        if(!File.Exists(path))
        {
            File.WriteAllText(path, "");
        }
        File.AppendAllText(path, whatToWrite + "\n"); // write all weights to the file
    }
    /// <summary>
    /// sets if the car was loaded
    /// </summary>
    public void loadChampion()
    {
        previousChampionLoaded = true;
    }
    /// <summary>
    /// reads the text document and loads the wieghts into a neural network
    /// </summary>
    public void readText()
    {
        //changes the first network to the loaded champion
        string path = Application.dataPath + "\\previousCars/savedWeights.txt";// path for the file
        StreamReader reader = new StreamReader(path);
        string text;
        for (int i = 0; i < neuralNetworkList[0].weights.Length; i++)
        {
            for (int j = 0; j < neuralNetworkList[0].weights[i].Length; j++)
            {
                for (int k = 0; k < neuralNetworkList[0].weights[i][j].Length; k++)
                { 
                    text = reader.ReadLine();
                    float loadedWeight = float.Parse(text);
                    neuralNetworkList[0].weights[i][j][k] = loadedWeight;

                }
            }
        }
    }
    /// <summary>
    /// increases the next generation length
    /// </summary>
    public void increaseGenerationLength()
    {
        if (generationLength < 900)
        {
            generationLength += 10;
            generationLengthText.text = "Generation length: " + generationLength;
        }
    }
    /// <summary>
    /// decreases the next generation length
    /// </summary>
    public void decreaseGenerationLength()
    {
        if (generationLength > 10)
        {
            generationLength -= 10;
            generationLengthText.text = "Generation length: " + generationLength;

        }
    }
    /// <summary>
    /// returns to the main menu scene
    /// </summary>
    public void returnToMainMenu() 
    {
        SceneManager.LoadScene("MainMenu");
    }

}
