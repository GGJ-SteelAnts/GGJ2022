using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGRotation : MonoBehaviour
{
    private Vector3 rotation;

    private void Start()
    {
        rotation = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
    }

    void FixedUpdate()
    {
        transform.Rotate(rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "platform")
        {
            Destroy(this.gameObject);
        }
    }
}
