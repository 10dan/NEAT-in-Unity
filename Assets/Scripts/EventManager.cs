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
        CalcAverageEnergy();
        int first = 0;
        int second = 0;
        for (int i = 0; i < activeMen.Length; i++) {
            if(activeMen[i].energy > activeMen[first].energy) {
                second = first;
                first = i;
            }else if(activeMen[i].energy > activeMen[second].energy) {
                second = i;
            }
        }
        float[][][] firstBrain = activeMen[first].nn.weights;
        float[][][] secondBrain = activeMen[second].nn.weights;

        for (int i = 0; i < numberMen; i++) {

            //Sex the 2 best brains.
            float[][][] newBrain = firstBrain;
            for (int x = 0; x < firstBrain.Length; x++) {
                for (int y = 0; y < firstBrain[x].Length; y++) {
                    for (int z = 0; z < firstBrain[x][y].Length; z++) {
                        int mutation = UnityEngine.Random.Range(0, 1000);
                        if (mutation < 20) { //Mutation.
                            newBrain[x][y][z] = UnityEngine.Random.Range(-0.5f, 0.5f);
                        } else { //Just sex and pick random DNA between parents.
                            int r = UnityEngine.Random.Range(0, 2);
                            if (r == 0) {
                                newBrain[x][y][z] = firstBrain[x][y][z];
                            } else {
                                newBrain[x][y][z] = secondBrain[x][y][z];
                            }
                        }
                    }
                }
            }

            activeMen[i].nn.CopyWeights(newBrain);
            activeMen[i].ResetStats();
            Vector3 pos = GenerateRandomLocation();
            activeMen[i].transform.position = pos;
        }

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

        for (int i = 0; i < numberMen; i++) {
            Vector3 spawnPos = GenerateRandomLocation();
            HungryGuy guy = Instantiate(man, spawnPos, Quaternion.identity, menParent);
            activeMen[i] = guy;
        }
    }

    Vector3 GenerateRandomLocation() {
        float angle = UnityEngine.Random.Range(-10f, 10f);
        float pos = UnityEngine.Random.Range(-arenaSize, arenaSize);
        float x = Mathf.Cos(angle) * pos;
        float z = Mathf.Sin(angle) * pos;
        float y = 0.5f;
        return new Vector3(x, y, z);
    }

    float CalcAverageEnergy() {
        float total = 0;
        for (int i = 0; i < numberMen; i++) {
            total += activeMen[i].energy;
        }
        float e = total / numberMen;
        print("Average energy this round: " + e);
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
