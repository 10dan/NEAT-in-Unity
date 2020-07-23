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
    [SerializeField] HungryGuy manPrefab;
    const float energyStart = 200f;

    float energy;
    float energyPerApple;
    float energyToMove;
    float energyToRotate;
    float maxRayLength;
    float energyPassiveLoss;
    float movementSlowFactor;
    float energyLossHitMan;
    Color color;

    NeuralNetwork nn;


    private void InitVariables() {
        energy = energyStart;
        energyPerApple = 50f;
        energyToMove = 0.1f;
        energyToRotate = 0f;
        maxRayLength = 20f;
        energyPassiveLoss = 0.1f;
        movementSlowFactor = 0.1f;
        energyLossHitMan = 1000f;

        color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        GetComponent<MeshRenderer>().material.color = color;
        GetComponent<TrailRenderer>().startColor = color;
        GetComponent<TrailRenderer>().endColor = color;

        //So that dont collide with parents imediatly.
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Rigidbody>().detectCollisions = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        //Random rotation so not alwys facing fowards.
        transform.Rotate(new Vector3(0f, UnityEngine.Random.Range(0, 360), 0f));
    }

    private void Awake() {
        InitVariables();
        int[] nnStructure = { numberEyes * thingsToSee, 10, 3 };
        nn = new NeuralNetwork(nnStructure);
        StartCoroutine(EnableCollisions());
    }

    public void Update() {
        GetComponentInChildren<TextMesh>().text = energy.ToString("F1");
        PerformAction();
        UpdateSize();
        CheckIfDead();
    }

    IEnumerator EnableCollisions() {
        yield return new WaitForSeconds(1f); //wait for a second then enable colliders.
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<Rigidbody>().detectCollisions = true;
        GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY;
    }

    public void CopyParent(GameObject parent) {
        HungryGuy parentScript = parent.GetComponent<HungryGuy>();
        color = parentScript.color;
        GetComponent<MeshRenderer>().material.color = color;
        GetComponent<TrailRenderer>().startColor = color;
        GetComponent<TrailRenderer>().endColor = color;
        nn.CopyWeights(parentScript.nn.weights);
        nn.Mutate();
    }

    private void CheckIfDead() {
        if (energy <= 0f) {
            ParticleSystem fx = Instantiate(deathFX, transform.position, Quaternion.identity);
            fx.transform.Rotate(new Vector3(-90f, 0f, 0f));
            Destroy(gameObject);
        }
    }

    private void UpdateSize() {
        float size = Mathf.InverseLerp(0, energyStart, energy);
        Vector3 newSize = new Vector3(size, size, size);
        transform.localScale = newSize;
    }

    private void PerformAction() {
        float[] food_inputs = ScanEnvironment("food");
        float[] men_inputs = ScanEnvironment("man");
        float[] inputs = new float[numberEyes * thingsToSee];
        food_inputs.CopyTo(inputs, 0);
        men_inputs.CopyTo(inputs, numberEyes);
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
        if (objHit.tag == "food") {
            energy += energyPerApple;
            //Make baby
            Vector3 newPos = transform.position;
            newPos.y = 0.5f;
            GameObject go = Instantiate(gameObject, newPos, Quaternion.identity);
            go.GetComponent<HungryGuy>().CopyParent(gameObject);
            //Destroy apple.
            Destroy(objHit);
        }
        if (objHit.tag == "man") {
            energy -= energyLossHitMan;
        }

    }
}
