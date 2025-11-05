using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

public static class ControllerUtils {
  
  private const int PlayerLayer = 3;
  
  public const int PlatformLayer = 7;

  public static void IgnorePlatformCollision(bool ignore = true) {
    Physics2D.IgnoreLayerCollision(PlayerLayer, PlatformLayer, ignore);
  }
  
  public static ButtonControl[] InitializeButtonControls(ReadOnlyArray<InputControl> controls) {
    ButtonControl[] buttonControls = new ButtonControl[controls.Count];
    
    for (int i = 0; i < controls.Count; i++) {
      buttonControls[i] = (ButtonControl) controls[i];      
    }

    return buttonControls;
  }
  
  public static bool IsButtonDown(ButtonControl[] buttonControls) {
    foreach (ButtonControl buttonControl in buttonControls) {
      if (buttonControl != null && buttonControl.isPressed)
        return true;
    }

    return false;
  }
  
}
