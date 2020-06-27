using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HungryGuy : MonoBehaviour {
    public float appleScore = 10f; //fitness per apple eaten

    public float rayLength = 10f;
    public float maxRayLength = 10f;
    int numberOfRays = 10;
    public float energy = 100f;
    public float startEnergy;
    public float moveEnergy = 2f;
    public float rotationEnergy = 0.1f;
    public bool dead = false;

    public NeuralNetwork nn;
    public float fitness = 0;

    public void Start() {
        startEnergy = energy;
        int[] nnStructure = { numberOfRays*2, 10,20, 3 };
        nn = new NeuralNetwork(nnStructure);
    }

    public void Update() {
        if (!dead) {
            float[] inputs = ScanEnvironment(); //Returns values for each eye.
            int action = PredictBestAction(inputs);
            PerformAction(action);
            print(Vector3.Distance(transform.position, Vector3.zero));
            if (Vector3.Distance(transform.position, Vector3.zero) > 10) {
                fitness -= 1;
            }
        }
        if(energy < 0) {
            fitness = -Mathf.Infinity;
            dead = true;
        }
        UpdateSize();
    }

    //Make those with less energy look small
    private void UpdateSize() {
        float size = Mathf.InverseLerp(0, startEnergy, energy);
        Vector3 newSize = new Vector3(size,size,size);
        transform.localScale = newSize;
    }

    private float[] ScanEnvironment() {
        RaycastHit hit;
        float angleOffset = 90 / numberOfRays; //Divide by number of rays.
        float[] inputs = new float[numberOfRays*2]; //*2, once for food, 2nd for enemy.

        int c = 0; //Identify which input neuron to calc value for.
        for (float angle = -45; angle <= 45; angle += angleOffset) {
            var direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
            int mask = LayerMask.GetMask("food");
            if (Physics.Raycast(transform.position, direction, out hit, maxRayLength, mask)) {
                float mapped = Mathf.InverseLerp(0, maxRayLength, hit.distance);
                inputs[c] = mapped;
            } else {
                inputs[c] = -0.1f;
            }
        }
        c++;

        c = numberOfRays-1;
        for (float angle = -45; angle <= 45; angle += angleOffset) {
            var direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
            int mask = LayerMask.GetMask("man");
            if (Physics.Raycast(transform.position, direction, out hit, maxRayLength, mask)) {
                Debug.DrawRay(transform.position, direction * hit.distance);
                float mapped = Mathf.InverseLerp(0, maxRayLength, hit.distance);
                inputs[c] = mapped;
            } else {
                Debug.DrawRay(transform.position, direction * maxRayLength);
                inputs[c] = -0.1f;
            }
            c++;
        }
        return inputs;
    }

    private void PerformAction(int action) {
        print(action);
        switch (action) {
            case (0):
                transform.Rotate(new Vector3(0, 5f, 0));
                energy -= moveEnergy;
                break;
            case (1):
                transform.Rotate(new Vector3(0, 5f, 0));
                energy -= rotationEnergy;
                break;
            case (2):
                transform.Translate(Vector3.forward * 0.3f);
                energy -= rotationEnergy;
                break;
        }
    }

    private int PredictBestAction(float[] inputs) {
        float[] outputs = nn.FeedForward(inputs); //get the nn prediction for best move. (left, right or forward)
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
            fitness += appleScore;
            energy += 20f;
            Destroy(objHit);
        }
    }
}
