using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class PointManagerScript : MonoBehaviour
{
    public int points = 0;
    public int damageUpgradeCost = 1000;
    public int reloadSpeedUpgreadeCost = 800;
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI damageUpgrade;
    public TextMeshProUGUI reloadUpgrade;

    private void Update()
    {
        pointText.text = "Points: " + points.ToString();
        damageUpgrade.text = "Upgrade Gun Damage: " + damageUpgradeCost.ToString() + " Points";
        reloadUpgrade.text = "Upgrade Reload Speed : " + reloadSpeedUpgreadeCost.ToString() + " Points";
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
