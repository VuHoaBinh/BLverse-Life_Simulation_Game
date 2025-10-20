using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapUI : MonoBehaviour
{
    // Update is called once per frame
    public Camera mainCamera;
    public GameObject box;

    public GameObject dot;
    public void drawCircle(Vector3 cellPos)
    {
        Instantiate(dot, cellPos, Quaternion.identity);
    }
    public void drawBox(Vector3 cellPos)
    {
        Instantiate(box, cellPos, Quaternion.identity);
    }

    public void deleteCircle(String name)
    {
        GameObject[] dots = GameObject.FindGameObjectsWithTag(name);
        if (dots == null || dots.Length == 0)
        {
            return; // Không có đối tượng nào để xóa
        }

        foreach (GameObject dot in dots)
        {
            Destroy(dot);
        }
    }

}
