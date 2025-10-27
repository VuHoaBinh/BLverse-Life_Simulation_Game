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
    private static int timeLine = -1;
    public Coroutine timeLineCouroutine;
    public HealthBar healthBar;
    public HealthBar foodBar;
    public HealthBar drinkBar;
    public List<Transform> listLocations;
    public Vector3Int posEat;
    public Vector3Int posDrink;
    public Vector3Int posStress;
    public Vector3Int posWork;
    public Vector3Int posSleep;
    public Vector3Int posPlayer;
    public TrajectoryCollector trajectoryCollector;
    public int TimeLine
    {
        get { return timeLine; }

        set
        {
            timeLine = Mathf.Clamp(value, 0, 1440);
        }
    }

    void Awake()
    {
        posEat = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[0].position);
        posDrink = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[1].position);
        posStress = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[2].position);
        posWork = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[3].position);
        posSleep = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[4].position);
        timeLine = 0;
        map.getListVertices();
        map.genMap();
        map.onAwake();
        Debug.Log(map.astar.startNode.position + "!!!!");
        // map.printTilePositions();
        Debug.Log("Tọa độ của bếp: " + listLocations[0].position);
    }
    public IEnumerator movePerStep(List<Vector3> path)
    {
        int i = 0;
        // Debug.Log(path.Count);
        while (i < path.Count)
        {
            timeLine++;
            Vector3 beforeCharacterPosition = character.transform.position;
            character.StartMove(map.changeCellPos(path[i++]));
            calcStat();
            TrajectoryStep trajectoryStep = new TrajectoryStep(map.changeCellPos(path[i - 1]) - beforeCharacterPosition, character, this);

            trajectoryCollector.addStep(trajectoryStep);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        currentState = GameState.Waiting;
        character.isMoving = false;
    }
    public void calcStat()
    {
        posPlayer = this.map.GetComponentInChildren<Tilemap>().WorldToCell(character.transform.position);
        Debug.Log("Sao không chạy vào đây!!!!");
        character.Food -= (1f / 18f);
        character.Drink -= (1f / 18f);
        character.Sleep -= (1f / 6f);
        //Giới hạn giá trị tối đa
        Debug.Log("Check!!" + posEat);
        Debug.Log("Check!!" + posPlayer);

        if (character.Food < 12 || character.Drink < 12)
        {
            character.Stress += 1.5f;
        }
        if (posPlayer == posEat && character.Money >= 15)
        {
            character.Food += 8;
            character.Money -= 15;
        }
        if (posPlayer == posDrink && character.Money >= 5)
        {
            character.Drink += 4;
            character.Money -= 5;
        }
        if (posPlayer == posSleep)
        {
            character.Sleep += 3;
            character.Stress -= 0.5f;

        }
        if (posPlayer == posWork)
        {
            character.Money += 25;
            character.Food -= ((1f / 18f) / 2f);
            character.Drink -= ((1f / 18f) / 2f);
            character.Stress += 2;
        }
        if (posPlayer == posStress)
        {
            character.Stress -= 1f;
        }
    }
    void Update()
    {
        map.onAwake();
        if (!character.isMoving)
        {
            map.onUpdate();
        }
        if (currentState == GameState.Waiting && Input.GetMouseButtonDown(0))
        {
            currentState = GameState.Process;
            ProcessStep(false);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            timeLine++;
            currentState = GameState.Process;
            ProcessStep(true);
        }
        //Để debug Trajectory
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(trajectoryCollector.ToString());
        }
        // character.StartMove(new Vector3(-12.5f, 4.5f, 0));
    }
    public void ProcessStep(bool isIdle)
    {
        map.onClick();
        character.isMoving = true;
        path = astar.path;
        if (!isIdle)
        {
            timeLineCouroutine = StartCoroutine(movePerStep(path));
        }
        else
        {
            calcStat();
            TrajectoryStep trajectoryStep = new TrajectoryStep(Vector3.zero, character, this);
            trajectoryStep.stepIndex += 1;
            trajectoryCollector.addStep(trajectoryStep);
            character.isMoving = false;
            currentState = GameState.Waiting;
        }
    }
}
