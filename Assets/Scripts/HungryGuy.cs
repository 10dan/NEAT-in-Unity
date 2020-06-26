using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HungryGuy : MonoBehaviour {
    public float rayLength = 100f;

    private void Update() {
        RaycastHit hit;
        float numberOfRays = 10;
        float angle = (2 * Mathf.PI) / numberOfRays; //Divide by number of rays.
        for (float i = 0f; i <= 2 * Mathf.PI; i += angle) {
            float x = Mathf.Cos(i);
            float y = 0f;
            float z = Mathf.Sin(i);
            Vector3 dir = new Vector3(x, y, z);
            if (Physics.Raycast(transform.position, dir, out hit, rayLength)) {

                Debug.DrawRay(transform.position, dir * hit.distance);
            } else {
                Debug.DrawRay(transform.position, dir * 100f);
            }
        }
    }
}
