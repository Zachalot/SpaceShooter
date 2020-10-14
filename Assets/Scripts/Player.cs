using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Outside Objects
    [SerializeField]
    private GameObject _laserPrefab;
    private SpawnManager _spawnManager;

    // Combat
    [SerializeField]
    private float _health = 10f;

    [SerializeField]
    private float _collisionDamage = 5f;

    [SerializeField]
    private float _fireRate = 0.1f; // only allow the p[layer to fire once every 0.2 seconds
    private float _cooldown = 0f; // seconds left until player can fire again

    // Movement Physics
    [SerializeField]
    private float _mass = 2000f; // mass of the player in kg

    [SerializeField]
    private float _thrust = 100f; // available force of thrusters in Newtons

    private float _speedHorizontal = 0f; // speed at which the player moves horizontally
    private float _speedVertical = 0f; // speed at which the player is currently moving vertically

    //[SerializeField]
    //private float _horizontalAccelModifier = 2f; // modifier to allow user to accelerate faster or than the normal rate on the horizontal axis

    //[SerializeField]
    //private float _verticalAccelModifier = 2f; // modifier to allow the user to accelerate faster than the normal rate in the vertical axis

    //[SerializeField]
    //private float _horizontalDecelModifier = 5f; // modifier to allow the user to decelerate faster in the horizontal axis

    //[SerializeField]
    //private float _verticalDecelModifier = 5f; // modifier to allow the user to decelerate faster in the horizontal axis

    // Boundaries
    private float _leftBound = -11f;
    private float _rightBound = 11f;

    private float _bottomBound = -4f;
    private float _topBound = 6f;

    [SerializeField]
    private bool _wrapPlayer = false; // if true, wrap the player to the opposite side of screen when they exceed the player bounds

    // Start is called before the first frame update
    void Start()
    {
        _spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.LogError("Spawn Manager is NULL in Plaer.Start() !!!");
        }
        //take the current position = new position (0, 0, 0)
        transform.position = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        this.MoveXY();
        this.ShootLaser();
    }

    public void ShootLaser()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _cooldown <= 0f)
        {
            // Instantiate a Laser Object
            Instantiate(_laserPrefab, new Vector3(transform.position.x, transform.position.y + transform.localScale.y * 0.5f, transform.position.z), Quaternion.identity);
            _cooldown = _fireRate;
        }
        else if (_cooldown > 0f) 
        {
            _cooldown = _cooldown - Time.deltaTime;
        }
    }

    public void MoveXY()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Input.GetAxis maps to the different axes defined in the InputManager under Unity>Edit>InputManager.
        float verticalInput = Input.GetAxis("Vertical");

        if (!_wrapPlayer)
        {
            _speedHorizontal = this.CalculateXSpeedNoWrap(horizontalInput, transform.position.x, _speedHorizontal, _thrust, _mass);
            _speedVertical = this.CalculateYSpeedNoWrap(verticalInput, _speedVertical);

            float xCoor = this.CalculateXPositionNoWrap(_speedHorizontal, transform.position.x, _leftBound, _rightBound);
            float yCoor = this.CalculateYPositionNoWrap(_speedVertical, transform.position.y, _bottomBound, _topBound);
            transform.position = new Vector3(xCoor, yCoor, 0);
        }
        else 
        {
            // this.CalculateSpeedWrap(horizontalInput, verticalInput);
            _speedHorizontal = this.CalculateXSpeedWrap(horizontalInput, _speedHorizontal);
            _speedVertical = this.CalculateYSpeedNoWrap(verticalInput, _speedVertical);

            float xCoor = this.CalculateXPositionWrap(_speedHorizontal, transform.position.x, transform.localScale.x, _leftBound, _rightBound);
            float yCoor = this.CalculateYPositionNoWrap(_speedVertical, transform.position.y, _bottomBound, _topBound);

            transform.position = new Vector3(xCoor, yCoor, 0);
        }

    }
    #region Position Calculations
    private float CalculateXPositionWrap(float horizontalSpeed, float xCoor, float xPlayerScale, float leftBound, float rightBound)
    {
        // next we calculate the new xPosition
        xCoor = xCoor + horizontalSpeed * Time.deltaTime;

        // first we must apply the wrap if appropriate
        return this.enforceHorizontalBoundWrap(xCoor, xPlayerScale, leftBound, rightBound);
    }

    private float CalculateYPositionNoWrap(float verticalSpeed, float yCoor, float bottomBound, float topBound)
    {
        if (this.bottomBoundIsViolated(yCoor)) { yCoor = bottomBound; }
        else if (this.topBoundIsViolated(yCoor)) { yCoor = topBound; }

        verticalSpeed = this.enforceVerticalBound(verticalSpeed, yCoor);
        return yCoor + verticalSpeed * Time.deltaTime;
    }

    private float CalculateXPositionNoWrap(float speedHorizontal, float xCoor, float leftBound, float rightBound)
    {
        if (this.leftBoundIsViolated(xCoor)) { xCoor = leftBound; }
        else if (this.rightBoundIsViolated(xCoor)) { xCoor = rightBound; }

        speedHorizontal = this.enforceHorizontalBound(speedHorizontal, xCoor);
        return xCoor + speedHorizontal * Time.deltaTime;
    }
    #endregion

    #region Speed Calculations
    /// <summary>
    /// Given the current horiztonalInput and verticalInput from the end user, calculate the speed of the Player
    /// in the vertical and horizontal direction assuming we have the "wrap" setting toggled off
    /// </summary>
    /// <param name="horizontalInput"></param>
    private float CalculateXSpeedNoWrap(float horizontalInput, float xCoor, float speedHorizontal, float thrust, float mass)
    {
        horizontalInput = this.enforceHorizontalBound(horizontalInput, xCoor);
        speedHorizontal = this.enforceHorizontalBound(speedHorizontal, xCoor); // set to 0 if  at boundary
        return speedHorizontal + (horizontalInput * thrust) / mass; // Acceleration = Force / Mass
    }

    private float CalculateYSpeedNoWrap(float verticalInput, float speedVertical)
    {
        verticalInput = this.enforceVerticalBound(verticalInput, this.transform.position.y);
        speedVertical = this.enforceVerticalBound(speedVertical, transform.position.y);

        return speedVertical + (verticalInput * _thrust) / _mass;
    }

    private float CalculateXSpeedWrap(float horizontalInput, float speedHorizontal)
    {
        return speedHorizontal + (_thrust * horizontalInput) / _mass;
    }
    #endregion

    #region Enforce Bounds With Wrapping
    /// <summary>
    /// If horizontalInput, xCoor, and the leftBound/rightBound given indicate that the player is about to step out of the horizontal bounds,
    /// calculate the new XPosition of the player to wrap them to the other side of the scrteen
    /// </summary>
    /// <param name="horizontalInput"></param>
    /// <param name="xCoor"></param>
    /// <param name="xPlayerScale"></param>
    /// <param name="leftBound"></param>
    /// <param name="rightBound"></param>
    /// <returns></returns>
    private float enforceHorizontalBoundWrap(float xCoor, float xPlayerScale, float leftBound, float rightBound)
    {
        if (this.leftBoundViolatedWrap(xCoor, xPlayerScale, leftBound))
        {
            return rightBound + xPlayerScale * 0.75f;
        }
        else if (this.rightBoundViolatedWrap(xCoor, xPlayerScale, rightBound))
        {
            return leftBound - xPlayerScale * 0.75f;
        }
        return xCoor;
    }

    private float enforceVerticalBoundWrap(float yCoor, float yPlayerScale, float topBound, float bottomBound)
    {
        if (yCoor >= (topBound + yPlayerScale * 0.75f))
        {
            yCoor = bottomBound - yPlayerScale * 0.75f;
        }
        else if (yCoor <= (bottomBound - yPlayerScale * 0.75f))
        {
            yCoor = topBound + yPlayerScale * 0.75f;
        }
        return yCoor;
    }

    /// <summary>
    /// Return true if the player is off the left side of the screen and we need to wrap them right
    /// </summary>
    /// <param name="horizontalInput"></param>
    /// <param name="playerTransform"></param>
    /// <param name="leftBound"></param>
    /// <returns></returns>
    private bool leftBoundViolatedWrap(float xCoor, float xPlayerScale, float leftBound)
    {
        return ((xCoor <= (leftBound - xPlayerScale * 0.75f)));
    }

    /// <summary>
    /// Return true if the player is off the right side of the screen and we need to wrap them left
    /// </summary>
    /// <param name="horizontalInput"></param>
    /// <param name="playerTransform"></param>
    /// <param name="rightBound"></param>
    /// <returns></returns>
    private bool rightBoundViolatedWrap(float xCoor, float xPlayerScale, float rightBound)
    {
        return ((xCoor >= (rightBound + xPlayerScale * 0.75f)));
    }


    #endregion

    #region Enforce Bounds With No Wrapping

    /// <summary>
    /// If player is about to break the left or right bounds, set horizontalInput to zero and return,
    /// otherwise return horizontalInput as is
    /// </summary>
    /// <param name="horizontalInput"> float between -1 and 1 indicating the direction the player is moving</param>
    /// <param name="playerPosition"> Vector3 indicating the players position </param>
    /// <returns></returns>
    private float enforceHorizontalBound(float horizontalInput, float xCoor)
    {
        // player attempting to move left and they fail the leftBound check
        if (horizontalInput < 0 && this.leftBoundIsViolated(xCoor))
        {
            horizontalInput = 0;
        }
        else if (horizontalInput > 0 && this.rightBoundIsViolated(xCoor))
        {
            horizontalInput = 0;
        }
        return horizontalInput;
    }

    /// <summary>
    /// If the player is about to break the top or bottom bounds, set verticalInput to zero and return,
    /// otherwise return verticalInput as is
    /// </summary>
    /// <param name="verticalInput"> float between -1 and 1 indicating direction the player is moving</param>
    /// <param name="playerPosition"> Vector3 indicating the position of the player</param>
    /// <returns></returns>
    private float enforceVerticalBound(float verticalInput, float yCoor)
    {
        if (verticalInput > 0 && this.topBoundIsViolated(yCoor))
        {
            verticalInput = 0;
        }
        else if (verticalInput < 0 && this.bottomBoundIsViolated(yCoor))
        {
            verticalInput = 0;
        }
        return verticalInput;
    }

    private bool leftBoundIsViolated(float xCoor) { return xCoor <= _leftBound; }
    private bool rightBoundIsViolated(float xCoor) { return xCoor >= _rightBound; }
    private bool topBoundIsViolated(float yCoor) { return yCoor >= _topBound; }
    private bool bottomBoundIsViolated(float yCoor) { return yCoor <= _bottomBound; }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player collided with " + other.tag + " applying " + _collisionDamage + " to " + other.tag);

        if (other.tag == "Enemy")
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.ApplyDamage(_collisionDamage);
        }
        else 
        {
            Debug.Log("Player collided with unknown object: " + other.tag);
        }
    }
    public void ApplyDamage(float incomingDamage)
    {
        _health = _health - incomingDamage;
        Debug.Log("Player is taking damage" + incomingDamage);
        if (_health <= 0)
        {
            _spawnManager.OnPlayerDeath();
            Destroy(gameObject);
        }
    }
}
