using UnityEngine;


public class EnemySensing : MonoBehaviour
{
    public float detectRange = 10f;
    public float closeRange = 2f;
    public float timer = 5f;
    public float maxTimer = 5f;
    public float viewAngle = 170f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;


    public Transform Target { get; private set; }
    public bool HasTarget => Target != null;


    void Update()
    {
        if (!HasTarget)
        {
            SearchForTarget();
        }
        else
        {
            if (!IsTargetVisible(Target))
                ClearTarget();
        }
    }


    void SearchForTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, targetMask);
        foreach (var hit in hits)
        {
            if (IsTargetVisible(hit.transform))
            {
                Target = hit.transform;
                break;
            }
        }
    }


    bool IsTargetVisible(Transform t)
    {
        Vector3 dir = (t.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, t.position);

        if (dist <= closeRange)
        {
            if (!Physics.Raycast(transform.position + Vector3.up, dir, dist, obstacleMask))
                return true;
            return false;
        }

        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, dist, obstacleMask))
            return false;

        return dist <= detectRange;
    }


    public void ClearTarget() => Target = null;
}