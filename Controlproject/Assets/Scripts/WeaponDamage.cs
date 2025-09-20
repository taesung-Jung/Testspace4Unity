using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    public int damage = 10;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // PlayerHealth
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
        }
    }
}
