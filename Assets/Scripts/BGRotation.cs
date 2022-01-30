using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGRotation : MonoBehaviour
{
    private Vector3 rotation;
    private ProceduralGeneration bgParrentScript;

    private void Start()
    {
        rotation = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)) * 0.009f;
        bgParrentScript = GameObject.Find("Level").GetComponent<ProceduralGeneration>();
    }

    void FixedUpdate()
    {
        transform.Rotate(rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "platform")
        {
            this.bgParrentScript.backgroundLevelBlocks.Remove(this.gameObject);
            Destroy(this.gameObject);
        }

    }
}
