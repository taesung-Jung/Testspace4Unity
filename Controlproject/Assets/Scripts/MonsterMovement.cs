using UnityEngine;
using UnityEngine.AI;

public class MonsterMovement : MonoBehaviour
{
    NavMeshAgent navi;
    public Transform target;

    void Awake(){
        navi = GetComponent<NavMeshAgent>();
    }
    void Update(){
        navi.SetDestination(target.position);
    }
}
