using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyBlueprint
    {
        public string name;
        public int reward;
    }

    public GameObject enemyPrefab;
    public Transform spawnPoint;

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
        var finalWave = ordered.ToList();

        // Aggregate (actual: suma total de recompensas)
        int totalGold = finalWave.Aggregate(0, (acum, e) => acum + e.reward);
        Debug.Log($"Wave generada: {finalWave.Count} enemigos | Oro total esperado: {totalGold}");

        // Time slicing
        StartCoroutine(SpawnInBatches(finalWave, batchSize, perSpawnDelay));
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

                var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
                go.name = data.name;

                var be = go.GetComponent<BlockEnemy>();
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

    void OnDrawGizmosSelected()
    {
        if (!spawnPoint) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.25f);
    }
}
