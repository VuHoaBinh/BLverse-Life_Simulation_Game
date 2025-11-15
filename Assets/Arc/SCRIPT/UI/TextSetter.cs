using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextSetter : MonoBehaviour
{
    //Truyền vào 1 danh sách các Object
    //Gán giá trị cho từng text theo mỗi frame
    [SerializeField] private List<TextInfomation> listInfo;

    public void setDatePerFrame(Character character)
    {
        //Gán
        listInfo[0].value = character.Food.ToString("F1");
        // Debug.Log("Food: " + character.Food.ToString("F1"));
        listInfo[1].value = character.Drink.ToString("F1");
        listInfo[2].value = character.Stress.ToString("F1");
        listInfo[3].value = character.Money.ToString("F1");
        listInfo[4].value = character.Sleep.ToString("F1");

        //Đưa lên UI
        listInfo[0].setInfoInUi(character.Food.ToString("F1"));
        listInfo[1].setInfoInUi(character.Drink.ToString("F1"));
        listInfo[2].setInfoInUi(character.Stress.ToString("F1"));
        listInfo[3].setInfoInUi(character.Money.ToString("F1"));
        listInfo[4].setInfoInUi(character.Sleep.ToString("F1"));
    }
}
