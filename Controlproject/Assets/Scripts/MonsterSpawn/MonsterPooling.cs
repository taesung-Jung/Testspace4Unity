using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterPooling : MonoBehaviour
{
    public GameObject[] prefabs;

    List<GameObject>[] pools;

    void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                item.SetActive(true);
                return item;
            }
        }

    // 못 찾았으면 새로 생성
        GameObject select = Instantiate(prefabs[index], transform);
        pools[index].Add(select);
        return select;
}

}
