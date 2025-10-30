using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class TrajectoryStep
{
    public int episodeIndex;
    public int stepIndex; //Để nói step này là step số mấy trong espisode
    public float[] state;
    public int action; //Hành động đã được chuyển thành dạng số
    public float reward;
    public bool isDone = false; //Để nói step này kết thúc chưa
    public TrajectoryStep(Vector3 direction, Character character, GameManager gameManager)
    {
        this.state = new float[43];
        CollectState(character, gameManager);
        this.action = changeDirectionToNumber(direction);
        calcReward_forTrajectoriesPerStep(character, gameManager, action);
    }
    public int changeDirectionToNumber(Vector3 direction)
    {
        if (direction == Vector3.up) return 0;                  // Lên
        if (direction == Vector3.down) return 1;               // Xuống
        if (direction == Vector3.left) return 2;                // Trái
        if (direction == Vector3.right) return 3;               // Phải
        if (direction == (Vector3.up + Vector3.left)) return 4;    // Lên - Trái
        if (direction == (Vector3.up + Vector3.right)) return 5;   // Lên - Phải
        if (direction == (Vector3.down + Vector3.left)) return 6;  // Xuống - Trái
        if (direction == (Vector3.down + Vector3.right)) return 7; // Xuống - Phải
        if (direction == Vector3.zero) return 8;                // Đứng yên
        return -1;
    }
    private void calcReward_forTrajectoriesPerStep(Character character, GameManager gameManager, int action)
    {
        if (character.Food <= 0 || character.Drink <= 0 || character.Stress >= 72 || character.Sleep <= 0)
        {
            this.reward += -1;
            this.isDone = true;
        }
        //Game still run

        //Hạn chế đứng yên không cần thiết
        if (action == 8 && (gameManager.posPlayer != gameManager.posEat
        || gameManager.posPlayer != gameManager.posEat
        || gameManager.posPlayer != gameManager.posDrink
        || gameManager.posPlayer != gameManager.posSleep
        || gameManager.posPlayer != gameManager.posStress
        || gameManager.posPlayer != gameManager.posWork))
        {
            this.reward += -0.05f;
        }
        //Khuyến khích tìm được chỗ để thực hiện hành vi
        if ((gameManager.posPlayer == gameManager.posEat
        || gameManager.posPlayer == gameManager.posDrink
        || gameManager.posPlayer == gameManager.posSleep
        || gameManager.posPlayer == gameManager.posStress
        || gameManager.posPlayer == gameManager.posWork) && action == 8)
        {
            this.reward += 0.06f;
        }
        //Khuyến khích nên duy trì chỉ số ăn , uống ở mức hợp lý
        if (character.Food >= 12 || character.Drink >= 12)
        {
            this.reward += 0.05f;
        }
        else
        {
            this.reward += -0.05f;
        }

        /*
            - Không nên ngủ vào buổi sáng
            - Nên đi làm vào lúc 8h30 sáng và về lúc 17h chiều ()
            - Nên ăn đủ 3 bữa 1 ngày
        */

        /* 
            - các mốc giờ
                + 0 - 360 : 0h - 6h sáng
                + 360 - 720: 6h - 12h trưa
                + 720 - 1080: 12h trưa đến 6h chiều
                + 1080 - 1440: 6h chiều đến 0h khuya

        */
        // Nên ngủ lúc 9h - 11h30
        if (gameManager.posPlayer == gameManager.posSleep && gameManager.TimeLine >= 1260 && gameManager.TimeLine <= 1410)
        {
            this.reward += 0.05f;
        }
        else
        {
            this.reward -= 0.1f;
        }
        //Nên đi làm lúc 8h30 sáng
        if (gameManager.posPlayer == gameManager.posWork && gameManager.TimeLine == 550)
        {
            this.reward += 0.05f;
        }
        else
        {
            this.reward -= 0.1f;
        }

        // Khuyến khích ngủ đủ giấc
        if (character.Sleep >= 12)
        {
            this.reward += 0.05f;
        }
        else
        {
            this.reward += -0.05f;
        }
        //Khuyến khích không nên để stress quá cao
        if (character.Stress <= 36)
        {
            this.reward += 0.05f;
        }
        else
        {
            this.reward += -0.05f;
        }
    }
    public void CollectState(Character character, GameManager gameManager)
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
            - [42]: thời gian trong ngày (1440 => 60step mất 1 tiếng)    
        */
        Vector3 agentCell = character.transform.position;

        // [0], [1]: tọa độ x, y của nhân vật
        this.state[0] = agentCell.x;
        this.state[1] = agentCell.y;

        // [2] - [6]: chỉ số trạng thái nhân vật
        this.state[2] = character.Sleep;
        this.state[3] = character.Food;
        this.state[4] = character.Drink;
        this.state[5] = character.Stress;
        this.state[6] = character.Money;

        // Bếp: [7..12]
        Vector3 posKitchen = gameManager.listLocations[0].position;
        this.state[7] = posKitchen.x;
        this.state[8] = posKitchen.y;
        this.state[9] = posKitchen.z;
        this.state[10] = posKitchen.x - agentCell.x;
        this.state[11] = posKitchen.y - agentCell.y;
        this.state[12] = posKitchen.z - agentCell.z;

        // Tủ lạnh: [13..18]
        Vector3 posFridge = gameManager.listLocations[1].position;
        this.state[13] = posFridge.x;
        this.state[14] = posFridge.y;
        this.state[15] = posFridge.z;
        this.state[16] = posFridge.x - agentCell.x;
        this.state[17] = posFridge.y - agentCell.y;
        this.state[18] = posFridge.z - agentCell.z;

        // Sofa: [19..24]
        Vector3 posSofa = gameManager.listLocations[2].position;
        this.state[19] = posSofa.x;
        this.state[20] = posSofa.y;
        this.state[21] = posSofa.z;
        this.state[22] = posSofa.x - agentCell.x;
        this.state[23] = posSofa.y - agentCell.y;
        this.state[24] = posSofa.z - agentCell.z;

        // Cửa: [25..30]
        Vector3 posDoor = gameManager.listLocations[3].position;
        this.state[25] = posDoor.x;
        this.state[26] = posDoor.y;
        this.state[27] = posDoor.z;
        this.state[28] = posDoor.x - agentCell.x;
        this.state[29] = posDoor.y - agentCell.y;
        this.state[30] = posDoor.z - agentCell.z;

        // Giường: [31..36]
        Vector3 posBed = gameManager.listLocations[4].position;
        this.state[31] = posBed.x;
        this.state[32] = posBed.y;
        this.state[33] = posBed.z;
        this.state[34] = posBed.x - agentCell.x;
        this.state[35] = posBed.y - agentCell.y;
        this.state[36] = posBed.z - agentCell.z;

        // [37..41]: khoảng cách tới các điểm
        this.state[37] = Vector3.Distance(agentCell, posKitchen);
        this.state[38] = Vector3.Distance(agentCell, posFridge);
        this.state[39] = Vector3.Distance(agentCell, posSofa);
        this.state[40] = Vector3.Distance(agentCell, posDoor);
        this.state[41] = Vector3.Distance(agentCell, posBed);

        // [42]: thời gian trong ngày (ví dụ)
        this.state[42] = gameManager.TimeLine;
    }
}
