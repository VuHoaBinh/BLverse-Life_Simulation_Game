using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveLog : MonoBehaviour
{
    [Header("Tham chiếu đến 2 nhân vật")]
    public Character char1;
    public Character char2;

    [Header("Cấu hình log")]
    public float logInterval = 0.02f;  // log mỗi 1 giây game
    private float timer = 0f;

    private List<string> lines = new List<string>();

    private void Start()
    {
        // Header của CSV
        lines.Add("time,char1_food,char1_drink,char1_sleep,char1_stress,char2_food,char2_drink,char2_sleep,char2_stress");
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= logInterval)
        {
            LogStats();
            timer = 0f;
        }
    }

    string FormatChar(Character c)
    {
        if (c.isDeath)
            return "NaN,NaN,NaN,NaN";
        return $"{c.Food},{c.Drink},{c.Sleep},{c.Stress}";
    }
    void LogStats()
    {
        string c1 = FormatChar(char1);
        string c2 = FormatChar(char2);

        string line = $"{Time.time:F2},{c1},{c2}";
        lines.Add(line);
    }


    private void OnApplicationQuit()
    {
        SaveCSV();
    }

    public void SaveCSV()
    {
        string path = Application.dataPath + "/stats_log.csv";
        File.WriteAllLines(path, lines);
        Debug.Log("Đã lưu CSV vào: " + path);
    }
}
