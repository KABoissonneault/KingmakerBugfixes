# KingmakerBugfixes

Bug fixing mod for Pathfinder: Kingmaker. Many long-standing bugs in Kingmaker are simple mistakes in the game data (Blueprints) that can easily be patched by a mod.

While some of these issues might be fixed in other large scale mods, this mod aims to stay "purist" and only have fixes that bring content the base game already has, rather than rebalancing or creating alternative content. When there is a mismatch between what the game says and what the data actually does, the data is fixed to match the description (ex: Datura Will Saving Throw DC should be 16 instead of 11).

Many of these are buffs to the player, since they make more abilities work, but a few might nerf them (ex: Bane of the Living actually working only on the living). Beware when relying on broken behavior.

Fixes:
- Blight Druid's Darkness Domain's "Moonfire" damage now scales properly with Druid levels
- Irlene's "Relations rank 3" tier 3 gift (when A Simple Favor or Coronation but not both are complete) properly uses Irlene items rather than Kimo Tavon's
- Fixed issue where Datura would wake up targets immediately after sleeping them. Added the sleep enchantment to the second head. Also increased DC to 16, as described
- Fixed Bane of the Living being a bane of all creatures (properly excludes Undead and Constructs)
- Fixed Nature's Wrath (trident) not being a bane of aberrations, constructs, humanoids, outsiders and undead. Also, the "Electricity Vulnerability" debuff now applies to the struck target rather than the wielder, as described
- Scroll of Summon Nature's Ally V (Single) now properly summons a Manticore
- Shrewish Gulch illustrated book event now uses Athletics instead of Lore (Nature) on the last stage for the first option in all cases

Known Issues:
- Successful Datura Sleep procs (Saving Throw fail) will not show above the target, only in logs
