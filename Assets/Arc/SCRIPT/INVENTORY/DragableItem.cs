using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class DragableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Item item;
    public Image image;
    [HideInInspector] public Transform afterDrag;

    private void Awake()
    {
        // Nếu chưa gán trong Inspector thì tự lấy
        if (image == null)
            image = GetComponent<Image>();
        // IntializeItem(item);
    }
    public void IntializeItem(Item newItem)
    {
        this.item = newItem;
        this.image.sprite = newItem.image;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Bắt đầu kéo!");
        afterDrag = transform.parent;
        Debug.Log(transform.parent);
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Đang kéo!");
        this.transform.position = Input.mousePosition;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Hết kéo!");
        transform.SetParent(afterDrag);
        image.raycastTarget = true;
    }
}
