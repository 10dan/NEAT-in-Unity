
using System;
using System.Collections.Generic;

public class NeuralNetwork : IComparable<NeuralNetwork>{
    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;
    private float fitness;

    private Random r;
    public NeuralNetwork(int[] layers) {
        //Set the number of neurons in each layer.
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++) {
            this.layers[i] = layers[i];
        }
        r = new Random(System.DateTime.Today.Millisecond);
        InitNeurons();
        InitWeights();
    }

    public NeuralNetwork(NeuralNetwork copy) {
        this.layers = new int[copy.layers.Length];
        for(int i = 0; i < copy.layers.Length; i++) {
            this.layers[i] = copy.layers[i];
        }
        InitNeurons();
        InitWeights();
        CopyWeights(copy.weights);
    }

    private void CopyWeights(float[][][] copyWeights) {
        for (int i = 0; i < weights.Length; i++) {
            for (int j = 0; j < weights[i].Length; j++) {
                for (int k = 0; k < weights[i][j].Length; k++) {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    private void InitWeights() {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++) {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }

    private void InitNeurons() {
        List<float[][]> weightsList = new List<float[][]>();
        //Apply weight values into all connections.
        for (int i = 1; i < layers.Length; i++) { //For every layer (starting at 1th layer as no input weights in 0)
            List<float[]> layerWeightList = new List<float[]>();
            int neuronsInPrevLayer = layers[i - 1];
            //Go through all neurons
            for (int j = 0; j < neurons[i].Length; j++) {
                float[] neuronWeights = new float[neuronsInPrevLayer];
                for (int k = 0; k < neuronsInPrevLayer; k++) {
                    neuronWeights[k] = (float)r.NextDouble() - 0.5f;
                }
                layerWeightList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] inputs) {
        for (int i = 0; i < inputs.Length; i++) {
            neurons[0][i] = inputs[i];
        }
        for (int i = 1; i < layers.Length; i++) {
            for (int j = 0; j < neurons[i].Length; j++) {
                float value = 0.25f;
                for (int k = 0; k < neurons[i - 1].Length; k++) {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = (float)Math.Tanh(value);
            }
        }
        return neurons[neurons.Length - 1];
    }

    public void Mutate() {
        for (int i = 0; i < weights.Length; i++) {
            for (int j = 0; j < weights[i].Length; j++) {
                for (int k = 0; k < weights[i][j].Length; k++) {
                    float weight = weights[i][j][k];
                    float rand = (float)r.NextDouble() * 1000f;
                    if (rand <= 2f) {
                        weight *= -1;
                    } else if (rand <= 4f) {
                        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    } else if (rand <= 6f) {
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    } else if (rand <= 8f) {
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                    }
                    weights[i][j][k] = weight;
                }
            }
        }
    }

    public void AddFitness(float fit) {
        fitness += fit;
    }
    public void SetFitness(float fit) {
        fitness = fit;
    }
    public float GetFitness() {
        return fitness;
    }

    public int CompareTo(NeuralNetwork other) {
        if (other == null) return 1;
        if (fitness > other.fitness) return 1;
        else if (fitness < other.fitness) return -1;
        else return 0;
    }
}
