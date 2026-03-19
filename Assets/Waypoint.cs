using System;
using UnityEngine;

/// <summary>
/// <see cref="Waypoint"/> is a wrapper class to make handling the dijkstra calculations easier for my purposes of
/// marking and highlighting <see cref="Connector"/> and <see cref="Node"/> objects.
/// </summary>
[Serializable]
public class Waypoint
{
    public Node Node { get; private set; }
    public Node PreviousNode { get; private set; }
    public int LowestMoveCost { get; private set; }
    public Connector Connection { get; private set; }

    public void SetBetterPreviousNode( Node prevNode, int totalMoveCost )
    {
        PreviousNode = prevNode;
        LowestMoveCost = totalMoveCost;
        if (Connector.TryGetConnector(Node, prevNode, out Connector connector))
            Connection = connector;
        else
            Debug.LogWarning($"Tried to set new previous node ({prevNode.name}) on waypoint of node {Node.name} but was unable to find connector.");
    }

    public void OverrideMoveCost(int newCost) => LowestMoveCost = newCost;
        
    public Waypoint(Node node, int dist, Node prevNode)
    {
        Node = node;
        LowestMoveCost = dist;
        PreviousNode = prevNode;
    }
}