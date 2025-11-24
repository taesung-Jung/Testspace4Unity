using UnityEngine;


public class EnemyAI : MonoBehaviour
{
    public EnemyMovement movement;
    public EnemyAttack1 attack;
    public EnemySensing sensing;
    public Animator animator;
    public string walkBool = "isWalking";
    public string runBool = "isRunning";
    public string attackTrigger = "attackTrigger";


    private void Update()
    {
        if (!sensing.HasTarget)
        {
            movement.Wander();
            animator.SetBool(walkBool, true);
            animator.SetBool(runBool, false);
        }
        else
        {
            movement.LookAt(sensing.Target);
    
            if (!movement.IsInAttackRange(sensing.Target))
            {
                movement.MoveTo(sensing.Target.position);
                animator.SetBool(runBool, true);
                animator.SetBool(walkBool, false);
            }
            else
            {
                movement.Stop();
                animator.SetBool(runBool, false);
                animator.SetBool(walkBool, false);
                animator.SetTrigger(attackTrigger);
            }
        }
        
    }
}
