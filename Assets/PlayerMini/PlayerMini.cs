using System;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class PlayerMini : SimpleMonoBehaviorSingleton<PlayerMini>
{
    private static bool s_isMoving;
    public static bool IsMoving
    {
        get => s_isMoving;
        private set
        {
            s_isMoving = value;
            MapManager.UpdateMapInteractability();
        } 
    }
    
    public static event Action PlayerMiniReachedCurrentNode;

    public void MoveToNode(Node newNode)
    {
        transform.rotation = Quaternion.LookRotation((newNode.transform.position - transform.position).normalized, Vector3.up);
        StartCoroutine(MovePlayer(newNode, () => SetMiniToNode(newNode) ));
    }

    private IEnumerator MovePlayer(Node node, Action callbackOnArrival)
    {
        IsMoving = true;
        yield return transform.DOMove(node.transform.position, 2).WaitForCompletion();
        IsMoving = false;
        callbackOnArrival?.Invoke();
    }

    public void SetMiniToNode(Node node)
    {
        transform.position = node.transform.position;
        PlayerMiniReachedCurrentNode?.Invoke();
    }
}
