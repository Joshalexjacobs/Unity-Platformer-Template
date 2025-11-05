using System;
using UnityEngine;

public class Hazard : MonoBehaviour {
  private void OnTriggerEnter2D(Collider2D col) {
    if (col.CompareTag(Player.Tag)) {
      var player = col.gameObject.GetComponent<Player>();
      
      player.Kill();
    }
  }
}
