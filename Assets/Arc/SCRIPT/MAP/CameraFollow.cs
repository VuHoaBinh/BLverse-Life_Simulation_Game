using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // Nhân vật cần camera theo dõi
    public float smoothSpeed = 0.125f; // Độ mượt khi bám theo
    public Vector3 offset;         // Độ lệch vị trí giữa camera và nhân vật

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, -10f); // Đặt camera ở phía sau nhân vật
    }
}
