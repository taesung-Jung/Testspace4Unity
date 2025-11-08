using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class enemyattack5 : MonoBehaviour
{
    [Header("타깃")]
    public string playerTag = "Player";
    private Transform target;

    [Header("이동 속도")]
    [Tooltip("배회(걷기) 속도")]
    public float walkSpeed = 2.8f;
    [Tooltip("추격(달리기) 속도")]
    public float runSpeed = 5.0f;

    [Header("배회")]
    public Vector2 idleTime = new Vector2(0.8f, 2.0f);
    public float turnSpeedDegPerSec = 240f;
    public float moveMinDistance = 4f;
    public float moveMaxDistance = 8f;
    public float sampleMaxDistance = 2.0f;
    public float stuckRepathTime = 2.0f;
    public float roamRadius = 15f;
    public float waypointTolerance = 0.5f;

    [Header("전투")]
    public float stopDistance = 1.5f;
    public float attackRange = 1.8f;
    public float damage = 15f;
    public Transform attackOrigin;
    public float hitRadius = 0.8f;
    public LayerMask targetLayers;
     public GameObject attackVfx;

    [Header("공격 속도 제어")]
    [Tooltip("공격 중에만 적용되는 애니메이션 속도 배수 (1 = 기본속도)")]
    public float attackAnimSpeed = 1.0f;
    [Tooltip("한 번의 공격 후 다음 공격까지 추가 대기 시간(초)")]
    public float nextAttackDelay = 0.35f;

    [Header("애니메이터")]
    public Animator animator;
    public string walkBool = "isWalking";
    public string runBool = "isRunning";
    public string attackBool = "isAttacking";
    [Tooltip("공격 트리거용 Bool을 잠깐 켰다가 끄는 시간(초)")]
    public float attackBoolPulse = 0.08f;

    [Header("감지 콜라이더 (Sphere)")]
    [Min(0.01f)]
    public float detectRadius = 6f;
    public Vector3 detectCenter = Vector3.zero;

    [Header("공격 이펙트")]
    public GameObject attackVfxPrefab;

    private enum State { Wander, Chase, Attack }
    private State state = State.Wander;

    private enum WanderPhase { Idle, Turning, Moving }
    private WanderPhase wanderPhase = WanderPhase.Idle;

    private NavMeshAgent agent;
    private Vector3 roamCenter;
    private float idleTimer;
    private float attackBoolTimer;
    private float attackTimer;
    private Quaternion targetRot;
    private float stuckTimer;
    private Vector3 lastPos;

    void Reset()
    {
        //SphereCollider 설정
        var sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.center = detectCenter;
        sc.radius = detectRadius;

        //물리 세팅(트리거 감지 전용)
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        roamCenter = transform.position;

        agent.speed = Mathf.Max(0f, walkSpeed);
        var sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.center = detectCenter;
        sc.radius = detectRadius;
    }

    void OnEnable()
    {
        EnterWanderIdle();
        if (animator) animator.speed = 1f;
    }

    void OnDisable()
    {
        if (animator) animator.speed = 1f;
    }

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

    // ========================= 배회 =========================
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
            agent.updateRotation = false;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRot) <= 1f)
            {
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
                agent.speed = Mathf.Max(0f, walkSpeed);
                agent.SetDestination(dest);

                stuckTimer = 0f;
                lastPos = transform.position;
                wanderPhase = WanderPhase.Moving;
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(waypointTolerance, agent.stoppingDistance))
            {
                EnterWanderIdle();
                return;
            }

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
        agent.updateRotation = false;
        agent.speed = Mathf.Max(0f, walkSpeed);
    }

    // ========================= 추격 =========================
    void TickChase()
    {
        if (!target) { ChangeState(State.Wander); return; }

        agent.stoppingDistance = stopDistance;
        agent.updateRotation = true;
        agent.isStopped = false;

        agent.speed = Mathf.Max(0f, runSpeed);
        agent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
            ChangeState(State.Attack);
    }

    // ========================= 공격 =========================
    void TickAttack()
    {
        if (!target) { ChangeState(State.Wander); return; }

        Vector3 look = target.position - transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 10f * Time.deltaTime);

        agent.isStopped = true;

        attackTimer -= Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) > attackRange + 0.5f)
        {
            agent.isStopped = false;
            ChangeState(State.Chase);
            return;
        }

        if (attackTimer <= 0f)
        {
            attackBoolTimer = attackBoolPulse;
            SpawnAttack();
            attackTimer = Mathf.Max(0f, nextAttackDelay);
        }
    }
    // ========================= 공격 생성 =========================
    void SpawnAttack()
    {
        if (attackVfxPrefab && attackOrigin)
        {
            attackVfx = Instantiate(attackVfxPrefab, attackOrigin.position, attackOrigin.rotation);
            Attack attack = attackVfx.GetComponent<Attack>();
            attackVfx.transform.SetParent(transform, worldPositionStays: attack.isRanged);
        }
    }

    // ========================= 애니메이터 =========================
    void UpdateAnimator()
    {
        if (!animator) return;

        float speedMag = agent.velocity.magnitude;
        bool walking = state == State.Wander && wanderPhase == WanderPhase.Moving && speedMag > 0.05f;
        bool running = state == State.Chase && speedMag > 0.05f;

        if (attackBoolTimer > 0f) attackBoolTimer -= Time.deltaTime;
        bool attackingPulse = attackBoolTimer > 0f;

        if (!string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, walking);
        if (!string.IsNullOrEmpty(runBool)) animator.SetBool(runBool, running);
        if (!string.IsNullOrEmpty(attackBool)) animator.SetBool(attackBool, attackingPulse);

        float targetAnimSpeed = (state == State.Attack) ? Mathf.Max(0.01f, attackAnimSpeed) : 1f;
        if (Mathf.Abs(animator.speed - targetAnimSpeed) > 0.0001f)
            animator.speed = targetAnimSpeed;
    }

    // ========================= 트리거 탐지 =========================
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

    // ========================= 유틸 =========================
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

        if (s == State.Wander) agent.speed = Mathf.Max(0f, walkSpeed);
        else if (s == State.Chase) agent.speed = Mathf.Max(0f, runSpeed);

        if (s == State.Wander) EnterWanderIdle();
        else { agent.updateRotation = true; agent.isStopped = (s == State.Attack); }

        if (s == State.Attack) attackTimer = 0f;

        if (s != State.Attack)
        {
            attackBoolTimer = 0f;
            if (animator) animator.speed = 1f;
        }
    }

    void OnValidate()
    {
        if (walkSpeed < 0f) walkSpeed = 0f;
        if (runSpeed < 0f) runSpeed = 0f;
        if (attackAnimSpeed < 0.01f) attackAnimSpeed = 0.01f;
        if (nextAttackDelay < 0f) nextAttackDelay = 0f;
        if (detectRadius < 0.01f) detectRadius = 0.01f;

        var sc = GetComponent<SphereCollider>();
        if (sc)
        {
            sc.isTrigger = true;
            sc.center = detectCenter;
            sc.radius = detectRadius;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (attackOrigin) Gizmos.DrawWireSphere(attackOrigin.position, hitRadius);

        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.3f);
        Vector3 center = Application.isPlaying ? roamCenter : transform.position;
        Gizmos.DrawWireSphere(center, roamRadius);

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
        Vector3 worldCenter = transform.TransformPoint(detectCenter);
        Gizmos.DrawWireSphere(worldCenter, detectRadius);
    }
#endif
}
