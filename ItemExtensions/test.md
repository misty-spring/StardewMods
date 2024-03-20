← [[Modding:Index]]

=Associated files location=

All the sprites for the farmer are stored in "Content/Characters/Farmer". The main sprite for the male farmer is "farmer_base.xnb" and for the female farmer is "farmer_girl_base.xnb".

=Layers=

The farmer sprite is drawn in several layers. On the bottom layer is the head, torso, and boots. The clothing, accessories, hair, and others are all drawn in multiple layers on top in this order:

# Head/Torso/Boots - Bottom layer
# Pants
# Shirt
# Accessories (''e.g.,'' glasses, makeup, earrings, facial hair)
# Hair
# Hat
# Arms - Top layer

The placement of layers stored in other files (such as the height of a shirt on the body) is dependent upon the animation, and cannot be modified by editing png files other than by shifting the accessory within its frame.

==Farmer (girl) base==

The farmer base sprite is divided into three sections. The Head/Torso/Boots layer are in the six columns on the far left. The pants are in the six columns on the far right. And the arms make up the central 12 columns. Each sprite is 16 pixels wide by 32 pixels tall.

The sorting of the sprites seems to be fairly logical with the six pants on the top row lining up with the six torsos on the top row, and similarly all the way down.

With the default shirts and pants, it is unnecessary to have a full torso drawn since the shirts cover most of it up. The pants cover up all of where the legs would go on the torso layer too. However, with mods that add more clothing options for shirts and pants, it can be necessary to have more torso drawn and legs added to the base farmer layer.

===Color palette===

When editing the farmer base, it is important to use the appropriate color palettes. The game identifies skin, eyes, boots, pants, arms, and shirt sleeves in the farmer base using the color. The game recolors these depending upon the options chosen during character creation and the color of the boots being worn.

==Shirt size difference==

Although the torso can be up to 16 pixels wide, shirts (which are stored in a separate file) are only 8x8 and unable to fully-cover a torso wider than that. The Hats, Accessories, and Pants all give full 16-pixel wide columns for covering the head and legs, though.

==Hats and Hairstyles==

The farmer's hairstyle will sometimes change when a hat is equipped.<sup>[https://community.playstarbound.com/threads/hairs-and-hats-behavior.128962/#post-3090496 source]</sup>

* Hairstyle 3 will change to hairstyle 11
* Hairstyles 22 and 26 will change to hairstyle 7.
* Hairstyles 18, 19, 21, and 31 will change to hairstyle 23
* Hairstyles 0, 2, 4, 7<sup>(?)</sup>, 8, 10, 12, 13, 14, 15, and 16 will change to hairstyle 30.
* Hairstyles 1, 5, 6, 9, 11, 17, 20, 23, 24, 25, 27, 28, 29, and 30 will not change.

Note: It seems strange that hairstyle 7 would change to 30, so it may actually stay the same.

=Sprite Index Breakdown=

As mentioned above, the pants, head/torso/boots, and arms all appear to line up. As all of the following information was tested specifically for pants, the illustrations will cover pants, but it most likely also covers head/torso/boots and arms.

==Notation==
The notation used here is R1F1 = Row 1 Frame 1 , and so on. In the code (which is located in FarmerSprite.cs), frame numbers are 0-indexed and go in order. The code notation is provided after the Row-Frame notation, with duration of each frame (in milliseconds) specified by the number after the @ symbol. For example, 1@60 is the Row 1, Frame 2 frame for duration 60.

In these notes, down refers to the farmer facing downwards (towards the screen), while up refers to the farmer facing upwards (away from the screen). Left and right are from the perspective of the player (screen left/screen right). As the left and right animations are universally mirror images of each other, they are always combined. The player facing to the right is the unflipped sprite, while facing left is flipped.

For the accompanying images, as they are quite large, they are displayed at small size, but if you click through you can see them larger. The male farmer side is greyed out, as this analysis was conducted with the female farmer base, but it should be exactly analogous. There is an extra small sprite at the bottom for the pants item inventory sprite.

==Farmer Movement==

===Walking===
Walking, down: R1F1, R1F2, R1F1, R1F3, repeat (frame indexes 0@200, 1@200, 0@200, 2@200, repeat)

Walking, left and right: R2F1, R2F2, R2F1, R2F3, repeat (frame indexes 6@200, 7@200, 6@200, 8@200, repeat)

Walking, up: R3F1, R3F2, R3F1, R3F3, repeat (frame indexes 12@200, 13@200, 12@200, 14@200, repeat)

[[File:Walking sprites.png|50px]]

===Running===
Running, down: R1F1, R1F2, R4F1, R1F2, R1F1, R1F3, R4F2, R1F3, repeat (frame indexes 0@90, 1@60, 18@120, 1@60, 0@90, 2@60, 19@120, 2@60, repeat)

Running, left and right: R2F1, R4F4, R3F6, R2F1, R4F3, R2F6, repeat (frame indexes 6@90, 21@140, 17@100, 6@90, 20@140, 11@100, repeat)

Running, up: R3F1, R3F2, R4F5, R3F2, R3F1, R3F3, R4F6, R3F3, repeat (frame indexes 12@90, 13@60, 22@120, 13@60, 12@90, 14@60, 23@120, 14@60, repeat)

[[File:Running sprites.png|50px]]

===Harvesting===
Harvesting down: R10F1, R10F2, R10F3, R10F4 (frame indexes 54@100, 55@100, 56@100, 57@100)

Harvesting left and right: R10F5, R10F6, R11F1, R11F2 (frame indexes 58@100, 59@100, 60@100, 61@100)

Harvesting up: R11F3, R11F4, R11F5, R11F6 (frame indexes 62@100, 63@100, 64@100, 65@100)

[[File:Harvesting sprites.png|50px]]

===Eating and Drinking===
Eating and drinking always take place with the farmer facing the screen (down).

Eating: R15F1, R15F2, R15F3, R15F4, R15F5, R15F4, R15F5, R15F4 (frame indexes 84@250, 85@400, 86@401, 87@250, 88@250, 87@250, 88@250, 87@250, 0@250)

Note: stardrops hold frame 84 for 1000 instead.

[[File:Eating sprites.png|50px]]

Drinking: R16F1, R16F2, R16F3, R16F4, R16F3, R16F4, R16F3, R16F4, R16F2, R16F1 (frame indexes 90@250, 91@150, 92@250, 93@200, 92@250, 93@200, 92@250, 93@200, 91@250, 90@50)

[[File:Drinking sprites.png|50px]]

===Horse Riding===
Horse riding down: R18F6 (frame index 107)

Horse riding left/right: R18F5 (frame index 106)

Horse riding up: R19F6 (frame index 113)

[[File:Horse riding sprites.png|50px]]

===Sitting===
Sitting down: R18F6 (frame index 107)

Sitting left and right: R20F4 (frame index 117)

Sitting up: R19F6 (frame index 113)

[[File:Sitting sprites.png|50px]]

==Farmer Basic Tool and Weapon Use==

===Heavy Tools===

Note: all frames are sped up by a factor of 0.66 when the Swift enchantment is applied to the tool. The second to last frame (frame 69, or 51, or ) is held for an additional 30*toolPower, where toolPower reflects the level of the held-down upgraded hoe or watering can (ex. showing 3 squares = level 1). This is all very fiddly and annoying.

Axe/pickaxe/hoe/club special down: R12F1, R12F2, R12F3, R12F4, R12F5, cycles once (frame indexes 66@150, 67@40, 68@40, 69@170, 70@75)

Upgraded hoe held down: R12F1 (until highlighted tiles show up), R13F3 (until increases >3 tiles), R13F4 (until releases), then the normal tool animation (frame indexes 66, 74, 75)

Axe/pickaxe/hoe/club special left and right: R9F1, R9F2, R9F3, R9F4, R9F5, cycles once (frame indexes 48@100, 49@40, 50@40, 51@220, 52@75)

Upgraded hoe held left and right: R9F1 (until highlighted tiles show up), R13F1 (until increases >3 tiles), R13F2 (until releases), then the normal tool animation (frame indexes 48, 72, 73)

Axe/pickaxe/hoe/club special up: R7F1, R7F2, R7F3, R11F4, R11F3, cycles once (frame indexes 36@100, 37@40, 38@40, 63@220, 62@75)

Upgraded hoe held up: R7F1 (until highlighted tiles show up), R13F5 (until increases >3 tiles), R13F6 (until releases), then the normal tool animation (frame indexes 36, 76, 77)

[[File:Tool sprites.png|50px]]

===Scythe/Melee Weapons===
Here, swipeSpeed is some number that depends on the weapon speed.

Swords:
<code>swipeSpeed = 6.5 * (10 - (weapon speed + farmer added speed)) * (1 - (farmer weapon speed modifier))</code>

Clubs:
<code>swipeSpeed = 10.4 * (10 - (weapon speed + farmer added speed)) * (1 - (farmer weapon speed modifier))</code>

Scythe/melee down: R5F1, R5F2, R5F3, R5F4, R5F5, R5F6, cycles once (frame indexes 24@55, 25@45, 26@25, 27@25, 28@25, 29@swipeSpeed)

Scythe/melee left and right: R6F1, R6F2, R6F3, R6F4, R6F5, R6F6, cycles once (frame indexes 30@55, 31@45, 32@25, 33@25, 34@25, 35@swipeSpeed)

Scythe/melee up: R7F1, R7F2, R7F3, R7F4, R7F5, R7F6, cycles once (frame indexes 36@55, 37@45, 38@25, 39@25, 40@25, 41@swipeSpeed)

[[File:Melee sprites.png|50px]]

===Daggers===

Daggers:
<code>swipeSpeed = 10 * (10 - (weapon speed + farmer added speed)) * (1 - (farmer weapon speed modifier))</code>

Not sure exactly how swipeSpeed from daggers plays in. Most likely it's passed in as interval.

Dagger down: R5F2, R5F4 (frame indexes 25@interval, 27@interval)

Dagger left and right: R6F5, R6F4 (frame indexes 34@interval, 33@interval)

Dagger up: R7F5, R7F3 (frame indexes 40@interval, 38@interval)

Dagger special: dagger repeated several times very fast

===Watering Can===
Watering down: R10F1, R10F2, R5F2, cycles once (frame indexes 54@75, 55@100, 25@500)

Watering left and right: R10F5, R10F6, R8F4, cycles once (frame indexes 58@75, 59@100, 45@500)

Watering up: R11F3, R11F4, R8F5, cycles once (frame indexes 62@75, 63@100, 46@500)

[[File:Watering sprites.png|50px]]

===Sword Block===
Sword sp. down: R5F5, held (frame index 28)

Sword sp. left and right: R6F5, held (frame index 34)

Sword sp. up: R7F5, held (frame index 40)

[[File:Sword block sprites.png|50px]]

===Slingshot===
Note: I think the slingshot arms are weird. They certainly look weird, and I think they break the normal grid pattern.

Slingshot down: R8F1 (frame index 42)

Slingshot left and right: R8F2 (frame index 43)

Slingshot up: R8F3 (frame index 44)

[[File:Slingshot sprites.png|50px]]

==Fishing==

===Casting===
This is when the farmer is throwing out a line (holding down the mouse while the bar over the farmer's head fills/shrinks), plus the animation right after the mouse is un-pressed.

Casting down: R12F1, held, then rest of heavy tool animation (frame indexes 66@100, 67@40, 68@40, 69@80, 70@200)

Casting left and right: R9F1, held, then R9F2, R9F3, R9F4, R9F5 (frame indexes 48@100, 49@40, 50@40, 51@80, 52@200)

Casting up: R13F5, held, then R7F3, R11F4, R11F3, repeat last two (frame indexes 76@100, 38@40, 63@40, 62@80, 63@200)

[[File:Casting sprites.png|50px]]

===Fishing===
This is when the farmer has a line out, and is waiting for a bite.

Fishing down: R12F5, held (frame index 70)

Fishing left and right: R15F6, held (frame index 91)

Fishing up: R8F3, held (frame index 44)

[[File:Fishing sprites.png|50px]]

===Reeling===
This is when the farmer is playing the minigame.

Reeling down: R12F1, held (frame index 66)

Reeling left and right: R9F1, held (frame index 48)

Reeling up: R7F1, held (frame index 36)

[[File:Reeling sprites.png|50px]]

===Fish Caught===
This is after the player has caught a fish, going from the reeling position to holding up a fish for display.

Fish caught down: R13F3, R10F4, R15F1 (frame indexes 74, 57, 84)

Fish caught left and right: R13F1, then farmer switches to down R10F4, R15F1 (frame indexes 72, 57, 84)

Fish caught up: R13F5, then farmer switches to down, R10F4, R15F1 (frame indexes 76, 57, 84)

[[File:Caught fish sprites.png|50px]]

==Farmer Other Tool Use==

===Panning===
Panning only ever occurs with the farmer facing down (towards the screen).

Panning down: R21F4, R21F5, R21F4, R21F6, repeat 3x, R21F4, R21F5, R21F4 (frame indexes 123@150, 124@150, 123@150, 125@150, 123@150, 124@150, 123@150, 125@150, 123@150, 124@150, 123@150, 125@150, 123@150, 124@150, 123@500)

[[File:Panning sprites.png|50px]]

===Milking===
Milking down: R10F1, R10F2, repeats twice total (frame indexes 54@400, 55@400, 54@400, 55@400)

Milking left/right: R10F5, R10F6, repeats twice total (frame indexes 58@400, 59@400, 58@400, 59@400)

Milking up: R11F3, R11F4, repeats twice total (frame indexes 56@400, 57@400, 56@400, 57@400)

[[File:Milking sprites.png|50px]]

===Shearing===
Shearing down: R14F1, R14F2, repeats twice total (frame indexes 78@400, 79@400, 78@400, 79@400)

Shearing left/right: R14F3, R14F4, repeats twice total (frame indexes 80@400, 81@400, 80@400, 81@400)

Shearing up: R14F5, R14F6, repeats twice total (frame indexes 82@400, 83@400, 82@400, 83@400)

[[File:Shearing sprites.png|50px]]

==Farmer Miscellaneous Movement==

===Passing Out===
This seems to be the same animation both for passing out due to low energy, passing out due to 2:00 AM, and passing out due to low health, but I have not confirmed the last one.

Passing out: R3F5, R1F1, R3F5, R1F5, R1F6 (frame indexes 16@1000, 0@500, 16@1000, 4@200, 5@6000)

[[File:Passing out sprites.png|50px]]

===Nausea===
This occurs after eating something poisonous, as well as in a couple cutscenes. It always occurs facing down.

Nausea: R18F3, R18F4, repeat several times (frame indexes 104@350, 105@350, 104@350, 105@350, 104@350, 105@350, 104@350, 105@350)

[[File:Nausea sprites.png|50px]]

===Kissing===
This is used in the wedding, for kissing your spouse, and in a couple cutscenes.

Kissing: R17F6 (frame index 101)

[[File:Kissing sprites.png|50px]]

===Flower Dance===
You can either be facing down or up, depending on which NPCs you ask to dance.

Flower dance down: R1F1, R1F5, repeat, R1F1, R1F4, repeat (frame indexes 0, 4, etc, 0, 3, etc)

Flower dance up: R3F1, R3F2, R3F1, R3F3, repeat at various speeds (frame indexes 12, 13, 12, 14, etc)

[[File:Flower dance sprites.png|50px]]

===Bathing Suit===
Each pants .png has its own bathing suit so that the bathing suit color reflects the pants color.

Bathing suit walking down: R19F1, R19F2, R19F1, R19F3 (frame indexes 108, 109, 108, 110)

Bathing suit walking left/right: R20F1, R20F2, R20F1, R20F3 (frame indexes 114, 115, 114, 116)

Bathing suit walking up: R21F1, R21F2, R21F1, R21F3 (frame indexes 120, 121, 120, 122)

[[File:Bathing suit sprites.png|50px]]

==Farmer in Cutscenes==
There are a variety of farmer frames and animations that are used in different cutscenes. Many of them are used in more than one cutscene, but a few are specific.

Reaching (used in Haley darkroom, George remote events): R3F4 (frame index 15)

Sitting down, sad (used in Alex sad mother event): R16F6 (frame index 95)

Looking sad/pensive (used in Abigail video game failure): R17F1 (frame index 96)

Head scratching/looking sheepish (used in Abigail video game event, several others): R17F2 (frame index 97)

Playing the harp (used in Abigail flute scene): R17F3, R17F4, R17F5 (frame indexes 98, 99, 100)

Laughing (used in various cutscenes): R18F1, R18F2 (frame indexes 102, 103)

Opening a jar (used in Haley jar event): R19F4, R19F5 (frame indexes 111, 112)

[[File:Event sprites.png|50px]]

==Unknown Frames==

There are 4 unidentified frames, shown below, including the head/torso/boots and arms, along with the frame index.

[[File:Unknown_Sprites.png|180px]]

[[Category:Modding]]

[[ru:Модификации:Спрайт фермера]]

== Indexes in sprite sheet ==