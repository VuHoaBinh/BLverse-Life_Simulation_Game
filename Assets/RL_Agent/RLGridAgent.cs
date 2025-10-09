using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class GridBrain : Agent
{
    public GameManager gameManager;
    public Character character;
    public Vector3 targetCell;   // Mục tiêu keos trên UI

    public override void OnEpisodeBegin()
    {
        // Reset nhân vật về ô random
        Vector3 startCell = new Vector3(-3.5f, -2.5f, 0);
        Debug.Log("!!!" + startCell);
        character.transform.position = startCell;
        character.rb.velocity = Vector2.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // character.StartMove(new Vector3(-12.5f, -2.5f, 0));
        Debug.Log("Chay day chua!!!");
        // Vị trí agent (chuẩn hóa)
        Vector3 agentCell = character.transform.position;
        sensor.AddObservation(agentCell.x);
        sensor.AddObservation(agentCell.y);

        // Vị trí mục tiêu (chuẩn hóa)
        sensor.AddObservation(targetCell.x);
        sensor.AddObservation(targetCell.y);
        // Các thông tin khác có thể thêm: máu, thức ăn, thời gian, ...
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("Chay day chua!!!!!!!");
        int action = actions.DiscreteActions[0];

        // Lấy vị trí hiện tại
        Vector3 currentCell = character.transform.position;
        Vector3 nextCell = currentCell;

        // 0 = lên, 1 = xuống, 2 = trái, 3 = phải
        switch (action)
        {
            case 0: nextCell += Vector3.up; break;
            case 1: nextCell += Vector3.down; break;
            case 2: nextCell += Vector3.left; break;
            case 3: nextCell += Vector3.right; break;
        }
        // Check hợp lệ (không đi ra ngoài)
        Debug.Log("????");
        if (gameManager.map.checkTileAtPosition(nextCell))
        {
            Vector3 worldTarget = nextCell;
            character.StartMove(worldTarget);
        }
        // Reward
        float distBefore = Vector3.Distance(currentCell, targetCell);
        float distAfter = Vector3.Distance(nextCell, targetCell);
        // Thưởng nếu tiến gần đích
        AddReward((distBefore - distAfter) * 0.1f);
        // Thưởng lớn nếu đến đích
        if (nextCell == targetCell)
        {
            SetReward(1f);
            EndEpisode();
        }
        // Phạt nhẹ mỗi bước để tránh vòng lặp vô hạn
        AddReward(-0.01f);
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
