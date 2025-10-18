using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryCollector : MonoBehaviour
{
    public List<TrajectoryStep> trajectorySteps;
    public bool isRecording;
    public int currentEpisode;
    public int stepCount;

    //Khởi tạo danh sách
    public TrajectoryCollector()
    {
        trajectorySteps = new List<TrajectoryStep>();
    }
    /*
    Bắt đầu record
*/
    public void addStep(TrajectoryStep trajectoryStep)
    {
        trajectorySteps.Add(trajectoryStep);
    }
}
