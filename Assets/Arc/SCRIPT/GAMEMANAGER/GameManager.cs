using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Map map;
    public MapUI mapUI;
    public Astar astar;
    public Character character;
    public List<Vector3> path;
    private Queue<Vector3> pathQueue = new Queue<Vector3>();
    private Vector3 currentTargetNode;

    void Awake()
    {
        map.getListVertices();
        // Debug.Log(map.checkTileAtPosition(new Vector3Int(-4, -1, 0)));
        map.genMap();
        map.onAwake();
        // map.printTilePositions();
    }
    void Update()
    {
        map.onAwake();
        map.onUpdate();
        if (Input.GetMouseButtonDown(0))
        {
            map.onClick();
            character.isMoving = true;
            path = astar.path;
        }
        if (path != null && path.Count > 0)
        {
            character.StartMove(path); // Gọi di chuyển
        }
    }
}
