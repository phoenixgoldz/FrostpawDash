using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EasterPathManager : MonoBehaviour
{
    [Header("Path Settings")]
    public GameObject floorPrefab;
    public float floorLength = 18f;
    public int initialSegments = 20;

    [Header("Walls")]
    public GameObject[] leftWallPrefabs;
    public GameObject[] rightWallPrefabs;

    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;

    [Header("Collectibles")]
    public GameObject collectiblePrefab;
    public int collectiblesPerRow = 5;
    public float collectibleSpawnChance = 0.6f;

    [Header("Gap Settings")]
    [Range(0f, 1f)] public float gapChance = 0.2f; // Chance to skip floor and create a fallable gap

    [Header("Bonus Path Options")]
    public GameObject bridgePrefab; // Assign Easter_Bridge in Inspector

    [Header("Bridge Settings")]
    public float initialBridgeChance = 0.3f;
    public float minBridgeChance = 0.05f;
    public float bridgeDecayRate = 0.0005f; // How quickly the bridge chance lowers over distance

    private Transform player;
    private List<GameObject> activePaths = new List<GameObject>();
    private float lastPathEndZ = 0f;

    private bool obstaclesEnabled = false;
    private bool lastWasGap = false;

    private class FloorSegment
    {
        public GameObject floorObject;
        public float startZ;
        public float endZ;

        public FloorSegment(GameObject obj, float zStart, float zEnd)
        {
            floorObject = obj;
            startZ = zStart;
            endZ = zEnd;
        }
    }
    private List<FloorSegment> floorSegments = new List<FloorSegment>();

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
        if (bridgePrefab == null)
        {
            Debug.LogWarning("⚠️ EasterPathManager: No bridgePrefab assigned!");
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

        // Ensure we always have 20 segments ahead of the player
        while (lastPathEndZ < player.position.z + (floorLength * initialSegments))
        {
            SpawnPathSegment();
        }

        DeleteOldPath();
    }
    void SpawnCollectiblesOnBridge(Vector3 bridgePosition)
    {
        int numEggs = Random.Range(1, 4); // Random number of eggs on the bridge

        for (int i = 0; i < numEggs; i++)
        {
            float offsetX = Random.Range(-2f, 2f); // Spread them across the bridge
            float offsetY = Random.Range(0.5f, 1.5f); // Above the bridge
            float offsetZ = Random.Range(-4f, 4f);

            Vector3 spawnPos = bridgePosition + new Vector3(offsetX, offsetY, offsetZ);
            GameObject collectible = Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
            activePaths.Add(collectible);
        }
    }
    void SpawnPathSegment()
    {
        float spawnZ = lastPathEndZ;

        // If last segment was a gap, force this one to be a floor
        bool forceFloor = lastWasGap;
        bool isGap = !forceFloor && (Random.value < gapChance);

        float distanceTraveled = player != null ? player.position.z : 0f;
        float currentBridgeChance = Mathf.Max(minBridgeChance, initialBridgeChance - distanceTraveled * bridgeDecayRate);
        bool spawnBridge = Random.value < currentBridgeChance;

        bool spawnCarrotCake = false;

        // Pre-determine if we’re placing CarrotCake (special floor-blocking obstacle)
        if (obstaclesEnabled)
        {
            GameObject testObstacle = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            if (testObstacle.name.ToLower().Contains("carrotcake"))
            {
                Vector3 carrotPos = new Vector3(0, testObstacle.transform.position.y, spawnZ);
                GameObject carrot = Instantiate(testObstacle, carrotPos, testObstacle.transform.rotation);
                activePaths.Add(carrot);
                spawnCarrotCake = true;
            }
        }

        if (!isGap && !spawnCarrotCake)
        {
            Vector3 floorPos = new Vector3(0, floorPrefab.transform.position.y, spawnZ);
            Quaternion floorRot = floorPrefab.transform.rotation;
            GameObject floor = Instantiate(floorPrefab, floorPos, floorRot);

            activePaths.Add(floor);
            floorSegments.Add(new FloorSegment(floor, spawnZ, spawnZ + floorLength));
        }

        // Bonus bridge
        if (spawnBridge && bridgePrefab != null)
        {
            Vector3 bridgePos = new Vector3(0, bridgePrefab.transform.position.y, spawnZ + floorLength * 0.5f);
            GameObject bridge = Instantiate(bridgePrefab, bridgePos, bridgePrefab.transform.rotation);
            activePaths.Add(bridge);
            SpawnCollectiblesOnBridge(bridgePos);
        }

        // Side walls
        if (leftWallPrefabs.Length > 0)
        {
            GameObject leftPrefab = leftWallPrefabs[Random.Range(0, leftWallPrefabs.Length)];
            Vector3 leftPos = new Vector3(-6f, leftPrefab.transform.position.y, spawnZ);
            GameObject leftWall = Instantiate(leftPrefab, leftPos, leftPrefab.transform.rotation);
            activePaths.Add(leftWall);
        }

        if (rightWallPrefabs.Length > 0)
        {
            GameObject rightPrefab = rightWallPrefabs[Random.Range(0, rightWallPrefabs.Length)];
            Vector3 rightPos = new Vector3(6f, rightPrefab.transform.position.y, spawnZ);
            GameObject rightWall = Instantiate(rightPrefab, rightPos, rightPrefab.transform.rotation);
            activePaths.Add(rightWall);
        }

        if (obstaclesEnabled && !isGap && !spawnCarrotCake)
        {
            SpawnObstacles(spawnZ);
        }

        SpawnCollectibles(spawnZ);

        lastPathEndZ += floorLength;
        lastWasGap = isGap;
    }

    void SpawnObstacles(float zPosition)
    {
        if (obstaclePrefabs.Length == 0 || floorSegments.Count == 0) return;

        FloorSegment targetFloor = floorSegments.Find(segment => zPosition >= segment.startZ && zPosition < segment.endZ);

        if (targetFloor == null || targetFloor.floorObject == null)
        {
            Debug.LogWarning("🚫 No valid floor found for obstacle spawn at Z = " + zPosition);
            return;
        }

        int attempts = 5; // How many total obstacles to try and spawn per floor
        int spawned = 2;

        for (int i = 0; i < attempts; i++)
        {
            // ✅ FIXED: Use different variable name for inner loop
            List<GameObject> weightedObstacles = new List<GameObject>();
            for (int j = 0; j < obstaclePrefabs.Length; j++)
            {
                int weight = (j <= 8) ? 4 : 1; // Prefabs 0–3 = 4x more likely
                for (int w = 0; w < weight; w++)
                {
                    weightedObstacles.Add(obstaclePrefabs[j]);
                }
            }

            GameObject prefab = weightedObstacles[Random.Range(0, weightedObstacles.Count)];
            Quaternion prefabRotation = prefab.transform.rotation;
            float prefabY = prefab.transform.position.y;

            float obstacleX = Random.Range(-3f, 3f);
            float obstacleZ = Random.Range(targetFloor.startZ + 2f, targetFloor.endZ - 2f);
            Vector3 obstaclePos = new Vector3(obstacleX, prefabY, obstacleZ);

            if (!IsOverlapping(obstaclePos))
            {
                GameObject obstacle = Instantiate(prefab, obstaclePos, prefabRotation);
                activePaths.Add(obstacle);
                spawned++;
            }
        }

        if (spawned == 0)
        {
            Debug.Log("❗ No obstacles spawned due to overlap at Z = " + zPosition);
        }
    }

    void SpawnCollectibles(float zPosition)
    {
        for (int i = 0; i < collectiblesPerRow; i++)
        {
            float collectibleX = Random.Range(-3f, 3f);
            Vector3 collectiblePos = new Vector3(collectibleX, collectiblePrefab.transform.position.y, zPosition + Random.Range(2f, floorLength - 2f));

            if (!IsOverlapping(collectiblePos))
            {
                GameObject collectible = Instantiate(collectiblePrefab, collectiblePos, collectiblePrefab.transform.rotation);
                activePaths.Add(collectible);
            }
        }
    }

    bool IsOverlapping(Vector3 position)
    {
        for (int i = activePaths.Count - 1; i >= 0; i--)
        {
            if (activePaths[i] == null)
            {
                activePaths.RemoveAt(i); // ✅ Clean up destroyed references
                continue;
            }

            if (Vector3.Distance(activePaths[i].transform.position, position) < 2f)
            {
                return true;
            }
        }

        return false;
    }
    void DeleteOldPath()
    {
        // Delete old segments behind the player to avoid memory bloat
        for (int i = activePaths.Count - 1; i >= 0; i--)
        {
            if (activePaths[i] == null) continue;

            // If the object is far behind the player
            if (activePaths[i].transform.position.z < player.position.z - floorLength * 2f)
            {
                Destroy(activePaths[i]);
                activePaths.RemoveAt(i);
            }
        }

        // Clean up floor segments as well
        floorSegments.RemoveAll(seg =>
            seg.floorObject == null ||
            seg.floorObject.transform.position.z < player.position.z - floorLength * 2f
        );
    }
}
