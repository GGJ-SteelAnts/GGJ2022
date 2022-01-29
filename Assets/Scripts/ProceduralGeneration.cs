using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    public List<GameObject> levelBlocks = new List<GameObject>();
    private List<GameObject> spawnedLevelBlocks = new List<GameObject>();
    public GameObject player = null;
    private Vector3 lastBlockSpawnPoint;
    private GameObject lastBlock;
    private GameObject lastBlockPrefab;
    private int blockIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        lastBlockPrefab = this.gameObject.transform.GetChild(0).gameObject;
        lastBlock = this.gameObject.transform.GetChild(0).gameObject;
        spawnedLevelBlocks.Add(lastBlock);


        int pieceCount = 10;
        float radius = (pieceCount + 1 / 2) * 2;
        float angle = 360f / (float)pieceCount;

        for (int i = 1; i < pieceCount + 1; i++)
        {


            Quaternion rotation = (Quaternion.AngleAxis(i * angle, Vector3.back));
            Vector3 direction = rotation * Vector3.down;
            Vector3 position = (transform.position + (direction * radius)) * (float)(1 + i * 0.05f);


            Instantiate(levelBlocks[0], new Vector3(position.x, position.y, position.z), rotation);
        }
    }



    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = this.player.transform.position;
        float distance = Vector3.Distance(spawnedLevelBlocks[0].transform.position, playerPosition);
        Debug.Log(distance);

        if (distance > 10.0f && spawnedLevelBlocks.Count > 10)
        {
            Destroy(spawnedLevelBlocks[0]);
            spawnedLevelBlocks.Remove(spawnedLevelBlocks[0]);
        }

        if (spawnedLevelBlocks.Count <= 10)
        {
            MeshFilter meshfilter = lastBlock.GetComponent<MeshFilter>();
            Bounds bounds = meshfilter.mesh.bounds;

            float scale = meshfilter.transform.localScale.x;
            Bounds b = new Bounds(bounds.center * scale, bounds.size * scale);

            int blockToSpawn = Random.Range(0, levelBlocks.Count);
            GameObject blockObjToSpawn = levelBlocks[blockToSpawn];

            if (blockObjToSpawn.name == lastBlockPrefab.name)
            {
                Debug.Log("Same Block");
                if ((blockToSpawn + 1) <= levelBlocks.Count)
                {
                    blockToSpawn++;
                }
                else if ((blockToSpawn - 1) >= levelBlocks.Count)
                {
                    blockToSpawn--;
                }
                blockObjToSpawn = levelBlocks[blockToSpawn];
            }

            GameObject instantiatedGameObject = Instantiate(blockObjToSpawn, new Vector3(0, 0, blockIndex * (b.size.z + 1.0f)), (Quaternion.identity));

            lastBlock = instantiatedGameObject;
            spawnedLevelBlocks.Add(lastBlock);
            lastBlockPrefab = blockObjToSpawn;
            lastBlockSpawnPoint = instantiatedGameObject.transform.position;
            blockIndex++;
        }
    }
}
