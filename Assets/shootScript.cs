using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class shootScript : MonoBehaviour
{
    public Transform ShootPoint;
    public float range = 100f;
    public int gunDamage = 50;
    private int upgradedGunDamage;
    public float reloadTime = 1.2f;
    private float upgradedReloadTime;
    private float reloadTimer = 0;
    public ParticleSystem muzzleFlash;
    public AudioSource gunShotSound;
    public PointManagerScript points;

    private XRGrabInteractable grabInteractable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(OnTriggerPulled);
        upgradedGunDamage = gunDamage;
        upgradedReloadTime = reloadTime;
    }

    private void Update()
    {
        reloadTimer -= Time.deltaTime;
    }

    private void OnTriggerPulled(ActivateEventArgs args)
    {
        if (reloadTimer <= 0)
        {
            Shoot();
        }
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
                enemy.takeDamage(upgradedGunDamage);
            }
        }

        reloadTimer = upgradedReloadTime;
    }

    public void upgradeReloadTime()
    {
        if (points.points >= points.reloadSpeedUpgreadeCost)
        {
            points.removePoints(points.reloadSpeedUpgreadeCost);
            reloadTime -= 0.2f;
            points.reloadSpeedUpgreadeCost += 800;
        }
    }

    public void upgradeDamage()
    {
        if (points.points >= points.damageUpgradeCost)
        {
            points.removePoints(points.damageUpgradeCost);
            gunDamage += 30;
            points.damageUpgradeCost += 1000;
        }
    }
}
