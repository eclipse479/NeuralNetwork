

using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using UnityEngine.Timeline;
using UnityEngine;

public class network : IComparable<network>
{
    private int[/*layer*/] layers; // how many layers -> input - hidden - hidden - output    ect
    private float[/*layer*/][/*how many neurons*/] neurons; //the neurons in the network
    [HideInInspector]
    public float[/*layer*/][/*neuron*/][/*weight*/] weights; // the weights in the network
    private float fitness; // the fitness score of the network
    //will cahnge the colour of the car 
    [HideInInspector]
    public bool champ;     //-> green
    [HideInInspector]
    public bool randomCar; //-> red
    [HideInInspector]
    public bool loaded;     //-> blue
    [HideInInspector]
    public bool hasCrashed;//-> black
    /// <summary>
    /// initialises the neural network
    /// </summary>
    /// <param name="numberOflayers"></param>
    public network(int[] numberOflayers) //constructor
    {
        champ = false;
        randomCar = false;
        loaded = false;
        hasCrashed = false;
        layers = new int[numberOflayers.Length];//creates layers
        for(int i = 0; i< numberOflayers.Length; i++)
        {
            this.layers[i] = numberOflayers[i];
        }
        intNeurons();
        intWeights();
    }
    /// <summary>
    /// copy constructor
    /// </summary>
    /// <param name="copyNetwork"></param>
    public network(network copyNetwork) //copy constructor
    {
        hasCrashed = false;
        randomCar = false;
        loaded = false;
        layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < copyNetwork.layers.Length; i++)
        {
            layers[i] = copyNetwork.layers[i];
        }
        intNeurons();
        intWeights();
        copyWeight(copyNetwork.weights);

    }
    /// <summary>
    /// copys the wieghts from one netwrok to another
    /// </summary>
    /// <param name="copyWeight"></param>
    private void copyWeight(float[][][] copyWeight)
    {
        //goes through each weight and deep copies it over
        for(int i = 0; i< weights.Length;i++)
        {
            for(int j = 0; j< weights[i].Length; j++)
            {
                for (int k =0; k< weights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeight[i][j][k];
                }
            }
        }
    }
    /// <summary>
    /// creates the neurons for each layer
    /// </summary>
    private void intNeurons()//create neurons
    {
        List<float[]> neuronsList = new List<float[]>(); // creates a list of the neurons
        for(int i = 0; i < layers.Length; i++)
        {
            // adds neurons to neuron list
            neuronsList.Add(new float[layers[i]]);
        }
        //converts the list to an array
        neurons = neuronsList.ToArray();
    }
    /// <summary>
    /// creates the weights of the connections between the neurons
    /// </summary>
    private void intWeights()//creates weights
    {
        List<float[][]> weightsList = new List<float[][]>(); // list to hold the weights of the network
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightList = new List<float[]>();//list to hold the weights in a layer
            
            int neuronsInPreviousLayer = layers[i - 1]; //gets how many neurons were in the previous layer

            for(int j = 0; j< neurons[i].Length; j++)
            {

                float[] neuronWeight = new float[neuronsInPreviousLayer]; // neuron weights -> the weights of each connection to a neuron
                for(int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //random starting weights
                    neuronWeight[k] = UnityEngine.Random.Range(-1.0f,1.0f);//give a random starting weight value
                }
                layerWeightList.Add(neuronWeight); // adds the 
            }
            weightsList.Add(layerWeightList.ToArray());
        }
        //converts list to an array
        weights = weightsList.ToArray(); //convert to array
    }

    /// <summary>
    /// puts in the inputs and makes it go throught the network to get outputs
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    public float[] FeedForward(float[] inputs) // send inputs through the network to get outputs
    {
        //add inputs to the netwrok
        for(int i = 0; i< inputs.Length;i++)
        {
            //adds the input to the first(input) layer
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layers.Length; i++)//goes through each layer skipping input layer
        {
            for(int j = 0; j < neurons[i].Length;j++)//goes through each neuron in the layer
            {
                float value = 0.0f; // value that the neuron will get
                for(int k = 0; k< neurons[i-1].Length;k++)//go through each neuron in the previous layer
                {
                    //weight[i][j][k] is the weight connecting neuron j in layer i and neuron k in the previous layer(i-1)
                    value += weights[i - 1][j][k] * neurons[i - 1][k]; // sum of all the neurons times by the weight of the connection
                }
                neurons[i][j] = (float)Math.Tanh(value); //hyperbolic tangent (squishes the value to be between -1 and 1) new value of current neuron
            }
        }
        return neurons[neurons.Length - 1]; //returns output layer
    }

    /// <summary>
    /// mutates the weights on a random chance
    /// </summary>
    public void mutate()
    {
        for (int i = 0; i < weights.Length; i++)//goes through each layer skipping input layer
        {
            for (int j = 0; j < weights[i].Length; j++)//goes through each neuron in the layer
            {
                for (int k = 0; k < weights[i][j].Length; k++)//goes through each weight on the neuron
                {
                    float weight = weights[i][j][k];
                    float randNum = UnityEngine.Random.Range(1, 1000);
                    //each weight has a 2% chance to mutate on a mutate one of 4 possible mutations will occur

                    if (randNum <= 5) // 0.5% chance to increase value by 10 - 90%
                    {
                        //increase by a percentage on 5,6
                        float percent = UnityEngine.Random.Range(0.1f, 0.9f);
                        weight *= (percent + 1);
                    }
                    else if (randNum <= 10)// 0.5% chance to decrease value by 10 - 90%
                    {
                        //decrease by a percentage on 7,8
                        float percent = UnityEngine.Random.Range(0.3f, 0.9f);
                        weight *= percent;
                    }
                    if (randNum <= 15) // 0.5% chance to increase value by an amount
                    {
                        //increase by a percentage on 5,6
                        float addition = UnityEngine.Random.Range(0.1f, 1.0f);
                        weight += addition;
                    }
                    else if (randNum <= 20)// 0.5% chance to decrease value by an amount
                    {
                        //decrease by a percentage on 7,8
                        float subtraction = UnityEngine.Random.Range(0.1f, 1.0f);
                        weight -= subtraction;
                    }
                    // if(RandNum > 20) --> no mutation


                    //put new weight value back
                    weights[i][j][k] = weight;
                }
            }
        }
    }
    //fitness related functions

    /// <summary>
    /// ///increases fitness score
    /// </summary>
    /// <param name="addScore"></param>
    public void addFitness(float addScore)
    {
        //add to current fitness
        fitness += addScore;
    }
    /// <summary>
    /// sets fitness to specified value
    /// </summary>
    /// <param name="newFitness"></param>
    public void setFitness(float newFitness)
    {
        //set a new fitness
        fitness = newFitness;
    }
    /// <summary>
    /// returs the current fitness of the network
    /// </summary>
    /// <returns></returns>
    public float getFitness()
    {
        //get the current fitness
        return fitness;
    }

    //sorts the networks by thier fitness score by acending order
    //orders the list in smallest at the start to largest at the beggining
    /// <summary>
    /// allows sorting the networks by thier fitness score
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(network other)
    {
        if(other == null)
        return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }
    // sets various bools which will allow different colours on spawn
    public void makeChamp()
    {
        champ = true;
    }
    public void israndomStart()
    {
        randomCar = true;
    }
    public void wasLoaded()
    {
        loaded = true;
    }
    /// <summary>
    /// genetic algorithm to create new children form two random parents
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public network createChild(network parent) //breeding
    {
        network child = new network(layers); // resulting child
        child.intNeurons();
        child.intWeights();
        for(int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    //randomly assigns the weight values for the child using either values from one of the parents
                    int chance = UnityEngine.Random.Range(0, 2); // picks 0 or 1
                    if(chance == 0)
                    {
                        child.weights[i][j][k] = weights[i][j][k]; //gets value from first parent
                    }
                    else if(chance == 1)
                    {
                        child.weights[i][j][k] = parent.weights[i][j][k]; //gets value from second parent
                    }
                }
            }
        }
        return child;
    }
}
