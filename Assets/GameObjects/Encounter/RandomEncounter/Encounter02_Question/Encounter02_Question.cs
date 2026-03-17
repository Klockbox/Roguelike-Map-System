using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class Encounter02_Question : MapEncounter
{
    [SerializeField]
    private RectTransform exitButton;
    private readonly Vector2 exitButtonStartSize = new (20, 10);

    private int currentPanelIndex = 0;
    
    [SerializeField] 
    private GameObject part1;
    [SerializeField] 
    private GameObject part2;
    [SerializeField] 
    private GameObject part3;
    [SerializeField] 
    private GameObject part4;

    private void Start()
    {
        part1.gameObject.SetActive(false);
        part2.gameObject.SetActive(false);
        part3.gameObject.SetActive(false);
        part4.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        ResetExitButton();
        AdvanceStory();
    }

    [Button]
    private void EnlargeExitButton()
    {
        exitButton.sizeDelta *= 1.5f;
    }
    
    [Button]
    private void ResetExitButton()
    {
        exitButton.sizeDelta = exitButtonStartSize;
    }

    public void AdvanceStory()
    {
        switch (currentPanelIndex)
        {
            case 0:
                currentPanelIndex = 1;
                part1.gameObject.SetActive(true);
                return;
            case 1:
                part1.gameObject.SetActive(false);
                currentPanelIndex = 2;
                part2.gameObject.SetActive(true);
                exitButton.gameObject.SetActive(true);
                return;
            case 2:
                part2.gameObject.SetActive(false);
                currentPanelIndex = 3;
                part3.gameObject.SetActive(true);
                EnlargeExitButton();
                return;
            case 3:
                part3.gameObject.SetActive(false);
                currentPanelIndex = 2;
                part2.gameObject.SetActive(true);
                EnlargeExitButton();
                return;
        }
    }
    
    public void BreakOutOfStory()
    {
        exitButton.gameObject.SetActive(false);
        part1.gameObject.SetActive(false);
        part2.gameObject.SetActive(false);
        part3.gameObject.SetActive(false);
        part4.gameObject.SetActive(true);
    }

    public void EndStory()
    {
        EndEncounter();
    }
}
