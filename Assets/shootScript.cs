using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class shootScript : MonoBehaviour
{
    public Transform ShootPoint;
    public float range = 100f;
    public int gunDamage = 50;
    public ParticleSystem muzzleFlash;
    public AudioSource gunShotSound;

    private XRGrabInteractable grabInteractable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(OnTriggerPulled);
    }

    private void OnTriggerPulled(ActivateEventArgs args)
    {
        Shoot();
    }

    void Shoot()
    {
        if (muzzleFlash) muzzleFlash.Play();
        if (gunShotSound) gunShotSound.Play();

        RaycastHit hit;
        if (Physics.Raycast(ShootPoint.position, ShootPoint.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            if (hit.transform.CompareTag("Enemy"))
            {
                enemyMoveScript enemy = hit.transform.GetComponent<enemyMoveScript>();
                enemy.takeDamage(gunDamage);
            }
        }
    }
}
