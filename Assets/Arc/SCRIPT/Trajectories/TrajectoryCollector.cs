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
        sb.AppendLine($"=== Trajectory Collector ===");
        sb.AppendLine($"Số episode hiện tại: {currentEpisode}");
        sb.AppendLine($"Tổng số step: {trajectorySteps.Count}");
        sb.AppendLine("----------------------------");

        foreach (var step in trajectorySteps)
        {
            sb.AppendLine($"Episode {step.episodeIndex}, Step {step.stepIndex} | Action: {step.action} | Reward: {step.reward} | Done: {step.isDone}");
        }

        return sb.ToString();
    }
}
