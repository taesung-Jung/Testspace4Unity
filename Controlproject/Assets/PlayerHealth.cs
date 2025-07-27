using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 100;
    public int currentHp;

    void Start() { currentHp = maxHp; }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;
    }

    // 0~1 ������ �� ��ȯ
    public float CurrentHpNormalized()
    {
        return (float)currentHp / maxHp;
    }
}

