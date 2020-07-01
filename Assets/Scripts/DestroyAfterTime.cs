using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {
    public float numSeconds = 5f;
    private void Start() {
        StartCoroutine(KillAfter(numSeconds));
    }
    IEnumerator KillAfter(float numSeconds) {
        yield return new WaitForSeconds(numSeconds);
        Destroy(gameObject);
    }
}
