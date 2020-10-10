using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Manager : MonoBehaviour
{
    [SerializeField]
    private float _spawnFrequency = 5f; // how often we spawn enemies in seconds

    [SerializeField]
    private GameObject _enemyPrefab;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemies(_spawnFrequency));
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Apparently anytime we return an IEnumerator, this is a coroutine ...... so I literally can't user IEnumerators for anything else in unity
    private IEnumerator SpawnEnemies(float spawnFrequency) 
    {
        while (true)
        {
            Instantiate(_enemyPrefab, new Vector3(RandomX(), 7, 0), Quaternion.identity);
            yield return new WaitForSeconds(spawnFrequency);
        }
    }

    private float RandomX()
    {
        return Random.Range(-10, 10);
    }
}
