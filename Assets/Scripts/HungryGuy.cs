using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HungryGuy : MonoBehaviour {
    public float appleScore = 10f; //fitness per apple eaten

    public float rayLength = 10f;
    public float maxRayLength = 10f;
    int numberOfRays = 1;

    public NeuralNetwork nn;
    public float fitness = 0;

    public void Start() {
        int[] nnStructure = { numberOfRays, 10, 3 };
        nn = new NeuralNetwork(nnStructure);
    }

    public void Update() {
        RaycastHit hit;
        float angle = (2 * Mathf.PI) / numberOfRays; //Divide by number of rays.
        float[] inputs = new float[numberOfRays]; //What will be sent to feedforward method.
        int c = 0; //Identify which input neuron to calc value for.
        //for (float i = 0f; i <= 2 * Mathf.PI; i += angle) {
           // float x = Mathf.Cos(i);
           // float y = 0f;
           // float z = Mathf.Sin(i);
            Vector3 currentDir = transform.forward;
            Vector3 dir = currentDir;// new Vector3(x, y, z);
            if (Physics.Raycast(transform.position, dir, out hit, rayLength)) {
                Debug.DrawRay(transform.position, dir * hit.distance);
                float mapped = Mathf.InverseLerp(0, maxRayLength, hit.distance);
                inputs[c] = mapped;
            } else {
                Debug.DrawRay(transform.position, dir * maxRayLength);
                inputs[c] = -0.1f;
            }
            c++;
       // }

        float[] outputs = nn.FeedForward(inputs); //get the nn prediction for best move. (left, right or forward)
        int action = 0;
        for (int i = 0; i < outputs.Length; i++) {
            if (outputs[i] > outputs[action]) {
                action = i;
            }
        }

        //Perform generated action
        print(action);
        switch (action) {
            case (0):
                transform.Rotate(new Vector3(0, -0.5f, 0));
                break;
            case (1):
                transform.Rotate(new Vector3(0, 0.5f, 0));
                break;
            case (2):
                transform.Translate(Vector3.forward * 0.1f);
                break;
        }
    }

    public void OnCollisionEnter(Collision collision) {
        GameObject objHit = collision.collider.gameObject;
        if (objHit.tag == "food") {
            fitness += appleScore;
            Destroy(objHit);
        }
    }
}
