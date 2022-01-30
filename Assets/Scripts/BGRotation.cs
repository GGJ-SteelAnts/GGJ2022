using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGRotation : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(new Vector3(Random.Range(0,360), Random.Range(0, 360), Random.Range(0, 360)));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "platform")
        {
            Destroy(this.gameObject);
        }
    }
}
