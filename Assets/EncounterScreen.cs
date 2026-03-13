using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EncounterScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup background;
    
    
    private IEnumerator FadeIn()
    {
        background.alpha = 0;
        yield return background.DOFade(1, 0.5f).WaitForCompletion();
        background.alpha = 1;
    }
}
