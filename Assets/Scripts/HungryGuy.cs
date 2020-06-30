using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HungryGuy : MonoBehaviour {

    public float energy = 100f;
    public float energyLossWhenHitMan = 30f;
    public float energyPerApple = 10f;
    public float energyLossOutside = 10f;
    public float energyStart;
    public float energyToMove = 0.5f;
    public float energyToRotate = 0.01f;
    public float maxRayLength = 10f;
    public float energyPassiveLoss = 0.005f;
    public float moveSpeed = 3;

    public int numberEyes = 10;
    public bool dead = false;
    public bool hasMostEnergy = false;

    public NeuralNetwork nn;


    public void Start() {
        energyStart = energy;
        int[] nnStructure = { numberEyes, 7,9, 3 };
        nn = new NeuralNetwork(nnStructure);
    }

    public void ResetStats() {
        energy = energyStart;
        dead = false;
        hasMostEnergy = false;
    }

    public void Update() {
        if (!dead) {
            PerformAction();
            if (Vector3.Distance(transform.position, Vector3.zero) > EventManager.arenaSize) {
                energy -= energyLossOutside;
            }
            if (energy < 0) {
                energy -= Mathf.Infinity;
                dead = true;
            }
            UpdateSize();
            if (hasMostEnergy) {
                GetComponent<MeshRenderer>().material.color = Color.red;
            } else {
                GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }

    }

    //Make those with less energy look small
    private void UpdateSize() {
        float size = Mathf.InverseLerp(0, energyStart, energy);
        Vector3 newSize = new Vector3(size, size, size);
        transform.localScale = newSize;
    }

    private float[] ScanEnvironment() {
        RaycastHit hit;
        float[] inputs = new float[numberEyes];
        float angleOffset = Mathf.Floor(180 / numberEyes); //Divide by number of rays.
        float angle = -90f;
        for (int i = 0; i < inputs.Length; i ++) {
            var direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
            int mask = LayerMask.GetMask("food");
            if (Physics.Raycast(transform.position, direction, out hit, maxRayLength, mask)) {
                float mapped = Mathf.InverseLerp(0, maxRayLength, hit.distance);
                inputs[i] = mapped;
                Debug.DrawRay(transform.position, direction * hit.distance);
            } else {
                inputs[i] = 1f;
                Debug.DrawRay(transform.position, direction * maxRayLength);
            }
            angle += angleOffset;
        }
        return inputs;
    }

    private void PerformAction() {
        float[] inputs = ScanEnvironment();
        float[] outputs = nn.FeedForward(inputs);
        int action = PredictBestAction(outputs);
        float amount = outputs[action];
        energy -= energyPassiveLoss;
        switch (action) {
            case (0):
                transform.Rotate(new Vector3(0, amount*10f, 0));
                energy -= energyToRotate;
                break;
            case (1):
                transform.Rotate(new Vector3(0, amount*10f, 0));
               energy -= energyToRotate;
                break;
            case (2):
                transform.Translate(Vector3.forward*amount*(1/moveSpeed));
                energy -= energyToMove * (1+Mathf.Abs(amount));
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
        if (objHit.tag == "food") {
            energy += energyPerApple;
            Destroy(objHit);
        }
        if (objHit.tag == "man") {
            //energy -= energyLossWhenHitMan;
        }
    }
}
