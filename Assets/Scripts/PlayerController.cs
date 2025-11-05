using System;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour {

  enum PlayerState {
    Idle, // grounded not moving
    Walking, // grounded moving
    Jumping, // pressed jump, hasn't touched ground yet
    Falling // didn't press jump, velocity of y is negative
  }

  [SerializeField] private PlayerState _playerState = PlayerState.Idle;
  
  private PlayerInputActions _playerActions;
  
  private ButtonControl[] _jumpButtonControls;

  [SerializeField] private Rigidbody2D _rigidbody2D;

  [SerializeField] private float _movementSpeed = 4;
  
  [SerializeField] private float _jumpStrength = 15;

  private float _jumpTimer = 0f; // time before you can be considered on the ground again

  private float _jumpLength = 0f; // how high you can jump depending on how long you hold down jump 

  private bool _canTriggerNewJump = true; // forces the player to let go of the jump button in order to jump again
  
  private float _graceJumpPeriod = 0f; // grace period for jumping after walking off a ledge
  
  [SerializeField] private LayerMask _groundMask;

  private int _layer = -1;
  
  private Vector2 _movementInput;

  private void Awake() {
    _playerActions = new PlayerInputActions();
    
    _playerActions.Player.Move.performed += context => {
      _movementInput = context.ReadValue<Vector2>();
    };
    
    _playerActions.Player.Jump.performed += context => {
      if (_layer == ControllerUtils.PlatformLayer && Math.Abs(_movementInput.y - (-1)) < 0.01f) {
        _playerState = PlayerState.Falling;
        
        _canTriggerNewJump = false;
        
        _jumpTimer = 0.5f;

        ControllerUtils.IgnorePlatformCollision();
      }
    };

    _jumpButtonControls = ControllerUtils.InitializeButtonControls(_playerActions.Player.Jump.controls);
  }

  private void TriggerJump() {
    _canTriggerNewJump = false;
    
    _playerState = PlayerState.Jumping;
        
    _jumpTimer = 0.2f;
    
    _jumpLength = 0.3f;
  }

  private float HandleJump() {
    if (ControllerUtils.IsButtonDown(_jumpButtonControls)) {
      if (_canTriggerNewJump && _playerState != PlayerState.Jumping && _playerState != PlayerState.Falling) {
        TriggerJump();

        return _jumpStrength;
      } else if (_playerState == PlayerState.Jumping && _jumpLength > 0f) {
        return _jumpStrength;
      } else if (_canTriggerNewJump && _playerState == PlayerState.Falling && _graceJumpPeriod > 0f) {
        TriggerJump();
        
        return _jumpStrength;
      }
    } else if (_playerState == PlayerState.Jumping) {
      _jumpLength = 0f;
    } else if (_canTriggerNewJump == false && (_playerState == PlayerState.Idle || _playerState == PlayerState.Walking)) {
      _canTriggerNewJump = true;
    }

    return _rigidbody2D.linearVelocity.y;
  }
  
  private Vector2 HandleControllerMovement() {
    float x = _movementInput.x * _movementSpeed;

    if (_playerState == PlayerState.Idle && (x > 0.01 || x < -0.01)) {
      _playerState = PlayerState.Walking;
    } else if (_playerState == PlayerState.Walking && x < 0.01 && x > -0.01) {
      _playerState = PlayerState.Idle;
    }
    
    return new Vector2(x, HandleJump());
  }

  private void Update() {
    if (_jumpTimer > 0f)
      _jumpTimer -= Time.deltaTime;

    if (_jumpLength > 0f) {
      _jumpLength -= Time.deltaTime;

      if (_jumpLength <= 0f && _playerState == PlayerState.Jumping) {
        _playerState = PlayerState.Falling;
      }
    }
    
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
        _layer = hit.collider.gameObject.layer;
        
        ControllerUtils.IgnorePlatformCollision(false);

        _playerState = PlayerState.Idle;
      }
      else {
        _layer = -1;
      }
    }
  }

  // collision detection

  private const float GroundRayCastRadius = 0.3f;
  private const float GroundRayCastDistance = 0.3f;

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
