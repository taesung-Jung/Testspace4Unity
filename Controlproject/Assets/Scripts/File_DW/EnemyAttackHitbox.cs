using UnityEngine;


public class EnemyAttackHitbox : MonoBehaviour
{
    public float damage = 10f;
    public float radius = 1f;
    public LayerMask targetMask;


    public void DoDamage()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, radius, targetMask);
        foreach (var c in cols)
        {
            if (c.TryGetComponent(out IDamageable d))
            d.TakeDamage(damage);
        }       
    }
}