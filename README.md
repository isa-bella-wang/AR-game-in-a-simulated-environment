# AR Game: Dart Throw

**Video**
<a> https://drive.google.com/drive/folders/1wOXxv4OWvRvY6L_YvaedSUTplKcdar5b?usp=sharing </a>
  
**Game Overview**
The game creates a virtual dartboard in AR space where players take turns throwing darts by tapping on the screen. It tracks scores and manages game flow.

**Key Technical Features**
Smooth Transitions: Uses progress-based animation with smoothing functions
Object Pooling: Manages dart objects with a Queue to handle multiple throws
Event-Driven Architecture: Listens for AR board placement events
Physics Integration: Uses raycasting for accurate hit detection

**Game Flow & State Management**
The game operates in three modes:
1. Main Mode: Waiting for player input, dart in hand
2. MainMotion Mode: Dart is in flight after being thrown
3. Dart Mode: (Not fully implemented) Likely for when dart is stationary on board

**Player Turn System**
Alternates between Player 1 and Player 2
Each player gets 3 throws per turn (implemented via ThrowNumber enum)
Automatically switches players after 3 throws
Tracks scores separately for each player

**Dart Throwing Mechanism**
1. Input Detection: Listens for mouse clicks (touch screen in AR)
2. Raycasting: Shoots a ray from camera to detect where player tapped
3. Dart Creation: Instantiates a dart GameObject at the hit position
4. Animation: Plays "Throw" animation when dart is launched
5. Queue Management: Keeps track of all thrown darts

**Scoring System**
The decodeScore() function calculates points based on:
1. Bullseye (offset < 0.1f): 50 points
2. Second Ring (offset < 0.2f): 25 points
3. Third Ring (0.25f-0.35f): 10 points (triple ring)
4. Outer Ring (>0.45f): 15 points (double ring)
5. Main Area: 1 point
6. Miss (>0.5f): 0 points

**Visual Feedback & UI**
1. Updates score displays for both players
2. Shows current player's turn and recent score
3. Displays status messages
4. Plays "Drop" animation when darts are cleared between turns


