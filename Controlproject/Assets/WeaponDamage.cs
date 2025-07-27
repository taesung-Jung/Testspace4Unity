using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    public int damage = 10;
    public string playerTag = "Player"; // 플레이어 오브젝트의 Tag

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // PlayerHealth 스크립트에 데미지 적용
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
        }
    }
}
