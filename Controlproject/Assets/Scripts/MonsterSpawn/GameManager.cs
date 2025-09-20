using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int dungeonLevel;
    public static GameManager instance;
    public MonsterPooling pool;
    void Awake()
    {
        instance = this;
    }
}
