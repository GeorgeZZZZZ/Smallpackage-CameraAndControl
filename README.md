# Smallpackage-CameraAndControl

##note
>To add Camera and RPG package:
- Need add key word "Rotate" and "Run" in to Project Settings > Input
>To add RTS package need assets shown below:
- A* Pathfinding Project
- Behavior Designer - Behavior Trees for Everyone
- Behavior Designer - Movement Pack
- Behavior Designer - Integrations - A* Pathfinding Integration
- _Customize_BehaviorTask

##log
>2019.02.12
- separated this project and make organize more simpler for use to orther project. Rewrite and delete unnecessary Astar pathfinding code.

>2019.05.09
- add prefabs for cam and player rpg
- change customer editor file order to avoid error when building
- add namespace to cam scrips

>2019.06.04
- add player controller rpg interface to separate functionality for other script to use 
- camera_controller.cs now follow camera_follow_target.cs not Player_Controller_RPG.cs