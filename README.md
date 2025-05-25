# TurnClash

A turn-based strategy game built in Unity featuring tactical combat on an isometric grid battlefield. Command your units, outmaneuver your opponent, and achieve victory through strategic positioning and combat.

![Unity](https://img.shields.io/badge/Unity-6000.0.48f1-blue)
![C#](https://img.shields.io/badge/C%23-Latest-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

## 🎮 Game Overview

TurnClash is a tactical turn-based strategy game where two players command armies on an isometric battlefield. Each player takes turns moving their units, attacking enemies, and using defensive strategies to eliminate the opposing force. The game features an intuitive UI system with detailed unit information, hover tooltips, and a comprehensive codex system.

## ✨ Key Features

- **Turn-Based Combat System**: Strategic gameplay with move limits per turn
- **Isometric Grid Battlefield**: Beautiful 3D isometric view with tile-based movement
- **Unit Management**: Select, move, and command units with mouse and keyboard
- **Combat Mechanics**: Attack enemies by moving into their positions
- **Defensive Actions**: Units can defend to become immune to attacks
- **Movement Preview**: Visual indicators show available moves and attack targets
- **Interactive UI**: Detailed unit stats, health bars, and status indicators
- **Hover Tooltips**: Get unit information by hovering over units
- **Codex System**: In-game documentation and lore
- **Camera Controls**: Smooth camera movement and zoom for battlefield overview

## 🎯 Game Rules

### Victory Conditions
- **Objective**: Eliminate all enemy units to win the game
- The game ends when one player has no remaining units

### Turn Structure
- **Player 1** (Blue units) starts first
- Each player gets **4 moves per turn**
- Players alternate turns after using all moves or ending turn early

### Unit Actions
1. **Movement**: Move to adjacent tiles (including diagonally)
2. **Attack**: Move into an enemy unit's position to attack
3. **Defend**: Become immune to attacks until your next turn

### Combat System
- **Damage Calculation**: `Attacker's Attack - Defender's Defense = Damage Dealt`
- **Minimum Damage**: At least 1 damage is always dealt
- **Advance Movement**: When a unit kills an enemy, it automatically moves to the victim's position
- **Defending Units**: Cannot be attacked and cannot move/attack until their next turn

## 🎮 Controls

### Unit Control
- **Left Click**: Select a unit or move to a highlighted tile
- **Right Click**: Deselect current unit
- **Arrow Keys**: Move selected unit (↑↓←→)
- **G Key**: Make selected unit defend
- **X Key**: End turn early
- **Escape**: Clear selection

### Camera Control
- **WASD**: Move camera around the battlefield
- **Mouse Scroll**: Zoom in/out
- **Alt + WASD**: Fast camera movement

### Game Management
- **R Key**: Reset the game
- **Mouse Hover**: View unit information without selecting

## 🎲 Gameplay Tips

1. **Positioning is Key**: Use terrain and unit placement strategically
2. **Defend Wisely**: Defending units can block enemy advances
3. **Plan Your Moves**: You only get 4 moves per turn - make them count
4. **Use Advance Movement**: Killing enemies gives you free repositioning
5. **Control the Center**: Central positions often provide tactical advantages
6. **Watch Enemy Ranges**: Red highlights show which enemies you can attack

## 🛠️ Technical Features

### Architecture
- **Singleton Pattern**: Centralized managers for game systems
- **Event-Driven Design**: Loose coupling between game systems
- **Component-Based**: Modular unit and tile components

### Key Systems
- **Turn Manager**: Handles player turns and move counting
- **Combat Manager**: Tracks combat events and player elimination
- **Unit Selection Manager**: Manages unit selection and UI updates
- **Movement Preview**: Visual feedback for available actions
- **Ground Manager**: Isometric grid generation and management

### UI Systems
- **Selection Info Panel**: Real-time unit statistics display
- **Turn Display**: Current player and move counter
- **Victory Panel**: End-game results and statistics
- **Hover Tooltips**: Contextual unit information

## 🚀 Getting Started

### Prerequisites
- Unity 6000.0.48f1 or later
- Basic understanding of Unity Editor

### Installation
1. Clone this repository
2. Open the project in Unity
3. Load the main scene from `Assets/Scenes/`
4. Press Play to start the game

### Building
1. Go to File → Build Settings
2. Add the main scene to build
3. Select your target platform
4. Click Build

## 🎨 Customization

The game features extensive customization options:

- **Grid Size**: Modify battlefield dimensions
- **Unit Stats**: Adjust health, attack, and defense values
- **Turn Settings**: Change moves per turn and starting player
- **Visual Effects**: Customize colors and highlight effects
- **Camera Settings**: Adjust movement speed and zoom limits

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Units/           # Unit behavior and selection
│   ├── UI/              # User interface components
│   ├── Environment/     # Grid and tile management
│   ├── Camera/          # Camera controls and boundaries
│   ├── Event/           # Input handling and events
│   └── Codex/           # Documentation system
├── Scenes/              # Game scenes
├── Resources/           # Game assets and data
└── Prefabs/             # Reusable game objects
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🎯 Future Enhancements

- [ ] Multiple unit types with unique abilities
- [ ] Terrain effects and obstacles
- [ ] Campaign mode with multiple levels
- [ ] AI opponent for single-player mode
- [ ] Network multiplayer support
- [ ] Save/load game functionality

---

**Enjoy commanding your forces in TurnClash!** ⚔️
