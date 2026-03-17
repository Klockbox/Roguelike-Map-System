using System;
using TMPro;
using UnityEngine;

public class CombatEncounter : MapEncounter
{
    [SerializeField] private int pointGain;
    [SerializeField] private TMP_Text pointGainText;
    [SerializeField] private int moneyGain;
    [SerializeField] private TMP_Text moneyGainText;
    
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

    private void Start()
    {
        pointGainText.text = pointGain.ToString();
        moneyGainText.text = moneyGain.ToString();
        page1.SetActive(true);
        page2.SetActive(false);
    }

    public void GoToPageTwo()
    {
        page1.SetActive(false);
        page2.SetActive(true);
    }

    public void ConcludeCombat()
    {
        // gain points
        MapManager.Instance.PointScore += pointGain;
        
        // gain money
        MapManager.Instance.Money += moneyGain;
        
        EndEncounter();
    }
}
