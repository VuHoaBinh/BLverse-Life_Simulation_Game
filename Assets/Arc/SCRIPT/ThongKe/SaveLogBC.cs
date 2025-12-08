using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveLogBC : MonoBehaviour
{
    // Start is called before the first frame update
    private string csvPath;
    void Start()
    {
        // Tạo đường dẫn file
        csvPath = Application.dataPath + "/actions.csv";

        // Nếu file chưa tồn tại → tạo header
        if (!File.Exists(csvPath))
        {
            File.WriteAllText(csvPath,
                "PosX,PosY," +
                "Sleep,Food,Drink,Stress,Money," +
                "DistKitchen,DistFridge,DistSofa,DistDoor,DistBed," +
                "Timeline," +
                "MoveAction,InteractAction\n"
            );
        }
    }

    // Update is called once per frame
    public void LogAction(int moveAction, int interactAction, Character character, GameManager gameManager)
    {
        Vector3 pos = character.transform.position;

        float sleep = character.Sleep;
        float food = character.Food;
        float drink = character.Drink;
        float stress = character.Stress;
        int money = character.Money;

        float distKitchen = Vector3.Distance(pos, gameManager.listLocations[0].position);
        float distFridge = Vector3.Distance(pos, gameManager.listLocations[1].position);
        float distSofa = Vector3.Distance(pos, gameManager.listLocations[2].position);
        float distDoor = Vector3.Distance(pos, gameManager.listLocations[3].position);
        float distBed = Vector3.Distance(pos, gameManager.listLocations[4].position);

        float timeline = gameManager.TimeLine;

        // ----- MOVE & INTERACT ACTION ĐÃ ĐƯA XUỐNG CUỐI -----
        string line =
            pos.x + "," +
            pos.y + "," +
            sleep + "," +
            food + "," +
            drink + "," +
            stress + "," +
            money + "," +
            distKitchen + "," +
            distFridge + "," +
            distSofa + "," +
            distDoor + "," +
            distBed + "," +
            timeline + "," +
            moveAction + "," +
            interactAction + "\n";

        File.AppendAllText(csvPath, line);
    }

}
