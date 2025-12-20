using UnityEngine;
using UnityEngine.AI;


public class EnemyMovement : MonoBehaviour
{
    NavMeshAgent agent;
    public float attackRange = 1.5f;
    public float wanderTimer = 0;
    public float radius = 10;


    void Awake() => agent = GetComponent<NavMeshAgent>();


    public void MoveTo(Vector3 pos)
    {
        agent.isStopped = false;
        agent.SetDestination(pos);
    }


    public void Stop() => agent.isStopped = true;


    public void Wander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            wanderTimer = 0f;
        }

        if (wanderTimer > 0f)
        {
            wanderTimer -= Time.deltaTime;
            return;
        }


        wanderTimer = Random.Range(6f, 9f);



        Vector3 center = transform.position;
        Vector3 randomDir = center + Random.insideUnitSphere * radius;


        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }



    public void LookAt(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;


        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
        }
    }


    public bool IsInAttackRange(Transform target)
    => Vector3.Distance(transform.position, target.position) <= attackRange;
}