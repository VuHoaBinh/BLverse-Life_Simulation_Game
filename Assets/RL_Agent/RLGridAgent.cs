using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using System.Collections;

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

    public override void OnEpisodeBegin()
    {
        bool isInitial = false;
        int maxAttempts = 100; // tránh loop vô hạn
        int attempts = 0;
        Vector3 startCell;
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
        character.Sleep = initSleep;
        character.Food = initFood;
        character.Drink = initDrink;
        character.Stress = initStress;
        character.Money = initMoney;
        gameManager.resetTimeLine();
        int randomTarget = rand.Next(0, 5);
        // Debug.Log($"Random ra {randomTarget}");
        // target = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(gameManager.listLocations[randomTarget].position);
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
        sensor.AddObservation(agentCell.x);
        sensor.AddObservation(agentCell.y);
        //Các chỉ số
        sensor.AddObservation(character.Sleep);
        sensor.AddObservation(character.Food);
        sensor.AddObservation(character.Drink);
        sensor.AddObservation(character.Stress);
        sensor.AddObservation(character.Money);

        //Vị trí của Bếp
        sensor.AddObservation(gameManager.listLocations[0].position); //Tuyệt đối
        sensor.AddObservation(gameManager.listLocations[0].position - agentCell); //Tương đối

        //Vị trí tủ lạnh
        sensor.AddObservation(gameManager.listLocations[1].position); //Tuyệt đối
        sensor.AddObservation(gameManager.listLocations[1].position - agentCell); //Tương đối

        //Vị trí của Sofa
        sensor.AddObservation(gameManager.listLocations[2].position); //Tuyệt đối
        sensor.AddObservation(gameManager.listLocations[2].position - agentCell); //Tương đối

        //Vị trí của Cửa
        sensor.AddObservation(gameManager.listLocations[3].position); //Tuyệt đối
        sensor.AddObservation(gameManager.listLocations[3].position - agentCell); //Tương đối

        //Vị trí của Giường
        sensor.AddObservation(gameManager.listLocations[4].position); //Tuyệt đối
        sensor.AddObservation(gameManager.listLocations[4].position - agentCell); //Tương đối

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

        // Lấy vị trí hiện tại
        Vector3 currentCell = character.transform.position;
        Vector3 nextCell = currentCell;

        switch (action)
        {
            case 0: nextCell += Vector3.up; break;                  // Lên
            case 1: nextCell += Vector3.down; break;                // Xuống
            case 2: nextCell += Vector3.left; break;                // Trái
            case 3: nextCell += Vector3.right; break;               // Phải
            case 4: nextCell += (Vector3.up + Vector3.left).normalized; break;    // Lên - Trái
            case 5: nextCell += (Vector3.up + Vector3.right).normalized; break;   // Lên - Phải
            case 6: nextCell += (Vector3.down + Vector3.left).normalized; break;  // Xuống - Trái
            case 7: nextCell += (Vector3.down + Vector3.right).normalized; break; // Xuống - Phải
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
            //     gameManager.posEat, gameManager.posDrink,
            //     gameManager.posSleep, gameManager.posStress,
            //     gameManager.posWork);
            calcReward_tuDuyTriChiSoSong();
        }
        else
        {
            gameManager.calcStat_noSpace();
            calcReward_tuDuyTriChiSoSong();
            Debug.Log("Nhân vật đã va vào tường");
            AddReward(-0.2f);
        }
    }
    private void calcReward_tuDuyTriChiSoSong()
    {
        // 👣 Phạt nhẹ mỗi bước để tránh đi lung tung
        AddReward(-0.002f);

        //Chuẩn hóa chỉ số về [0,1]
        float hunger = Mathf.Clamp01(1f - character.Food / 24f);
        float thirst = Mathf.Clamp01(1f - character.Drink / 24f);
        float tired = Mathf.Clamp01(1f - character.Sleep / 24f);
        float stress = Mathf.Clamp01(character.Stress / 72f);

        //Phạt dần theo trạng thái xấu
        AddReward(-0.01f * (hunger + thirst + tired + stress));

        //Phạt mạnh nếu chạm ngưỡng nguy hiểm
        if (character.Food <= 2f) AddReward(-1f);
        if (character.Drink <= 2f) AddReward(-1f);
        if (character.Sleep <= 2f) AddReward(-1f);
        if (character.Stress >= 60f) AddReward(-1f);

        //Nếu nhân vật "chết" (kiệt sức, đói, khát, stress max)
        if (character.Food <= 0f || character.Drink <= 0f ||
            character.Sleep <= 0f || character.Stress >= 72f)
        {
            Debug.Log("Nhân vật đã chết");
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




    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.UpArrow)) d[0] = 0;
        else if (Input.GetKey(KeyCode.DownArrow)) d[0] = 1;
        else if (Input.GetKey(KeyCode.LeftArrow)) d[0] = 2;
        else if (Input.GetKey(KeyCode.RightArrow)) d[0] = 3;
        else if (Input.GetKey(KeyCode.Space)) d[0] = 8;
    }

}
