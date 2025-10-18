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
            Vector3 nextCell = path[i++];
            character.StartMove(nextCell);
            calcReward();
            // Check hợp lệ (không đi ra ngoài)
            Debug.Log("Bộ đếm thời gian: " + timeLine);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        currentState = GameState.Waiting;
        character.isMoving = false;
    }
    private void calcReward()
    {
        Vector3Int posEat = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[0].position);
        Vector3Int posDrink = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[1].position);
        Vector3Int posStress = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[2].position);
        Vector3Int posWork = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[3].position);
        Vector3Int posSleep = this.map.GetComponentInChildren<Tilemap>().WorldToCell(this.listLocations[4].position);
        Vector3Int posPlayer = this.map.GetComponentInChildren<Tilemap>().WorldToCell(character.transform.position);
        character.Food -= (1f / 18f);
        character.Drink -= (1f / 18f);
        character.Sleep -= (1f / 6f);
        //Giới hạn giá trị tối đa
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
        if (!checkTimeline())
        {
            Debug.Log("Đã chỉnh sửa timeline");
        }
        if (currentState == GameState.Waiting && Input.GetMouseButtonDown(0))
        {
            currentState = GameState.Process;
            ProcessStep(false);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentState = GameState.Process;
            ProcessStep(true);

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
            calcReward();
            currentState = GameState.Waiting;
        }
        // currentState = GameState.Waiting;
    }
}
