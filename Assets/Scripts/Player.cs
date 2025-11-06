using System;
using UnityEngine;

public class Player : MonoBehaviour {

  [SerializeField] private PlayerController _playerController;

  [SerializeField] private Animator _animator;

  [SerializeField] private Rigidbody2D _rigidbody2D;

  [SerializeField] private GameObject _sprite;

  [SerializeField] private GameObject _eyeSprite;

  [SerializeField] private ParticleSystem _dustParticles;
  
  [SerializeField] private ParticleSystem _burstDustParticles;
  
  public const string Tag = "Player";

  private void Awake() {
    _playerController.onJumpEvent.AddListener(() => {
      _burstDustParticles.Play();
      
      if (_playerController.jumps == 2) {
        _animator.SetTrigger("Double Jump");
      } else {
        _animator.SetTrigger("Jump");  
      }
    });
  }

  private void RotateSprite(float rotation) {
    var currentRotation = _sprite.transform.eulerAngles;
        
    currentRotation.z = rotation;
        
    _sprite.transform.eulerAngles = currentRotation;
  }

  private void HandleAnimations() {
    // rotate sprite in direction of player movement
    if (_rigidbody2D.linearVelocity.x > 0.01f) {
      RotateSprite(-5f);

      _eyeSprite.transform.localPosition = new Vector3(0.1f, _eyeSprite.transform.localPosition.y, 0f);
    } else if (_rigidbody2D.linearVelocity.x < -0.01f) {
      RotateSprite(5f);
      
      _eyeSprite.transform.localPosition = new Vector3(-0.1f, _eyeSprite.transform.localPosition.y, 0f);
    } else {
      RotateSprite(0f);
      
      _eyeSprite.transform.localPosition = new Vector3(0f, _eyeSprite.transform.localPosition.y, 0f);
    }

    if (_playerController.playerState == PlayerController.PlayerState.WallClinging) {
      if (_playerController.wallClingingDirection == 1) {
        RotateSprite(-5f);
        
        _eyeSprite.transform.localPosition = new Vector3(0.1f, _eyeSprite.transform.localPosition.y, 0f);
      } else {
        RotateSprite(5f);
        
        _eyeSprite.transform.localPosition = new Vector3(-0.1f, _eyeSprite.transform.localPosition.y, 0f);
      }
    }
    
    _animator.SetBool("isWallClinging", _playerController.playerState == PlayerController.PlayerState.WallClinging);

    // play dust particles if player is moving
   if (_playerController.playerState == PlayerController.PlayerState.Jumping ||
                _playerController.playerState == PlayerController.PlayerState.Falling) {
     if (!_dustParticles.isStopped) {
       _dustParticles.Stop();
     }
    } else if (_dustParticles.isStopped) {
      _dustParticles.Play();
    }

    // set x velocity in animator if player is moving
    if (_playerController.playerState == PlayerController.PlayerState.Idle) {
      _animator.SetFloat("x", 0f);
    } else if (_playerController.playerState == PlayerController.PlayerState.Walking) {
      _animator.SetFloat("x", _rigidbody2D.linearVelocity.x);
    } else {
      _animator.SetFloat("x", 0f);
    }

    _animator.SetBool("isFalling", _playerController.playerState == PlayerController.PlayerState.Falling 
                                   || _playerController.playerState == PlayerController.PlayerState.Jumping);
  }
  
  private void Update() {
    HandleAnimations();
  }

  public void Kill() {
    // death animation
    Destroy(gameObject);
  }
  
}
