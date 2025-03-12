using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PathManager : MonoBehaviour
{
    [Header("Path Settings")]
    public GameObject[] pathPrefabs;
    public GameObject flatIcePathPrefab; // FlatIcePath prefab for safe start
    public GameObject[] leftWallPrefabs;
    public GameObject[] rightWallPrefabs;
    public GameObject[] obstaclePrefabs;

    public int initialSegments = 5;
    public float pathLength = 18f;
    public float wallXOffset = 12f;

    public GameObject blueCrystalsPrefab;
    private bool canSpawnBlueCrystals = false; // Prevent early spawning so player can move around

    private Transform player;
    private List<GameObject> activePaths = new List<GameObject>();
    private float lastPathEndZ = 0f;
    private int pathsSpawned = 10; // Track how many paths are spawned

    [Header("Collectibles")]
    public GameObject gemPrefab; //  Reference to the Gem Prefab
    public int gemsPerRow = 10; // Number of gems per spawn row

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("❌ PathManager: Player not found!");
            return;
        }

        for (int i = 0; i < initialSegments; i++)
        {
            SpawnPath();
        }

        // Start the delay timer for BlueCrystals spawning
        StartCoroutine(EnableBlueCrystalsSpawn());
    }
    IEnumerator EnableBlueCrystalsSpawn()
    {
        yield return new WaitForSeconds(15f); // Wait for 15 seconds
        canSpawnBlueCrystals = true; // Allow spawning after delay
    }


    void Update()
    {
        if (player == null) return;

        if (player.position.z + pathLength > lastPathEndZ)
        {
            SpawnPath();
            DeleteOldPath();
        }
    }

    public void SpawnPath()
    {
        GameObject newPath;

        // Ensure first 15 paths are always FlatIcePath for safe testing
        if (pathsSpawned < 15)
        {
            newPath = Instantiate(flatIcePathPrefab, new Vector3(0, 0, lastPathEndZ), Quaternion.identity);
        }
        else
        {
            int floorIndex = Random.Range(0, pathPrefabs.Length);
            newPath = Instantiate(pathPrefabs[floorIndex], new Vector3(0, 0, lastPathEndZ), Quaternion.identity);
        }
        //if (pathsSpawned >= 15 && obstaclePrefabs.Length > 0 && Random.Range(0, 100) < 40)
        //{
        //    int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        //    Vector3 obstaclePosition = new Vector3(
        //        Random.Range(-1f, 1f), // Random X position
        //        1.0f, // Fixed Y-axis spawn at 1.0f
        //        lastPathEndZ + Random.Range(1f, pathLength - 1f) // Random Z position ahead
        //    );

        //    GameObject obstacle = Instantiate(obstaclePrefabs[obstacleIndex], obstaclePosition, Quaternion.identity);

        //    activePaths.Add(obstacle);
        //}

        newPath.tag = "PathTrigger";
        activePaths.Add(newPath);
        pathsSpawned++; // Increment path count

        //  Ensure Crystal Caverns Floor is at Y = 0.83
        if (newPath.name.Contains("Crystal Caverns Floor"))
        {
            newPath.transform.position = new Vector3(0, 0.83f, lastPathEndZ);
            newPath.transform.rotation = Quaternion.LookRotation(Vector3.right);
        }

        //  Spawn left wall
        if (leftWallPrefabs.Length > 0)
        {
            int leftIndex = Random.Range(0, leftWallPrefabs.Length);
            GameObject leftWall = Instantiate(leftWallPrefabs[leftIndex], new Vector3(-wallXOffset, 7.64f, lastPathEndZ), Quaternion.Euler(0, 90, 0));
            if (leftWall.name.Contains("CavernWall"))
            {
                leftWall.transform.position = new Vector3(leftWall.transform.position.x, leftWall.transform.position.y - 3.25f, leftWall.transform.position.z);
            }
            activePaths.Add(leftWall);
            RepositionIfColliding(leftWall);
        }

        //  Spawn right wall
        if (rightWallPrefabs.Length > 0)
        {
            int rightIndex = Random.Range(0, rightWallPrefabs.Length);
            GameObject rightWall = Instantiate(rightWallPrefabs[rightIndex], new Vector3(wallXOffset, 7.64f, lastPathEndZ), Quaternion.Euler(0, 90, 0));
            if (rightWall.name.Contains("CavernWall"))
            {
                rightWall.transform.position = new Vector3(rightWall.transform.position.x, rightWall.transform.position.y - 3.25f, rightWall.transform.position.z);
            }
            activePaths.Add(rightWall);
            RepositionIfColliding(rightWall);
        }

        //  Spawn obstacles randomly (40% chance)
        if (obstaclePrefabs.Length > 0 && Random.Range(0, 100) < 40)
        {
            int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject obstacle = Instantiate(obstaclePrefabs[obstacleIndex], new Vector3(Random.Range(-5f, 5f), 1, lastPathEndZ + Random.Range(1f, pathLength - 1f)), Quaternion.identity);

            // Ensure Ice Archway is placed correctly at Y = 4.4
            if (obstacle.name.Contains("Ice Archway"))
            {
                //obstacle.transform.position = new Vector3(obstacle.transform.position.x, 4.4f, obstacle.transform.position.z);
                obstacle.transform.position = new Vector3(0, 4.4f, obstacle.transform.position.z);
            }

            activePaths.Add(obstacle);
        }
        if (canSpawnBlueCrystals && blueCrystalsPrefab != null)
        {
            // Ensure the BlueCrystals spawn with a fixed Y-axis at 3.0f
            Vector3 crystalPosition = new Vector3(
                Random.Range(-5f, 5f), // Random X position
                3.0f, // Fixed Y-axis spawn
                lastPathEndZ + Random.Range(1f, pathLength - 1f) // Random Z position ahead
            );

            // Instantiate and add to activePaths
            GameObject crystal = Instantiate(blueCrystalsPrefab, crystalPosition, Quaternion.identity);
            activePaths.Add(crystal);

           // Debug.Log($"💎 Blue Crystal spawned at {crystalPosition}");
        }

        //  Ensure Gems spawn at Y = 2.5, with random X positions between -18 and +8
        if (gemPrefab != null)
        {
            for (int i = 0; i < gemsPerRow; i++)
            {
                float gemX = Random.Range(-5f, 5f); // Random X position
                float gemZ = lastPathEndZ + Random.Range(1f, pathLength - 1f); // Slightly ahead
                Vector3 gemPosition = new Vector3(gemX, 1.5f, gemZ);

                GameObject gem = Instantiate(gemPrefab, gemPosition, Quaternion.identity);
                activePaths.Add(gem);
            }
        }

        lastPathEndZ += pathLength;
    }
    void RepositionIfColliding(GameObject wall)
    {
        int maxAttempts = 2; // Prevent infinite loop
        int attempts = 0;
        bool repositioned = false;

        while (attempts < maxAttempts)
        {
            Collider[] colliders = Physics.OverlapSphere(wall.transform.position, 2f);
            repositioned = false;

            foreach (Collider col in colliders)
            {
                if (col.gameObject != wall && col.CompareTag("Wall"))
                {
                    //Debug.Log("⚠️ Wall Collision Detected! Repositioning...");

                    wall.transform.position += new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                    repositioned = true;
                    break;
                }
            }

            if (!repositioned) break;
            attempts++;
        }

        if (attempts == maxAttempts)
        {
           // Debug.LogWarning("🚨 Failed to reposition wall, destroying & respawning!");
            Destroy(wall);
        }
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
