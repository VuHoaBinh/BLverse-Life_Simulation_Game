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

public class GridBrain : Agent
{
    public GameManager gameManager;
    public Character character;

    private System.Random rand = new System.Random();
    [SerializeField] private float initSleep = 24;
    [SerializeField] private float initFood = 24;
    [SerializeField] private float initDrink = 24;
    [SerializeField] private float initStress = 0;
    [SerializeField] private int initMoney = 100;
    private Vector3 target;
    private int count = 0;
    Vector3 startCell;
    private float prevDistancePhase1 = 0f;
    private float prevNeedDistance = Mathf.Infinity; // track distance to current highest-need target

    public override void OnEpisodeBegin()
    {
        prevDistancePhase1 = Mathf.Infinity;
        prevNeedDistance = Mathf.Infinity;
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
                startCell = new Vector3(x + 0.5f, y + 0.5f, 0); //trừ 0.5 để chuyển từ ô sang tọa độ world cho nhân vật
                character.transform.position = startCell;
                character.rb.velocity = Vector2.zero;
                // Debug.Log($"Đặt nhân vật tại ô sau khi đã random thành công: {key}");
                isInitial = true;
            }
        }
        if (!isInitial)
        {
            startCell = new Vector3(-12.5f, -2.5f, 0);
            character.transform.position = startCell;
            Debug.Log($"Dùng tọa độ mặc định: {startCell}");
        }
        // Debug.Log($"Đặt nhân vật tại ô sau khi đã random thành công: {key}");
        //Random intial stat
        character.Sleep = rand.Next(12, 20);
        character.Food = rand.Next(12, 20);
        character.Drink = rand.Next(12, 20);
        character.Stress = rand.Next(0, 37);
        character.Money = initMoney;
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
        sensor.AddObservation(character.Sleep / 24);
        sensor.AddObservation(character.Food / 24);
        sensor.AddObservation(character.Drink / 24);
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
        sensor.AddObservation(gameManager.TimeLine);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        count++;
        if (count >= 1000)
        {
            EndEpisode();
        }
        // Lấy vị trí hiện tại
        Vector3 currentCell = character.transform.position;
        Vector3 nextCell = currentCell;
        // Debug.Log($"Vẫn gọi {action}");
        // Debug.Log("dau vao:" + action);
        switch (action)
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

        Vector2Int key = new Vector2Int((int)(nextCell.x - 0.5f), (int)(nextCell.y - 0.5f));
        gameManager.map.TilePositions.TryGetValue(key, out bool isValidToMoving);
        bool isInMap = gameManager.map.TilePositions.ContainsKey(key);

        if (isInMap && !isValidToMoving)
        {
            AddReward(0.2f);
            character.transform.position = nextCell;
            gameManager.calcStat_noSpace();
            if (gameManager.posPlayer == gameManager.posEat || gameManager.posPlayer == gameManager.posDrink
                || gameManager.posPlayer == gameManager.posSleep || gameManager.posPlayer == gameManager.posWork || gameManager.posPlayer == gameManager.posStress)
            {
                if (action == 8)
                {
                    // Debug.Log("Da nhan space o: " + key);
                }
            }

            // calcReward_tuDuyTriChiSoSong();
            // calcReward_phase1(action);
            calcReward_tuDuyTriChiSoSong_phase2(action);
        }
        else
        {
            gameManager.calcStat_noSpace();
            calcReward_tuDuyTriChiSoSong_phase2(action);
            // calcReward_phase1(action);
            // Debug.Log("Nhân vật đã va vào tường");
            AddReward(-0.2f);
        }
    }

    public Astar astar;
    private bool isFinish = false;
    private bool isInteracable = false;
    Node target_ = new Node(new Vector3(-2, -9, 0));

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        Vector3 currentPosition = character.transform.position;
        Vector3 currentPosition_gridBase = currentPosition - new Vector3(0.5f, 0.5f, 0);
        astar.startNode = new Node(currentPosition_gridBase);
        if (isFinish)
        {
            if (character.Food <= 12)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posEat);
                // Debug.Log($"Đi ăn lúc: {gameManager.TimeLine}");
            }
            else if (character.Drink <= 12)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posDrink);
                // Debug.Log($"Đi uông lúc: {gameManager.TimeLine}");
            }
            else if (character.Sleep <= 6)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posSleep);
                // Debug.Log($"Đi ngủ lúc: {gameManager.TimeLine}");
            }
            else if (character.Stress >= 10)
            {
                isInteracable = true;
                target_ = new Node(gameManager.posStress);
                // Debug.Log($"Đi giải trí: {gameManager.TimeLine}");
            }
            else if (character.Money <= 25)
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
                    int x = rand.Next(-5, -1);  // Random ngẫu nhiên theo kích thước map chiều ngang
                    int y = rand.Next(-11, -9); //Random ngẫu nhiên theo kích thước map chiều dọc
                    Vector2Int key = new Vector2Int(x, y);
                    bool inMap = gameManager.map.TilePositions.ContainsKey(key);
                    gameManager.map.TilePositions.TryGetValue(key, out bool isValid);
                    if (inMap && !isValid)
                    {
                        target_ = new Node(new Vector3(x, y, 0));
                        // Debug.Log($"Đặt nhân vật tại ô sau khi đã random thành công: {key}");
                        // Debug.Log($"Đi quanh lúc: {gameManager.TimeLine}");
                    }
                }
                // isRandom = true;
                // isInteracable = true;
                // if (gameManager.TimeLine == 21 * 60)
                // {
                //     target_ = new Node(gameManager.posSleep);
                //     // Debug.Log($"Đi ngủ lúc: {gameManager.TimeLine}");
                // }
                // else if (gameManager.TimeLine == 5 * 60)
                // {
                //     target_ = new Node(gameManager.posEat);
                //     // Debug.Log($"Đi ăn lúc: {gameManager.TimeLine}");
                // }
                // else if (gameManager.TimeLine == 7 * 60)
                // {
                //     target_ = new Node(gameManager.posDrink);
                //     // Debug.Log($"Đi uống lúc: {gameManager.TimeLine}");
                // }
                // else if (gameManager.TimeLine == 8 * 60)
                // {
                //     target_ = new Node(gameManager.posWork);
                //     // Debug.Log($"Đi làm lúc: {gameManager.TimeLine}");
                // }
                // else if (gameManager.TimeLine >= 16 * 60)
                // {
                //     target_ = new Node(gameManager.posStress);
                //     // Debug.Log($"Đi giải trí lúc: {gameManager.TimeLine}");
                // }
                // else
                // {
                //     isInteracable = false;
                //     bool isRandom = false;
                //     int maxAttempts = 100; // tránh loop vô hạn
                //     int attempts = 0;
                //     //Random 1 toa do ngau nhien hop le
                //     while (!isRandom && attempts < maxAttempts)
                //     {
                //         attempts++;
                //         // Reset nhân vật về ô random
                //         int x = rand.Next(-5, -1);  // Random ngẫu nhiên theo kích thước map chiều ngang
                //         int y = rand.Next(-11, -9); //Random ngẫu nhiên theo kích thước map chiều dọc
                //         Vector2Int key = new Vector2Int(x, y);
                //         bool inMap = gameManager.map.TilePositions.ContainsKey(key);
                //         gameManager.map.TilePositions.TryGetValue(key, out bool isValid);
                //         if (inMap && !isValid)
                //         {
                //             target_ = new Node(new Vector3(x, y, 0));
                //             // Debug.Log($"Đặt nhân vật tại ô sau khi đã random thành công: {key}");
                //             Debug.Log($"Đi quanh lúc: {gameManager.TimeLine}");

                //             isRandom = true;
                //         }
                //     }
                // }
            }
        }
        astar.goalNode = target_;

        if (Vector3.Distance(currentPosition_gridBase, astar.goalNode.position) >= 0.01f)
        {
            isFinish = false;
            Vector3 next = astar.FindPath()[0];
            Vector3 direction = next - currentPosition_gridBase;
            // Debug.Log(direction);

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
                gameManager.textSetter.setDatePerFrame(character);
            }
        }
    }
    // public void Start()
    // {
    //     Debug.Log((Vector3.up + Vector3.left));
    // }
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

        float currentDistance = Vector3.Distance(gameManager.posPlayer, targets[nearestIndex]);

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
            Debug.Log($"Nhân vật đã chết 1: {character.Food}, {character.Drink}, {character.Sleep}, {character.Stress} với step là {count}");
            gameManager.resetTimeLine();
            AddReward(-1f);
            EndEpisode();
        }

        //Thưởng hợp lý theo vị trí hiện tại và trạng thái
        Vector3 pos = gameManager.posPlayer;

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

    private void calcReward_tuDuyTriChiSoSong_phase2(int action){

        // 👣 Nhẹ nhàng giảm trừ mỗi bước (nhỏ hơn để tập trung kéo dài tuổi thọ)
        AddReward(-0.0002f);

        //Chuẩn hóa chỉ số về [0,1]
        float hunger = Mathf.Clamp01(1f - character.Food / 24f);   // hunger ∈ [0,1], lớn = cần ăn
        float thirst = Mathf.Clamp01(1f - character.Drink / 24f);  // thirst ∈ [0,1]
        float tired = Mathf.Clamp01(1f - character.Sleep / 24f);   // tired ∈ [0,1]
        float stress = Mathf.Clamp01(character.Stress / 72f);      // stress ∈ [0,1]

        // Phạt dần theo trạng thái xấu (nhẹ hơn)
        AddReward(-0.0002f * (hunger + thirst + tired + stress));

        // Phạt lớn nếu chạm ngưỡng nguy hiểm
        if (character.Food <= 1f) AddReward(-1f);
        if (character.Drink <= 1f) AddReward(-1f);
        if (character.Sleep <= 1f) AddReward(-1f);
        if (character.Stress >= 70f) AddReward(-1f);

        // --- Reward shaping: khuyến khích đi về mục tiêu cần nhất ---
        // Tìm need cao nhất
        float[] needs = new float[] { hunger, thirst, tired, stress };
        int needIdx = 0;
        float needMax = needs[0];
        for (int i = 1; i < needs.Length; i++)
        {
            if (needs[i] > needMax)
            {
                needMax = needs[i];
                needIdx = i;
                // break;
            }
        }

        Vector3 needTarget = gameManager.posEat;
        if (needIdx == 0) needTarget = gameManager.posEat;   // ăn
        else if (needIdx == 1) needTarget = gameManager.posDrink; // uống
        else if (needIdx == 2) needTarget = gameManager.posSleep; // ngủ
        else if (needIdx == 3) needTarget = gameManager.posStress; // giải trí

        float curNeedDist = Vector3.Distance(gameManager.posPlayer, needTarget);
        if (float.IsInfinity(prevNeedDistance)) prevNeedDistance = curNeedDist;
        // thưởng khi tiến gần, phạt nhẹ khi đi xa
        if (curNeedDist < prevNeedDistance)
        {
            AddReward(0.02f * needMax); // reward tỉ lệ với mức cần thiết
        }
        else
        {
            AddReward(-0.01f * needMax);
        }
        prevNeedDistance = curNeedDist;

        //Nếu nhân vật "chết" (kiệt sức, đói, khát, stress max)
        if (character.Food <= 0f || character.Drink <= 0f ||
            character.Sleep <= 0f || character.Stress >= 72f)
        {
            Debug.Log($"Nhân vật đã chết 2: {character.Food}, {character.Drink}, {character.Sleep}, {character.Stress} với step là {count}");
            gameManager.resetTimeLine();
            AddReward(-1f);
            EndEpisode();
        }

        // Thưởng hợp lý theo vị trí hiện tại và trạng thái (scale nhỏ hơn, dựa trên mức need)
        Vector3 pos = gameManager.posPlayer;

        if (pos == gameManager.posEat)
        {
            if (action == 8)
            {
                AddReward(0.3f * hunger); // thưởng khi dùng đúng action, tỉ lệ với hunger
            }
            if (character.Food < 15f) AddReward(0.5f * hunger + 0.1f);   // ăn khi đói → tốt (càng đói càng thưởng nhiều)
            else AddReward(-0.02f);                                       // ăn khi no → lãng phí
        }
        else if (pos == gameManager.posDrink)
        {
            if (action == 8)
            {
                AddReward(0.3f * thirst);
            }
            if (character.Drink < 15f) AddReward(0.45f * thirst + 0.08f);
            else AddReward(-0.02f);
        }
        else if (pos == gameManager.posSleep)
        {
            if (action == 8)
            {
                AddReward(0.4f * tired);
            }
            if (character.Sleep < 10f) AddReward(0.6f * tired + 0.1f);  // ngủ khi mệt → tốt
            else AddReward(-0.02f);
        }
        else if (pos == gameManager.posWork)
        {
            if (action == 8)
            {
                AddReward(0.15f);
            }
            if (character.Sleep > 10f && character.Food > 10f && character.Drink > 10f)
                AddReward(0.5f);                        // đủ điều kiện làm việc → thưởng vừa phải
            else
                AddReward(-0.02f);                       // làm khi mệt → phạt nhẹ
        }
        else if (pos == gameManager.posStress)
        {
            if (action == 8)
            {
                AddReward(0.25f * stress);
            }
            if (character.Stress > 30f) AddReward(0.5f * stress + 0.05f); // đi xả stress khi stress cao
            else AddReward(-0.05f);                                        // stress thấp mà vẫn đi → lãng phí
        }

        //Thưởng nhỏ khi duy trì trạng thái cân bằng tổng thể
        if (character.Food > 12f && character.Drink > 12f &&
            character.Sleep > 12f && character.Stress < 30f)
        {
            AddReward(0.02f);
        }
    }

    // private int idleSteps = 0;  // Track stand still tại target
    // private float prevMoney = 100f;  // Track money delta
    // private float cycleBonusTimer = 0f;  // Để thưởng cycle hoàn thành

    // private void calcReward_tuDuyTriChiSoSong_phase2(int action)
    // {
    //     // 👣 Penalty mỗi step: Tăng nhẹ để khuyến khích efficiency, scale với time
    //     float timePenalty = -0.0005f * (gameManager.TimeLine / 1440f);
    //     AddReward(timePenalty);

    //     // Chuẩn hóa needs [0,1]
    //     float hunger = Mathf.Clamp01(1f - character.Food / 24f);
    //     float thirst = Mathf.Clamp01(1f - character.Drink / 24f);
    //     float tired = Mathf.Clamp01(1f - character.Sleep / 24f);
    //     float stress = Mathf.Clamp01(character.Stress / 72f);

    //     // Phạt stat xấu: Giảm scale để focus vào shaping (thay vì -0.002 → -0.001)
    //     AddReward(-0.001f * (hunger + thirst + tired + stress));

    //     // Phạt nguy hiểm: Giữ nguyên, nhưng scale với severity
    //     if (character.Food <= 1f) AddReward(-2f * hunger);  // Càng thấp càng phạt nặng
    //     if (character.Drink <= 1f) AddReward(-2f * thirst);
    //     if (character.Sleep <= 1f) AddReward(-2f * tired);
    //     if (character.Stress >= 70f) AddReward(-2f * stress);

    //     // --- Reward shaping cho need target (cải thiện: cap reward, thêm potential) ---
    //     float[] needs = { hunger, thirst, tired, stress };
    //     int needIdx = 0; float needMax = needs[0];
    //     for (int i = 1; i < needs.Length; i++)
    //     {
    //         if (needs[i] > needMax) { needMax = needs[i]; needIdx = i; }
    //     }

    //     Vector3 needTarget;
    //     switch (needIdx)
    //     {
    //         case 0: needTarget = gameManager.posEat; break;
    //         case 1: needTarget = gameManager.posDrink; break;
    //         case 2: needTarget = gameManager.posSleep; break;
    //         default: needTarget = gameManager.posStress; break;  // stress
    //     }

    //     float curNeedDist = Vector3.Distance(gameManager.posPlayer, needTarget);
    //     if (float.IsInfinity(prevNeedDistance)) prevNeedDistance = curNeedDist;

    //     // Shaping: Thưởng progress, nhưng cap = needMax * 0.01 để tránh dominate
    //     float distReward = (prevNeedDistance - curNeedDist) * 0.05f * needMax;  // Tăng scale nhẹ
    //     distReward = Mathf.Clamp(distReward, -0.02f, 0.03f);  // Cap để smooth
    //     AddReward(distReward);
    //     prevNeedDistance = curNeedDist;

    //     // Death check: Phạt nặng hơn nếu chết sớm (scale với steps)
    //     if (character.Food <= 0f || character.Drink <= 0f || character.Sleep <= 0f || character.Stress >= 72f)
    //     {
    //         float deathPenalty = -5f - (count / 1000f);  // -5 đến -6, nặng hơn nếu chết sớm
    //         Debug.Log($"Chết phase2: Stats {character.Food:F1},{character.Drink:F1},{character.Sleep:F1},{character.Stress:F1} at step {count}");
    //         gameManager.resetTimeLine();
    //         AddReward(deathPenalty);
    //         EndEpisode();
    //         return;
    //     }

    //     // Track idle và money
    //     Vector3 pos = gameManager.posPlayer;
    //     bool atTarget = (pos == needTarget);
    //     if (action == 8 && atTarget) idleSteps++; else idleSteps = 0;  // Reset nếu move

    //     float moneyDelta = character.Money - prevMoney;
    //     prevMoney = character.Money;

    //     // Phạt idle quá lâu: -0.01 per extra step sau 5 steps
    //     if (idleSteps > 5) AddReward(-0.01f * (idleSteps - 5));

    //     // --- Action & Position rewards (cải thiện: scale với need + money, thêm work bonus) ---
    //     if (pos == gameManager.posEat)
    //     {
    //         float eatReward = (character.Food < 15f) ? 0.6f * hunger : -0.05f;
    //         if (action == 8) eatReward += 0.4f * hunger;  // Bonus stand
    //         AddReward(eatReward);
    //         cycleBonusTimer = 0f;  // Reset nếu đang satisfy need
    //     }
    //     else if (pos == gameManager.posDrink)
    //     {
    //         float drinkReward = (character.Drink < 15f) ? 0.55f * thirst : -0.05f;
    //         if (action == 8) drinkReward += 0.35f * thirst;
    //         AddReward(drinkReward);
    //     }
    //     else if (pos == gameManager.posSleep)
    //     {
    //         float sleepReward = (character.Sleep < 10f) ? 0.7f * tired : -0.05f;
    //         if (action == 8) sleepReward += 0.5f * tired;
    //         AddReward(sleepReward);
    //     }
    //     else if (pos == gameManager.posWork)
    //     {
    //         // Cải thiện: Chỉ reward nếu đủ stats VÀ money low (ưu tiên earn khi cần)
    //         float workCond = (character.Sleep > 10f && character.Food > 10f && character.Drink > 10f) ? 1f : 0f;
    //         float moneyNeed = Mathf.Clamp01(1f - character.Money / 200f);  // Need money nếu <200
    //         float workReward = 0.8f * workCond * moneyNeed + 0.1f * moneyDelta;  // + delta money
    //         if (action == 8) workReward += 0.2f * moneyNeed;
    //         if (workCond < 1f) workReward -= 0.3f;  // Phạt mạnh hơn nếu mệt
    //         AddReward(workReward);
    //     }
    //     else if (pos == gameManager.posStress)
    //     {
    //         float stressReward = (character.Stress > 30f) ? 0.6f * stress : -0.08f;
    //         if (action == 8) stressReward += 0.3f * stress;
    //         AddReward(stressReward);
    //     }
    //     else
    //     {
    //         // Phạt nhẹ nếu không tại target (khuyến khích purposeful movement)
    //         AddReward(-0.005f);
    //     }

    //     // Cycle bonus: Thưởng nếu satisfy need trong <10 steps (hiệu quả)
    //     if (atTarget && needMax < 0.2f)
    //     {  // Need đã thấp
    //         cycleBonusTimer += 1f;
    //         if (cycleBonusTimer >= 10f)
    //         {  // Hoàn thành cycle
    //             AddReward(1f);  // Bonus lớn cho full cycle
    //             cycleBonusTimer = 0f;
    //         }
    //     }

    //     // Balance bonus: Tăng nhẹ nếu all good
    //     if (character.Food > 12f && character.Drink > 12f && character.Sleep > 12f && character.Stress < 30f)
    //     {
    //         AddReward(0.005f);  // Tăng nhẹ để encourage maintain
    //     }

    //     // Money bonus/penalty: Thưởng nếu tăng, phạt nếu giảm (nếu có chi tiêu)
    //     if (moneyDelta > 0) AddReward(0.2f * (moneyDelta / 10f));  // Scale với gain
    //     else if (character.Money < 10f) AddReward(-0.5f);  // Phá sản
    // }

}
