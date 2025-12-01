using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class PointManagerScript : MonoBehaviour
{
    public int points = 0;
    public int damageUpgradeCost = 1000;
    public int reloadSpeedUpgreadeCost = 800;
    public int newGunCost = 5000;
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI damageUpgrade;
    public TextMeshProUGUI reloadUpgrade;
    public TextMeshProUGUI newGun;

    public Transform newGunLoc;
    public GameObject gunPrefab;

    private void Update()
    {
        pointText.text = "Points: " + points.ToString();
        damageUpgrade.text = "Upgrade Gun Damage: " + damageUpgradeCost.ToString() + " Points";
        reloadUpgrade.text = "Upgrade Reload Speed : " + reloadSpeedUpgreadeCost.ToString() + " Points";
        newGun.text = "Get another gun: " + newGunCost.ToString() + " Points";
    }

    public void spawnNewGun()
    {
        if (points >= newGunCost)
        {
            points -= newGunCost;
            Instantiate(gunPrefab, newGunLoc.position, newGunLoc.rotation);
        }

    }
    public void addPoints(int point)
    {
        points += point;
    }

    public void removePoints(int point)
    {
        points -= point;
    }
}
