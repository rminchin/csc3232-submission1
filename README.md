#Rollerball
Start the game by running the MenuController scene in Unity. From here you can navigate through the game intuitively.

###How to Play
You control a ball through a level filled with obstacles. Your task is to avoid the obstacles and collect stars, while also collecting various powerups to aid the gameplay experience. With collected stars, you can buy permanent upgrades which come into play during the 'battle', where you fight with an AI opponent.

###Controls
* Use A and D to move left and right respectively (level only)
* Use F to fire a bullet (battle only)
* Use C to crouch (battle only)
* Use R to reload your weapon (battle only)
* Use Space to jump (both level and battle)

###Text documents
The game makes use of text documents to permanently store progress and purchases.
* `previouslevel.txt` stores the last loaded level so that the user returns to the correct level when returning to the overworld.
* `stars-level1` stores how many stars were collected from level 1, and which specific stars these were so that previously collected stars aren't respawned and available to be collected again. The first line represents the 3 stars in order, with `1` for collected and `0` for not collected, with a space in between. The second line stores the number of stars collected in level 1.
* `shop.txt` stores stars available to spend, stars spent, and if the 3 available upgrades have been purchased or not. Each of these is represented by a single digit integer on separate lines. In order: total stars available to spend (this can be a different number to stars collected); total stars spent on upgrades; if the additional health upgrade was purchased (`1` or `0` for `true` or `false` respectively); if the faster bullets upgrade was purchased (same boolean format applies); and if the smaller enemy magazine size upgrade was purchased (same boolean format applies).

These documents can be manually edited in order to simulate progress which can allow for easier marking - as long as the format remains the same after manual editing.