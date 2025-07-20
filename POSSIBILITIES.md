# Действия персонажа и их параметры

## Walk

[//]: # (![PlayerColliderExample]&#40;Assets/ForGithub/640_360_25frames/1_Walk.gif&#41;)
[//]: # (<img src="Assets/ForGithub/640_360_25frames/1_Walk.gif" alt="Walk">)

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/1_Walk.gif)

### Walk Parameters


| Parameter name      	| Description                                           	|
|---------------------	|-------------------------------------------------------	|
| WalkSpeed           	| Character speed while walking                         	|
| WalkAcceleration    	| Acceleration of the character while walking           	|
| WalkDeceleration    	| Slowing down the character while walking              	|
| WalkAirAcceleration 	| Acceleration of the character in the air from walking 	|
| WalkAirDeceleration 	| Slowing down a character in the air from walking      	|


## Run

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/2_Run.gif)

### Run Parameters

| Parameter name        | Description                                           |
|-----------------------|-------------------------------------------------------|
| RunSpeed              | Character speed while running                         |
| RunAcceleration       | Acceleration of the character while running           |
| RunDeceleration       | Slowing down the character while running              |
| RunAirAcceleration    | Acceleration of the character in the air from running |
| RunAirDeceleration    | Slowing down a character in the air from running      |

## Jump 

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/3_Jump.gif)


### Jump Parameters

| Parameter name                | Description                                    |
|-------------------------------|------------------------------------------------|
| MaxJumpHeight                 | Maximum jump height                            |
| MinJumpHeight                 | Minimum jump height                            |
| TimeTillJumpApex              | Time to jump peak                              |
| JumpHeightCompensationFactor  | Jump height compensation factor                |
| JumpGravityMultiplayer        | Gravity during jump (upward flight)           |
| FallGravityMultiplayer        | Gravity during fall (downward flight)         |

## Variable Jump 

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/4_VariableJump.gif)

## Multi Jump

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/5_MultiJump.gif)

### MultiJump Parameters

| Parameter name    | Description                    |
|-------------------|--------------------------------|
| MaxNumberJumps    | Maximum number of jumps        |


## Buffer Jump

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/6_BufferJump.gif)

### Buffer Jump Parameters

| Parameter name    | Description                                              |
|-------------------|----------------------------------------------------------|
| BufferTime        | Timer for buffer jump (works for WallJump)              |

## Coyote Jump

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/7_CoyoteJump.gif)

### Coyote Jump Parameters

| Parameter name    | Description                    |
|-------------------|--------------------------------|
| CoyoteTime        | Timer for coyote jump          |

## Dash

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/8_Dash.gif)

### Dash Parameters

| Parameter name              | Description                                    |
|-----------------------------|------------------------------------------------|
| DashVelocity                | Dash force                                     |
| MaxNumberDash               | Maximum number of dashes                       |
| DashTime                    | Timer for dash                                 |
| DashFallSpeed               | Speed during fall after dash                  |
| DashFallAirAcceleration     | Air acceleration after dash                   |
| DashFallAirDeceleration     | Air deceleration after dash                   |

## Crouch

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/9_Crouch.gif)

### Crouch Parameters

| Parameter name         | Description                                    |
|------------------------|------------------------------------------------|
| CrouchMoveSpeed        | Movement speed while crouching                 |
| CrouchAcceleration     | Character acceleration while crouching         |
| CrouchDeceleration     | Character deceleration while crouching         |

## Crouch Roll

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/10_CrouchRoll.gif)

### Crouch Roll Parameters

| Parameter name         | Description                    |
|------------------------|--------------------------------|
| CrouchRollVelocity     | Crouch roll force              |
| CrouchRollTime         | Timer for crouch roll          |

## Wall Slide

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/11_JumpSlide.gif)

### Wall Slide Parameters

| Parameter name            | Description                                                          |
|---------------------------|----------------------------------------------------------------------|
| StartVelocityWallSlide    | Initial sliding velocity                                             |
| WallSlideSpeedMax         | Maximum allowed sliding speed                                        |
| WallSlideDeceleration     | Deceleration while sliding                                           |
| WallFallTime              | Time to detach from wall sideways (detach, not jump)               |

## Wall Jump

![PlayerColliderExample](Assets/ForGithub/640_360_25frames/12_WallJump.gif)

### Wall Jump Parameters

| Parameter name              | Description                                                |
|-----------------------------|-------------------------------------------------------------|
| WallJumpClimb (X, Y)        | Jump towards the wall                                      |
| WallJumpOff (X, Y)          | Jump off the wall (simple jump button on wall)           |
| WallLeap (X, Y)             | Jump to the opposite side of the wall                     |
| WallJumpTime                | Time for input command after pressing jump button on wall |
| WallFallSpeed               | Speed during fall after wall jump                         |
| WallFallAirAcceleration     | Acceleration during fall after wall jump                  |
| WallFallAirDeceleration     | Deceleration during fall after wall jump                  |
















