using UnityEngine;

public class Attack : MonoBehaviour
{
    public int damage = 10;
    public string playerTag = "Player";
    public bool isRanged = false;
    public float timer = 0;
    public float lifeTime;

    private Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
    }

    void Update(){
        timer += Time.deltaTime;
        if(timer >= lifeTime){
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        // 스크립트 가져오기
        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        DefenseScript defense = other.GetComponent<DefenseScript>();

        if (defense != null && defense.isDefending && defense.stamina > 0)
        {
            // 공격 방향 계산 (공격 -> 플레이어)
            Vector3 attackDir = (other.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(other.transform.forward, -attackDir);

            // dot > 0.5f 정도면 정면 공격
            if (dot > 0.5f)
            {
                defense.UseStamina(10f);
                Destroy(this.gameObject);
            }
        }

        if (hp != null)
        {
            hp.TakeDamage(damage);
            Destroy(this.gameObject);
        }
    }

}
