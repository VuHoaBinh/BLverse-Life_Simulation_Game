using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(menuName = "Scriptable object/Item")]
public class Item : ScriptableObject
{



    [Header("Only gameplay")]
    public Sprite image;
    public ItemType itemType;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5, 4);
    [Header("Only UI")]

    public bool stackable = true;

    [Header("Both")]
    public TileBase tile;
}

public enum ItemType
{
    Food,
    Drink
}
public enum ActionType
{
    Eat,
    Drink
}
