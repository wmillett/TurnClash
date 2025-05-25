# Unity Game Project

## Description
This project is a Unity-based game assignment consisting of two main parts: a UI component resembling a simplified Codex Panel from Suzerain, and a 3D turn-based strategy game. The project is developed using Unity 6000.0.48f1 LTS and C#.

## Duration
The maximum duration for this assignment is 7 days.

## Requirements
- Unity 6000.0.48f1 LTS
- C#
- GitHub for version control

## Evaluation Criteria
- Prioritization and time management
- Code quality
- Performance and code optimization
- Future-proofing
- Feature completion
- UI usability and responsiveness
- Early submission
- Taking initiative
- Version control

## Part 1: UI
### Description
Create a simplified version of the Codex Panel from Suzerain that can be shown/hidden with a navigation menu toggle on the right side of the screen. The panel displays data serialized from a JSON file for multiple entries.

### Features
- **Navigation**: Toggle the Codex Panel on/off from the right side of the screen.
- **Categories**: Four categories - Characters, History, Locations, Organizations.
- **Pages**: Menu Page and Entry Page.
  - **Menu Page**: Category toggles and click-to-expand topic toggles.
  - **Entry Page**: Title, description, and optional image for a codex entry.
- **Back Button**: Navigate back to the Menu Page from the Entry Page.

### Tips
- Use serialization/deserialization for handling data.
- Handle error states due to data.
- Implement search functionality (optional).
- Use tweening libraries for better UI animations.
- Ensure UI works with different resolutions.

## Part 2: Gameplay
### Description
Create a simple 3D turn-based strategy game played on tiles with tokens between 2 players on the same PC.

### Features
- **Tiles and Tokens**: Tiles can be squares, hexagonal, or any shape. Tokens have different models and are differentiated by colors.
- **Players**: Two players alternate turns.
- **Token Parameters**: Health, Attack, and Defence.
- **Actions**: Move, Attack, Defend, Cancel action.
- **Action Points**: Each player gets 4 Action Points per turn.
- **Combat**: Calculate damage based on Attack and Defence parameters.
- **UI Elements**: Health, Attack, Defence parameters, Action Points, Current Player, End Turn button, Current Turn Number, Win/Lose.

### Tips
- Focus on the most important features first.
- Configurable number of players and token placement is a plus.
- Add a reset button to reset the game (optional).
- Ensure visual feedback for all actions.

## GitHub Repository
[Provide the link to your GitHub repository here]

## How to Run
1. Clone the repository.
2. Open the project in Unity 6000.0.48f1 LTS.
3. Build and run the project.

## Contact
For any questions or feedback, please contact [Your Contact Information].
