using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private float _spawnFrequency = 2f; // how often we spawn enemies in seconds

    [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField]
    private GameObject _enemyContainer;

    private bool _spawnEnemies = true;

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
        while (_spawnEnemies)
        {
            GameObject newEnemy = Instantiate(_enemyPrefab, new Vector3(RandomX(), 7, 0), Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(spawnFrequency);
        }
    }

    private float RandomX()
    {
        return Random.Range(-10, 10);
    }

    public void OnPlayerDeath()
    {
        _spawnEnemies = false;
    }
}
