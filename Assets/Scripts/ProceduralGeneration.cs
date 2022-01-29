using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    public List<GameObject> levelBlocks = new List<GameObject>();
    public List<GameObject> spawnedLevelBlocks = new List<GameObject>();
    public GameObject player = null;
    private Vector3 lastBlockSpawnPoint;
    public GameObject lastBlock;
    private GameObject lastBlockPrefab;
    private int blockIndex = 0;
    private int spavnetobjectIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        lastBlockPrefab = this.gameObject.transform.GetChild(0).gameObject;
        lastBlock = this.gameObject.transform.GetChild(0).gameObject;
        this.spawnedLevelBlocks.Add(lastBlock);
    }

    List<GameObject> drawLoop(GameObject lastObject, GameObject objToSpawn)
    {
        // configuration:
        float heightOffset = 10f;
        float horizontalDistancePerPlatform = 10f;
        //

        List<GameObject> levelBlocksSpawnTemp = new List<GameObject>();
        Debug.Log("Building LOOP");

        int pieceCount = 10;
        float radius = (pieceCount / 2) * 2;
        float angle = 360f / (float)pieceCount;
        Vector3 centerPoint = new Vector3(lastObject.transform.position.x, (lastObject.transform.position.y + radius), lastObject.transform.position.z);

        for (int i = 1; i < pieceCount + 2; i++)
        {
            Quaternion rotation = (Quaternion.AngleAxis((i - 1) * angle, Vector3.back));
            Vector3 direction = rotation * Vector3.down;
            Vector3 position = (lastObject.transform.position + (direction * radius));

            levelBlocksSpawnTemp.Add(Instantiate(objToSpawn, new Vector3(position.x, position.y + heightOffset, position.z + (float)(i * horizontalDistancePerPlatform)), rotation));
        }

        return levelBlocksSpawnTemp;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = this.player.transform.position;
        float distance = Vector3.Distance(this.spawnedLevelBlocks[0].transform.position, playerPosition);

        Debug.Log("Index" + 0);

        if (distance > 10.0f && this.spawnedLevelBlocks.Count > 10)
        {
            Destroy(this.spawnedLevelBlocks[0]);
            this.spawnedLevelBlocks.Remove(this.spawnedLevelBlocks[0]);
            spavnetobjectIndex++;
        }

        if (this.spawnedLevelBlocks.Count <= 10)
        {
            MeshFilter meshfilter = lastBlock.GetComponent<MeshFilter>();
            Bounds bounds = meshfilter.mesh.bounds;

            float scale = meshfilter.transform.localScale.x;
            Bounds b = new Bounds(bounds.center * scale, bounds.size * scale);

            int blockToSpawn = Random.Range(0, levelBlocks.Count - 1);

            Debug.Log(blockToSpawn);

            GameObject instantiatedGameObject;
            GameObject blockObjToSpawn;

            blockObjToSpawn = levelBlocks[blockToSpawn];
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
                if ((blockToSpawn + 1) <= levelBlocks.Count)
                {
                    blockToSpawn++;
                }
                else if ((blockToSpawn - 1) >= levelBlocks.Count)
                {
                    blockToSpawn--;
                }
            }

            if ((blockToSpawn > -1 && (blockToSpawn < (levelBlocks.Count - 1))))
            {
                blockObjToSpawn = levelBlocks[blockToSpawn];
                instantiatedGameObject = Instantiate(blockObjToSpawn, new Vector3(0, 0, blockIndex * (b.size.z + 1.0f)), (Quaternion.identity));
                this.spawnedLevelBlocks.Add(instantiatedGameObject);
                blockIndex++;
            }
            else
            {
                List<GameObject> instantiatedGameObjectLists = this.drawLoop(lastBlock, levelBlocks[0]);
                foreach (var spavnedBlock in instantiatedGameObjectLists)
                {
                    this.spawnedLevelBlocks.Add(spavnedBlock);
                    blockIndex++;

                }
                instantiatedGameObject = this.spawnedLevelBlocks[this.spawnedLevelBlocks.Count - 1];
                blockObjToSpawn = levelBlocks[0];
            }

            Debug.Log("Spawn" + blockToSpawn);


            lastBlock = instantiatedGameObject;
            lastBlockSpawnPoint = instantiatedGameObject.transform.position;
            lastBlockPrefab = blockObjToSpawn;
        }
    }
}

