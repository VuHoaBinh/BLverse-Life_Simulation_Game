using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        this.currentEpisode = 0;
        this.stepCount = 0;
    }
    /*
    Bắt đầu record
*/
    private bool isDonePre;
    public void addStep(TrajectoryStep trajectoryStep)
    {
        if (isDonePre)
        {
            this.currentEpisode += 1;
            this.stepCount = 0;
            isDonePre = false;
        }
        if (trajectoryStep.isDone)
        {
            isDonePre = true;
        }
        trajectorySteps.Add(trajectoryStep);
        trajectoryStep.episodeIndex = currentEpisode;
        trajectoryStep.stepIndex = stepCount++;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("episode,step");
        for (int i = 0; i <= 42; i++) sb.Append($", state_{i}");
        sb.AppendLine(",action,reward,done");

        foreach (var step in trajectorySteps)
        {
            sb.Append($"{step.episodeIndex}, {step.stepIndex}");
            if (step.states != null && step.states.Length >= 43)
            {
                for (int i = 0; i <= 42; i++) sb.Append($",{step.states[i]}");
            }
            else
            {
                for (int i = 0; i <= 42; i++) sb.Append(",0.0");
            }
            sb.Append($",{step.action},{step.reward},{step.isDone}");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
