using UnityEngine;
using UnityEngine.AI;


public class EnemyMovement : MonoBehaviour
{
    NavMeshAgent agent;
    public float attackRange = 1.5f;


    void Awake() => agent = GetComponent<NavMeshAgent>();


    public void MoveTo(Vector3 pos)
    {
        agent.isStopped = false;
        agent.SetDestination(pos);
    }


    public void Stop() => agent.isStopped = true;


    public void Wander()
    {
        agent.isStopped = false;
        // TODO: random wander target
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