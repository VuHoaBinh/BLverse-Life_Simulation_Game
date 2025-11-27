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
    public TextSetter textSetter;
    private static int timeLine = -1;
    public int TimeLine
    {
        get { return timeLine; }

        set
        {
            timeLine = (value + 1441) % 1441;
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
        // map.printTilePositions();
    }
    public void resetTimeLine()
    {
        this.TimeLine = 0;
    }
    public IEnumerator movePerStep(List<Vector3> path)
    {
        int i = 0;
        // Debug.Log(path.Count);
        while (i < path.Count)
        {
            Vector3 beforeCharacterPosition = character.transform.position;
            character.StartMove(map.changeCellPos(path[i++]));
            calcStat(character);
            textSetter.setDatePerFrame(character);
            TrajectoryStep trajectoryStep = new TrajectoryStep(map.changeCellPos(path[i - 1]) - beforeCharacterPosition, character, this);

            trajectoryCollector.addStep(trajectoryStep);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        currentState = GameState.Waiting;
        character.isMoving = false;
    }
    public void calcStat(Character character)
    {
        posPlayer = this.map.GetComponentInChildren<Tilemap>().WorldToCell(character.transform.position);
        this.TimeLine++;
        // Debug.Log("Thời gian hiện tại: " + timeLine);


        character.Food -= (1f / 18f);
        character.Drink -= (1f / 18f);
        character.Sleep -= (1f / 18f);


        if (character.Food < 12 || character.Drink < 12)
        {
            character.Stress += 1.5f;
        }
        if (posPlayer == posEat && character.Money >= 15 && character.isEating)
        {
            character.Food += 80;
            character.Money -= 15;
            this.TimeLine += 30;
        }
        if (posPlayer == posDrink && character.Money >= 5 && character.isDrinking)
        {
            character.Drink += 4 * (80 / 24);
            character.Money -= 5;
            this.TimeLine += 1;
        }
        if (posPlayer == posSleep && character.isSleeping)
        {
            character.Sleep += 24 * (160 / 24);
            character.Stress -= 0.5f;
            this.TimeLine += 480;
        }
        if (posPlayer == posWork && character.isWorking)
        {
            character.Money += 25 * 8;
            character.Food -= ((1f / 18f) / 2f) * 8 * 60;
            character.Drink -= ((1f / 18f) / 2f) * 8 * 60;
            character.Stress += 10f;
            this.TimeLine += 480;
        }
        if (posPlayer == posStress && character.isRelaxing)
        {
            character.Stress -= 9f;
            this.TimeLine += 60;
        }
    }
    public void calcStat_noSpace()
    {
        this.TimeLine++;
        // Debug.Log("Thời gian hiện tại: " + this.TimeLine);
        posPlayer = this.map.GetComponentInChildren<Tilemap>().WorldToCell(character.transform.position);
        character.Food -= (1f / 18f);
        character.Drink -= (1f / 18f);
        character.Sleep -= (1f / 6f);

        if (character.Food < 12 || character.Drink < 12)
        {
            character.Stress += 1.5f;
        }
        if (posPlayer == posEat && character.Money >= 15)
        {
            character.Food += 80;
            character.Money -= 15;
            this.TimeLine += 30;
        }
        if (posPlayer == posDrink && character.Money >= 5)
        {
            character.Drink += 4 * (80 / 24);
            character.Money -= 5;
            this.TimeLine += 1;

        }
        if (posPlayer == posSleep)
        {
            character.Sleep += 24 * (160 / 24);
            character.Stress -= 0.5f;
            this.TimeLine += 480;
        }
        if (posPlayer == posWork)
        {
            character.Money += 25 * 8;
            character.Food -= ((1f / 18f) / 2f) * 8 * 60;
            character.Drink -= ((1f / 18f) / 2f) * 8 * 60;
            character.Stress += 10f;
            this.TimeLine += 480;
        }
        if (posPlayer == posStress)
        {
            character.Stress -= 9f;
            this.TimeLine += 60;
        }
    }
    void Update()
    {
        // map.onAwake();
        if (!character.isMoving)
        {
            // map.onUpdate();
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
            calcStat(character);
            TrajectoryStep trajectoryStep = new TrajectoryStep(Vector3.zero, character, this);
            trajectoryStep.stepIndex += 1;
            trajectoryCollector.addStep(trajectoryStep);
            character.isMoving = false;
            currentState = GameState.Waiting;
        }
    }
}
