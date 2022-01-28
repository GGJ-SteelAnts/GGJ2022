using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessingController : MonoBehaviour
{
    [ColorUsageAttribute(true, true)] public Color color;
    public Renderer renderer;
    public bool emissionUp = false;
    private int emmisionStep = 0;
    public int emmisionStepChange = 30;

    // Start is called before the first frame update
    void Start()
    {
        color = renderer.material.GetColor("_EmissionColor");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (emmisionStep < emmisionStepChange)
        {
            if (emissionUp)
            {
                color += color * 0.01f;
            }
            else
            {
                color -= color * 0.01f;
            }
        }
        else
        {
            emissionUp = !emissionUp;
            emmisionStep = 0;
        }
        emmisionStep++;

        renderer.material.SetColor("_EmissionColor", color);
        //Debug.Log(color);
    }
}
