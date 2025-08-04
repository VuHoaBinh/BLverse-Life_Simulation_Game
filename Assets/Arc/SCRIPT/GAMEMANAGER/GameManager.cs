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

    void Awake()
    {
        map.getListVertices();
        // Debug.Log(map.checkTileAtPosition(new Vector3Int(-4, -1, 0)));
        map.genMap();
        map.onAwake();
        // map.printTilePositions();
    }
    private void changeState(int lenght, Vector3 target, Vector3 target2)
    {
        Debug.Log("Change state to: " + lenght);
        Debug.Log("Target position: " + target);
        character.StartMove(target);
        character.StartMove(target2);
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
            changeState(path.Count, path[0], path[1]);
        }
    }
}
