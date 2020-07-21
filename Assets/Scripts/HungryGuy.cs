using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HungryGuy : MonoBehaviour {

    //Constants:
    int numberEyes = 30;
    int thingsToSee = 2; //Food and other men.
    [SerializeField] ParticleSystem deathFX;
    [SerializeField] bool debugMode = false;
    //References needed to spawn more men.
    [SerializeField] HungryGuy manPrefab;
    public bool dead = false;
    const float energyStart = 100f;


    public float energy; //Track how much energy they have left.
    public float energyPerApple;
    public float energyToMove;
    public float energyToRotate;
    public float maxRayLength;
    public float energyPassiveLoss;
    public float movementSlowFactor;
    public float energyLossHitMan;
    public float energyForBaby;
    public float sizeFactor;
    public Color color;

    public NeuralNetwork nn;

    private void Start() {
        //Determine energy genes.
        energy = energyStart;
        energyPerApple = UnityEngine.Random.value * 50f;
        energyToMove = UnityEngine.Random.value * 2f;
        energyToRotate = UnityEngine.Random.value * 0.1f;
        maxRayLength = UnityEngine.Random.value * 10f;
        energyPassiveLoss = UnityEngine.Random.value * 0.1f;
        movementSlowFactor = (UnityEngine.Random.value);
        energyLossHitMan = UnityEngine.Random.value * 200f;
        energyForBaby = UnityEngine.Random.value * 100f;
        color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        GetComponent<MeshRenderer>().material.color = color;

        //Make the brain.
        int[] nnStructure = { numberEyes * thingsToSee, 10, 10, 3 };
        nn = new NeuralNetwork(nnStructure);

        //So that dont collide with parents imediatly.
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Rigidbody>().detectCollisions = false;
        StartCoroutine(EnableCollisions());

        StartCoroutine(MakeBaby());
    }

    IEnumerator EnableCollisions() {
        yield return new WaitForSeconds(1f); //wait for a second then enable colliders.
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<Rigidbody>().detectCollisions = true;
    }

    IEnumerator MakeBaby() {
        while (true) {
            yield return new WaitForSeconds(3f); //wait for a second then enable colliders.
            if (energy > energyForBaby) {
                //TODO: make it so genes actually get passed to offspring.
                GameObject go = Instantiate(gameObject);
                HungryGuy baby = go.GetComponent<HungryGuy>();
                baby.energyPerApple = energyPerApple + (UnityEngine.Random.value - 0.5f);
                baby.energyToMove = energyPerApple + (UnityEngine.Random.value - 0.5f);
                baby.energyToRotate = energyPerApple + (UnityEngine.Random.value - 0.5f);
                baby.maxRayLength = energyPerApple + (UnityEngine.Random.value - 0.5f);
                baby.energyPassiveLoss = energyPerApple + (UnityEngine.Random.value - 0.5f);
                baby.movementSlowFactor = energyPerApple + (UnityEngine.Random.value - 0.5f);
                baby.energyLossHitMan = energyPerApple + (UnityEngine.Random.value - 0.5f);
                baby.energyForBaby = energyPerApple + (UnityEngine.Random.value - 0.5f);
                baby.GetComponent<MeshRenderer>().material.color = color;
                baby.nn = nn;
                baby.nn.Mutate();
            }
        }
    }

    public void Update() {
        if (!dead) {
            PerformAction();
            UpdateSize();
            CheckIfDead();
        }
    }

    private void CheckIfDead() {
        if (energy <= 0f) {
            dead = true;
            Instantiate(deathFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    //Make those with less energy look small
    private void UpdateSize() {
        float size = Mathf.InverseLerp(-10, energyStart, energy);
        if (dead) size = 0;
        Vector3 newSize = new Vector3(size, size, size);
        transform.localScale = newSize;
    }

    private float[] ScanEnvironment(string maskName) {
        //TODO: Give memory of last scan.
        RaycastHit hit;
        float[] inputs = new float[numberEyes];
        float angleOffset = Mathf.Floor(240 / numberEyes); //Divide by number of rays.
        float angle = -120;
        for (int i = 0; i < inputs.Length; i++) {
            var direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
            int mask = LayerMask.GetMask(maskName);
            if (Physics.Raycast(transform.position, direction, out hit, maxRayLength, mask)) {
                float mapped = Mathf.InverseLerp(0, maxRayLength, hit.distance);
                inputs[i] = mapped;
                if (debugMode) Debug.DrawRay(transform.position, direction * hit.distance);
            } else {
                inputs[i] = 1f;
                if (debugMode) Debug.DrawRay(transform.position, direction * maxRayLength);
            }
            angle += angleOffset;
        }
        return inputs;
    }

    private void PerformAction() {
        float[] foodInputs = ScanEnvironment("food");
        float[] menInputs = ScanEnvironment("man");
        float[] inputs = new float[numberEyes * 2];
        //Copy the scanned information into one array.
        foodInputs.CopyTo(inputs, 0);
        menInputs.CopyTo(inputs, numberEyes);
        float[] outputs = nn.FeedForward(inputs);
        int action = PredictBestAction(outputs);
        float amount = outputs[action];
        energy -= energyPassiveLoss; //Passively lose energy if you do nothing.
        switch (action) {
            case (0):
                transform.Rotate(new Vector3(0, -amount * 10f, 0));
                energy -= energyToRotate;
                break;
            case (1):
                transform.Rotate(new Vector3(0, amount * 10f, 0));
                energy -= energyToRotate;
                break;
            case (2):
                transform.Translate(Vector3.forward * amount * movementSlowFactor);
                energy -= energyToMove;
                break;
        }
    }

    private int PredictBestAction(float[] outputs) {
        //get the nn prediction for best move. (left, right or forward)
        int action = 0;
        for (int i = 0; i < outputs.Length; i++) {
            if (outputs[i] > outputs[action]) {
                action = i;
            }
        }
        return action;
    }

    public void OnCollisionEnter(Collision collision) {
        GameObject objHit = collision.collider.gameObject;
        if (!dead) {
            if (objHit.tag == "food") {
                energy += energyPerApple;
                Destroy(objHit);
            }
            if (objHit.tag == "man") {
                energy -= energyLossHitMan;
            }
        }
    }
}
