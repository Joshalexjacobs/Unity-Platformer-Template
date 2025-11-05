using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour {
  public enum PlayerState {
    Idle, // grounded not moving
    Walking, // grounded moving
    Jumping, // pressed jump, hasn't touched ground yet
    Falling, // didn't press jump, velocity of y is negative
    WallClinging, // player is clinging to a wall
  }

  public PlayerState playerState = PlayerState.Idle;

  public UnityEvent onJumpEvent; 
  
  private PlayerInputActions _playerActions;
  
  private ButtonControl[] _jumpButtonControls;

  [SerializeField] private Rigidbody2D _rigidbody2D;
  
  [SerializeField] private BoxCollider2D _boxCollider2D;

  [SerializeField] private float _movementSpeed = 4;
  
  [SerializeField] private float _jumpStrength = 15;

  private float _jumpTimer = 0f; // time before you can be considered on the ground again

  private float _jumpLength = 0f; // how high you can jump depending on how long you hold down jump 

  private bool _canTriggerNewJump = true; // forces the player to let go of the jump button in order to jump again

  public int jumps = 0;

  private int _maxJumps = 2;
  
  private float _graceJumpPeriod = 0f; // grace period for jumping after walking off a ledge

  public int wallClingingDirection = 0;

  private float _wallClingTimer = 0f;

  private bool _headBump = false;
  
  [SerializeField] private LayerMask _groundMask;
  
  [SerializeField] private LayerMask _wallClingMask;
  
  [SerializeField] private LayerMask _headBumpMask;

  private int _layerBeneath = -1;
  
  private Vector2 _movementInput;

  private void Awake() {
    _playerActions = new PlayerInputActions();
    
    _playerActions.Player.Move.performed += context => {
      _movementInput = context.ReadValue<Vector2>();
    };
    
    _playerActions.Player.Jump.performed += context => {
      if (_wallClingTimer > 0f) return;
      
      if (playerState == PlayerState.WallClinging) {
        _rigidbody2D.AddForce(new Vector2(wallClingingDirection == 1 ? 25 : -25, 50) * _jumpStrength);

        _wallClingTimer = 0.15f;

        TriggerJump();

        return;
      }
      
      if (_layerBeneath == ControllerUtils.PlatformLayer && Math.Abs(_movementInput.y - (-1)) < 0.01f) {
        playerState = PlayerState.Falling;
        
        _canTriggerNewJump = false;
        
        _jumpTimer = 0.5f;

        ControllerUtils.IgnorePlatformCollision();
      } else if (playerState == PlayerState.Jumping  && jumps < _maxJumps) {
        _rigidbody2D.AddForce(new Vector2(0f, 0.5f) * _jumpStrength);

        TriggerJump();
      }
    };

    _jumpButtonControls = ControllerUtils.InitializeButtonControls(_playerActions.Player.Jump.controls);
  }

  private void TriggerJump() {
    jumps++;
    
    onJumpEvent.Invoke();
    
    _canTriggerNewJump = false;
    
    playerState = PlayerState.Jumping;
        
    _jumpTimer = 0.2f;
    
    _jumpLength = 0.3f;
  }

  private float HandleJump() {
    if (playerState == PlayerState.WallClinging) {
      return _rigidbody2D.linearVelocity.y / 1.5f;
    }

    if (_headBump) {
      return -Mathf.Abs(_rigidbody2D.linearVelocity.y);
    }
    
    if (ControllerUtils.IsButtonDown(_jumpButtonControls)) {
      if (_canTriggerNewJump && playerState != PlayerState.Jumping && playerState != PlayerState.Falling) {
        TriggerJump();

        return _jumpStrength;
      } else if (playerState == PlayerState.Jumping && _jumpLength > 0f) {
        return _jumpStrength;
      } else if (_canTriggerNewJump && playerState == PlayerState.Falling && _graceJumpPeriod > 0f) {
        TriggerJump();
        
        return _jumpStrength;
      }
    } else if (playerState == PlayerState.Jumping) {
      _jumpLength = 0f;
    } else if (_canTriggerNewJump == false && (playerState == PlayerState.Idle || playerState == PlayerState.Walking)) {
      _canTriggerNewJump = true;
    }

    return _rigidbody2D.linearVelocity.y;
  }
  
  private Vector2 HandleControllerMovement() {
    float x = _movementInput.x * _movementSpeed;

    if (playerState == PlayerState.Idle && (x > 0.01 || x < -0.01)) {
      playerState = PlayerState.Walking;
    } else if (playerState == PlayerState.Walking && x < 0.01 && x > -0.01) {
      playerState = PlayerState.Idle;
    }
    
    return new Vector2(x, HandleJump());
  }

  private void Update() {
    if (_jumpTimer > 0f)
      _jumpTimer -= Time.deltaTime;

    if (_jumpLength > 0f) 
      _jumpLength -= Time.deltaTime;

    if (_graceJumpPeriod > 0f)
      _graceJumpPeriod -= Time.deltaTime;

    if (_wallClingTimer > 0f)
      _wallClingTimer -= Time.deltaTime;
  }

  private RaycastHit2D CastInDirection(float radius, Vector2 direction, float distance, LayerMask mask) {
    return Physics2D.CircleCast(transform.position, radius, direction, distance, mask);
  }

  private void FixedUpdate() {
    if (_wallClingTimer <= 0f)
      _rigidbody2D.linearVelocity = HandleControllerMovement();

    if (_rigidbody2D.linearVelocity.y < 0 && playerState != PlayerState.Jumping && playerState != PlayerState.Falling) {
      _graceJumpPeriod = 0.15f;
      
      playerState = PlayerState.Falling;
    }
    
    if (_jumpTimer <= 0f && (playerState == PlayerState.Falling || playerState == PlayerState.Jumping)) {
      RaycastHit2D bottomHit = CastInDirection(RayCastRadius, Vector2.down, RayCastDistance, _groundMask);
      
      RaycastHit2D topHit = CastInDirection(RayCastRadius, Vector2.up, RayCastDistance, _headBumpMask);
      
      RaycastHit2D rightHit = CastInDirection(RayCastRadius, Vector2.right, RayCastDistance, _wallClingMask);
      
      RaycastHit2D leftHit = CastInDirection(RayCastRadius, Vector2.left, RayCastDistance, _wallClingMask);

      if (rightHit && (rightHit.collider.CompareTag("Platform") || rightHit.collider.CompareTag("Wall")) && Math.Abs(_movementInput.x - 1) < 0.1f) {
        wallClingingDirection = -1;
        
        playerState = PlayerState.WallClinging;
        
        jumps = 0;
      } else if (leftHit && (leftHit.collider.CompareTag("Platform") || leftHit.collider.CompareTag("Wall")) && Math.Abs(_movementInput.x - (-1)) < 0.1f) {
        wallClingingDirection = 1;
        
        playerState = PlayerState.WallClinging;
        
        jumps = 0;
      } else {
        wallClingingDirection = 0;
      }
      
      if (bottomHit) {
        _headBump = false;
        
        _layerBeneath = bottomHit.collider.gameObject.layer;

        jumps = 0;
        
        ControllerUtils.IgnorePlatformCollision(false);

        playerState = PlayerState.Idle;
      } else {
        _layerBeneath = -1;
      }

      if (topHit) { // kill the current jump
        _headBump = true;
      }
    }
  }

  // collision detection

  private const float RayCastRadius = 0.3f;
  private const float RayCastDistance = 0.3f;

  // gizmos
  
  private void OnDrawGizmos() {
    RaycastHit2D bottomHit = CastInDirection(RayCastRadius, Vector2.down, RayCastDistance, _groundMask);
      
    DrawHit(bottomHit, Vector2.down);
    
    RaycastHit2D rightHit = CastInDirection(RayCastRadius, Vector2.right, RayCastDistance, _wallClingMask);
      
    DrawHit(rightHit, Vector2.right);
    
    RaycastHit2D leftHit = CastInDirection(RayCastRadius, Vector2.left, RayCastDistance, _wallClingMask);
    
    DrawHit(leftHit, Vector2.left);
    
    RaycastHit2D topHit = CastInDirection(RayCastRadius, Vector2.up, RayCastDistance, _headBumpMask);
    
    DrawHit(topHit, Vector2.up);
  }

  private void DrawHit(RaycastHit2D hit, Vector2 direction) {
    Gizmos.color = hit ? Color.green : Color.red;
    Gizmos.DrawLine(transform.position, (Vector2) transform.position + direction * RayCastDistance);
    
    Gizmos.DrawWireSphere((Vector2) transform.position + direction * RayCastDistance, RayCastRadius);

    if (hit) {
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(hit.point, RayCastRadius);
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
