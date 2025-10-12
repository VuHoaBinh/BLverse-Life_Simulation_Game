using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.WSA;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

public class GridBrain : Agent
{
    public GameManager gameManager;
    public Character character;
    public Vector3 targetCell;   // Mục tiêu keos trên UI



    public override void OnEpisodeBegin()
    {
        // Reset nhân vật về ô random
        Vector3 startCell = new Vector3(12.5f, -2.5f, 0);
        character.sleep = 24;
        character.food = 24;
        character.drink = 24;
        character.stress = 0;
        character.money = 100;

        character.transform.position = startCell;
        character.rb.velocity = Vector2.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // character.StartMove(new Vector3(-12.5f, -2.5f, 0));
        Vector3 agentCell = character.transform.position;
        sensor.AddObservation(agentCell.x);
        sensor.AddObservation(agentCell.y);

        //Các chỉ số
        sensor.AddObservation(character.sleep);
        sensor.AddObservation(character.food);
        sensor.AddObservation(character.drink);
        sensor.AddObservation(character.stress);
        sensor.AddObservation(character.money);



        // Vị trí mục tiêu (chuẩn hóa)
        sensor.AddObservation(targetCell.x);
        sensor.AddObservation(targetCell.y);

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

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        // Lấy vị trí hiện tại
        Vector3 currentCell = character.transform.position;
        Vector3 nextCell = currentCell;
        Debug.Log(action + "," + nextCell);
        // 0 = lên, 1 = xuống, 2 = trái, 3 = phải, 4 đứng im
        switch (action)
        {
            case 0: nextCell += Vector3.up; break;
            case 1: nextCell += Vector3.down; break;
            case 2: nextCell += Vector3.left; break;
            case 3: nextCell += Vector3.right; break;
            case 4: nextCell += Vector3.zero; break;
        }
        Vector2Int key = new Vector2Int((int)(nextCell.x - 0.5f), (int)(nextCell.y - 0.5f));
        gameManager.map.TilePositions.TryGetValue(key, out bool hasTile);
        gameManager.map.TilePositions.TryGetValue(new Vector2Int(4, -1), out bool hasTile_t);
        // Check hợp lệ (không đi ra ngoài)
        if (!hasTile && gameManager.map.TilePositions.ContainsKey(key))
        {
            Vector3 worldTarget = nextCell;
            character.transform.position = worldTarget;
            Vector3Int posPlayer = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(character.transform.position);
            Vector3Int posEat = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(gameManager.listLocations[0].position);
            Vector3Int posDrink = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(gameManager.listLocations[1].position);
            Vector3Int posStress = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(gameManager.listLocations[2].position);
            Vector3Int posWork = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(gameManager.listLocations[3].position);
            Vector3Int posSleep = gameManager.map.GetComponentInChildren<Tilemap>().WorldToCell(gameManager.listLocations[4].position);
            character.food -= (1f / 18f);
            Debug.Log("Check giá trị" + (1f / 18f));
            character.drink -= (1f / 18f);
            character.sleep -= (1f / 6f);
            if (character.food < 12 || character.drink < 12)
            {
                character.stress += 1.5f;
            }
            if (posPlayer == posEat && character.money >= 15)
            {
                character.food += 8;
                character.money -= 15;
            }
            if (posPlayer == posDrink && character.money >= 5)
            {
                character.drink += 4;
                character.money -= 5;
            }
            if (posPlayer == posSleep)
            {
                character.sleep += 3;
                character.stress -= 0.5f;
            }
            if (posPlayer == posWork)
            {
                character.money += 25;
                character.food -= ((1f / 18f) / 2f);
                character.drink -= ((1f / 18f) / 2f);
                character.stress += 2;
            }
            if (posPlayer == posStress && character.stress <= 72)
            {
                character.stress -= 1f;
            }
        }
        else
        {
            Debug.Log("Fix Vật cản");
        }

        // Rule (có thể sai số)
        /*
            mỗi lần ở ô ngủ +10 ngủ
            cứ mỗi bước trừ độ đói 2 / khát 1 / ngủ 4
            nếu không làm gì thì stress giảm 1/lần
            mỗi lần ăn tiền trừ đi 15(+25 ăn), uống trừ 10(+25 khát)
            mỗi lần đói/khát dưới 50 tăng stress 1 điểm mỗi bước
            mỗi lần đi làm kiếm được 5đ/mỗi bước đứng ở ô đi làm
                + 10 điểm stress mỗi bước
                - 1 đói/khát mỗi bước
            mỗi bước ở ô sofa -0.5đ stress

            Chết thì quay lại giường
                Nếu đói/khát < 0 thì chết
                Nếu tress > 100 thì chết
                Nếu ngủ < 0 thì chết
        */
        // Reward 
        /*
            Nếu mức độ đói dưới 50 thì phạt ??
                Mỗi bước nếu độ đói trên 50 thưởng ??
            Nếu mức độ khát dưới 50 thì phạt ??
                Mỗi bước nếu độ khát trên 50 thưởng ??
            Nếu stress trên 50 thì phạt ??
            Nếu stress trên 80 thì phạt ??
            Nếu 
        */
        // Game over
        if (character.food <= 0 || character.drink <= 0 || character.stress >= 72 || character.sleep < 0)
        {
            AddReward(-1f);
            EndEpisode();
        }
        //Game still run
        if (character.food >= 12 || character.drink >= 12)
        {
            AddReward(0.5f);
        }
        else
        {
            AddReward(-0.5f);
        }
        if (character.sleep >= 12)
        {
            AddReward(0.5f);
        }
        else
        {
            AddReward(-0.5f);
        }
        if (character.stress <= 36)
        {
            AddReward(0.5f);
        }
        else
        {
            AddReward(-0.5f);
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.UpArrow)) d[0] = 0;
        else if (Input.GetKey(KeyCode.DownArrow)) d[0] = 1;
        else if (Input.GetKey(KeyCode.LeftArrow)) d[0] = 2;
        else if (Input.GetKey(KeyCode.RightArrow)) d[0] = 3;
    }
}
