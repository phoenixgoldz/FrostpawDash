using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PathManager : MonoBehaviour
{
    [Header("Path Settings")]
    public GameObject[] pathPrefabs;
    public GameObject flatIcePathPrefab;
    public GameObject[] leftWallPrefabs;
    public GameObject[] rightWallPrefabs;
    public GameObject[] obstaclePrefabs;

    public int initialSegments = 5;
    public float pathLength = 18f;
    public float wallXOffset = 12f;

    public GameObject blueCrystalsPrefab;
    public GameObject crystalWallCavernPrefab;

    private bool canSpawnBlueCrystals = false;
    private Transform player;
    private List<GameObject> activePaths = new List<GameObject>();
    private float lastPathEndZ = 0f;
    private int pathsSpawned = 10;

    [Header("Collectibles")]
    public GameObject gemPrefab;
    public int gemsPerRow = 10;

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

        StartCoroutine(EnableBlueCrystalsSpawn());
    }

    IEnumerator EnableBlueCrystalsSpawn()
    {
        yield return new WaitForSeconds(15f);
        canSpawnBlueCrystals = true;
    }

    void Update()
    {
        if (player == null) return;

        float speedFactor = Mathf.Clamp(player.GetComponent<PlayerController>().GetSpeed() / 10f, 1f, 3f); // Adjust spawn rate dynamically

        if (player.position.z + pathLength > lastPathEndZ)
        {
            SpawnPath();
            DeleteOldPath();
        }
    }

    public void SpawnPath()
    {
        GameObject newPath;

        if (pathsSpawned < 15)
        {
            newPath = Instantiate(flatIcePathPrefab, new Vector3(0, 0.05f, lastPathEndZ), Quaternion.identity);
        }
        else
        {
            int floorIndex = Random.Range(0, pathPrefabs.Length);
            newPath = Instantiate(pathPrefabs[floorIndex], new Vector3(0, 0.05f, lastPathEndZ), Quaternion.identity);
        }

        newPath.transform.position = new Vector3(0, 0.05f, lastPathEndZ); // Ensure it's above 0
        newPath.transform.rotation = Quaternion.identity;

        newPath.tag = "PathTrigger";
        activePaths.Add(newPath);
        pathsSpawned++;

        if (newPath.name.Contains("Crystal Caverns Floor"))
        {
            newPath.transform.position = new Vector3(0, 0.83f, lastPathEndZ);
            newPath.transform.rotation = Quaternion.LookRotation(Vector3.right);
        }

        // Ensure left wall always spawns
        GameObject leftWall = (leftWallPrefabs.Length > 0)
            ? Instantiate(leftWallPrefabs[Random.Range(0, leftWallPrefabs.Length)], new Vector3(-wallXOffset, 7.64f, lastPathEndZ), Quaternion.Euler(0, 90, 0))
            : Instantiate(crystalWallCavernPrefab, new Vector3(-17f, 7.64f, lastPathEndZ), Quaternion.Euler(0, 90, 0));

        if (leftWall.name.Contains("CavernWall"))
        {
            leftWall.transform.position = new Vector3(leftWall.transform.position.x, 5f, leftWall.transform.position.z);
        }

        activePaths.Add(leftWall);

        // Ensure right wall always spawns
        GameObject rightWall = (rightWallPrefabs.Length > 0)
            ? Instantiate(rightWallPrefabs[Random.Range(0, rightWallPrefabs.Length)], new Vector3(wallXOffset, 7.64f, lastPathEndZ), Quaternion.Euler(0, 90, 0))
            : Instantiate(crystalWallCavernPrefab, new Vector3(18.7f, 7.64f, lastPathEndZ), Quaternion.Euler(0, -90, 0));

        if (rightWall.name.Contains("CavernWall"))
        {
            rightWall.transform.position = new Vector3(rightWall.transform.position.x, 5f, rightWall.transform.position.z);
        }

        activePaths.Add(rightWall);

        // Random obstacle spawning (40% chance)
        if (obstaclePrefabs.Length > 0 && Random.Range(0, 100) < 40)
        {
            int obstacleIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject obstacle = Instantiate(obstaclePrefabs[obstacleIndex], new Vector3(Random.Range(-5f, 5f), 1, lastPathEndZ + Random.Range(1f, pathLength - 1f)), Quaternion.identity);

            // Ensure Ice Archway is placed correctly at Y = 4.4
            if (obstacle.name.Contains("Ice Archway"))
            {
                obstacle.transform.position = new Vector3(0, 4.4f, obstacle.transform.position.z);
            }

            activePaths.Add(obstacle);
        }

        if (canSpawnBlueCrystals && blueCrystalsPrefab != null)
        {
            Vector3 crystalPosition = new Vector3(Random.Range(-5f, 5f), 3.0f, lastPathEndZ + Random.Range(1f, pathLength - 1f));
            GameObject crystal = Instantiate(blueCrystalsPrefab, crystalPosition, Quaternion.identity);
            activePaths.Add(crystal);
        }

        if (gemPrefab != null)
        {
            for (int i = 0; i < gemsPerRow; i++)
            {
                float gemX = Random.Range(-3f, 3f);
                float gemZ = lastPathEndZ + Random.Range(1f, pathLength - 1f);
                Vector3 gemPosition = new Vector3(gemX, 2f, gemZ);

                GameObject gem = Instantiate(gemPrefab, gemPosition, Quaternion.identity);
                activePaths.Add(gem);
            }
        }
        // ✅ Ensure IcyFloorPath maintains exactly 18.59f Z-spacing
        if (newPath.name.Contains("FlatIcePath"))
        {
            lastPathEndZ += 20f; // Set fixed spacing for ice paths
        }
        else
        {
            lastPathEndZ += pathLength; // Use default spacing for other paths
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
