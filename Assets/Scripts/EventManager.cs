using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {


    public static int arenaSize = 50;
    public int numberFood;
    public int numberMen;
    public float fruitTimer; //Time between fruit spawns
    public float lifeTimer; //Time between fruit spawns
    public float bestGenerationAvg = 0f;
    public int generationNum = 0;

    public GameObject food;
    public HungryGuy man;
    public Transform foodParent;
    public Transform menParent;

    HungryGuy[] activeMen;

    private void Start() {
        activeMen = new HungryGuy[numberMen];
        MakeInitialArena();
        StartCoroutine(SpawnFruit());
        StartCoroutine(AutoBreed());
    }


    IEnumerator SpawnFruit() {
        while (true) {
            Vector3 spawnPos = GenerateRandomLocation();
            GameObject apple = Instantiate(food, spawnPos, Quaternion.identity, foodParent);
            yield return new WaitForSeconds(fruitTimer);
        }
    }

    IEnumerator AutoBreed() {
        while (true) {
            yield return new WaitForSeconds(lifeTimer);
            NewArena();
        }
    }


    void NewArena() {
        //Work out average and update best generations average.
        float avg = CalcAverageEnergy();
        if (avg > bestGenerationAvg) {
            bestGenerationAvg = avg;
        }
        generationNum++;

        //Print generation stats
        print("o-> Generation: " + generationNum + " <-o");
        print("Average energy this round: " + avg);
        print("The best generation: " + bestGenerationAvg);

        // ----   FIND THE FITTEST FROM THIS GENERATION ----
        int num = numberMen / 20; //Take 5% of the top population for breeding.
        HungryGuy[] top = new HungryGuy[num]; //Store the top "num" of men with highest energy.
        //Fill top up randomly. Avoid null pointers in following loop.
        for (int i = 0; i < top.Length; i++) {
            top[i] = activeMen[i];
        }
        //go through all the men and find the ones with the best energy.
        for (int i = 0; i < activeMen.Length; i++) {
            bool added = false;
            //Go through the top list and see what needs replacing.
            for (int j = 0; j < top.Length; j++) {
                //If the man we are looking at has better energy than any in our current top list.
                if (activeMen[i].energy > top[j].energy && !added) {
                    added = true;
                    for (int k = top.Length - 1; k > j; k--) {
                        top[k] = top[k - 1];
                    }
                    top[j] = activeMen[i];
                }
            }
        }

        // ---- PICK 2 PARENTS FROM THE FITTEST ----
        int parent1 = UnityEngine.Random.Range(0, top.Length);
        int parent2 = UnityEngine.Random.Range(0, top.Length); //Who cares if they the same, just a bit of incest.



        //Give all the men new brains.
        for (int i = 0; i < numberMen; i++) {
            //Init to proper size.
            float[][][] newBrain = top[0].nn.weights;
            for (int x = 0; x < newBrain.Length; x++) {
                for (int y = 0; y < newBrain[x].Length; y++) {
                    for (int z = 0; z < newBrain[x][y].Length; z++) {
                        //0 or 1, pick randomly between parents.
                        int r = UnityEngine.Random.Range(0, 2);
                        if (r == 0) {
                            newBrain[x][y][z] = top[parent1].nn.weights[x][y][z];
                        } else {
                            newBrain[x][y][z] = top[parent2].nn.weights[x][y][z];
                        }
                    }
                }
            }


            //Replace the mens old brains, with the one we just made.
            activeMen[i].nn.CopyWeights(newBrain);
            activeMen[i].nn.Mutate();
            float ra = UnityEngine.Random.Range(0,100);
            if(ra < 5) {
                activeMen[i].nn.InitNeurons();
                activeMen[i].nn.InitWeights();
            }
            activeMen[i].ResetStats(); //Reset their energy.
            activeMen[i].transform.position = GenerateCirclePos(i);
        }

        //Remove all old food. for fair testing of new generation.
        foreach (Transform child in foodParent) {
            GameObject.Destroy(child.gameObject);
        }


        //Fill board up with more fresh food.
        for (int i = 0; i < numberFood; i++) {
            Vector3 spawnPos = GenerateRandomLocation();
            Instantiate(food, spawnPos, Quaternion.identity, foodParent);
        }

    }



    void MakeInitialArena() {
        for (int i = 0; i < numberFood; i++) {
            Vector3 spawnPos = GenerateRandomLocation();
            Instantiate(food, spawnPos, Quaternion.identity, foodParent);
        }

        float angle = 360f / numberMen;
        for (int i = 0; i < numberMen; i++) {
            Vector3 spawnPos = GenerateCirclePos(i);
            HungryGuy guy = Instantiate(man, spawnPos, Quaternion.identity, menParent);
            activeMen[i] = guy;
        }
    }


    Vector3 GenerateCirclePos(int i) {
        float angle = 360f / numberMen;
        float x = Mathf.Cos(angle * i) * (arenaSize - (arenaSize / 5));
        float y = 0.5f;
        float z = Mathf.Sin(angle * i) * (arenaSize - (arenaSize / 5));
        return new Vector3(x, y, z);
    }


    Vector3 GenerateRandomLocation() {

        float y = 0.5f;
        float x = UnityEngine.Random.Range(-arenaSize, arenaSize);
        float z = UnityEngine.Random.Range(-arenaSize, arenaSize);
        while (Vector3.Distance(Vector3.zero, new Vector3(x, 0, z)) > arenaSize) {
            x = UnityEngine.Random.Range(-arenaSize, arenaSize);
            z = UnityEngine.Random.Range(-arenaSize, arenaSize);
        }
        return new Vector3(x, y, z);

    }

    float CalcAverageEnergy() {
        float total = 0;
        for (int i = 0; i < numberMen; i++) {
            total += activeMen[i].energy;
        }
        float e = total / numberMen;
        return e;
    }

    private void CalcBestMan() {
        int best = 0;
        for (int i = 0; i < activeMen.Length; i++) {
            if (activeMen[i].dead == false) {
                if (activeMen[i].energy >= activeMen[best].energy) {
                    best = i;
                    activeMen[i].hasMostEnergy = false;
                }
            }
        }
        activeMen[best].hasMostEnergy = true;
    }
}
