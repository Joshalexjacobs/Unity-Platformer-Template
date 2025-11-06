# Unity Platformer Template
A quick and dirty Unity project that acts as a jumping off point for anyone looking to make a simple 2D platformer.

## Features
- Basic player state management (Idle, Walking, Jumping, Falling, and WallClinging)
- Variable jump
  - The longer the player holds down the jump button, the higher the jump is
- Wall clinging/wall jumping
  - Wall clinging currently slows the rate of gravity by a little bit more than half, but this can also be easily changed by modifying this line in `PlayerController.cs`:
  https://github.com/Joshalexjacobs/Unity-Platformer-Template/blob/30aab3512f5995c047a54b9f168e761dbc1ba514/Assets/Scripts/PlayerController.cs#L111-L113
- Double jump (easily removable by altering `_maxJumps` from 2 to 1): 
  https://github.com/Joshalexjacobs/Unity-Platformer-Template/blob/30aab3512f5995c047a54b9f168e761dbc1ba514/Assets/Scripts/PlayerController.cs#L38
- Fall through platforms (down button + jump button)
  - Allows the player to drop through platforms using `Physics2D.IgnoreLayerCollision` (can be found in `ControllerUtils.cs`)
- Head bump
  - Kills a jump if the player bumps their head on the ceiling above them
 

## Getting Started
Either clone, download, or fork this repo to use this as a starting point. 

Or just grab the `PlayerController.cs` and `ControllerUtils.cs` scripts and bring them into any existing project (note: you'll need to modify PlayerController to use whatever input system you currently have as well as setup any physics layers for platforming).
