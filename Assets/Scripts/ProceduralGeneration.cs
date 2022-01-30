using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    [Header("Level")]
    public List<GameObject> levelBlocks = new List<GameObject>();
    private int maximumNumberOfPlatformsAtScene = 100;
    public List<GameObject> spawnedLevelBlocks = new List<GameObject>();
    private float maximumDistanceOfPlatformFromPlayer = 20.0f;
    private GameObject levelParrent = null;

    [Header("Background")]
    public List<GameObject> backgroundBlocks = new List<GameObject>();
    public List<GameObject> backgroundLevelBlocks = new List<GameObject>();
    private float maximumDistanceOfPlatformFromPlayerBg = 600.0f;
    public GameObject bgParrent = null;

    public GameObject player = null;
    public GameObject lastBlock;
    private GameObject lastBlockPrefab;

    List<GameObject> spawnBackgroundObjects(Vector3 playerPosition, List<GameObject> backgroundBlocks, GameObject parentLevelObject)
    {
        List<GameObject> bgBlocksSpawnTemp = new List<GameObject>();
        for (var i = 0; i < 200; i++)
        {
            int bgElement = Random.Range(0, backgroundBlocks.Count - 1);

            GameObject newBgObject = Instantiate(backgroundBlocks[bgElement], (Random.insideUnitSphere * Random.Range(300, maximumDistanceOfPlatformFromPlayerBg) + playerPosition + new Vector3(0, 0, 50)), (Quaternion.identity));
            newBgObject.transform.Rotate(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
            newBgObject.transform.parent = bgParrent.transform;

            bgBlocksSpawnTemp.Add(newBgObject);
        }

        return bgBlocksSpawnTemp;
    }
    Bounds getPrefabBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
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
        Bounds b = this.getPrefabBounds(objToSpawn);
        Vector3 nextBlockLocation = new Vector3(lastObject.transform.position.x, lastObject.transform.position.y, lastObject.transform.position.z + bounds.extents.z + b.extents.z);
        GameObject newObject = Instantiate(objToSpawn, nextBlockLocation, (Quaternion.identity));
        newObject.transform.parent = parentLevelObject.transform;
        return newObject;
    }
    List<GameObject> spawnSpiralOfPlatforms(GameObject lastObject, GameObject objToSpawn, GameObject parentLevelObject)
    {
        // configuration:
        float horizontalDistancePerPlatform = (float)Random.Range(2.0f, 3.0f);

        List<GameObject> levelBlocksSpawnTemp = new List<GameObject>();
        // Debug.Log("Building LOOP");

        int pieceCount = 10;
        float radius = (pieceCount / 2) + 1.9f;
        float angle = 360f / (float)pieceCount;

        Bounds bounds = this.getPrefabBounds(lastObject);
        Bounds b = this.getPrefabBounds(objToSpawn);

        Vector3 centerPoint = new Vector3(lastObject.transform.position.x, (lastObject.transform.position.y + radius), lastObject.transform.position.z + (lastObject.name.Contains("chunk") ? bounds.size.z : bounds.extents.z) + b.extents.z);

        float heightOffset = radius;

        for (int i = 1; i < pieceCount + 1; i++)
        {
            Quaternion rotation = (Quaternion.AngleAxis(i * angle, Vector3.back));
            Vector3 direction = rotation * Vector3.down;
            Vector3 position = (lastObject.transform.position + (direction * radius));

            GameObject newObject = Instantiate(objToSpawn, new Vector3(position.x, position.y + heightOffset, position.z + (float)(i * horizontalDistancePerPlatform) + (lastObject.name.Contains("chunk") ? bounds.extents.z : 0f)), rotation);
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

        // if (playerControlsSript.isFalling)
        // {
        //     return;
        // }

        Vector3 playerPosition = this.player.transform.position;
        PlayerController playerControlsSript = this.player.GetComponent<PlayerController>();

        if (backgroundLevelBlocks.Count < 200)
        {
            foreach (var spavnedBgBlock in this.spawnBackgroundObjects(playerPosition, this.backgroundBlocks, this.levelParrent))
            {
                this.backgroundLevelBlocks.Add(spavnedBgBlock);
            }
        }

        for (var i = 0; i < this.backgroundLevelBlocks.Count; i++)
        {
            float distance = Vector3.Distance(this.backgroundLevelBlocks[i].transform.position, playerPosition);
            if (distance > this.maximumDistanceOfPlatformFromPlayer + maximumDistanceOfPlatformFromPlayerBg)
            {

                Destroy(this.backgroundLevelBlocks[i]);
                this.backgroundLevelBlocks.Remove(this.backgroundLevelBlocks[i]);
            }
        }

        for (var i = 0; i < this.spawnedLevelBlocks.Count; i++)
        {
            float distance = Vector3.Distance(this.spawnedLevelBlocks[i].transform.position, playerPosition);
            if (distance > this.maximumDistanceOfPlatformFromPlayer && this.spawnedLevelBlocks.Count >= this.maximumNumberOfPlatformsAtScene && playerPosition.z > this.spawnedLevelBlocks[i].transform.position.z)
            {
                Destroy(this.spawnedLevelBlocks[i]);
                this.spawnedLevelBlocks.Remove(this.spawnedLevelBlocks[i]);
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


            if (levelBlocks[blockToSpawn].name == lastBlockPrefab.name)
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

