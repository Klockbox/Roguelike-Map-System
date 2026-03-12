using System;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class PlayerMini : SimpleMonoBehaviorSingleton<PlayerMini>
{
    public static bool IsMoving { get; private set; }

    public static event Action PlayerStartedMove; 
    public static event Action<Node> PlayerReachedNode; 
    
    public void MoveToNode(Node newNode)
    {
        IsMoving = true;
        PlayerStartedMove?.Invoke();
        StartCoroutine(MovePlayer(newNode));
        transform.rotation = Quaternion.LookRotation((newNode.transform.position - transform.position).normalized, Vector3.up);
    }

    private IEnumerator MovePlayer(Node node)
    {
        yield return transform.DOMove(node.transform.position, 2).WaitForCompletion();
        IsMoving = false;
        SetToNode(node);
    }

    public void SetToNode(Node node)
    {
        transform.position = node.transform.position;
        PlayerReachedNode?.Invoke(node);
    }
}
