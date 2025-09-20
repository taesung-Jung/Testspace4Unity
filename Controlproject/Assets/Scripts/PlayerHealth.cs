using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 100;
    public int currentHp;

    public float NodamageTimer = 30.0f;
    public bool isDamaged = false;

    void Start()
    {
        currentHp = maxHp;
        NodamageTimer = 30.0f;
    }

    void Update()
    {
        if (isDamaged)
        {
            NodamageTimer -= 1.0f;
            if (NodamageTimer <= 0f)
            {
                NodamageTimer = 30.0f;
                isDamaged = false;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if(isDamaged) return;
        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;
        isDamaged = true;
    }

    // 0~1
    public float CurrentHpNormalized()
    {
        return (float)currentHp / maxHp;
    }
}

