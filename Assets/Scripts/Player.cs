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
    [SerializeField]
    private GameObject _laserPrefab;

    [SerializeField]
    private float _health = 10f;

    [SerializeField]
    private float _collisionDamage = 5f;

    [SerializeField]
    private float _fireRate = 0.1f; // only allow the p[layer to fire once every 0.2 seconds
    private float _cooldown = 0f; // seconds left until player can fire again

    [SerializeField]
    private float _mass = 2000f; // mass of the player in kg

    [SerializeField]
    private float _thrust = 100f; // available force of thrusters in Newtons

    private float _speedHorizontal = 0f; // speed at which the player moves horizontally
    private float _speedVertical = 0f; // speed at which the player is currently moving vertically

    [SerializeField]
    private float _horizontalAccelModifier = 2f; // modifier to allow user to accelerate faster or than the normal rate on the horizontal axis

    [SerializeField]
    private float _verticalAccelModifier = 2f; // modifier to allow the user to accelerate faster than the normal rate in the vertical axis

    [SerializeField]
    private float _horizontalDecelModifier = 5f; // modifier to allow the user to decelerate faster in the horizontal axis

    [SerializeField]
    private float _verticalDecelModifier = 5f; // modifier to allow the user to decelerate faster in the horizontal axis

    private float _leftBound = -11f;
    private float _rightBound = 11f;

    private float _bottomBound = -4f;
    private float _topBound = 6f;

    [SerializeField]
    private bool _wrapPlayer = false; // if true, wrap the player to the opposite side of screen when they exceed the player bounds

    // Start is called before the first frame update
    void Start()
    {
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
            horizontalInput = this.enforceHorizontalBound(horizontalInput, transform.position.x);
            verticalInput = this.enforceVerticalBound(verticalInput, transform.position.y);
        }
        else 
        {
            float playerXPosition = this.enforceHorizontalBoundWrap(_speedHorizontal, transform.position.x, transform.localScale.x, _leftBound, _rightBound);
            float playerYPosition = this.enforceVerticalBoundWrap(_speedVertical, transform.position.y, transform.localScale.y, _topBound, _bottomBound);

            transform.position = new Vector3(playerXPosition, playerYPosition, transform.position.z);
        }

        _speedHorizontal = _speedHorizontal + (horizontalInput * _thrust) / _mass; // Acceleration = Force / Mass
        _speedVertical = _speedVertical + (verticalInput * _thrust) / _mass; // Acceleration = force / Mass
        transform.Translate(new Vector3(Time.deltaTime * _speedHorizontal, Time.deltaTime * _speedVertical, 0));
    }

    // Acceleration Modifiers are only relevant if we use arcade physics instead of newtonian physics
    ///// <summary>
    ///// Applies horizontalAccelModifier to the users keyboard input to let them accelerate more quickly in the x direction.
    ///// Also stop movement in the x direction immediately if the user releases a horizontal movement key
    ///// </summary>
    ///// <param name="input"></param>
    ///// <returns></returns>
    //public float ApplyHorizontalAccelModifier(float input)
    //{
    //    Debug.Log(Input.GetKey(KeyCode.D));
    //    if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) ||
    //        Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
    //    {
    //        return Mathf.Clamp(input * _horizontalAccelModifier, -1, 1);
    //    }
    //    else 
    //    {
    //        return Math.Abs(input / _horizontalDecelModifier) > 0.2 ? input / _horizontalDecelModifier : 0;
    //    }
    //}

    ///// <summary>
    ///// Applies horizontalAccelModifier to the users keyboard input to let them accelerate more or less quickly in the Y direction.
    ///// Also stop the user immediately when they release the key
    ///// </summary>
    ///// <param name="input"></param>
    ///// <returns></returns>
    //public float ApplyVerticalAccelModifier(float input)
    //{
    //    if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) ||
    //        Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
    //    {
    //        return Mathf.Clamp(input * _verticalAccelModifier, -1, 1);
    //    }
    //    else
    //    {
    //        return Math.Abs(input / _verticalDecelModifier) > 0.2 ? input / _verticalDecelModifier : 0;
    //    }
    //}

    #region PlayerBoundsWrap
    private float enforceHorizontalBoundWrap(float horizontalInput, float xCoor, float xPlayerScale, float leftBound, float rightBound)
    {
        if (this.leftBoundViolatedWrap(horizontalInput, xCoor, xPlayerScale, leftBound))
        {
            return rightBound + xPlayerScale * 0.75f;
        }
        else if (this.rightBoundViolatedWrap(horizontalInput, xCoor, xPlayerScale, rightBound))
        {
            return leftBound - xPlayerScale * 0.75f;
        }
        return xCoor;
    }

    private float enforceVerticalBoundWrap(float verticalInput, float yCoor, float yPlayerScale, float topBound, float bottomBound)
    {
        if (verticalInput > 0 && yCoor >= (topBound + yPlayerScale * 0.75f))
        {
            yCoor = bottomBound - yPlayerScale * 0.75f;
        }
        else if (verticalInput < 0 && yCoor <= (bottomBound - yPlayerScale * 0.75f))
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
    private bool leftBoundViolatedWrap(float horizontalInput, float xCoor, float xPlayerScale, float leftBound)
    {
        return (horizontalInput < 0 && (xCoor <= (leftBound - xPlayerScale * 0.75f)));
    }

    /// <summary>
    /// Return true if the player is off the right side of the screen and we need to wrap them left
    /// </summary>
    /// <param name="horizontalInput"></param>
    /// <param name="playerTransform"></param>
    /// <param name="rightBound"></param>
    /// <returns></returns>
    private bool rightBoundViolatedWrap(float horizontalInput, float xCoor, float xPlayerScale, float rightBound)
    {
        return (horizontalInput > 0 && (xCoor >= (rightBound + xPlayerScale * 0.75f)));
    }


    #endregion

    #region PlayerBoundsNoWrap

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

    private void OnTriggerEnter(Collider other)
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
            Destroy(gameObject);
        }
    }
}
