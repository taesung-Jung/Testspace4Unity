using UnityEngine;


public class EnemyAttack1 : MonoBehaviour
{
    public GameObject attackVfxPrefab;
    public Transform attackOrigin;
    public float attackCooldown = 1f;
    float timer;


    public bool CanAttack => timer <= 0f;


    void Update() => timer -= Time.deltaTime;


    public void PerformAttack(Transform target)
    {
        if (!CanAttack) return;
        timer = attackCooldown;


        // VFX 스폰
        if (attackVfxPrefab)
        {   
            var vfx = Instantiate(attackVfxPrefab, attackOrigin.position, attackOrigin.rotation);
            vfx.transform.SetParent(transform, true);
        }
    }
}