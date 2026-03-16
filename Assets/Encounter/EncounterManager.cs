using System;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class EncounterManager : SimpleMonoBehaviorSingleton<EncounterManager>
{
    [SerializeField, ReadOnly] private MapEncounter currentMapEncounter;
    
    
    [SerializeField] private MapEncounter debugMapEncounter;
    [SerializeField] private CanvasGroup encounterScreenBackground;
    [SerializeField] private Canvas encounterUIRoot;
    private bool isFading;

    private void Start()
    {
        encounterScreenBackground.alpha = 0;
    }

    [Button]
    private void ToggleEncounterScreen()
    {
        switch (MapManager.Instance.CurrentMapState)
        {
            case MapState.Travel:
                StartEncounter();
                break;
            default:
            case MapState.Encounter:
                currentMapEncounter?.EndEncounter();
                break;
        }
    }
    
    public void StartEncounter(MapEncounter mapEncounter = null)
    {
        MapManager.Instance.CurrentMapState = MapState.Encounter;
        
        mapEncounter ??= debugMapEncounter; // if no encounter given, use debug
        
        currentMapEncounter = Instantiate(mapEncounter, encounterUIRoot.transform); // spawn panel
        currentMapEncounter.Close += EndMapEncounter; // listen to end event
        
        StartCoroutine(FadeEncounterBackground(0, 1, 0.5f));
    }
    
    public void EndMapEncounter()
    {
        currentMapEncounter.Close -= EndMapEncounter;
        StartCoroutine(EndEncounterCoroutine());
    }

    public IEnumerator EndEncounterCoroutine()
    {
        StartCoroutine(FadeEncounterBackground(1, 0, 0.5f));
        
        yield return new WaitWhile(IsClosing);
        
        MapManager.Instance.CurrentMapState = MapState.Travel;
        currentMapEncounter = null;
        
        yield break;
        
        bool IsClosing()
        {
            if (currentMapEncounter is not null)
                return isFading || currentMapEncounter.IsClosing;
            return isFading;
        }
    }
    
    
    private IEnumerator FadeEncounterBackground(float from, float to, float duration)
    {
        isFading = true;
        encounterScreenBackground.alpha = from;
        yield return encounterScreenBackground.DOFade(to, duration).WaitForCompletion();
        encounterScreenBackground.alpha = to;
        isFading = false;
    }
}
