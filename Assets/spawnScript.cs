using UnityEngine;
using System.Collections;
using TMPro;

public class SpawnScript : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject spawnZone;
    public Transform endZone;
    public float spawnInterval = 3f;
    public int enemiesPerRound = 5;

    private int roundNum = 0;
    public int enemiesSpawned = 0;
    public int enemiesKilled = 0;

    private Collider[] spawnZones;
    private bool isSpawning = false;
    private bool inARound = false;

    private float startGameTime = 20f;
    private float betweenRoundTime = 20f;
    private bool gameStarted = false;

    public TextMeshProUGUI roundText;
    public TextMeshProUGUI roundTimeText;
    public TextMeshProUGUI highScoreText;

    private bool betweenRoundsStarted;
    private bool firstRoundStarted;

    public PointManagerScript points;
    public int lastRound;
    public int highestRound = 0;
    public int maxEnemyHealth = 100;
    public float maxEnemyMovementSpeed = 2f;

    void Start()
    {
        spawnZones = GetComponentsInChildren<Collider>();
        highScoreText.text = "High Score: Round 0";
    }

    void Update()
    {
        // Countdown before first round
        if (gameStarted && !firstRoundStarted)
        {
            startGameTime -= Time.deltaTime;
            roundTimeText.text = "Round Starts In: " + Mathf.Ceil(startGameTime).ToString("0");

            if (startGameTime <= 0f && !firstRoundStarted)
            {
                firstRoundStarted = true;
                roundNum = 1;
                StartCoroutine(SpawnRoutine());
            }
        }
        else
        {
            if (inARound)
                roundTimeText.text = "Round Has Started";

            // Check for round completion
            if (inARound && enemiesSpawned == enemiesPerRound && enemiesKilled == enemiesSpawned)
            {
                if (!betweenRoundsStarted)
                {
                    betweenRoundsStarted = true;
                    inARound = false;
                    StartCoroutine(BetweenRounds());
                }
            }
        }

        // Update highest round
        if (roundNum > highestRound)
        {
            highestRound = roundNum;
            highScoreText.text = "Highest Round: Round " + highestRound.ToString();
        }

        roundText.text = "Round: " + roundNum;
    }

    // Start the game
    public void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            startGameTime = 5f; // optional quick countdown
        }
    }

    // End the game
    public void endGame()
    {
        StopAllCoroutines();  
        gameStarted = false;
        lastRound = roundNum;
        roundNum = 0;
        enemiesKilled = 0;
        enemiesSpawned = 0;
        enemiesPerRound = 5;
        inARound = false;
        firstRoundStarted = false;
        betweenRoundsStarted = false;
        points.removePoints(points.points);
        startGameTime = 20;

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject obj in objects)
        {
            enemyMoveScript script = obj.GetComponent<enemyMoveScript>();
            if (script != null)
                script.Die();
        }

        roundText.text = "Round: " + roundNum;
        roundTimeText.text = "Game Over: You made it to round " + lastRound.ToString();
    }

    // Coroutine for spawning enemies
    private IEnumerator SpawnRoutine()
    {
        enemiesSpawned = 0;
        enemiesKilled = 0;
        inARound = true;
        isSpawning = true;

        // Calculate enemies for this round: Round 1 = 5, Round 2 = 7, Round 3 = 9, etc.
        enemiesPerRound = 5 + (roundNum - 1) * 2;

        while (enemiesSpawned < enemiesPerRound)
        {
            SpawnEnemyAtRandomZone();
            enemiesSpawned++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    // Coroutine for between rounds countdown
    private IEnumerator BetweenRounds()
    {
        float timer = betweenRoundTime;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            roundTimeText.text = "Round Starts In: " + Mathf.Ceil(timer).ToString("0");
            yield return null;
        }

        roundNum++;

        // Buff enemies and spawn speed at certain rounds

        increaseEnemySpawnSpeed();

        if (roundNum % 2 == 0)
        {
            increaseEnemyMovementSpeed();
        }

        if (roundNum % 5 == 0)
            increaseEnemyHealth();

        betweenRoundsStarted = false;
        StartCoroutine(SpawnRoutine());
    }

    // Spawn a single enemy at a random spawn zone
    private void SpawnEnemyAtRandomZone()
    {
        if (spawnZones.Length == 0 || enemyPrefab == null)
            return;

        Collider zone = spawnZones[Random.Range(0, spawnZones.Length)];
        Vector3 randomPoint = GetRandomPointInsideCollider(zone);

        GameObject newEnemy = Instantiate(enemyPrefab, randomPoint, Quaternion.identity);

        var moveScript = newEnemy.GetComponent<enemyMoveScript>();
        if (moveScript != null)
        {
            moveScript.spawnZone = spawnZone;
            moveScript.endZone = endZone;
            moveScript.spawner = this;
            moveScript.maxHealth = maxEnemyHealth;
            moveScript.health = maxEnemyHealth;
            moveScript.moveSpeed = maxEnemyMovementSpeed;
        }
    }

    // Get a random point inside a box collider
    private Vector3 GetRandomPointInsideCollider(Collider col)
    {
        if (col is BoxCollider box)
        {
            Vector3 localCenter = box.center;
            Vector3 localSize = box.size;
            Vector3 worldCenter = box.transform.TransformPoint(localCenter);
            Vector3 halfSize = Vector3.Scale(localSize * 0.5f, box.transform.lossyScale);

            return worldCenter + new Vector3(
                Random.Range(-halfSize.x, halfSize.x),
                Random.Range(-halfSize.y, halfSize.y),
                Random.Range(-halfSize.z, halfSize.z)
            );
        }
        return col.bounds.center;
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

    // Called when an enemy dies
    public void EnemyKilled()
    {
        enemiesKilled++;
        points.addPoints(100); // only once per kill
    }

    public void increaseEnemySpawnSpeed()
    {
        if (spawnInterval > 0.2f)
            spawnInterval -= 0.3f;
    }

    public void increaseEnemyMovementSpeed()
    {
        if (maxEnemyMovementSpeed < 5f)
            maxEnemyMovementSpeed += 0.3f;
    }

    public void increaseEnemyHealth()
    {
        if (maxEnemyHealth < 300)
            maxEnemyHealth += 50;
    }
}
