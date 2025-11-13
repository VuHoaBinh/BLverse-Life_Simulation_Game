using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextInfomation : MonoBehaviour
{
    [SerializeField] private String name { get; set; }
    [SerializeField] public String value { get; set; }

    public void setInfoInUi(String value)
    {
        this.GetComponent<TextMeshProUGUI>().text = value;
    }
}
