using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class shootScript : MonoBehaviour
{
    public Transform ShootPoint;
    public float range = 100f;
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

            // Optional: apply damage or effects
            // var target = hit.transform.GetComponent<Target>();
            // if (target != null) target.TakeDamage(10);
        }
    }
}
