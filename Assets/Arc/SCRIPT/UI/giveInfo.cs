using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GiveInfo : MonoBehaviour
{
    public Character character;
    public void setName(String name)
    {
        this.transform.Find("Name").GetComponent<TextMeshProUGUI>().SetText(name);
    }
    public void setCount(int count)
    {
        this.transform.Find("Count").GetComponent<TextMeshProUGUI>().SetText(count.ToString());
    }
    public void highLightName()
    {
        this.transform.Find("Name").GetComponent<TextMeshProUGUI>().color = Color.red;
    }
    public void unHighLightName()
    {
        this.transform.Find("Name").GetComponent<TextMeshProUGUI>().color = Color.white;
    }
}
