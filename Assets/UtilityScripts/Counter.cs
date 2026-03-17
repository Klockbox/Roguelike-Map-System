using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Counter : MonoBehaviour
{
    private protected TMP_Text TextElement;
    private protected int CurrentValue;
    
    private void Awake()
    {
        TextElement = GetComponent<TMP_Text>();
        Subscribe();
    }
    
    private protected void ChangeCounter(int newValue)
    {
        CurrentValue = newValue;
        TextElement.text = CurrentValue.ToString();
    }

    private protected virtual void Subscribe() { }
    private protected virtual void Unsubscribe() { }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}