using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    public List<Tilemap> tilemapshasCollider;
    public GameObject vertices;
    private Dictionary<Vector2Int, bool> tilepositions;
    public List<GameObject> verticesList;
    public GameObject player;
    public Vector3Int cellBefore = new Vector3Int(-1000, -1000, 0); // Khởi tạo với giá trị không hợp lệ
    public Vector3 recentCellPos = new Vector3(-1000, -1000, 0); // Khởi tạo với giá trị không hợp lệ
    public MapUI mapUI;
    public Astar astar;
    //Constructor
    public Map(GameObject vertices)
    {
        this.vertices = vertices;
        tilepositions = new Dictionary<Vector2Int, bool>();
    }
    //Get and set
    public GameObject Vertices
    {
        get { return vertices; }
        set { vertices = value; }
    }
    public Dictionary<Vector2Int, bool> TilePositions
    {
        get { return tilepositions; }
        set { tilepositions = value; }
    }
    //to string
    public override string ToString()
    {
        return "Map with vertices: " + vertices.name + " and tile positions count: " + tilepositions.Count;
    }
    //Other Methods
    public void onAwake()
    {
        astar.startNode = new Node(player.transform.position - new Vector3(0.5f, 0.5f, 0)); // Example start node
        astar.goalNode = new Node(recentCellPos);
        astar.calcHeuristic(astar.startNode, astar.goalNode);
        astar.startNode.totalCost = astar.startNode.distance + astar.startNode.heuristic; // Initialize total cost
    }
    public void getListVertices()
    {
        verticesList = new List<GameObject>();
        for (int i = 0; i < 3; i++)
        {
            verticesList.Add(vertices.transform.GetChild(i).gameObject);
            // Debug.Log("Vertex " + i + ": " + verticesList[i].transform.position);
        }
    }
    public bool checkTileAtPosition(Vector3Int position)
    {
        foreach (Tilemap tilemap in tilemapshasCollider)
        {
            if (tilemap.HasTile(position))
            {
                return true;
            }
        }
        return false;
    }
    public void genMap()
    {
        tilepositions = new Dictionary<Vector2Int, bool>();
        for (int y = (int)verticesList[0].transform.position.y; y <= (int)verticesList[2].transform.position.y; y++)
        {
            for (int x = (int)verticesList[0].transform.position.x; x <= (int)verticesList[1].transform.position.x; x++)
            {
                tilepositions.Add(new Vector2Int(x, y), checkTileAtPosition(new Vector3Int(x, y, 0)));
            }
        }
    }
    public void printTilePositions()
    {
        foreach (var position in tilepositions)
        {
            Debug.Log("Position: " + position.Key + ", Has Tile: " + position.Value);
        }
    }
    public void onUpdate()
    {
        Vector3 mousePos = Input.mousePosition;

        Vector3 mouseWorldPos = mapUI.mainCamera.ScreenToWorldPoint(mousePos);
        Vector3Int cellPos = gameObject.GetComponentInChildren<Tilemap>().WorldToCell(mouseWorldPos);
        // Debug.Log("Mouse Screen Pos: " + cellPos);
        mouseWorldPos.z = 0;
        Vector2Int key = new Vector2Int(cellPos.x, cellPos.y);


        tilepositions.TryGetValue(key, out bool hasTile);
        if (cellPos != cellBefore && !hasTile)
        {
            // Debug.Log("Mouse World Pos: " + mouseWorldPos);
            cellBefore = cellPos;
            mapUI.deleteCircle("dot");
            mapUI.drawCircle(changeCellPos(cellPos));

            recentCellPos = cellPos;
            astar.goalNode.position = recentCellPos;
            astar.FindPath(astar.startNode, astar.goalNode);
        }
        else if (cellPos != cellBefore && hasTile)
        {
            mapUI.deleteCircle("dot");
        }
    }
    public Vector3 nextCellPos = new Vector3(-1000, -1000, 0); // Khởi tạo với giá trị không hợp lệ
    public void onClick()
    {
        mapUI.deleteCircle("path");
        Vector3 mousePos = Input.mousePosition;
        Vector3 mouseWorldPos = mapUI.mainCamera.ScreenToWorldPoint(mousePos);
        Vector3Int cellPos = gameObject.GetComponentInChildren<Tilemap>().WorldToCell(mouseWorldPos);
        // Debug.Log("Mouse Screen Pos: " + cellPos);
        mouseWorldPos.z = 0;


        Vector2Int key = new Vector2Int(cellPos.x, cellPos.y);
        tilepositions.TryGetValue(key, out bool hasTile);
        if (!hasTile)
        {
            nextCellPos = cellPos;
        }
    }
    public Vector3 getPosition()
    {
        return changeCellPos(recentCellPos);
    }
    public Vector3 changeCellPos(Vector3 cellPos)
    {
        Vector3 newCellPos = new Vector3(cellPos.x + 0.5f, cellPos.y + 0.5f, 0);
        return newCellPos;
    }
}
