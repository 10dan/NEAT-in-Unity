using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
    public int numberFood;
    public int numberMen = 10;
    public GameObject food;
    public HungryGuy man;
    public Transform foodParent;
    public Transform menParent;
    public float fruitTimer = 3f; //Time between fruit spawns
    public float lifeTimer = 5f; //Time between fruit spawns

    HungryGuy[] activeMen;

    private void Start() {
        activeMen = new HungryGuy[numberMen];
        MakeInitialArena();
        StartCoroutine(SpawnFruit());
        StartCoroutine(AutoBreed());
    }

    IEnumerator SpawnFruit() {
        while (true) {
            float x = UnityEngine.Random.Range(-10f, 10f);
            float z = UnityEngine.Random.Range(-10f, 10f);
            Vector3 spawnPos = new Vector3(x, 10f, z);
            GameObject apple = Instantiate(food, spawnPos, Quaternion.identity, foodParent);
            apple.GetComponent<Rigidbody>().useGravity = true;
            yield return new WaitForSeconds(fruitTimer);
        }
    }

    IEnumerator AutoBreed() {
        while (true) {
            yield return new WaitForSeconds(lifeTimer);
            NewArena();
        }
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            NewArena();
        }
    }

    void NewArena() {
        int best = 0;
        for (int i = 0; i < activeMen.Length; i++) {
            if (activeMen[i].fitness > activeMen[best].fitness) {
                best = i;
            }
        }
        HungryGuy bestMan = activeMen[best];
        for (int i = 0; i < numberMen; i++) {
            //Modify genes of all men.
            if (i != best) {
                activeMen[i].nn.CopyWeights(activeMen[best].nn.weights);
                activeMen[i].nn.Mutate();
            }
            float x = UnityEngine.Random.Range(-10, 10);
            float y = UnityEngine.Random.Range(-10, 10);
            Vector3 spawnPos = new Vector3(x, 0.5f, y);
            activeMen[i].transform.position = spawnPos;
            activeMen[i].fitness = 0;
            activeMen[i].dead = false;
            activeMen[i].energy = activeMen[i].startEnergy;
        }

        for (int i = 0; i < numberFood; i++) {
            float x = UnityEngine.Random.Range(-10, 10);
            float y = UnityEngine.Random.Range(-10, 10);
            Vector3 spawnPos = new Vector3(x, 0.5f, y);
            GameObject apple = Instantiate(food, spawnPos, Quaternion.identity, foodParent);
            apple.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0, 5), 0));
        }
    }

    void MakeInitialArena() {
        for (int i = 0; i < numberFood; i++) {
            float x = UnityEngine.Random.Range(-10f, 10f);
            float y = UnityEngine.Random.Range(-10f, 10f);
            Vector3 spawnPos = new Vector3(x, 0.5f, y);
            GameObject apple = Instantiate(food, spawnPos, Quaternion.identity, foodParent);
        }

        for (int i = 0; i < numberMen; i++) {
            float x = UnityEngine.Random.Range(-10, 10);
            float y = UnityEngine.Random.Range(-10, 10);
            Vector3 spawnPos = new Vector3(x, 0.5f, y);
            HungryGuy guy = Instantiate(man, spawnPos, Quaternion.identity, menParent);
            activeMen[i] = guy;
        }
    }
}
