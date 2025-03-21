using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EasterPathManager : MonoBehaviour
{
    [Header("Path Settings")]
    public GameObject floorPrefab;
    public float floorLength = 18f;
    public int initialSegments = 5;

    [Header("Walls")]
    public GameObject[] leftWallPrefabs;
    public GameObject[] rightWallPrefabs;

    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;

    [Header("Collectibles")]
    public GameObject collectiblePrefab;
    public int collectiblesPerRow = 5;
    public float collectibleSpawnChance = 0.6f;

    private Transform player;
    private List<GameObject> activePaths = new List<GameObject>();
    private float lastPathEndZ = 0f;

    private bool obstaclesEnabled = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("❌ EasterPathManager: Player not found!");
            return;
        }

        // Spawn initial segments
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnPathSegment();
        }

        StartCoroutine(EnableObstaclesAfterDelay(10f)); // Delay obstacle spawning for 10 seconds
    }

    IEnumerator EnableObstaclesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        obstaclesEnabled = true; // Enable obstacles after delay
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.z + floorLength > lastPathEndZ)
        {
            SpawnPathSegment();
            DeleteOldPath();
        }
    }

    void SpawnPathSegment()
    {
        float spawnZ = lastPathEndZ;

        // ✅ Spawn floor at correct height
        GameObject floor = Instantiate(floorPrefab, new Vector3(0, -0.5f, spawnZ), Quaternion.identity);
        activePaths.Add(floor);

        // ✅ Spawn left and right walls at fixed positions
        if (leftWallPrefabs.Length > 0)
        {
            GameObject leftWall = Instantiate(leftWallPrefabs[Random.Range(0, leftWallPrefabs.Length)],
                new Vector3(-6f, 0, spawnZ), Quaternion.Euler(0, 90, 0));
            activePaths.Add(leftWall);
        }

        if (rightWallPrefabs.Length > 0)
        {
            GameObject rightWall = Instantiate(rightWallPrefabs[Random.Range(0, rightWallPrefabs.Length)],
                new Vector3(6f, 0, spawnZ), Quaternion.Euler(0, -90, 0));
            activePaths.Add(rightWall);
        }

        // ✅ Spawn obstacles ONLY after 10 seconds
        if (obstaclesEnabled)
        {
            SpawnObstacles(spawnZ);
        }

        // ✅ Always spawn collectibles
        SpawnCollectibles(spawnZ);

        lastPathEndZ += floorLength;
    }

    void SpawnObstacles(float zPosition)
    {
        if (obstaclePrefabs.Length > 0)
        {
            for (int i = 0; i < obstaclePrefabs.Length; i++)
            {
                GameObject prefab = obstaclePrefabs[i];

                // ✅ Get original prefab rotation & height
                Quaternion prefabRotation = prefab.transform.rotation;
                float prefabY = prefab.transform.position.y;

                float obstacleX = Random.Range(-3f, 3f);
                Vector3 obstaclePos = new Vector3(obstacleX, prefabY, zPosition + Random.Range(2f, floorLength - 2f));

                if (!IsOverlapping(obstaclePos))
                {
                    GameObject obstacle = Instantiate(prefab, obstaclePos, prefabRotation);
                    activePaths.Add(obstacle);
                }
            }
        }
    }

    void SpawnCollectibles(float zPosition)
    {
        for (int i = 0; i < collectiblesPerRow; i++)
        {
            float collectibleX = Random.Range(-3f, 3f);
            Vector3 collectiblePos = new Vector3(collectibleX, 1f, zPosition + Random.Range(2f, floorLength - 2f));

            if (!IsOverlapping(collectiblePos))
            {
                GameObject collectible = Instantiate(collectiblePrefab, collectiblePos, Quaternion.identity);
                activePaths.Add(collectible);
            }
        }
    }

    bool IsOverlapping(Vector3 position)
    {
        foreach (GameObject obj in activePaths)
        {
            if (Vector3.Distance(obj.transform.position, position) < 2f)
            {
                return true;
            }
        }
        return false;
    }

    void DeleteOldPath()
    {
        if (activePaths.Count > initialSegments * 2)
        {
            Destroy(activePaths[0]);
            activePaths.RemoveAt(0);
        }
    }
}
