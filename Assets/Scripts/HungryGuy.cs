using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
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

    private void Awake() {

        energy = energyStart;

        //energyPerApple = UnityEngine.Random.value * 100f;
        //energyToMove = UnityEngine.Random.value * 1f;
        //energyToRotate = UnityEngine.Random.value * 1f;
        //maxRayLength = UnityEngine.Random.value * 30f;
        //energyPassiveLoss = UnityEngine.Random.value * 0.1f;
        //movementSlowFactor = (UnityEngine.Random.value) * 0.3f;
        //energyLossHitMan = UnityEngine.Random.value * 400f;
        //energyForBaby = (UnityEngine.Random.value+1) * 50f;

        energyPerApple = 300f;
        energyToMove = 0.3f;
        energyToRotate = 0.05f;
        maxRayLength = 20f;
        energyPassiveLoss = 0.2f;
        movementSlowFactor = 0.2f;
        energyLossHitMan = 100f;
        energyForBaby = 100f;

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
            yield return new WaitForSeconds(1f); //wait for a second then enable colliders.
            if (energy > energyForBaby) {
                int c = GameObject.FindGameObjectsWithTag("man").Length;
                if (c < 150) {
                    Vector3 newPos = transform.position + Vector3.left * 2;
                    GameObject go = Instantiate(gameObject, newPos, Quaternion.identity);
                    go.GetComponent<HungryGuy>().CopyParent(gameObject);
                    energy -= energyForBaby/2;
                }
            }
        }
    }

    public void CopyParent(GameObject parent) {
        HungryGuy parentScript = parent.GetComponent<HungryGuy>();
        //energyPerApple = parentScript.energyPerApple + Mathf.Lerp(-0.05f, 0.05f, UnityEngine.Random.value);
        //energyToMove = parentScript.energyToMove + Mathf.Lerp(-0.05f, 0.05f, UnityEngine.Random.value);
        //energyToRotate = parentScript.energyToRotate + Mathf.Lerp(-0.05f, 0.05f, UnityEngine.Random.value);
        //maxRayLength = parentScript.maxRayLength + Mathf.Lerp(-0.05f, 0.05f, UnityEngine.Random.value);
        //energyPassiveLoss = parentScript.energyPassiveLoss + Mathf.Lerp(-0.05f, 0.05f,UnityEngine.Random.value);
        //movementSlowFactor = parentScript.movementSlowFactor +Mathf.Lerp(-0.05f, 0.05f, UnityEngine.Random.value); 
        //energyLossHitMan = parentScript.energyLossHitMan + Mathf.Lerp(-0.05f, 0.05f, UnityEngine.Random.value);
        //energyForBaby = parentScript.energyForBaby + Mathf.Lerp(-0.05f, 0.05f, UnityEngine.Random.value);
        color = parentScript.color;
        GetComponent<MeshRenderer>().material.color = color;
        nn.CopyWeights(parentScript.nn.weights);
        nn.Mutate();
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
