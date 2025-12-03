using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;
    public float speed = 12f;
    public bool isMoving = false;
    [SerializeField] private float food;
    [SerializeField] private float drink;
    [SerializeField] private float sleep;
    [SerializeField] private float stress;
    [SerializeField] private int money;
    // Đang ăn?
    public bool isEating = false;
    // Đang uống?
    public bool isDrinking = false;
    // Đang ngủ?
    public bool isSleeping = false;
    // Đang làm việc?
    public bool isWorking = false;
    // Đang giảm stress?
    public bool isRelaxing = false;
    public bool isIdle = false;
    public void ResetFlags()
    {
        isEating = false;
        isDrinking = false;
        isSleeping = false;
        isWorking = false;
        isRelaxing = false;
        isIdle = false;
    }
    public void Eating()
    {
        ResetFlags();
        isEating = true;
    }
    public void Drinking()
    {
        ResetFlags();
        isDrinking = true;
    }

    public void Sleeping()
    {
        ResetFlags();
        isSleeping = true;
    }

    public void Working()
    {
        ResetFlags();
        isWorking = true;
    }

    public void Relaxing()
    {
        ResetFlags();
        isRelaxing = true;
    }
    public void Standing()
    {
        ResetFlags();
        isIdle = true;
    }
    //Get and set
    public float Food
    {
        get { return food; }
        set
        {
            food = Mathf.Clamp(value, 0f, 240f);
        }
    }

    public float Drink
    {
        get { return drink; }
        set
        {
            drink = Mathf.Clamp(value, 0f, 80f);
        }
    }

    public float Sleep
    {
        get { return sleep; }
        set
        {
            sleep = Mathf.Clamp(value, 0f, 160f);
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
        /*19/10/2025: Chỗ này nếu cần thì nên fix đồ họa*/
        while (Vector3.Distance(player.transform.position, targetPosition) > 0.1f)
        {
            Vector3 direction = (targetPosition - player.transform.position).normalized;
            Vector3 moveStep = direction * speed * Time.deltaTime;
            this.transform.position = targetPosition;
            rb.MovePosition(player.transform.position + moveStep);
            yield return null; // Chờ đến frame tiếp theo
        }
        // this.transform.position = targetPosition;
        moveCoroutine = null;
    }
}
