using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class TrajectoryStep
{
    public float[] state;
    public float[] action;
    public float reward;
    public bool isDone; //Để nói step này kết thúc chưa
    public int episodeID; //Để nói step này là thuộc episode số mấy
    public int stepIndex; //Để nói step này là step số mấy trong espisode
    public TrajectoryStep(float[] state, float[] action, bool isDone, int episodeID, int stepIndex)
    {
        this.state = state;
        this.action = action;
        this.isDone = isDone;
        this.episodeID = episodeID;
        this.stepIndex = stepIndex;
    }
    // public float calcReward(float[] state, float[] action)
    // {
    //     if()
    //     return 0;
    // }
}
