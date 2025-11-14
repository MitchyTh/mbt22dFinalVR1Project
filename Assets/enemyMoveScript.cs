using UnityEngine;

public class enemyMoveScript : MonoBehaviour
{
    public Transform endZone;
    public GameObject spawnZone;
    public Animator enemyAnimator;

    private bool isDead = false;
    private Rigidbody rb;
    public float moveSpeed = 3f;

    public int health = 100;

    public SpawnScript spawner;

    void Start()
    {
        // Get Rigidbody if it exists
        rb = GetComponent<Rigidbody>();

        // Get Animator (search children in case model is nested)
        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (isDead || endZone == null)
            return;

        MoveTowardsEndZone();

        // Update animation speed
        float currentSpeed = rb != null ? rb.linearVelocity.magnitude : moveSpeed;
        enemyAnimator.SetFloat("Speed", currentSpeed);
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
        if (health < 0)
        {
            Die();
            spawner.EnemyKilled();
        }
    }
}
