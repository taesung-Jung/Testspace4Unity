using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    void Update()
    {
            if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q 키 입력");
            // Q 키 동작
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E 키 입력");
            // E 키 동작
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R 키 입력");
            // R 키 동작
        }

        // 마우스 - 좌클릭 = 공격
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("공격");
        }

        // 마우스 - 우클릭 = 방어
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("방어");
        }
    }
}