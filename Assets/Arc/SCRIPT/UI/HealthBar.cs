using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fillHP;
    public void setHP(float newHP)
    {
        slider.value = newHP;
        fillHP.color = gradient.Evaluate(slider.normalizedValue);
    }
    // public resetHP();
    // public setHPMax();
}
