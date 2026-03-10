using System;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class PlayerMini : SimpleMonoBehaviorSingleton<PlayerMini>
{
    public Node currentNode;
    
    [field: SerializeField, ReadOnly]
    public bool IsMoving { get; private set; }

    public static event Action PlayerStartedMove; 
    public static event Action PlayerEndedMove; 
    
    private void OnValidate()
    {
        SetToCurrentNode();
    }

    private void Start()
    {
        SetToCurrentNode();
        PlayerEndedMove?.Invoke();
    }
    
    
    public void MoveToNode(Node newNode)
    {
        IsMoving = true;
        PlayerStartedMove?.Invoke();
        
        currentNode = newNode;
        StartCoroutine(MovePlayer(newNode.transform.position, () => PlayerEndedMove?.Invoke()));
    }

    private IEnumerator MovePlayer(Vector3 targetPos, Action callback)
    {
        yield return transform.DOMove(targetPos, 2).WaitForCompletion();
        callback.Invoke();
    }

    private void SetToCurrentNode()
    {
        if(!currentNode) return;
        transform.position = currentNode.transform.position;
    }
}
