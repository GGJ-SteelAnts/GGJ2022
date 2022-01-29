using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public enum PlatformType {Pull, Push, RotateZ, RotateY, Speed};
    public PlatformType type = PlatformType.Pull;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (type == PlatformType.RotateZ)
        {
            transform.Rotate(transform.forward);
        } else if (type == PlatformType.RotateY) {
            transform.Rotate(transform.up);
        }
    }
}
