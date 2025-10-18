using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;
    public float speed = 12f;
    public bool isMoving = false;
    [SerializeField] private float food = 24;
    [SerializeField] private float drink = 24;
    [SerializeField] private float sleep = 24;
    [SerializeField] private float stress = 0;
    [SerializeField] private int money = 100;

    //Get and set
    public float Food
    {
        get { return food; }
        set
        {
            food = Mathf.Clamp(value, 0f, 24f);
        }
    }

    public float Drink
    {
        get { return drink; }
        set
        {
            drink = Mathf.Clamp(value, 0f, 24f);
        }
    }

    public float Sleep
    {
        get { return sleep; }
        set
        {
            sleep = Mathf.Clamp(value, 0f, 24f);
        }
    }

    public float Stress
    {
        get { return stress; }
        set
        {
            stress = Mathf.Clamp(value, 0f, 72f);
        }
    }

    public int Money
    {
        get { return money; }
        set
        {
            money = Mathf.Max(value, 0);  // chỉ cần >= 0
        }
    }

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
