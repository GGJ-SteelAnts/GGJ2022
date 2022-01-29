using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePlatformGenerator : MonoBehaviour
{
    public Object objectToCopy;
    // Start is called before the first frame update
    void Start()
    {
        int pieceCount = 10;
        float angle = 360f / (float)pieceCount;
        // var axis = transform.RotateAround(axis, angle * Time.deltaTime * 5f);
        for (int i = 0; i < pieceCount; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(i * angle, Vector3.forward);
            Vector3 direction = rotation * Vector3.forward;

            Vector3 position = transform.position + (direction * 20);
            Instantiate(this.objectToCopy, Vector3.zero, rotation);
        }

    }
}
