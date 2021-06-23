using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {


    public static int arenaSize = 50;
    public int startNumberFood;
    public int startNumberMen;
    public float fruitTimer = 0.5f;
    public float newManTimer = 5f;
    public int maxFruit = 0;
    public int maxMen = 10;

    public GameObject food;
    public HungryGuy man;


    private void Start() {
        MakeInitialArena();
        StartCoroutine(SpawnMan());
        Time.timeScale = 3.0f;
    }


    void MakeInitialArena() {

        //Initially fill the map with some food.
        for (int i = 0; i < startNumberFood; i++) {
            Vector3 spawnPos = GenerateRandomLocation();
            Instantiate(food, spawnPos, Quaternion.identity);
        }

        //Create a set of men
        for (int i = 0; i < startNumberMen; i++) {
            Vector3 spawnPos = GenerateCirclePos(i);
            HungryGuy guy = Instantiate(man, spawnPos, Quaternion.identity);
            guy.transform.Rotate(new Vector3(0, i * 10f, 0));
        }
    }

    private void Update() {
        int c = GameObject.FindGameObjectsWithTag("food").Length;
        if (c < maxFruit) {
            Vector3 spawnPos = GenerateRandomLocation();
            GameObject apple = Instantiate(food, spawnPos, Quaternion.identity);
            float r = UnityEngine.Random.value * 360f;
            apple.transform.Rotate(new Vector3(r, r, r));
        }

    }

    IEnumerator SpawnMan() {
        while (true) {
            yield return new WaitForSeconds(newManTimer);
            int c = GameObject.FindGameObjectsWithTag("man").Length;
            if (c < maxMen) {
                Vector3 spawnPos = GenerateRandomLocation();
                Instantiate(man, spawnPos, Quaternion.identity);
            }
        }
    }

    Vector3 GenerateCirclePos(int i) {
        float angle = 360f / startNumberMen;
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
