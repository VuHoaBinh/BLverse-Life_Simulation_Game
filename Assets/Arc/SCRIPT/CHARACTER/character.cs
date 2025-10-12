using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;
    public float speed = 12f;
    public bool isMoving = false;
    public float food = 24;
    public float drink = 24;
    public float sleep = 24;
    public float stress = 0;
    public int money = 100;

    private Coroutine moveCoroutine;

    public void StartMove(Vector3 targetPosition)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine); // Dừng nếu đang di chuyển
        moveCoroutine = StartCoroutine(MoveToPosition(targetPosition));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;

        while (Vector3.Distance(player.transform.position, targetPosition) > 0.1f)
        {
            Vector3 direction = (targetPosition - player.transform.position).normalized;
            Vector3 moveStep = direction * speed * Time.deltaTime;
            rb.MovePosition(player.transform.position + moveStep);
            yield return null; // Chờ đến frame tiếp theo
        }

        rb.MovePosition(targetPosition);
        moveCoroutine = null;
    }
}
