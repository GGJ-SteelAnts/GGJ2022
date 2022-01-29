using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public enum PlatformType {Pull, Push, RotateZ, RotateY, Speed};
    public PlatformType type = PlatformType.Pull;
    public float speed = 5;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (type == PlatformType.RotateZ)
        {
            transform.Rotate(transform.forward * speed * Time.deltaTime);
        } else if (type == PlatformType.RotateY) {
            transform.Rotate(transform.up * speed * Time.deltaTime);
        }
    }
}
