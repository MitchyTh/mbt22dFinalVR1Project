using UnityEngine;
using System.Collections;
using TMPro;

public class SpawnScript : MonoBehaviour
{
    public GameObject enemyPrefab;           // Enemy prefab to spawn
    public GameObject spawnZone;             // General spawn zone reference (if needed elsewhere)
    public Transform endZone;                // Target zone for enemies to move towards
    public float spawnInterval = 3f;         // Time interval between enemy spawns
    public int enemiesPerRound = 5;          // Initial enemies per round

    private int roundNum = 0;                // Current round number
    private int enemiesSpawned = 0;          // Number of enemies spawned in the current round
    private int enemiesKilled = 0;           // Number of enemies killed in the current round

    private Collider[] spawnZones;           // Array to hold all spawn zone colliders
    private bool isSpawning = false;         // Whether spawning is in progress or not
    private bool inARound = false;

    private float startGameTime = 20f;       // Time before the game starts (countdown)
    private float betweenRoundTime = 10f;    // Time to wait between rounds
    private bool gameStarted = false;        // Whether the game has started or not

    public TextMeshProUGUI roundText;        // UI TextMeshProUGUI for round number
    public TextMeshProUGUI roundTimeText;    // UI TextMeshProUGUI for round time

    void Start()
    {
        // Automatically find all trigger or box colliders in children
        spawnZones = GetComponentsInChildren<Collider>();
    }

    void Update()
    {
        // Countdown before the game starts
        if (!gameStarted)
        {
            startGameTime -= Time.deltaTime;
            roundTimeText.text = Mathf.Ceil(startGameTime).ToString("0");

            // Start game when the countdown ends
            if (startGameTime <= 0)
            {
                StartGame();
            }
        }
        else
        {
            if (inARound)
            {
                roundTimeText.text = "0";
            }

            // Check for round completion (all enemies spawned and killed)
            if (enemiesSpawned == enemiesPerRound && enemiesKilled == enemiesSpawned)
                if (!isSpawning && !inARound) // Avoid triggering multiple rounds
                {
                    StartCoroutine(BetweenRounds());
                }
        }

        // Update round display
        roundText.text = "Round: " + roundNum;
    }

    // Starts the game, initializing round and spawning
    private void StartGame()
    {
        gameStarted = true;
        roundNum = 1;          // Set the round number

        StartCoroutine(SpawnRoutine());
    }

    // Coroutine to handle the spawning of enemies
    private IEnumerator SpawnRoutine()
    {
        enemiesSpawned = 0;     // Reset the enemies spawned count for this round
        enemiesPerRound = (roundNum * 3) + 3;
        enemiesKilled = 0;      // Reset the enemies killed count
        inARound = true;
        isSpawning = true;

        // Spawn enemies at the defined interval
        while (enemiesSpawned < enemiesPerRound)
        {
            SpawnEnemyAtRandomZone();
            enemiesSpawned++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
        inARound = false;
    }

    // Coroutine to handle the transition between rounds
    private IEnumerator BetweenRounds()
    {
        float timer = betweenRoundTime;

        // Wait for the countdown before starting next round
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            roundTimeText.text = Mathf.Ceil(timer).ToString("0");
            yield return null; // wait one frame
        }

        roundNum++;          // Increase the round number
        StartCoroutine(SpawnRoutine()); // Start the next round's enemy spawn routine
    }

    // Spawns an enemy at a random point inside one of the spawn zones
    private void SpawnEnemyAtRandomZone()
    {
        if (spawnZones.Length == 0 || enemyPrefab == null)
            return;

        // Pick a random spawn zone
        Collider zone = spawnZones[Random.Range(0, spawnZones.Length)];

        // Get a random point inside this zone
        Vector3 randomPoint = GetRandomPointInsideCollider(zone);

        // Instantiate the new enemy at the random point
        GameObject newEnemy = Instantiate(enemyPrefab, randomPoint, Quaternion.identity);

        // Get the enemy's movement script and set properties
        var moveScript = newEnemy.GetComponent<enemyMoveScript>();
        if (moveScript != null)
        {
            moveScript.spawnZone = spawnZone;
            moveScript.endZone = endZone;
            moveScript.spawner = this; // Pass this spawner to the enemy
        }
    }

    // Helper function to generate a random point inside a collider (box collider assumed here)
    private Vector3 GetRandomPointInsideCollider(Collider col)
    {
        if (col is BoxCollider box)
        {
            Vector3 localCenter = box.center;
            Vector3 localSize = box.size;
            Vector3 worldCenter = box.transform.TransformPoint(localCenter);

            // Get half-extents in world space
            Vector3 halfSize = Vector3.Scale(localSize * 0.5f, box.transform.lossyScale);

            // Random point within the box
            Vector3 randomOffset = new Vector3(
                Random.Range(-halfSize.x, halfSize.x),
                Random.Range(-halfSize.y, halfSize.y),
                Random.Range(-halfSize.z, halfSize.z)
            );

            return worldCenter + randomOffset;
        }
        else
        {
            // Fallback for non-box colliders (e.g., sphere or capsule)
            return col.bounds.center;
        }
    }

    // Optional: Draw spawn zones in the editor to visualize them
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f); // Light green color

        foreach (var col in GetComponentsInChildren<BoxCollider>())
        {
            Matrix4x4 matrix = Matrix4x4.TRS(col.transform.position, col.transform.rotation, col.transform.lossyScale);
            Gizmos.matrix = matrix;
            Gizmos.DrawCube(col.center, col.size); // Draw a box around the spawn zone
        }
    }

    // Method to be called when an enemy is killed
    public void EnemyKilled()
    {
        enemiesKilled++;
    }
}
