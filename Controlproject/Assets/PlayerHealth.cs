using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 100;
    public int currentHp;

    public float NodamageTime = 5.0f;
    public float NodamageTimer = 0.0f;
    public bool isDamaged = false;

    void Start() { currentHp = maxHp; }

    void Update()
    {
        if (isDamaged)
        {
            NodamageTimer -= Time.deltaTime;
            if (NodamageTimer <= 0f)
            {
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

