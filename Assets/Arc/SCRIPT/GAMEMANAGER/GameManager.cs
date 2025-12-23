using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DevionGames;
using TMPro;
using Unity.Entities.UniversalDelegates;
using Unity.VisualScripting;



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
    public List<Character> character;
    public List<Vector3> path;
    public enum GameState { Waiting, Process, End };
    public GameState currentState = GameState.Waiting;
    public Coroutine timeLineCouroutine;
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
    public GameObject UI;

    public CameraFollow cameraFollow;
    public int TimeLine
    {
        get { return timeLine; }

        set
        {
            timeLine = (value + 1441) % 1441;
        }
    }
    public GameObject prefabInfo;
    public List<GiveInfo> giveInfoList;
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
        giveInfoList = new List<GiveInfo>();
        Transform infoTab = UI.transform.Find("SpaceIF/Bg_Tab");
        GameObject infoObj = infoTab.gameObject;
        for (int numCharacter = 0; numCharacter < character.Count; numCharacter++)
        {
            GameObject instance = Instantiate(prefabInfo, infoObj.transform);


            GiveInfo giveInfo = instance.GetComponent<GiveInfo>();

            Character char_ = character[numCharacter];
            giveInfo.character = char_;
            giveInfo.setName($"Player {numCharacter}");
            giveInfo.setCount(char_.countStep);

            giveInfoList.Add(giveInfo);
        }
    }
    public void resetTimeLine()
    {
        this.TimeLine = 0;
    }
    public IEnumerator movePerStep(List<Vector3> path)
    {
        int i = 0;
        while (i < path.Count)
        {
            character[0].countStep += 1;
            giveInfoList[0].setCount(character[0].countStep);

            Vector3 beforeCharacterPosition = character[0].transform.position;
            character[0].StartMove(map.changeCellPos(path[i++]));
            calcStat(character[0]);
            textSetter.setDatePerFrame(character[0]);
            TrajectoryStep trajectoryStep = new TrajectoryStep(map.changeCellPos(path[i - 1]) - beforeCharacterPosition, character[0], this);

            trajectoryCollector.addStep(trajectoryStep);
            if (posPlayer == posEat)
            {
                character[0].notMove();
                yield return StartCoroutine(loadAction());
                character[0].Eating();
            }
            else if (posPlayer == posDrink)
            {
                character[0].notMove();
                yield return StartCoroutine(loadAction());
                character[0].Drinking();
            }
            else if (posPlayer == posStress)
            {
                character[0].notMove();
                yield return StartCoroutine(loadAction());
                character[0].Relaxing();
            }
            else if (posPlayer == posSleep)
            {
                character[0].notMove();
                yield return StartCoroutine(loadAction());
                character[0].Sleeping();
            }
            else if (posPlayer == posWork)
            {
                character[0].notMove();
                yield return StartCoroutine(loadAction());
                character[0].Working();
            }
            else
            {
                character[0].Standing();
            }
            calcStat(character[0]);
            yield return new WaitForSeconds(0.1f);
            character[0].notMove();
        }
        currentState = GameState.Waiting;
        character[0].isMoving = false;
    }
    public IEnumerator loadAction()
    {
        float value = 0f;

        while (value < 1f)
        {
            value += Time.deltaTime; // 1 giây
            character[0].loadBar.setHP(value * 100f);
            yield return null;
        }
        character[0].loadBar.setHP(0);
    }

    public void calcStat(Character character)
    {
        posPlayer = this.map.GetComponentInChildren<Tilemap>().WorldToCell(character.transform.position);
        this.TimeLine++;
        // Debug.Log("Thời gian hiện tại: " + timeLine);


        character.Food -= (1f / 18f);
        character.Drink -= (1f / 18f);
        character.Sleep -= (1f / 18f);


        if (character.Food < 24 || character.Drink < 8 || character.Sleep <= 16)
        {
            character.Stress += 0.2f;
        }
        if (posPlayer == posEat && character.Money >= 15 && character.isEating)
        {
            character.Food += 80;
            character.Money -= 15;
            this.TimeLine += 30;
        }
        if (posPlayer == posDrink && character.Money >= 5 && character.isDrinking)
        {
            character.Drink += 40;
            character.Money -= 5;
            this.TimeLine += 1;
        }
        if (posPlayer == posSleep && character.isSleeping)
        {
            character.Sleep += 24 * (160 / 24);
            character.Stress -= 2;
            character.Food -= ((1f / 18f) / 2f) * 60;
            character.Drink -= ((1f / 18f) / 2f) * 60;
            this.TimeLine += 480;
        }
        if (posPlayer == posWork && character.isWorking && character.Sleep >= 30)
        {
            character.Money += 25 * 3;
            character.Food -= ((1f / 18f) / 2f) * 8 * 60;
            character.Drink -= ((1f / 18f) / 2f) * 8 * 60;
            character.Stress += 10f;
            this.TimeLine += 480;
        }
        if (posPlayer == posStress && character.isRelaxing)
        {
            character.Stress -= 10f;
            this.TimeLine += 60;
        }
        character.foodBar.setHP(character.Food);
        character.drinkBar.setHP(character.Drink);
        character.sleepBar.setHP(character.Sleep);
        character.StressBar.setHP(character.Stress);
    }
    public void calcStat_noSpace()
    {
        this.TimeLine++;
        // Debug.Log("Thời gian hiện tại: " + this.TimeLine);
        posPlayer = this.map.GetComponentInChildren<Tilemap>().WorldToCell(character[0].transform.position);
        character[0].Food -= (1f / 18f);
        character[0].Drink -= (1f / 18f);
        character[0].Sleep -= (1f / 6f);

        if (character[0].Food < 12 || character[0].Drink < 12)
        {
            character[0].Stress += 1.5f;
        }
        if (posPlayer == posEat && character[0].Money >= 15)
        {
            character[0].Food += 80;
            character[0].Money -= 15;
            this.TimeLine += 30;
        }
        if (posPlayer == posDrink && character[0].Money >= 5)
        {
            character[0].Drink += 4 * (80 / 24);
            character[0].Money -= 5;
            this.TimeLine += 1;

        }
        if (posPlayer == posSleep)
        {
            character[0].Sleep += 24 * (160 / 24);
            character[0].Stress -= 0.5f;
            this.TimeLine += 480;
        }
        if (posPlayer == posWork)
        {
            character[0].Money += 25 * 8;
            character[0].Food -= ((1f / 18f) / 2f) * 8 * 60;
            character[0].Drink -= ((1f / 18f) / 2f) * 8 * 60;
            character[0].Stress += 10f;
            this.TimeLine += 480;
        }
        if (posPlayer == posStress)
        {
            character[0].Stress -= 9f;
            this.TimeLine += 60;
        }
    }

    public int currentChar = 0;
    void Update()
    {
        for (int numCharacter = 0; numCharacter < character.Count; numCharacter++)
        {
            giveInfoList[numCharacter].setCount(character[numCharacter].countStep);
        }

        if (!character[0].isMoving)
        {
            map.onUpdate();
        }
        if (currentState == GameState.Waiting && Input.GetMouseButtonDown(0))
        {
            // Debug.Log(map.nextCellPos);
            currentState = GameState.Process;
            ProcessStep(false);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Transform stats = character[0].transform.Find("stats");
            GameObject statsObj = stats.gameObject;
            statsObj.SetActive(!statsObj.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Transform infoTab = UI.transform.Find("SpaceIF");
            GameObject infoObj = infoTab.gameObject;
            infoObj.SetActive(!infoObj.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            giveInfoList[currentChar].unHighLightName();
            if (currentChar != 0) currentChar -= 1;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            giveInfoList[currentChar].unHighLightName();
            if (currentChar != character.Count - 1) currentChar += 1;
        }
        giveInfoList[currentChar].highLightName();
        cameraFollow.target = character[currentChar].transform;
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     timeLine++;
        //     currentState = GameState.Process;
        //     // ProcessStep(true);
        // }

    }
    public void ProcessStep(bool isIdle)
    {
        map.onClick();
        character[0].isMoving = true;
        path = character[0].GetComponent<Astar>().FindPath(new Node(character[0].transform.position - new Vector3(0.5f, 0.5f, 0)), new Node(map.nextCellPos));

        if (!isIdle)
        {
            timeLineCouroutine = StartCoroutine(movePerStep(path));
        }
        else
        {
            calcStat(character[0]);
            TrajectoryStep trajectoryStep = new TrajectoryStep(Vector3.zero, character[0], this);
            trajectoryStep.stepIndex += 1;
            trajectoryCollector.addStep(trajectoryStep);
            character[0].isMoving = false;
            currentState = GameState.Waiting;
        }
    }

    public void ProcessSteptoA(bool isIdle)
    {
        // map.onClick();
        character[0].isMoving = true;
        path = character[0].GetComponent<Astar>().FindPath(new Node(character[0].transform.position - new Vector3(0.5f, 0.5f, 0)), new Node(map.nextCellPos));
        if (!isIdle)
        {
            timeLineCouroutine = StartCoroutine(movePerStep(path));
        }
        else
        {
            calcStat(character[0]);
            TrajectoryStep trajectoryStep = new TrajectoryStep(Vector3.zero, character[0], this);
            trajectoryStep.stepIndex += 1;
            trajectoryCollector.addStep(trajectoryStep);
            character[0].isMoving = false;
            currentState = GameState.Waiting;
        }
    }
}
