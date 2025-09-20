using UnityEngine;

public class Spawner : MonoBehaviour
{
    
    public Transform[] spawnPoints;
    public int[] monsterTypes;
    public int minimumMonsters;
    public int maxMonsters;
    public int cusrrentMonsterCount;

    void Awake()
    {
        spawnPoints = GetComponentsInChildren<Transform>();
    }
    void Start()
    {
        SpawnMonsters();
    }

    public void SpawnMonsters()
    {
        //스폰해야할 몬스터 수
        float monsterNumber = (GameManager.instance.dungeonLevel * 0.15f + 1f) * minimumMonsters;
        int newMonsterNumber;
        if (maxMonsters < monsterNumber)
        {
            newMonsterNumber = maxMonsters;
        }
        else
        {
            newMonsterNumber = (int)monsterNumber;
        }
        for (int i = 0; i < newMonsterNumber; i++)
        {
            int randomIndex = Random.Range(0, monsterTypes.Length);
            GameObject monster = GameManager.instance.pool.Get(monsterTypes[randomIndex]);
            // 스폰 기준점
            Vector3 basePos = spawnPoints[Random.Range(1, spawnPoints.Length)].position;

            float offsetX = Random.Range(-2f, 2f);
            float offsetZ = Random.Range(-2f, 2f);

            Vector3 spawnPos = new Vector3(basePos.x + offsetX, basePos.y, basePos.z + offsetZ);

            monster.transform.position = spawnPos;

        }
    }
}
