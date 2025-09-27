// enemyattack3.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class enemyattack3 : MonoBehaviour
{
    [Header("Ÿ��")]
    public string playerTag = "Player";
    private Transform target;


    [Header("��ȸ")]
    public Vector2 idleTime = new Vector2(0.8f, 2.0f); // �����ִ� �ð�(��)
    public float turnSpeedDegPerSec = 240f;            // ���ڸ� ȸ�� �ӵ�(��/��)
    public float moveMinDistance = 4f;                 // ȸ�� �� ���� �ּ� �Ÿ�
    public float moveMaxDistance = 8f;                 // ȸ�� �� ���� �ִ� �Ÿ�
    public float sampleMaxDistance = 2.0f;             // NavMesh.SamplePosition Ž�� �ݰ�
    public float stuckRepathTime = 2.0f;               // �̵� �� ���� ���� �ð�
    public float roamRadius = 15f;
    public float waypointTolerance = 0.5f;


    [Header("����")]
    public float stopDistance = 1.5f;
    public float attackRange = 1.8f;
    public float attackCooldown = 1.2f;
    public float damage = 15f;
    public Transform attackOrigin;
    public float hitRadius = 0.8f;
    public LayerMask targetLayers; // ����θ� �� ���̾�(~0)

    [Header("�ִϸ�����")]
    public Animator animator;
    public string walkBool = "isWalking";
    public string runBool = "isRunning";
    public string attackBool = "isAttacking";
    public float attackBoolPulse = 0.08f;

    private enum State { Wander, Chase, Attack }
    private State state = State.Wander;

    private enum WanderPhase { Idle, Turning, Moving }
    private WanderPhase wanderPhase = WanderPhase.Idle;

    private NavMeshAgent agent;
    private Vector3 roamCenter;
    private float idleTimer;
    private float attackBoolTimer;
    private float cooldown;
    private Quaternion targetRot;
    private float stuckTimer;
    private Vector3 lastPos;

    void Reset()
    {
        var box = GetComponent<BoxCollider>(); box.isTrigger = true;
        var rb = GetComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        roamCenter = transform.position;
    }

    void OnEnable() => EnterWanderIdle();

    void Update()
    {
        switch (state)
        {
            case State.Wander: TickWander(); break;
            case State.Chase: TickChase(); break;
            case State.Attack: TickAttack(); break;
        }
        UpdateAnimator();
    }

    // ========================= ��ȸ =========================
    void TickWander()
    {
        agent.stoppingDistance = 0f;

        if (wanderPhase == WanderPhase.Idle)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                float yaw = Random.Range(-180f, 180f);
                Vector3 newFwd = Quaternion.Euler(0f, yaw, 0f) * transform.forward;
                targetRot = Quaternion.LookRotation(new Vector3(newFwd.x, 0f, newFwd.z));
                wanderPhase = WanderPhase.Turning;
            }
        }
        else if (wanderPhase == WanderPhase.Turning)
        {
            agent.isStopped = true;
            agent.updateRotation = false; // ���ڸ� ȸ��
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRot) <= 1f)
            {
                // ȸ�� �Ϸ� �� �� �������� ���� ������ ����
                Vector3 forward = transform.forward;
                float dist = Random.Range(moveMinDistance, moveMaxDistance);
                Vector3 desired = transform.position + forward * dist;

                Vector3 dest;
                if (!TryProjectOnNavMesh(desired, out dest))
                {
                    if (!TryRandomPointInRadius(roamCenter, roamRadius, out dest))
                    { EnterWanderIdle(); return; }
                }

                agent.updateRotation = true;
                agent.isStopped = false;
                agent.SetDestination(dest);

                stuckTimer = 0f;
                lastPos = transform.position;
                wanderPhase = WanderPhase.Moving;
            }
        }
        else // Moving
        {
            if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(waypointTolerance, agent.stoppingDistance))
            {
                EnterWanderIdle();
                return;
            }

            // ���� ���� �� ����
            float moved = (transform.position - lastPos).magnitude;
            if (moved < 0.01f) stuckTimer += Time.deltaTime; else stuckTimer = 0f;
            lastPos = transform.position;

            if (stuckTimer >= stuckRepathTime)
            {
                Vector3 dest;
                if (TryRandomPointInRadius(roamCenter, roamRadius, out dest))
                    agent.SetDestination(dest);
                stuckTimer = 0f;
            }
        }
    }

    void EnterWanderIdle()
    {
        wanderPhase = WanderPhase.Idle;
        idleTimer = Random.Range(idleTime.x, idleTime.y);
        agent.ResetPath();
        agent.isStopped = true;
        agent.updateRotation = false; // ���� �ܰ迡�� ���� ȸ��
    }

    // ========================= �߰� =========================
    void TickChase()
    {
        if (!target) { ChangeState(State.Wander); return; }

        agent.stoppingDistance = stopDistance;
        agent.updateRotation = true;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
            ChangeState(State.Attack);
    }

    // ========================= ���� =========================
    void TickAttack()
    {
        if (!target) { ChangeState(State.Wander); return; }

        // ���ڸ����� Ÿ�� �ٶ󺸱�
        Vector3 look = target.position - transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 10f * Time.deltaTime);

        agent.isStopped = true;

        cooldown -= Time.deltaTime;
        if (cooldown <= 0f)
        {
            attackBoolTimer = attackBoolPulse; // �ִϿ� �޽�
            DealDamage();                      // �ִ� �̺�Ʈ�� ��ü�ϸ� �� �� �ּ�
            cooldown = attackCooldown;
        }

        if (Vector3.Distance(transform.position, target.position) > attackRange + 0.5f)
        {
            agent.isStopped = false;
            ChangeState(State.Chase);
        }
    }

    // ========================= �ִϸ����� =========================
    void UpdateAnimator()
    {
        if (!animator) return;

        float speed = agent.velocity.magnitude;
        bool walking = state == State.Wander && wanderPhase == WanderPhase.Moving && speed > 0.05f;
        bool running = state == State.Chase && speed > 0.05f;

        if (attackBoolTimer > 0f) attackBoolTimer -= Time.deltaTime;
        bool attackingPulse = attackBoolTimer > 0f;

        if (!string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, walking);
        if (!string.IsNullOrEmpty(runBool)) animator.SetBool(runBool, running);
        if (!string.IsNullOrEmpty(attackBool)) animator.SetBool(attackBool, attackingPulse);
    }

    // ========================= ����� ���� =========================
    public void DealDamage()
    {
        Vector3 pos = attackOrigin ? attackOrigin.position :
                       transform.position + transform.forward * (attackRange * 0.5f);

        int mask = (targetLayers.value == 0) ? ~0 : targetLayers.value;
        Collider[] cols = Physics.OverlapSphere(pos, hitRadius, mask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < cols.Length; i++)
        {
            Collider col = cols[i];
            if (!col.CompareTag(playerTag)) continue;

            // (A) ���� ���: TakeDamage(float) �޽����� �÷� ���� (������ ����)
            col.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            // (B) Ȯ�� ���: �ʿ��� ��� �ڽŸ��� DamageInfo/�޼������ ������Ʈ�� ���� �߰�
            // col.SendMessageUpwards("TakeDamageInfo", new DamageInfo{...}, SendMessageOptions.DontRequireReceiver);
        }
    }

    // ========================= Ʈ���� Ž�� =========================
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            target = other.transform;
            ChangeState(State.Chase);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform == target)
        {
            target = null;
            ChangeState(State.Wander);
        }
    }

    // ========================= ��ƿ =========================
    bool TryProjectOnNavMesh(Vector3 pos, out Vector3 result)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, sampleMaxDistance, NavMesh.AllAreas))
        { result = hit.position; return true; }
        result = Vector3.zero; return false;
    }

    bool TryRandomPointInRadius(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 12; i++)
        {
            Vector2 circle = Random.insideUnitCircle * radius;
            Vector3 candidate = new Vector3(center.x + circle.x, center.y, center.z + circle.y);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, sampleMaxDistance, NavMesh.AllAreas))
            { result = hit.position; return true; }
        }
        result = center; return false;
    }

    void ChangeState(State s)
    {
        state = s;

        if (s == State.Wander) EnterWanderIdle();
        else { agent.updateRotation = true; agent.isStopped = (s == State.Attack); }

        if (s != State.Attack) attackBoolTimer = 0f;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (attackOrigin) Gizmos.DrawWireSphere(attackOrigin.position, hitRadius);

        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.3f);
        Vector3 center = Application.isPlaying ? roamCenter : transform.position;
        Gizmos.DrawWireSphere(center, roamRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * moveMaxDistance);
    }
#endif
}
