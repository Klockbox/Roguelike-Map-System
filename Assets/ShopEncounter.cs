using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopEncounter : MapEncounter
{
    [SerializeField]
    private Button exitButton;
    
    [SerializeField]
    private Button buyPointsButton;
    
    [SerializeField]
    private Button buyFuelButton;

    private protected override void Awake()
    {
        base.Awake();
        MapManager.MoneyChanged += UpdateBuyOptions;
    }

    private void Start()
    {
        UpdateBuyOptions();
    }

    private void OnDestroy()
    {
        MapManager.MoneyChanged -= UpdateBuyOptions;
    }

    private void UpdateBuyOptions(int newMoney = 0)
    {
        buyPointsButton.interactable = MapManager.Instance.Money >= 5;
        buyFuelButton.interactable = MapManager.Instance.Money >= 5;
    }

    public void BuyPoints()
    {
        MapManager.Instance.Money -= 5;
        MapManager.Instance.PointScore += 5;
    }
    
    public void BuyFuel()
    {
        MapManager.Instance.Money -= 5;
        MapManager.CurrentFuel++;
    }
}
