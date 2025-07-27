using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    public int damage = 10;
    public string playerTag = "Player"; // �÷��̾� ������Ʈ�� Tag

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // PlayerHealth ��ũ��Ʈ�� ������ ����
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
        }
    }
}
