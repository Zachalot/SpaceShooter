using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed = 2f;

    [SerializeField]
    private float _health = 1f;

    [SerializeField]
    private float _damage = 1f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(RandomXPosition(), 7, 0);
    }

    // Update is called once per frame
    void Update()
    {
        MoveXY();
    }

    #region collision
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Enemy has collided with: " + other.tag);
        if (other.tag == "Player")
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.ApplyDamage(_damage);
            }
        }
        else if (other.tag == "Enemy")
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ApplyDamage(_damage);
            }
        }
    }

    public void ApplyDamage(float incomingDamage)
    {
        Debug.Log("Enemy taking " + incomingDamage + " damage");
        _health = _health - incomingDamage;
        if (_health <= 0)
        {
            Debug.Log("Enemy has been destroyed");
            Destroy(gameObject);
        }
    }

    #endregion
    #region movement

    private void MoveXY()
    {
        // first check if the enemy is off the bottom of the screen, spawn at top if so
        transform.position = EnforceVerticalBoundWrap(transform.position);

        // then translate enemy on the y axis
        transform.position = AdvanceEnemy(transform.position, _speed);

    }

    private Vector3 AdvanceEnemy(Vector3 position, float speed)
    {
        position.y = position.y - speed * Time.deltaTime;
        return position;
    }

    private Vector3 EnforceVerticalBoundWrap(Vector3 position)
    {
        if (position.y < -6)
        {
            return new Vector3(RandomXPosition(), 7.2f, position.z);
        }
        else
        {
            return position;
        }
    }

    private float RandomXPosition()
    {
        return UnityEngine.Random.Range(-10f, 10f);
    }
    #endregion
}
