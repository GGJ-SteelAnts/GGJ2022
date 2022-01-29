using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gizmoManager : MonoBehaviour
{

    void OnDrawGizmos()
    {
        Bounds bounds = GetChildRendererBounds(gameObject);
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
    Bounds GetChildRendererBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1, ni = renderers.Length; i < ni; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }
}
