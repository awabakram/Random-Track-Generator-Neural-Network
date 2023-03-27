using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Random_Track_Generation
{
    class NeuralNetwork
    {
        //Attributes of the neural network

        //layers will be in the format {9,7,7,4} as in, 9 neurons in the first layer, 7 neurons in the second layer, 7 in the third layer, and 4 in the last layer
        int[] layers;
        float[][] neurons;
        float[][] biases;
        float[][][] weights;

        static Random rand = new Random();



        public NeuralNetwork(int[] layers) 
        {
            //Standard constructor for a neural netwotk where all weights and biases are initiallised randomly

            this.layers = layers;
            InitialiseNeurons();
            InitialiseBiases();
            InitialiseWeights();
        }

        public NeuralNetwork(string netName, int[] layers, float[][] biases, float[][][] weights) 
        {
            //Constructor for a neural network where biases and weights are given
            this.layers = layers;
            InitialiseNeurons();
            this.biases = biases;
            this.weights = weights;
        }

        public NeuralNetwork(string FilePath, ref string statusString)
        {
            //Constructor for loading a neural network from a file
            if (FilePath == ".txt")
            {
                statusString = "Enter a file Name to\nload the neural net from";
                return;
            }

            if (File.Exists(FilePath) == false)
            {
                statusString = "File does not exist";
                return;
            }
            using (StreamReader reader = new StreamReader(FilePath))
            {
                string layersString = reader.ReadLine();
                string[] layersStrings = layersString.Split(',');

                List<int> layersList = new List<int>();
                for (int i = 0; i < layersStrings.Length; i++)
                {
                    layersList.Add(Convert.ToInt32(layersStrings[i]));
                }
                layers = layersList.ToArray();

                InitialiseNeurons();
                InitialiseBiases();
                InitialiseWeights();

                for (int i = 0; i < biases.Length; i++)
                {
                    for (int j = 0; j < biases[i].Length; j++)
                    {
                        biases[i][j] = (float)Convert.ToDouble(reader.ReadLine());
                    }
                }

                for (int i = 0; i < weights.Length; i++)
                {
                    for (int j = 0; j < weights[i].Length; j++)
                    {
                        for (int k = 0; k < weights[i][j].Length; k++)
                        {
                            weights[i][j][k] = (float)Convert.ToDouble(reader.ReadLine());
                        }
                    }
                }

                statusString = "Neural Network Loaded\nSuccessfully";
            }
        }

        void InitialiseNeurons()
        {
            //Method to initialise all neurons,
            //initially their values are all set randomly but theyre changed during each feedforward
            List<float[]> list = new List<float[]>();
            for (int i = 0; i < layers.Length; i++)
            {
                list.Add(new float[layers[i]]);
            }
            neurons = list.ToArray();
        }

        void InitialiseBiases()
        {
            //Method to initialise all biases to a random value

            List<float[]> biasesList = new List<float[]>();
            for (int i = 0; i < layers.Length; i++)
            {
                float[] layerBiases = new float[layers[i]];

                for (int j = 0; j < layerBiases.Length; j++)
                {
                    double doubleVal = rand.NextDouble(); //generates random double between 0 and 1
                    doubleVal = doubleVal - 0.5; //changes the number so that its between -0.5 and 0.5
                    layerBiases[j] = (float)doubleVal;
                }
                biasesList.Add(layerBiases);
            }
            biases = biasesList.ToArray();
        }

        void InitialiseWeights()
        {
            //Method to initialise all Weights to a random value

            List<float[][]> weightsList = new List<float[][]>();
            for (int i = 1; i < layers.Length; i++)
            {
                List<float[]> listLayerWeights = new List<float[]>();

                for (int j = 0; j < neurons[i].Length; j++)
                {
                    float[] neuronWeights = new float[layers[i - 1]];

                    for (int k = 0; k < layers[i-1]; k++)
                    {
                        double doubleVal = rand.NextDouble(); //generates random double between 0 and 1
                        doubleVal = doubleVal - 0.5; //changes the number so that its between -0.5 and 0.5
                        neuronWeights[k] = (float)doubleVal;
                    }
                    listLayerWeights.Add(neuronWeights);
                }
                weightsList.Add(listLayerWeights.ToArray());
            }
            weights = weightsList.ToArray();
        }

        public float[] FeedForward(float[] inputs)
        {
            //put the inputs into the input layer
            for (int i = 0; i < inputs.Length; i++)
            {
                neurons[0][i] = inputs[i];
            }

            //start with the first hidden layer, multiply weights by activations and add them up, add a bias at the end and then use the compression function
            for (int i = 1; i < layers.Length; i++)
            {
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    float weightedSum = 0f;
                    for (int k = 0; k < neurons[i-1].Length; k++)
                    {
                        weightedSum += weights[i - 1][j][k] * neurons[i - 1][k];
                    }
                    weightedSum += biases[i][j];
                    neurons[i][j] = SigmoidFunction(weightedSum);
                }
            }

            //return the last (output) layer
            return neurons[neurons.Length - 1];
        }

        float SigmoidFunction(float input)
        {
            //Method to act as a Sigmoid compression function for any input

            return (float) ((float) 1 / (1 + Math.Pow(Math.E, -input)));
        }

        public void mutate(int percentChanceOfMutation)
        {
            //Method to mutate the neural network by
            //randomly adding or subtracting a random amount from random weights and biases
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    int randomNumber = rand.Next(0, 101);
                    if (randomNumber <= percentChanceOfMutation)
                    {
                        double doubleVal = rand.NextDouble();
                        doubleVal = doubleVal - 0.5;
                        biases[i][j] += (float)doubleVal;

                    }
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        int randomNumber = rand.Next(0, 101);
                        if (randomNumber <= percentChanceOfMutation)
                        {
                            double doubleVal = rand.NextDouble();
                            doubleVal = doubleVal - 0.5;
                            weights[i][j][k] += (float) doubleVal;
                        }
                    }
                }
            }

        }

        public int[] getLayers()
        {
            return layers;
        }

        public float[][] getBiases()
        {
            return biases;
        }

        public float[][][] getWeights()
        {
            return weights;
        }

        public void setBiases(float[][] biases)
        {
            this.biases = biases;
        }

        public void setWeights(float[][][] weights)
        {
            this.weights = weights;
        }

        public NeuralNetwork copyNetwork(NeuralNetwork copyNet)
        {
            //Method to that return a copy of this neural network
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    copyNet.biases[i][j] = biases[i][j];
                }
            }
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                       copyNet.weights[i][j][k] = weights[i][j][k];
                    }
                }
            }
            
            return copyNet;
        }

        public void saveNeuralNetwork(string fileName, ref string statusString)
        {
            //Method to save this neural network's weights and biases

            if (fileName == "")
            {
                statusString = "Enter a filename to\nsave the Neural Network";
                return;
            }
            string filePath = fileName + ".txt";
            if (File.Exists(filePath))
            {
                statusString = "A Neural Network with this name Already Exists";
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                string layersString = "";
                for (int i = 0; i < layers.Length; i++)
                {
                    layersString += layers[i] + ",";
                }
                layersString = layersString.Substring(0, layersString.Length - 1);

                writer.WriteLine(layersString);
                for (int i = 0; i < biases.Length; i++)
                {
                    for (int j = 0; j < biases[i].Length; j++)
                    {
                        writer.WriteLine(biases[i][j]);
                    }
                }

                for (int i = 0; i < weights.Length; i++)
                {
                    for (int j = 0; j < weights[i].Length; j++)
                    {
                        for (int k = 0; k < weights[i][j].Length; k++)
                        {
                            writer.WriteLine(weights[i][j][k]);
                        }
                    }
                }
            }

            statusString = "Neural Network Successfully \nSaved";
            
        }
    }
}
