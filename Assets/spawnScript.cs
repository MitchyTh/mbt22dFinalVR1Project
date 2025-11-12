using UnityEngine;
using System.Collections;
using System.Threading;

public class spawnScript : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject spawnZone;
    public Transform endZone;
    public float spawnInterval = 3f;
    public int enemiesPerRound = 5;

    private int roundNum = 0;
    private int enemiesSpawned = 0;
    private int enemiesKilled = 0;

    private Collider[] spawnZones;
    private bool isSpawning = false;

    private float startGameTime = 20f;
    private float betweenRoundTime = 10f;
    private bool gameStarted = false;
    void Start()
    {
        // Automatically find all trigger or box colliders in children
        spawnZones = GetComponentsInChildren<Collider>();
    }
    private void Update()
    {
        startGameTime -= Time.deltaTime;

        if (startGameTime < 0 && !gameStarted)
        {
            StartCoroutine(SpawnRoutine());
            gameStarted = true;
            roundNum++;
        }

        if(enemiesKilled == enemiesSpawned && enemiesPerRound == enemiesSpawned)
        {
            StartCoroutine(BetweenRounds());
        }
    }
    private IEnumerator SpawnRoutine()
    {
        isSpawning = true;
        enemiesPerRound = (roundNum * 3) + 3; //Sets number of enemies per round
        enemiesSpawned = 0;
        enemiesKilled = 0;

        for (enemiesSpawned = 0; enemiesSpawned < enemiesPerRound; enemiesSpawned++)
        {
            SpawnEnemyAtRandomZone();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private IEnumerator BetweenRounds()
    {
        float timer = betweenRoundTime;

        // Wait for the countdown before starting next round
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null; // wait one frame
        }
        roundNum++;
        StartCoroutine(SpawnRoutine());
    }


    private void SpawnEnemyAtRandomZone()
    {
        if (spawnZones.Length == 0 || enemyPrefab == null)
            return;

        // Pick a random zone
        Collider zone = spawnZones[Random.Range(0, spawnZones.Length)];

        // Pick a random point inside it
        Vector3 randomPoint = GetRandomPointInsideCollider(zone);

        // Spawn enemy
        GameObject newEnemy = Instantiate(enemyPrefab, randomPoint, Quaternion.identity);
        var moveScript = newEnemy.GetComponent<enemyMoveScript>();

        moveScript.spawnZone = spawnZone;
        moveScript.endZone = endZone;
        moveScript.spawner = this;
    }

    private Vector3 GetRandomPointInsideCollider(Collider col)
    {
        if (col is BoxCollider box)
        {
            Vector3 localCenter = box.center;
            Vector3 localSize = box.size;
            Vector3 worldCenter = box.transform.TransformPoint(localCenter);

            // get half-extents in world space
            Vector3 halfSize = Vector3.Scale(localSize * 0.5f, box.transform.lossyScale);

            Vector3 randomOffset = new Vector3(
                Random.Range(-halfSize.x, halfSize.x),
                Random.Range(-halfSize.y, halfSize.y),
                Random.Range(-halfSize.z, halfSize.z)
            );

            return worldCenter + randomOffset;
        }
        else
        {
            // fallback for non-box colliders
            return col.bounds.center;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);

        foreach (var col in GetComponentsInChildren<BoxCollider>())
        {
            Matrix4x4 matrix = Matrix4x4.TRS(col.transform.position, col.transform.rotation, col.transform.lossyScale);
            Gizmos.matrix = matrix;
            Gizmos.DrawCube(col.center, col.size);
        }
    }

    public void enemyKilled()
    {
        enemiesKilled++;
    }
}
