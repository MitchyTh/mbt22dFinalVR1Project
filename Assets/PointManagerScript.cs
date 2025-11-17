using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class PointManagerScript : MonoBehaviour
{
    public int points = 0;
    public TextMeshProUGUI pointText;

    private void Update()
    {
        pointText.text = "Points: " + points.ToString();
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
