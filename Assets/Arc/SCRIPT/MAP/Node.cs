using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public float distance { get; set; }
    public float heuristic { get; set; }
    public float totalCost { get; set; }
    public Vector3 position { get; set; }
    public Dictionary<Node, float> neighbors = new Dictionary<Node, float>();
    public Node parent { get; set; }
    public Node(Vector3 pos)
    {
        position = pos;
        distance = 0f;
        heuristic = 0f;
        totalCost = 0f;
        parent = null;
    }
    public void AddNeighbor(Node neighbor, float cost)
    {
        if (!neighbors.ContainsKey(neighbor))
        {
            neighbors.Add(neighbor, cost);
        }
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        Node other = (Node)obj;
        return position == other.position;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }
}
