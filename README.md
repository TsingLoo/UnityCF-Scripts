# UnityCF-Scripts

Preview: https://www.youtube.com/watch?v=X1rtstNY4rE&t=22s

Unity CF FPS-TPS Template Free Source Code

All the 3D assets (animations/sounds/icons ...) are from CrossFire

Source Code: https://github.com/csm12s/UnityCF-Scripts.git

Framework Features:

1, Data driven design, read and write animations/sounds/animation events/icons via Excel or Json
  Takes about 20 minutes to convert a Half Life .mdl file into a weapon in unity with FPS and TPS animations ready

2, Weapons controller: 1 Base Weapon animator for all weapons, weapon animation/sound data are called via code, use animation override controller to call the animations

3, Player controller: 1 Base animator for player and 6 override animators for 6 kinds of weapons

4, FPS and TPS animations play times are synced based on FPS weapons animations

5, Weapon (Animation) types: Melee1, Melee2, Rifle, Grenade, Sniper, Pistol

6, Player pawn and Enemy pawn using the same base character controller

7, Support ray cast/projectile, semi/full auto fire mode

8, State driven weapon design

9, Using Helpers/Extensions/Enums/Interfaces

10, Bullets/sounds fx using pool system

11, DOTween animation for UI

12, Melee weapon suppors ray cast/collider attack

13, Simple Inventory system that mimic CS1.6 which have 1-5 positions for different weapons

14, Weapon items drop and pickup

Other Weapon Features:

1, Melee combos support 2 or 3 combo animations, usually 2 is used for most FPS games

2, Grenade launcher with trail predict and trail effects

3, Sniper/rifle/grenade launcher uses different FOV/mouse sensitivity/scopes

4, Grenade pre-fire and fire when release trigger button

End

