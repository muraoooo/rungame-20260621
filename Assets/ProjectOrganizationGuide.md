# Project Organization Guide

## Scene Managers

- `GameManager`
  - Put one in the scene.
  - Holds `GameOverController`.
  - Owns game over image, retry image, player damage image, and retry flow.

## Enemy Prefabs

- `Prefabs/SlimeEnemy.prefab`
  - Good prefab candidate.
  - Contains slime sprite, collider, Rigidbody2D, simple side movement, and enemy contact handling.
  - Drag it from `Assets/Prefabs` into the scene to add another slime.

## Sorting Order

- Far background: `-30`
- Main background: `-20`
- Enemy: `10`
- Player: `20`
- Key / important items: `30`
- Foreground parallax objects: around `80`

## Good Future Prefab Candidates

- Player
  - Useful if students need to place the same playable character in multiple scenes.
- GoldenKey
  - Useful if more keys or collectible items will be added.
- Goal object
  - Useful if multiple stages use the same goal behavior.

## Keep As Scene Objects

- Main Camera
- GameManager
- Global Light 2D
- Main background / far background
- Fixed Board ground collider
