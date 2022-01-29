using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public enum PlatformType {Pull, Push, Rotate};
    public PlatformType type = PlatformType.Pull;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (type == PlatformType.Rotate)
        {
            transform.Rotate(transform.forward);
        }
    }
}
