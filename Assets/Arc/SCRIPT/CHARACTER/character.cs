using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;
    public float speed = 5f;
    public bool isMoving = false;

    private Queue<Vector3> pathQueue = new Queue<Vector3>();
    private Vector3 currentTargetNode;

    void FixedUpdate()
    {
        if (!isMoving || pathQueue.Count == 0) return;

        // Di chuyển tới node hiện tại
        Vector2 direction = (currentTargetNode - player.transform.position).normalized;
        Vector2 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Kiểm tra đã đến node chưa
        if (Vector2.Distance(player.transform.position, currentTargetNode) < 0.1f)
        {
            if (pathQueue.Count > 0)
            {
                currentTargetNode = pathQueue.Dequeue(); // Lấy node tiếp theo
            }
            else
            {
                isMoving = false; // Hết đường đi
                Debug.Log("Đã đi hết đường!");
            }
        }
    }

    public void StartMove(List<Vector3> path)
    {
        if (path == null || path.Count == 0) return;

        pathQueue = new Queue<Vector3>(path);
        currentTargetNode = pathQueue.Dequeue();
        isMoving = true;
    }
}
