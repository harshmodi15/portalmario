# Game Architecture Documentation

## Overview
This is a 2D game built in Unity that features a player character who can interact with portals, boxes, and enemies. The game mechanics are inspired by Portal-style gameplay with additional combat elements.

## Core Systems

### Player System (`PlayerController.cs`)
The player character has the following capabilities:

#### Movement
- Basic 2D platformer movement with horizontal motion and jumping
- Ground checking using Physics2D.OverlapCircle
- Movement speed and jump force are configurable
- Sprite automatically flips based on movement direction

#### Interaction System
- Can pick up and drop boxes using the 'E' key (configurable)
- Interaction range is visualized in the editor
- Can damage enemies through collision based on velocity

#### Key Properties
- `moveSpeed`: Controls horizontal movement speed
- `jumpForce`: Determines jump height
- `interactionRange`: Range for picking up boxes
- `groundCheckRadius`: Size of ground detection area

### Enemy System (`Enemy.cs`)
Enemies feature a patrol-based behavior system with health management.

#### Behavior
- Patrols within a defined area
- Automatically turns around at patrol boundaries
- Only moves when grounded
- Visual feedback on damage (flashes red)

#### Properties
- `maxHealth`: Enemy's initial health
- `moveSpeed`: Patrol movement speed
- `patrolDistance`: Distance to patrol from spawn point
- `groundCheckDistance`: Distance to check for ground

#### Events
- `onDeath`: Unity Event triggered when enemy dies
- Death can be caused by player collision with sufficient velocity

### Portal System
The portal system consists of two main components:

#### Portal Class (`Portal.cs`)
- Handles individual portal behavior
- Manages teleportation logic
- Maintains portal colors (blue/orange)
- Handles cooldown between teleports
- Preserves object momentum through portals

#### Portal Manager (`PortalManager.cs`)
- Controls portal creation and placement
- Manages portal pairs
- Handles mouse input for portal placement
- Validates portal placement positions

Key Features:
- Left click creates blue portal
- Right click creates orange portal
- Maximum of two portals at once
- Minimum distance required between portals
- Portals can teleport both players and boxes

## Layer Setup
The game uses the following important layers:
- Ground Layer: Used for ground detection
- Box Layer: Used for interactable boxes
- Enemy Layer: Used for enemy collision detection
- Portal Placement Layer: Controls where portals can be placed

## Development Guidelines

### Adding New Features
1. Follow the existing component-based architecture
2. Use SerializeField for editor-configurable properties
3. Implement visualization using OnDrawGizmosSelected where appropriate
4. Add comments for complex logic

### Best Practices
1. Use the built-in event system for component communication
2. Maintain separation of concerns between systems
3. Use physics layers appropriately for collision detection
4. Implement proper cooldowns for actions (e.g., portal teleportation)

### Debug Visualization
The following debug visualizations are available in the Scene view:
- Ground check areas (green)
- Interaction ranges (blue)
- Enemy patrol areas (yellow)
- Enemy ground checks (green)

## Common Tasks

### Adding a New Enemy Type
1. Create a new script inheriting from the base Enemy class
2. Override behavior methods as needed
3. Configure health and movement parameters
4. Add appropriate collision detection

### Modifying Portal Behavior
1. Adjust portal settings in PortalManager
2. Modify teleportation logic in Portal class
3. Update portal placement validation if needed

### Extending Player Capabilities
1. Add new variables to PlayerController
2. Implement new input handling in Update()
3. Add physics interactions in FixedUpdate() if needed
4. Update interaction system as required