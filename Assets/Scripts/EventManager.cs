using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {


    public static int arenaSize = 50;
    public int numberFood;
    public int numberMen;
    public float fruitTimer = 0.5f;
    public float newManTimer = 5f;

    public GameObject food;
    public HungryGuy man;
    public Transform foodParent;
    public Transform menParent;


    private void Start() {
        MakeInitialArena();
        StartCoroutine(SpawnFruit());
        StartCoroutine(SpawnMan());
    }

    void MakeInitialArena() {

        //Initially fill the map with some food.
        for (int i = 0; i < numberFood; i++) {
            Vector3 spawnPos = GenerateRandomLocation();
            Instantiate(food, spawnPos, Quaternion.identity, foodParent);
        }

        //Create a set of men
        for (int i = 0; i < numberMen; i++) {
            Vector3 spawnPos = GenerateCirclePos(i);
            HungryGuy guy = Instantiate(man, spawnPos, Quaternion.identity, menParent);
            guy.transform.Rotate(new Vector3(0, i * 10f, 0));
        }
    }

    IEnumerator SpawnFruit() {
        while (true) {
            yield return new WaitForSeconds(fruitTimer);
            Vector3 spawnPos = GenerateRandomLocation();
            GameObject apple = Instantiate(food, spawnPos, Quaternion.identity, foodParent);
        }
    }

    IEnumerator SpawnMan() {
        while (true) {
            yield return new WaitForSeconds(newManTimer);
            Vector3 spawnPos = GenerateRandomLocation();
            HungryGuy guy = Instantiate(man, spawnPos, Quaternion.identity, menParent);
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
}
