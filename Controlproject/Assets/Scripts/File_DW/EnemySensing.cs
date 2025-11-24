using UnityEngine;


public class EnemySensing : MonoBehaviour
{
    public Transform Target { get; private set; }
    public bool HasTarget => Target != null;
    public string targetTag = "Player";


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        Target = other.transform;
    }


    void OnTriggerExit(Collider other)
    {
        if (other.transform == Target)
        Target = null;
    }
}