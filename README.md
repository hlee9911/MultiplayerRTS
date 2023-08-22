# **Mutiplayer RTS Game using Mirror & Steamwork**

A Gamedev.tv 4-players Free For All RTS game created in Unity and Mirror.

Place buildings to gain resources and produce units. Controls units to fight against enemy and ultimately defeat your enemies to win.

Builds available at here: [link](https://drive.google.com/drive/folders/1RJvu4N2wnsUqqrMoWh2mFDhMsIgDro9M?usp=sharing)

## Gameplay Manual

### To Start
 - A host must create a room and the other players can join the room using Host's Address IP.
 - If more than two players are in the lobby, a host can start the game.

### Buildings

 - Drag the building icons located at bottom right corner and place it on desired location (must be withing certain radius with your other buildings).
 - When the location you want to spawn is unavailable, the preview building instance will be red, when it is green color, you can place the building at that location.
 - There are four buildings in this games:
 - Unit Base:
    - The most important building in which is spawned automatically for you when you enter the game scene. You will lose if this building is destroyed. Must protect this building at all cost. You cannot spawn this building.
 - Resource Generator (Icon with 3) (cost: 150)
    - A buildling that generates 20 resources every 1.5 seconds. Should be the first building to spawn. Resources won't be generated if you don't have any resource generator at all.
 - Tank Unit Spawner (Icon with 2) (cost: 125)
    - A building that allows you to produce a tank unit that is equipped with a heavy armor and stronger attack power but moves slow. Great for head-on fights.
 - Small Unit spawner (Icon with 1) (cost: 75)
    - A building that allows you to produce a small unit that is equipped with a light armor but moves very fast. Great for harrasing and guerilla warfare.

### Units

 - Click the spawner building to produce units. Can queue upto 5 units at a time.
 - To select, you can either drag or select using L-shift key to add a unit to your currently selected units list
 - Small Unit (cost: 30)
    - Equipped with a light armor but moves very fast. Great for harrasing and guerilla warfare.
 - Tank Unit (cost: 60)
    - Equipped with a heavy armor and stronger attack power but moves slow. Great for head-on fights.
