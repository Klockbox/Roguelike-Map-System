using UnityEngine;

public abstract class SimpleMonoBehaviorSingleton<T> : MonoBehaviour where T : SimpleMonoBehaviorSingleton<T>
{
    public static T Instance { get; private set; }
    
    /// <summary>
    /// The Simple MonoBehavior Singleton sets itself in the base.Awake function.
    /// Further Awake functionality should be appended.
    /// </summary>
    protected virtual void Awake()
    {
        if (!Instance)
            Instance = this as T;
        else if (Instance != this)
        {
            Debug.LogWarning($"Tried to create Instance of {typeof(T)}, but there already was another instance set. Self destruct.");
            Destroy(this.gameObject);
        }
    }
}