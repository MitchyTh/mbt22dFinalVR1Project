using UnityEngine;

public class enemyMoveScript : MonoBehaviour
{
    public Transform endZone;
    public GameObject spawnZone;
    public Animator enemyAnimator;

    private bool isDead = false;
    private Rigidbody rb;
    public float maxMoveSpeed = 3f;
    public float moveSpeed = 2f;
    public bool takingDamage = false;
    private float stunTime = 1f;
    public float stunTimer = 0f;

    public int maxHealth = 100;
    public int health = 100;

    public SpawnScript spawner;

    void Start()
    {
        // Get Rigidbody if it exists
        rb = GetComponent<Rigidbody>();

        // Get Animator (search children in case model is nested)
        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<Animator>();

        health = spawner.maxEnemyHealth;
        moveSpeed = spawner.maxEnemyMovementSpeed;
    }

    void Update()
    {
        if (isDead || endZone == null)
            return;

        stunTimer -= Time.deltaTime;

        takingDamage = stunTimer > 0;

        if (!takingDamage)
        {
            MoveTowardsEndZone();
            // Resume walking animation
            enemyAnimator.SetFloat("MoveSpeed", moveSpeed);
        }
        else
        {
            // Stop movement animation while taking damage
            enemyAnimator.SetFloat("MoveSpeed", 0f);
        }
    }

    private void MoveTowardsEndZone()
    {
        // Calculate direction to target
        Vector3 direction = (endZone.position - transform.position).normalized;

        // Move using Transform if no Rigidbody
        if (rb == null)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Move with Rigidbody for physics
            rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
        }

        // Rotate to face the target
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (enemyAnimator != null)
            enemyAnimator.SetBool("IsDead", true);

        // Optional: disable physics/colliders
        if (rb != null) rb.isKinematic = true;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        gameObject.SetActive(false);
    }

    public void takeDamage(int damage)
    {
        health = health - damage;
        enemyAnimator.SetTrigger("TakeDamage");

        stunTimer = stunTime;

        if (health < 0)
        {
            Die();
            spawner.EnemyKilled();
        }
    }
}
