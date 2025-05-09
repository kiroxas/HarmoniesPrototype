# HarmoniesPrototype
Prototype of the Harmonies boardgame in Unity

![HarmoniesProto](https://github.com/user-attachments/assets/e0c6f557-5c8e-4855-aa2e-9f00ed2b762f)

Implemented rules for tiles on hexagonal grid, and shape detection for cards.
Unit tests are written for the logic of the game.

Only simulate one player, so you can keep playing until triggering a card.
What you can do : 
Select a group of tokens, and then put it on the board. 
If you match a card shape, it will add an animal cube.

You can easily add some new cards by adding a CardData scriptable object, with an animal type, a sprite and a shape.
Just drag and drop it in the view script afterward. 

Not implemented : 
- Multiple players
- UI and screen
- Score count (just missing blue token count right now)
