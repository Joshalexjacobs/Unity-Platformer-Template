using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {

  enum PlayerState {
    Idle, // grounded not moving
    Walking, // grounded moving
    Jumping, // pressed jump, hasn't touched ground yet
    Falling // didn't press jump, velocity of y is negative
  }

  [SerializeField] private PlayerState _playerState = PlayerState.Idle;
  
  private PlayerInputActions _playerActions;

  [SerializeField] private Rigidbody2D _rigidbody2D;

  [SerializeField] private float _movementSpeed = 4;
  
  [SerializeField] private float _jumpStrength = 15;

  private float _jumpTimer = 0f;
  
  private float _graceJumpPeriod = 0f;
  
  [SerializeField] private LayerMask _groundMask;
  
  private Vector2 _movementInput;

  private void Awake() {
    _playerActions = new PlayerInputActions();
    
    _playerActions.Player.Move.performed += context => {
      _movementInput = context.ReadValue<Vector2>();
    };
    
    // replace this with the "mario jump", player has a bigger jump the longer they hold down the jump button
    
    // -- jump -- from https://github.com/Joshalexjacobs/Fantastic-Guac-Endless-Demo/blob/master/player.lua
    // if (love.keyboard.isDown('n') and player.isJumping == false and player.isGrounded and player.jumpLock == false) or (pressX() and player.isJumping == false and player.isGrounded and player.jumpLock == false) then -- when the player hits jump
      // player.isJumping = true
      // player.jumpLock = true
      // player.isGrounded = false
      // player.dy = -player.initVel -- 6 is our current initial velocity
      // jumpTimer = jumpTimerMax // this is like a "jump hold" timer
    // elseif (love.keyboard.isDown('n') and jumpTimer > 0 and player.isJumping) or (pressX() and jumpTimer > 0 and player.isJumping) then
      // player.dy = player.dy + (-0.5) // I think this is a double jump
    // elseif (love.keyboard.isDown('n') == false and player.isJumping) or (pressX() == false and player.isJumping) then -- if the player releases the jump button mid-jump...
      // if player.dy < player.termVel then -- and if the player's velocity has reached the minimum velocity (minimum jump height)...
        // player.dy = player.termVel -- terminate the jump
      // end
      // player.isJumping = false
    // end
    
    _playerActions.Player.Jump.performed += _ => {
      if (_playerState != PlayerState.Jumping && _playerState != PlayerState.Falling) {
        _playerState = PlayerState.Jumping;
        
        _rigidbody2D.AddForce(new Vector2(0f, 40f) * _jumpStrength);

        _jumpTimer = 0.2f;
      } else if (_playerState == PlayerState.Falling && _graceJumpPeriod > 0f) {
        _playerState = PlayerState.Jumping;
        
        _rigidbody2D.AddForce(new Vector2(0f, 40f) * _jumpStrength);

        _jumpTimer = 0.2f;
      }
    };
  }

  private Vector2 HandleControllerMovement() {
    float x = _movementInput.x * _movementSpeed;

    if (_playerState == PlayerState.Idle && (x > 0.01 || x < -0.01)) {
      _playerState = PlayerState.Walking;
    }  else if (_playerState == PlayerState.Walking && x < 0.01 && x > -0.01) {
      _playerState = PlayerState.Idle;
    }
    
    return new Vector2(x, _rigidbody2D.linearVelocity.y);
  }

  private void Update() {
    if (_jumpTimer > 0f)
      _jumpTimer -= Time.deltaTime;
    
    if (_graceJumpPeriod > 0f)
      _graceJumpPeriod -= Time.deltaTime;
  }

  private void FixedUpdate() {
    _rigidbody2D.linearVelocity = HandleControllerMovement();

    if (_rigidbody2D.linearVelocity.y < 0 && _playerState != PlayerState.Jumping && _playerState != PlayerState.Falling) {
      _graceJumpPeriod = 0.15f;
      
      _playerState = PlayerState.Falling;
    }
    
    if (_jumpTimer <= 0f && (_playerState == PlayerState.Falling || _playerState == PlayerState.Jumping)) {
      RaycastHit2D hit = Physics2D.CircleCast(transform.position, GroundRayCastRadius, Vector2.down, GroundRayCastDistance, _groundMask);
      
      if (hit) {
        _playerState = PlayerState.Idle;
      }
    }
  }

  // collision detection

  private const float GroundRayCastRadius = 0.25f;
  private const float GroundRayCastDistance = 0.25f;

  // gizmos
  
  private void OnDrawGizmos() {
    GroundRayCastDetection();
  }

  private void GroundRayCastDetection() {
    Vector2 origin = transform.position;
    Vector2 direction = Vector2.down;
    
    RaycastHit2D hit = Physics2D.CircleCast(origin, GroundRayCastRadius, direction, GroundRayCastDistance, _groundMask);

    Gizmos.color = hit ? Color.green : Color.red;
    Gizmos.DrawLine(origin, origin + direction * GroundRayCastDistance);
    Gizmos.DrawWireSphere(origin + direction * GroundRayCastDistance, GroundRayCastRadius);

    if (hit) {
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(hit.point, GroundRayCastRadius);
    }
  }

  // enable/disable

  private void OnEnable() {
    _playerActions.Enable();
  }

  private void OnDisable() {
    _playerActions.Enable();
  }
  
}
