using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    IEnumerator Spawner()
    {
        GameObject last = null;
        while(true)
        {
            float time = Random.RandomRange(MinSpawnTime, MaxSpawnTime);
            yield return new WaitForSeconds(time);
            var location = SpawnLocations[Random.RandomRange(0, SpawnLocations.Count)];
            string tag = "PowerUpTag";
            
            if (GameObject.FindGameObjectWithTag(tag) == null)
            {
                last = Instantiate(powerUpPrefab, location, Quaternion.identity);
                powerUpPrefab.tag = tag;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Spawner());
    }

    public GameObject powerUpPrefab;
    Coroutine spawner;
    public List<Vector3> SpawnLocations;
    public float MinSpawnTime;
    public float MaxSpawnTime;
}
