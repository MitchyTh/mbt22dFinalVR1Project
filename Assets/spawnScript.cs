using UnityEngine;
using System.Collections;

public class spawnScript : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject spawnZone;
    public Transform endZone;
    public float spawnInterval = 3f;
    public int enemiesPerRound = 5;

    private Collider[] spawnZones;
    private bool isSpawning = false;

    void Start()
    {
        // Automatically find all trigger or box colliders in children
        spawnZones = GetComponentsInChildren<Collider>();

        if (spawnZones.Length == 0)
        {
            Debug.LogWarning($"{name} has no child colliders — add Box Colliders as spawn zones!");
        }
        else
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        isSpawning = true;

        for (int i = 0; i < enemiesPerRound; i++)
        {
            SpawnEnemyAtRandomZone();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
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
}
