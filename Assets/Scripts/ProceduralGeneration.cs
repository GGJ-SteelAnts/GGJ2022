using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    public List<GameObject> levelBlocks = new List<GameObject>();
    public Vector3 lastBlockSpawnPoint;
    public GameObject lastBlock;


    // Start is called before the first frame update
    void Start()
    {

        for (var i = 0; i < 10; i++)
        {
            MeshFilter meshfilter = lastBlock.GetComponent<MeshFilter>();
            Bounds bounds = meshfilter.mesh.bounds;

            float scale = meshfilter.transform.localScale.x;
            Bounds b = new Bounds(bounds.center * scale, bounds.size * scale);

            Debug.Log(b.size.x);

            GameObject instantiatedGameObject = Instantiate(levelBlocks[1], new Vector3(0, 0, i * (b.size.z * 2)), Quaternion.identity);
            instantiatedGameObject.transform.SetParent(this.gameObject.transform);

            lastBlock = instantiatedGameObject;
            lastBlockSpawnPoint = instantiatedGameObject.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
