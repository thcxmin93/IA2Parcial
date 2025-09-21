using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyBlueprint
    {
        public string name;
        public int reward;
    }
    
    public List<GameObject> enemyPrefabs;
    public Transform spawnPoint;
    public List<EnemyBlueprint> enemyList;
    [Header("Wave Setup")]
    public int enemiesPerWave = 20;
    public int minReward = 5;
    public int maxReward = 21;
    public int minRewardFilter = 10;

    public int batchSize = 1;
    public float perSpawnDelay = 1f;

    [Header("Path")]
    public List<Transform> waypoints = new List<Transform>();
    public float enemySpeed = 3f;

    void Start()
    {
        // Generator
        var rawWave = Enumerable.Range(1, enemiesPerWave)
            .Select(i => new EnemyBlueprint
            {
                name = $"Enemy_{i}",
                reward = Random.Range(minReward, maxReward)
            })
            .ToList();

        // LINQ Where
        var filtered = rawWave.Where(e => e.reward >= minRewardFilter);

        // LINQ OrderByDescending
        var ordered = filtered.OrderByDescending(e => e.reward);

        // LINQ ToList
        enemyList = ordered.ToList();
        
        // Aggregate (actual: suma total de recompensas)
        int totalGold = enemyList.Aggregate(0, (acum, e) => acum + e.reward);
        Debug.Log($"Wave generada: {enemyList.Count} enemigos | Oro total esperado: {totalGold}");

        // Time slicing
        StartCoroutine(SpawnInBatches(enemyList, batchSize, perSpawnDelay));
    }

    IEnumerator SpawnInBatches(List<EnemyBlueprint> wave, int chunk, float delay)
    {
        int spawned = 0;
        while (spawned < wave.Count)
        {
            var slice = wave.Skip(spawned).Take(chunk).ToList();

            foreach (var data in slice)
            {
                Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;

                var go = Instantiate(GenerateRandomEnemy().First(), pos, Quaternion.identity);
                go.name = data.name;

                var be = go.GetComponent<Enemy>();
                if (be != null)
                    be.reward = data.reward;

                var follower = go.GetComponent<EnemyFollowPath>();
                if (follower == null)
                    follower = go.AddComponent<EnemyFollowPath>();

                follower.Init(waypoints, enemySpeed);

                Debug.Log($"SPAWN {go.name} → {data.reward} Gold");

                if (delay > 0f)
                    yield return new WaitForSeconds(delay);
                else
                    yield return null;
            }

            spawned += slice.Count;
        }
    }
    
    public IEnumerable<GameObject> GenerateRandomEnemy()
    {
        while (true)
        {
            int skipAmount = Random.Range(0, enemyPrefabs.Count());
            yield return enemyPrefabs.Skip(skipAmount).First();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!spawnPoint) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.25f);
    }
    
    //Gizmos del path
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;
    
        Gizmos.color = Color.cyan;
        foreach (var wp in waypoints)
        {
            if (wp != null)
                Gizmos.DrawSphere(wp.position, 0.2f);
        }
        
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }

}
