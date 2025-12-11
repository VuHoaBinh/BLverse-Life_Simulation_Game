using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using System.Collections;
using System.Linq.Expressions;
using NUnit.Framework;
using Unity.MLAgents.Integrations.Match3;

public class GridBrain : Agent
{
    public ThongKe ThongKe;
    public GameManager gameManager;
    public Character character;
    private System.Random rand = new System.Random();
    private float initSleep = 160;
    private float initFood = 240;
    private float initDrink = 80;
    private float initStress = 0;
    private int initMoney = 100;
    private Vector3 target;
    public int count = 0;
    Vector3 startCell;
    private float prevDistancePhase1 = 0f;
    private Vector3 prePosition;
    public Vector3Int posPlayer;

    public override void OnEpisodeBegin()
    {
        prevDistancePhase1 = Mathf.Infinity;
        count = 0;
        bool isInitial = false;
        int maxAttempts = 100; // tránh loop vô hạn
        int attempts = 0;

        gameManager.TimeLine = 21 * 60 - 60;
        while (!isInitial && attempts < maxAttempts)
        {
            attempts++;
            // Reset nhân vật về ô random
            int x = rand.Next(-8, 20);  // Random ngẫu nhiên theo kích thước map chiều ngang
            int y = rand.Next(-14, -2); //Random ngẫu nhiên theo kích thước map chiều dọc
            Vector2Int key = new Vector2Int(x, y);
            bool inMap = gameManager.map.TilePositions.ContainsKey(key);
            gameManager.map.TilePositions.TryGetValue(key, out bool isValid);
            if (inMap && !isValid)
            {
                startCell = new Vector3(x + 0.5f, y + 0.5f, 0); //cộng 0.5 để chuyển từ ô sang tọa độ world cho nhân vật
                character.transform.position = startCell;
                character.rb.velocity = Vector2.zero;
                isInitial = true;
            }
        }
        if (!isInitial)
        {
            startCell = new Vector3(-2.5f, -8.5f, 0);
            character.transform.position = startCell;
            Debug.Log($"Dùng tọa độ mặc định: {startCell}");
        }
        //Random intial stat
        character.Sleep = rand.Next(20, (int)initSleep);
        character.Food = rand.Next(20, (int)initFood);
        character.Drink = rand.Next(20, (int)initDrink);
        character.Stress = rand.Next(0, 21);


        // character.Sleep = initSleep;
        // character.Food = initFood;
        // character.Drink = initDrink;
        // character.Stress = initStress;

        character.Money = 20;
        posPlayer = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(character.transform.position);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // character.StartMove(new Vector3(-12.5f, -2.5f, 0));

        /*
            - [0]: giá trị trục x của tọa độ npc
            - [1]: giá trị trục y của tọa độ npc
            - [2]: chỉ số ngủ của npc
            - [3]: chỉ số đồ ăn của npc
            - [4]: chỉ số thức uống của npc
            - [5]: chỉ số căng thẳng của npc
            - [6]: lượng tiền mà player có
            - [7,8,9,10,11,12]: Tọa độ x,y,z của Bếp, và vector 
            - [13,14,15,16,17,18]: Tọa độ x,y,z của Tủ Lạnh, và vector 
            - [19,20,21,22,23,24]: Tọa độ x,y,z của Sofa, và vector 
            - [25,26,27,28,29,30]: Tọa độ x,y,z của Cửa, và vector 
            - [31,32,33,34,35,36]: Tọa độ x,y,z của Giường, và vector  
            - [37,38,39,40,41]: Khoảng cách của npc đến bếp, tủ lạnh, sofa, cửa, giường
            - [42]: thời gian trong ngày (1440=>60step mất 1 tiếng)    
        */
        Vector3 agentCell = character.transform.position;
        //Debug.Log("Toa do cua player la: " + agentCell);
        sensor.AddObservation(agentCell.x);
        sensor.AddObservation(agentCell.y);
        //Các chỉ số
        sensor.AddObservation(character.Sleep / 160);
        // Debug.Log(character.Sleep);
        sensor.AddObservation(character.Food / 240);
        sensor.AddObservation(character.Drink / 80);
        sensor.AddObservation(character.Stress / 72);
        sensor.AddObservation(character.Money);
        //Khoảng cách đến Bếp
        sensor.AddObservation(Vector3.Distance(agentCell, gameManager.listLocations[0].position));
        //Khoảng cách đến Tủ lạnh
        sensor.AddObservation(Vector3.Distance(agentCell, gameManager.listLocations[1].position));
        //Khoảng cách đến sofa
        sensor.AddObservation(Vector3.Distance(agentCell, gameManager.listLocations[2].position));
        //Khoảng cách đến cửa
        sensor.AddObservation(Vector3.Distance(agentCell, gameManager.listLocations[3].position));
        //Khoảng cách đến giường
        sensor.AddObservation(Vector3.Distance(agentCell, gameManager.listLocations[4].position));
        //Timeline
        sensor.AddObservation(gameManager.TimeLine / 1440);
    }
    // public SaveLogBC loggerBC;
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (character.isMoving)
        {
            return;
        }

        int moveAction = actions.DiscreteActions[0];
        int interactAction = actions.DiscreteActions[1];

        //Lưu action vào đây
        count++;
        character.countStep = count;
        // loggerBC.LogAction(moveAction, interactAction, character, gameManager);
        // Lấy vị trí hiện tại
        // Vector3 currentCell = character.transform.position;

        Vector3 currentCell = character.transform.position;
        // prePosition = posPlayer;
        Vector3 nextCell = currentCell;


        switch (moveAction)
        {
            case 0: nextCell += Vector3.up; break;                  // Lên
            case 1: nextCell += Vector3.down; break;                // Xuống
            case 2: nextCell += Vector3.left; break;                // Trái
            case 3: nextCell += Vector3.right; break;               // Phải
            case 4: nextCell += Vector3.up + Vector3.left; break;    // Lên - Trái
            case 5: nextCell += Vector3.up + Vector3.right; break;   // Lên - Phải
            case 6: nextCell += Vector3.down + Vector3.left; break;  // Xuống - Trái
            case 7: nextCell += Vector3.down + Vector3.right; break; // Xuống - Phải
            case 8: nextCell += Vector3.zero; break;                // Đứng yên
        }
        switch (interactAction)
        {
            case 0: character.Standing(); break;
            case 1: character.Eating(); break;
            case 2: character.Drinking(); break;
            case 3: character.Sleeping(); break;
            case 4: character.Working(); break;
            case 5: character.Relaxing(); break;
        }
        Vector2Int key = new Vector2Int((int)(nextCell.x - 0.5f), (int)(nextCell.y - 0.5f));
        gameManager.map.TilePositions.TryGetValue(key, out bool isInValidToMoving);
        bool isInMap = gameManager.map.TilePositions.ContainsKey(key);

        if (isInMap && !isInValidToMoving)
        {
            AddReward(0.01f);
            StartCoroutine(MoveSmooth(nextCell));
            gameManager.calcStat(character);
            calcReward_tuDuyTriChiSoSong_phase2(interactAction);
        }
        else if (!(isInMap && !isInValidToMoving))
        {
            ThongKe.ThemLanVaTuong();
            gameManager.calcStat(character);
            calcReward_tuDuyTriChiSoSong_phase2(interactAction);
            AddReward(-0.01f);
        }
        posPlayer = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(character.transform.position);
    }
    private IEnumerator MoveSmooth(Vector3 nextCell)
    {
        character.StartMove(nextCell);
        yield return new WaitForSeconds(0.12f);
        // character.notMove();
        character.isMoving = false;
    }

    public IEnumerator loadAction()
    {
        float value = 0f;
        while (value < 1f)
        {
            value += Time.deltaTime; // 1 giây
            character.loadBar.setHP(value * 100f);
            yield return null;
        }
        yield return null;
        character.loadBar.setHP(0);
        character.isInteracting = false;
    }
    public Astar astar;
    private bool isFinish = false;
    private bool isInteracable = false;
    Node target_ = new Node(new Vector3(-2, -9, 0));

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        if (character.isMoving)
        {
            d[0] = 8;
            d[1] = 0;
            return;
        }
        float randomFood = rand.Next(30, 121);
        float randomDrink = rand.Next(10, 41);
        float randomSleep = rand.Next(20, 81);
        float randomStress = rand.Next(31, 65);
        float randomMoney = rand.Next(0, 21);
        Vector3 currentPosition = character.transform.position;
        Vector3 currentPosition_gridBase = currentPosition - new Vector3(0.5f, 0.5f, 0);
        if (isFinish)
        {
            if (character.Food <= randomFood)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posEat);
                // Debug.Log($"Đi ăn lúc: {gameManager.TimeLine}");
            }
            else if (character.Drink <= randomDrink)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posDrink);
                // Debug.Log($"Đi uông lúc: {gameManager.TimeLine}");
            }
            else if (character.Sleep <= randomSleep)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posSleep);
                // Debug.Log($"Đi ngủ lúc: {gameManager.TimeLine}");
            }
            else if (character.Stress >= randomStress)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posStress);
                // Debug.Log($"Đi giải trí: {gameManager.TimeLine}");
            }
            else if (character.Money <= randomMoney)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posWork);
                // Debug.Log($"Đi làm lúc: {gameManager.TimeLine}");
            }
            else
            {
                isInteracable = false;
                bool isRandom = false;
                int maxAttempts = 100; // tránh loop vô hạn
                int attempts = 0;
                //Random 1 toa do ngau nhien hop le
                while (!isRandom && attempts < maxAttempts)
                {
                    attempts++;
                    // Reset nhân vật về ô random
                    int x = rand.Next(-8, 20);
                    int y = rand.Next(-14, -2);
                    Vector2Int key = new Vector2Int(x, y);
                    bool inMap = gameManager.map.TilePositions.ContainsKey(key);
                    gameManager.map.TilePositions.TryGetValue(key, out bool isValid);
                    if (inMap && !isValid)
                    {
                        target_ = new Node(new Vector3(x, y, 0));
                    }
                }
            }
        }

        if (Vector3.Distance(currentPosition_gridBase, target_.position) >= 0.01f)
        {
            isFinish = false;
            Vector3 next = astar.FindPath(new Node(currentPosition_gridBase), target_)[0];
            Vector3 direction = next - currentPosition_gridBase;

            if (direction == Vector3.up) d[0] = 0;             // Lên
            else if (direction == Vector3.down) d[0] = 1;      // Xuống
            else if (direction == Vector3.left) d[0] = 2;      // Trái
            else if (direction == Vector3.right) d[0] = 3;     // Phải
            else if (direction == Vector3.up + Vector3.left) d[0] = 4;  // Lên - Trái
            else if (direction == Vector3.up + Vector3.right) d[0] = 5;   // Lên - Phải
            else if (direction == Vector3.down + Vector3.left) d[0] = 6; // Xuống - Trái
            else if (direction == Vector3.down + Vector3.right) d[0] = 7;  // Xuống - Phải
            gameManager.textSetter.setDatePerFrame(character);
        }
        else
        {
            isFinish = true; //Để đổi sang vị trí mới
            if (isInteracable)
            {
                isInteracable = false;
                d[0] = 8;
                // --- Thực hiện hành động tương ứng với mục tiêu ---
                if (target_.position == gameManager.posEat)
                {
                    d[1] = 1;
                }
                else if (target_.position == gameManager.posDrink)
                {
                    d[1] = 2;
                }
                else if (target_.position == gameManager.posSleep)
                {
                    d[1] = 3;
                }
                else if (target_.position == gameManager.posWork)
                {
                    d[1] = 4;
                }
                else if (target_.position == gameManager.posStress)
                {
                    d[1] = 5;
                }
            }
            gameManager.textSetter.setDatePerFrame(character);
        }
    }
    private void calcReward_phase1(int action)
    {
        Vector3 startCell_grid = startCell + new Vector3(0.5f, 0.5f, 0);

        // Các điểm mục tiêu
        Vector3[] targets = new Vector3[]
        {
            gameManager.posEat,
            // gameManager.posDrink,
            gameManager.posSleep,
            // gameManager.posWork,
            gameManager.posStress
        };

        // Tính khoảng cách từ agent tới từng mục tiêu
        float[] distances = new float[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            distances[i] = Vector3.Distance(startCell_grid, targets[i]);
        }

        // Tìm index điểm gần nhất
        int nearestIndex = 0;
        float nearestDistance = distances[0];

        for (int i = 1; i < distances.Length; i++)
        {
            if (distances[i] < nearestDistance)
            {
                nearestDistance = distances[i];
                nearestIndex = i;
            }
        }

        float currentDistance = Vector3.Distance(posPlayer, targets[nearestIndex]);

        // ================================
        //      TỚI ĐÚNG ĐIỂM GẦN NHẤT
        // ================================
        float reachThreshold = 0.1f;
        if (currentDistance < reachThreshold)
        {
            if (action == 8)
                AddReward(1f);      // đúng action → thưởng lớn
            else
                AddReward(-0.2f);   // sai action → phạt

            EndEpisode();
            return;
        }

        // ================================
        //       REWARD TIẾN GẦN
        // ================================

        if (prevDistancePhase1 > currentDistance)
        {
            AddReward(+0.01f); // tiến gần → thưởng nhẹ
        }
        else
        {
            AddReward(-0.01f); // đi xa → phạt nhẹ
        }

        prevDistancePhase1 = currentDistance;
    }
    private void calcReward_tuDuyTriChiSoSong()
    {
        //Chuẩn hóa chỉ số về [0,1]
        float hunger = Mathf.Clamp01(1f - character.Food / 24f);
        float thirst = Mathf.Clamp01(1f - character.Drink / 24f);
        float tired = Mathf.Clamp01(1f - character.Sleep / 24f);
        float stress = Mathf.Clamp01(character.Stress / 72f);

        //Phạt dần theo trạng thái xấu
        AddReward(-0.01f * (hunger + thirst + tired + stress));

        //Phạt mạnh nếu chạm ngưỡng nguy hiểm
        if (character.Food <= 4f) AddReward(-1f);
        if (character.Drink <= 4f) AddReward(-1f);
        if (character.Sleep <= 4f) AddReward(-1f);
        if (character.Stress >= 60f) AddReward(-1f);

        //Nếu nhân vật "chết" (kiệt sức, đói, khát, stress max)
        if (character.Food <= 0f || character.Drink <= 0f ||
            character.Sleep <= 0f || character.Stress >= 72f)
        {
            // Debug.Log($"Nhân vật đã chết: {character.Food}, {character.Drink}, {character.Sleep}, {character.Stress} với step là {count}");
            gameManager.resetTimeLine();
            AddReward(-1f);
            EndEpisode();
        }

        //Thưởng hợp lý theo vị trí hiện tại và trạng thái
        Vector3 pos = posPlayer;

        if (pos == gameManager.posEat)
        {
            if (character.Food < 15f) AddReward(0.5f);   // ăn khi đói → tốt
            else AddReward(-0.2f);                       // ăn khi no → lãng phí
        }
        else if (pos == gameManager.posDrink)
        {
            if (character.Drink < 15f) AddReward(0.5f);
            else AddReward(-0.2f);
        }
        else if (pos == gameManager.posSleep)
        {
            if (character.Sleep < 10f) AddReward(0.8f);  // ngủ khi mệt → tốt
            else AddReward(-0.2f);
        }
        else if (pos == gameManager.posWork)
        {
            if (character.Sleep > 10f && character.Food > 10f && character.Drink > 10f)
                AddReward(1f);                        // đủ điều kiện làm việc → tốt
            else
                AddReward(-0.5f);                        // làm khi mệt → xấu
        }
        else if (pos == gameManager.posStress)
        {
            if (character.Stress > 30f) AddReward(0.6f); // đi xả stress khi stress cao
            else AddReward(-0.1f);                       // stress thấp mà vẫn đi → lãng phí
        }

        //Thưởng nhỏ khi duy trì trạng thái cân bằng tổng thể
        if (character.Food > 12f && character.Drink > 12f &&
            character.Sleep > 12f && character.Stress < 30f)
        {
            AddReward(0.05f);
        }
    }

    // public int[] countAction()
    // {

    // }

    /// <summary>
    ///     - 29/11: Thêm cơ chế chán cho NPC
    /// </summary>
    /// <param name="action"></param>
    private void calcReward_tuDuyTriChiSoSong_phase2(int action)
    {
        //Chuẩn hóa chỉ số về [0,1]
        float hunger = Mathf.Clamp01(1f - character.Food / 240f);
        float thirst = Mathf.Clamp01(1f - character.Drink / 80f);
        float tired = Mathf.Clamp01(1f - character.Sleep / 160f);
        float stress = Mathf.Clamp01(character.Stress / 72f);

        //Phạt dần theo trạng thái xấu
        AddReward(-0.01f * (hunger + thirst + tired + stress));

        //Phạt mạnh nếu chạm ngưỡng nguy hiểm
        if (character.Food <= 30f) AddReward(-0.05f);
        if (character.Drink <= 20f) AddReward(-0.05f);
        if (character.Sleep <= 25f) AddReward(-0.05f);
        if (character.Stress >= 60f) AddReward(-0.05f);

        //Nếu nhân vật "chết" (kiệt sức, đói, khát, stress max)
        if (character.Food <= 0f || character.Drink <= 0f ||
            character.Sleep <= 0f || character.Stress >= 72f)
        {
            ThongKe.ThemLanChet();
            // Debug.Log($"Nhân vật đã chết: {character.Food}, {character.Drink}, {character.Sleep}, {character.Stress} với step là {count}");
            gameManager.resetTimeLine();
            AddReward(-5f);
            EndEpisode();
            character.isDeath = true;
            return;
        }

        //Thưởng hợp lý theo vị trí hiện tại và trạng thái
        //Nếu chỉ số phù hợp thì mới thưởng
        Vector3 pos = posPlayer;
        if (pos == gameManager.posEat)
        {
            if (action == 1 && character.Money >= 15)
            {
                ThongKe.ThemLanThucHienDungAction(action);
                AddReward(0.04f);
            }
            else if (action == 1 && character.Money < 15)
            {
                AddReward(0.0005f);
            }
            if (character.Food < 120f) AddReward(0.05f);
            else AddReward(-0.02f);
        }
        else if (pos == gameManager.posDrink)
        {
            if (action == 2 && character.Money >= 5)
            {
                ThongKe.ThemLanThucHienDungAction(action);
                AddReward(0.02f);
            }
            else if (action == 1 && character.Money < 5)
            {
                AddReward(0.0005f);
            }
            if (character.Drink < 40f) AddReward(0.05f);
            else AddReward(-0.02f);
        }
        else if (pos == gameManager.posSleep)
        {
            if (action == 3)
            {
                ThongKe.ThemLanThucHienDungAction(action);
                AddReward(0.1f);
            }
            if (character.Sleep < 60f) AddReward(0.05f);
            else AddReward(-0.02f);
        }
        else if (pos == gameManager.posWork)
        {
            if (action == 4 && character.Sleep >= 30)
            {
                ThongKe.ThemLanThucHienDungAction(action);
                AddReward(0.04f);
            }
            if (character.Sleep > 80f && character.Food > 120f && character.Drink > 40f)
                AddReward(0.05f);
            else
                AddReward(0.02f);
        }
        else if (pos == gameManager.posStress)
        {
            if (action == 5)
            {
                ThongKe.ThemLanThucHienDungAction(action);
                AddReward(0.03f);
            }
            if (character.Stress > 30f) AddReward(0.05f);
            else AddReward(-0.02f);
        }
        else
        {
            if (action == 0)
            {
                ThongKe.ThemLanThucHienDungAction(action);
                AddReward(0.0005f);
            }
        }

        //Thưởng nhỏ khi duy trì trạng thái cân bằng tổng thể
        if (character.Food > 120f && character.Drink > 40f &&
            character.Sleep > 80f && character.Stress < 30f && character.Money >= 50)
        {
            AddReward(0.4f);
        }
    }
}
