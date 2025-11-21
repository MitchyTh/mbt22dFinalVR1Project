using NUnit.Framework.Constraints;
using UnityEngine;
using System.Collections;

public class enemyMoveScript : MonoBehaviour
{
    public Transform endZone;
    public Transform playerTransform;
    public GameObject spawnZone;
    public Animator enemyAnimator;

    private bool isDead = false;
    private Rigidbody rb;
    private GameObject playerObject;
    public float maxMoveSpeed = 3f;
    public float moveSpeed = 2f;
    public bool takingDamage = false;
    private float stunTime = 1f;
    public float stunTimer = 0f;
    private float distanceToEndzone;
    private float distanceToPlayer;
    private float chaseDistance = 12f;

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

        playerObject = GameObject.FindGameObjectWithTag("Player");

        health = spawner.maxEnemyHealth;
        moveSpeed = spawner.maxEnemyMovementSpeed;
    }

    void Update()
    {
        playerTransform = playerObject.transform;
        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        distanceToEndzone = Vector3.Distance(transform.position, endZone.transform.position);

        if (isDead || endZone == null)
            return;

        stunTimer -= Time.deltaTime;

        takingDamage = stunTimer > 0;

        if (!takingDamage)
        {
            if (distanceToPlayer > chaseDistance)
            {
                MoveTowardsEndZone();
                // Resume walking animation
                enemyAnimator.SetFloat("MoveSpeed", moveSpeed);
            }
            else if (distanceToPlayer <= chaseDistance)
            {
                MoveTowardsPlayer();
                enemyAnimator.SetFloat("MoveSpeed", moveSpeed);
            }
            else
            {
                // Stop movement animation while taking damage
                enemyAnimator.SetFloat("MoveSpeed", 0f);
            }
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

    private void MoveTowardsPlayer()
    {
        // Calculate direction to target
        Vector3 direction = (playerTransform.position - transform.position).normalized;

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
        // Trigger death animation immediately
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("IsDead");
            // Force Animator to play the death animation instantly
            enemyAnimator.Update(0f);
        }

        // Optional: disable physics/colliders
        if (rb != null) rb.isKinematic = true;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        enemyAnimator.Play("root|death", 0, 0f);
        StartCoroutine(DestroyAfterDeath(1f));
    }

    public void takeDamage(int damage)
    {
        health = health - damage;
        enemyAnimator.SetTrigger("TakeDamage");

        stunTimer = stunTime;

        if (health < 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            enemyAnimator.SetTrigger("HitPlayer");
            StartCoroutine(EndGameAfterDelay(2f));
        }
    }


    private IEnumerator EndGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // wait 2 seconds
        spawner.endGame(); // now call endGame after the wait
    }

    private IEnumerator DestroyAfterDeath(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Notify spawner that enemy is dead
        if (spawner != null)
            spawner.EnemyKilled();

        // Remove the object
        Destroy(gameObject); // or gameObject.SetActive(false);
    }
}
