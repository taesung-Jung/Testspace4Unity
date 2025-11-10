using UnityEngine;
using System.Threading;
using System;


[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    public Transform cameraTransform;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 inputDir = new Vector2(h, v).normalized;
        bool isMoving = inputDir.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            // 4방향 전용: x/y 기준값 분리
            float MoveX = Mathf.Abs(h) > Mathf.Abs(v) ? Mathf.Sign(h) : 0;
            float MoveY = Mathf.Abs(v) > Mathf.Abs(h) ? Mathf.Sign(v) : 0;

            animator.SetFloat("MoveX", MoveX);
            animator.SetFloat("MoveY", MoveY);
        }
        else
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
        }

        // 마우스 입력
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            animator.SetTrigger("Attack"); // 공격
        }
        else if (Input.GetMouseButtonDown(1)) // 우클릭
        {
            animator.SetTrigger("Defence"); // 방어
        }
    }
}