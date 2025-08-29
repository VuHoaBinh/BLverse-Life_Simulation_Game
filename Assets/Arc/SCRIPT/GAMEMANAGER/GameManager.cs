using System;
using System.Collections;
using System.Collections.Generic;

// using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Map map;
    public MapUI mapUI;
    public Astar astar;
    public Character character;
    public List<Vector3> path;
    public enum GameState { Waiting, Process, End };
    public GameState currentState = GameState.Waiting;
    public static int timeLine = -1;
    public Coroutine timeLineCouroutine;
    void Awake()
    {
        timeLine = 0;
        map.getListVertices();
        // Debug.Log(map.checkTileAtPosition(new Vector3Int(-4, -1, 0)));
        map.genMap();
        map.onAwake();
        // map.printTilePositions();
    }
    public bool checkTimeline()
    {
        if (timeLine >= 144)
        {
            timeLine = 0;
            return false;
        }
        return true;
    }
    public IEnumerator movePerStep(List<Vector3> path)
    {
        int i = 0;
        Debug.Log(path.Count);
        while (i < path.Count)
        {
            timeLine++;
            character.StartMove(path[i++]);
            Debug.Log("Bộ đếm thời gian: " + timeLine);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        currentState = GameState.Waiting;
        character.isMoving = false;
    }
    void Update()
    {
        map.onAwake();
        if (!character.isMoving)
        {
            map.onUpdate();
        }
        if (!checkTimeline())
        {
            Debug.Log("Đã chỉnh sửa timeline");
        }
        if (currentState == GameState.Waiting && Input.GetMouseButtonDown(0))
        {
            currentState = GameState.Process;
            ProcessStep();
        }
    }
    public void ProcessStep()
    {
        map.onClick();
        character.isMoving = true;
        path = astar.path;
        timeLineCouroutine = StartCoroutine(movePerStep(path));
        // currentState = GameState.Waiting;
    }
}
