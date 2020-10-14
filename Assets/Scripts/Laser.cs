using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _damage = 1f;

    [SerializeField]
    private float _speed = 20f;

    [SerializeField]
    private float _range = 10;
    private float _distanceTraveled = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(_speed * Time.deltaTime);
        if (_distanceTraveled >= _range)
        {
            Destroy(this.gameObject);
        }
        else 
        {
            transform.Translate(new Vector3(0, _speed * Time.deltaTime, 0));
            _distanceTraveled = _distanceTraveled + _speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Laser collided with: " + other.tag);
        if (other.tag == "Enemy")
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.ApplyDamage(_damage);
            }
        }
        else if (other.tag == "Player") 
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.ApplyDamage(_damage);
            }
        }

        Debug.Log("Laser is applying damage " + _damage);
        Destroy(gameObject);
    }
}
