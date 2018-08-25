using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatSpawner : MonoBehaviour
{
    public Transform prefab;
    public Network network;
    public float spawnRate;
    public int limit;
    void Start()
    {
        StartCoroutine("Spawn");
    }

    IEnumerator Spawn()
    {
        for(int i=0; i< limit; i++)
        {
            yield return new WaitForSeconds(spawnRate);
            GameObject newObject = Instantiate(prefab).gameObject;
            newObject.GetComponent<NetworkWalker>().network = network;
        }
    }
}
