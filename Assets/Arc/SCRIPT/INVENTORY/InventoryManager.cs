using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public GameObject invItemPrefab;
    public void addItem(Item item)
    {
        for (int index = 0; index < inventorySlots.Length; index++)
        {
            InventorySlot slot = inventorySlots[index];
            DragableItem itemInSlot = slot.GetComponentInChildren<DragableItem>();
            if (itemInSlot == null)
            {
                spawnNewItem(item, slot);
                return;
            }
        }
    }
    public void spawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItemGo = Instantiate(this.invItemPrefab, slot.transform);
        DragableItem dragableItem = newItemGo.GetComponent<DragableItem>();
        dragableItem.IntializeItem(item);
    }
}
