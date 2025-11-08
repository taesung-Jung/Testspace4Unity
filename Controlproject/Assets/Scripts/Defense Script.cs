using UnityEngine;

using System.Collections;

public class DefenseScript : MonoBehaviour
{
    public bool isAttacking;
    public bool isDefending;
    public bool isColliding;
    public bool broken;

    public float maxStamina;
    public float stamina;
    public float staminaRegenRate;


    void Update()
    {   

        //스태미나 회복
        if (!isDefending && stamina < maxStamina)
        {
            stamina += staminaRegenRate * Time.deltaTime;
        }
        if (!broken)
        {
            //공격 중 방어 불가
            if (!isAttacking)
            {
                isDefending = Input.GetKey(KeyCode.Tab);
            }
        }
        if (stamina > maxStamina)
        {
            stamina = maxStamina;
        }


        if (stamina <= 0 && !broken)
        {
            StartCoroutine(GuardBreak());
        }
    }

        
    public void UseStamina(float amount)
    {
        stamina -= amount;
        if (stamina < 0)
            stamina = 0;
    }

    IEnumerator GuardBreak()
    {
        broken = true;

        yield return new WaitForSeconds(3f); // 3초간 방어 불가

        broken = false;

    }
}