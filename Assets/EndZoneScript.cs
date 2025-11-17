using UnityEngine;

public class EndZoneScript : MonoBehaviour
{
    public SpawnScript manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            manager.endGame();
        }
    }
}
