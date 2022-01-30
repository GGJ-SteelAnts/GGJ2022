using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gizmoManager : MonoBehaviour
{

    void OnDrawGizmos()
    {
        if (this.gameObject != null && !this.gameObject.transform.parent.name.Contains("chunk"))
        {
            Bounds bounds = GetChildRendererBounds(gameObject);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
    Bounds GetChildRendererBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 0, ni = renderers.Length; i < ni; i++)
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
