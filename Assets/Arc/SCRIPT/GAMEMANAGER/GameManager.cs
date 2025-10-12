using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
public class GameManager : MonoBehaviour
{
    public Item[] itemsdemo;
    // Start is called before the first frame update
    public Map map;
    public InventoryManager inventoryManager;
    public MapUI mapUI;
    public Astar astar;
    public Character character;
    public List<Vector3> path;
    public enum GameState { Waiting, Process, End };
    public GameState currentState = GameState.Waiting;
    public static int timeLine = -1;
    public Coroutine timeLineCouroutine;
    public HealthBar healthBar;
    public HealthBar foodBar;
    public HealthBar drinkBar;
    public List<Transform> listLocations;
    void Awake()
    {

        timeLine = 0;
        map.getListVertices();
        map.genMap();
        map.onAwake();
        Debug.Log(map.astar.startNode.position + "!!!!");
        // map.printTilePositions();
        Debug.Log("Tọa độ của bếp: " + listLocations[0].position);
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
            // character.food -= 1f;
            // character.drink -= 1f;
            // if (character.food <= 50 || character.drink <= 50)
            // {
            //     character.health -= 1f;
            // }
            // else if (character.food <= 30 || character.drink <= 30)
            // {
            //     character.health -= 2f;
            // }
            // healthBar.setHP(character.health);
            // foodBar.setHP(character.food);
            // drinkBar.setHP(character.drink);
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
        // character.StartMove(new Vector3(-12.5f, 4.5f, 0));
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
