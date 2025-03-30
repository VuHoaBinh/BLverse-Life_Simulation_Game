using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class playerControl : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5f;    
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;

    Vector2 movementInput;
    Rigidbody2D rb;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    private Animator animator;

    public void Awake(){
        animator = this.GetComponent<Animator>();
    }

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (movementInput != Vector2.zero)
        {
            animator.SetBool("isMoving", true);
            animator.SetFloat("moveX", movementInput.x);
            animator.SetFloat("moveY", movementInput.y);
            int count = rb.Cast(
                movementInput.normalized,
                movementFilter,
                castCollisions,
                speed * Time.fixedDeltaTime * collisionOffset
            );
            Debug.Log(count);
            if (count == 0)
            {
                rb.MovePosition(rb.position + movementInput * speed * Time.fixedDeltaTime);
                Debug.Log(movementInput);
            }
        }
        else{
            animator.SetBool("isMoving", false);
        }

    }
    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }
}
