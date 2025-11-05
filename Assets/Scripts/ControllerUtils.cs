using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

public static class ControllerUtils {
  
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
