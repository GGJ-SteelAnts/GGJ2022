using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    public List<GameObject> levelBlocks = new List<GameObject>();
    public List<GameObject> spawnedLevelBlocks = new List<GameObject>();
    public GameObject player = null;
    public GameObject lastBlock;
    private GameObject lastBlockPrefab;
    private int spavnetobjectIndex = 0;
    private int maximumNumberOfPlatformsAtScene = 100;
    private float maximumDistanceOfPlatformFromPlayer = 20.0f;
    private GameObject levelParrent = null;


    Bounds getPrefabBounds(GameObject go)
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
    GameObject drawPlatform(GameObject lastObject, GameObject objToSpawn, GameObject parentLevelObject)
    {
        Bounds bounds = this.getPrefabBounds(lastObject);
        Bounds b = new Bounds(bounds.center, bounds.size);

        Vector3 nextBlockLocation = new Vector3(lastObject.transform.position.x, lastObject.transform.position.y, lastObject.transform.position.z + b.size.z + 1.0f);

        GameObject newObject = Instantiate(objToSpawn, nextBlockLocation, (Quaternion.identity));
        newObject.transform.parent = parentLevelObject.transform;
        return newObject;
    }

    List<GameObject> spawnSpiralOfPlatforms(GameObject lastObject, GameObject objToSpawn, GameObject parentLevelObject)
    {
        // configuration:
        float horizontalDistancePerPlatform = (float)Random.Range(0.5f, 2.0f); ;

        List<GameObject> levelBlocksSpawnTemp = new List<GameObject>();
        // Debug.Log("Building LOOP");

        int pieceCount = 10;
        float radius = (pieceCount / 2) * 2;
        float angle = 360f / (float)pieceCount;

        Bounds bounds = this.getPrefabBounds(lastObject);
        Bounds b = new Bounds(bounds.center, bounds.size);

        Vector3 centerPoint = new Vector3(lastObject.transform.position.x, (lastObject.transform.position.y + radius), lastObject.transform.position.z + b.size.z + 1.0f);

        float heightOffset = radius;

        for (int i = 1; i < pieceCount + 1; i++)
        {
            Quaternion rotation = (Quaternion.AngleAxis(i * angle, Vector3.back));
            Vector3 direction = rotation * Vector3.down;
            Vector3 position = (lastObject.transform.position + (direction * radius));

            GameObject newObject = Instantiate(objToSpawn, new Vector3(position.x, position.y + heightOffset, position.z + (float)(i * horizontalDistancePerPlatform)), rotation);
            newObject.transform.parent = parentLevelObject.transform;
            levelBlocksSpawnTemp.Add(newObject);
        }

        return levelBlocksSpawnTemp;
    }

    // Start is called before the first frame update
    void Start()
    {
        levelParrent = this.gameObject;
        lastBlockPrefab = this.gameObject.transform.GetChild(0).gameObject;
        lastBlock = this.gameObject.transform.GetChild(0).gameObject;
        this.spawnedLevelBlocks.Add(lastBlock);


    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = this.player.transform.position;
        PlayerController playerControlsSript = this.player.GetComponent<PlayerController>();

        if (playerControlsSript.isFalling)
        {
            return;
        }

        for (var i = 0; i < this.spawnedLevelBlocks.Count; i++)
        {
            float distance = Vector3.Distance(this.spawnedLevelBlocks[i].transform.position, playerPosition);
            if (distance > this.maximumDistanceOfPlatformFromPlayer && this.spawnedLevelBlocks.Count >= this.maximumNumberOfPlatformsAtScene)
            {
                Destroy(this.spawnedLevelBlocks[i]);
                this.spawnedLevelBlocks.Remove(this.spawnedLevelBlocks[i]);
                spavnetobjectIndex++;
            }
            else
            {
                break;
            }
        }

        if (this.spawnedLevelBlocks.Count <= this.maximumNumberOfPlatformsAtScene)
        {
            GameObject instantiatedGameObject;
            GameObject blockObjToSpawn;

            int blockToSpawn = Random.Range(0, (levelBlocks.Count + 1));


            if (blockToSpawn > 31 && (blockToSpawn < levelBlocks.Count) && levelBlocks[blockToSpawn].name == lastBlockPrefab.name)
            {
                Debug.Log("Same Block");
                if (blockToSpawn > levelBlocks.Count || blockToSpawn < 0)
                {
                    blockToSpawn = Random.Range(0, levelBlocks.Count);
                }
            }


            if (blockToSpawn > -1 && (blockToSpawn < levelBlocks.Count))
            {
                blockObjToSpawn = levelBlocks[blockToSpawn];
                instantiatedGameObject = this.drawPlatform(this.lastBlock, this.levelBlocks[blockToSpawn], this.levelParrent);
                this.spawnedLevelBlocks.Add(instantiatedGameObject);
            }
            else
            {
                List<GameObject> instantiatedGameObjectLists = this.spawnSpiralOfPlatforms(lastBlock, levelBlocks[0], this.levelParrent);
                foreach (var spavnedBlock in instantiatedGameObjectLists)
                {
                    this.spawnedLevelBlocks.Add(spavnedBlock);
                }
                instantiatedGameObject = this.spawnedLevelBlocks[this.spawnedLevelBlocks.Count - 1];
                blockObjToSpawn = levelBlocks[0];
            }

            this.lastBlock = instantiatedGameObject;
            this.lastBlockPrefab = blockObjToSpawn;

        }
    }
}

