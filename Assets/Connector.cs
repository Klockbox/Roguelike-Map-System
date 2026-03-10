using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Connector : MonoBehaviour
{
    #region static class behavior

    public static List<Connector> s_Connectors = new List<Connector>();
    public static List<Connector> GetAllConnectionsFrom(Node node) => s_Connectors.Where(connector => connector.Connects(node)).ToList();
    
    #endregion

    private void OnEnable()
    {
        s_Connectors.Add(this);
    }
    private void OnDisable() { s_Connectors.Remove(this); }
    
    public Node Node1, Node2;
    public int MoveCost;
    private bool IsValid => Node1 && Node2 && Node1 != Node2;
    
    public bool Connects(Node node) => Node1 == node || Node2 == node; // does this node connect the given node to anything?

    public Node GetOther(Node node)
    {
        if (Node1 == node || Node2 == node)
            return node == Node1 ? Node2 : Node1; // we know its one of the two, so if its 1 return 2, or if it is not node 1 return 1.
        
        Debug.Log($"{name} does not connect {node.name}");
        return null;
    }
    
    private void OnValidate()
    {
        if (!IsValid)
        {
            name = $"invalid connector";
            return;
        }
        
        transform.position = Node1.transform.position + (Node2.transform.position - Node1.transform.position) / 2;
        MoveCost = Random.Range(1, 7);
        
        name = $"Connector: {Node1.name} - {Node2.name} @ {MoveCost}";
    }

    private void OnDrawGizmos()
    {
        if(!IsValid) return;
        Gizmos.DrawLine(Node1.transform.position, Node2.transform.position);
        Handles.Label(transform.position, MoveCost.ToString());
    }
}

public enum ENUMSDOÖASHDLOÖA : byte
{
    aldiohas,
    asdhasd
}
