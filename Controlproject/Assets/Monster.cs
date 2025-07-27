using UnityEngine;

public class Monster : MonoBehaviour
{
    public float speed;
    public Rigidbody target;
    bool isLive;
    bool isRealized;

    Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 dirVec = target.position - rigid.position;
        Vector3 moveVec = dirVec.normalized * speed * Time.fixedDeltaTime;
    }
}
