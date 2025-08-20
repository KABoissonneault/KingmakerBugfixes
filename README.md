
# KingmakerBugfixes

Bug fixing mod for Pathfinder: Kingmaker. Many long-standing bugs in Kingmaker are simple mistakes in the game data (Blueprints) that can easily be patched by a mod.

While some of these issues might be fixed in other large scale mods, this mod aims to stay "purist" and only have fixes that bring content the base game already has, rather than rebalancing or creating alternative content. When there is a mismatch between what the game says and what the data actually does, the data is fixed to match the description (ex: Datura Will Saving Throw DC should be 16 instead of 11).

Many of these are buffs to the player, since they make more abilities work, but a few might nerf them (ex: Bane of the Living actually working only on the living). Beware when relying on broken behavior.

## Fixes

[Opt-out] means those fixes can be disabled in the Unity Mod Manager settings (CTRL+F10 while in-game). Generally, those fixes remove exploits that benefit the player. You need to reboot the game after changing the settings for the fixes to apply or be reverted.

[Modify Save] means those fixes may not be able to fully fix the issue if your save already contains the bug. In that case, to fix the issue, you need to go into the UMM settings (CTRL+F10) and press the "Modify Current Save" while a save is loaded. **Those fixes are permanent, and can only be reverted by going back to an earlier save**

[!CotW] means this fix is not applied if using the mod Call of the Wild. Note that some fixes may be different from the CotW implementation, but are still disabled to prevent conflicts.

### Core
- [Opt-out] "Area of Effect" with UnitEnter and Round events (ex: Stinking Cloud) do not double trigger on cast
- Fixed turn-based combat sometimes making the main character lose their surprise action, and sometimes giving the party a surprise round for no reason
- Fixed peaceful or quest-related world map encounters not being possible to trigger on upgraded regions, making some quests impossible to complete in those conditions
- Polymorphed characters now preserve their natural weapon enchantments (ex: Magic Fang) through save and load
- Feral Mutagen characters now preserve their natural weapon enchantments (ex: Magic Fang) through save. Unfortunately, they still lose it on load
- Fixed issue where a sneak attack damage could fail to bypass Damage Reduction when the weapon you're using should bypass it
- [Opt-out] Fixed issue where all enchantments counted as an extra "+1" for the purpose of bypassing Damage Reduction, including traits like Composite or Thrown. Now, if a DR requires +3 to bypass, it really requires +3. Also applies to Regeneration and Incorporeal magical weapon checks


### Ability fix

#### Class & Spells

- [Opt-out] Fixed Arcane Trickster to properly require non-lawful alignment
- Blight Druid's Darkness Domain's "Moonfire" damage now scales properly with Druid levels
- "Magic Vestment, Shield" gives Shield Enhancer AC instead of Shield AC
- Fixed Freedom of Movement to properly grant Immunity to Grapple
- Fixed Rogue "Double Debilitation" not applying two Debilitating Injuries
- Fixed Hampering Injury debuff icon to match the active
- Fixed Kineticist "Deadly Earth: Metal" (and Rare Metal) having half the damage scaling it should have
- Fixed Raise Dead not draining 2 levels on difficulty Core and above (turn Enemy Stats Adjustment down to to Normal or less to disable, like in Wrath)
- Fixed Breath of Life not giving 1 temporary negative level when resurrected (turn Enemy Stats Adjustment down to to Normal or less to disable, like in Wrath)
- Fixed Joyful Rapture to properly affect all negative emotion effects
- Fixed Protection from Arrows protecting against magical weapons instead of the mundane weapons as intended. Fixed the Communal version checking Spell Resistance
- Fixed Leopard animal companion level 4 upgrade getting "+4 str / -2 dex / +4 con" instead of "+2 dex / +2 con" as described
- Fixed Baleful Gaze working on targets immune to gaze attacks or sight-based attacks
- [Opt-out] Fixed Controlled Fireball not applying minimal damage to allies as intended

#### Feat & Traits

- [Opt-out, !CotW] Fixed Shatter Defenses to only apply after landing a hit, like in Wrath of the Righteous and tabletop. Unlike the CotW fix, this bonus still applies if you hit a Shaken/Frightened enemy and it no longer is next turn, matching the description
- [!CotW] Fixed Vital Strike not working on ranged weapons
- Fixed Ekundayo's Dog "Loyal Companion" and "Enraged Companion" not giving stats
- [Opt-out] Fixed Crane Wing to check for shield in offhand. Still allows 2h weapons, since KM has no 1h option
- Fixed Foulspawn Tiefling bonus against Clerics, Paladins, and Inquisitors only working against characters that had all three classes instead of any

#### Item

- [Opt-out, !CotW] Fixed Necklace of Double Crosses to only apply to melee attacks, and implemented the "ally attack of opportunity" mechanics as described
- [Opt-out] Fixed Living Bane (Bane of the Living, Penalty) being a bane of all creatures (properly excludes Undead and Constructs)
- Fixed issue where Datura would wake up targets immediately after sleeping them. Added the sleep enchantment to the second head. Also increased DC to 16, as described
- Fixed Nature's Wrath (trident) not being a bane of aberrations, constructs, humanoids, outsiders and undead. Also, the "Electricity Vulnerability" debuff now applies to the struck target rather than the wielder, as described
- Scroll of Summon Nature's Ally V (Single) now properly summons a Manticore
- "Solid Strategy" now gives Immunity to Attacks of Opportunity, rather than immunity to Immunity to Attacks of Opportunity
- Fixed Ring of Reckless Courage not applying on Kingdom Advisor stat
- Fixed Quiver of Lightning Arrows and Quiver of Lover's Arrows not having the extra haste attack despite mentioning "speed" in the description
- Fixed Explosion Ring not giving +12 damage to bomb abilities

#### Misc

- [Opt-out] "Poison" descriptor removed from Nauseated debuff, and thus can be applied by non-poison sources (ex: Swarm Distraction, Ooze Spit). Poison Immunity still blocks poison spells like Stinking Cloud

### Quests & Event

- Fixed "Honor and Duty" quest from Renown Across Golarion to always trigger when solving the Kingdom event with a Regent (General expels the crusaders). Easier to find the cultists in North Narlmarches after
- Fixed issue where Storyteller would grabs relics one at a time if using a mod to stack them (ex: Inventory Tweaks)
- Fixed "Unrest in the Streets" angry mob first check having wrong DCs with unrest modifiers at -2, -3, or -4. The DC is now 13, 8, and 3, instead of 23, 23, -22
- Irlene's "Relations rank 3" tier 3 gift (when A Simple Favor or Coronation but not both are complete) properly uses Irlene items rather than Kimo Tavon's
- Shrewish Gulch illustrated book event now uses Athletics instead of Lore (Nature) on the last stage for the first option in all cases
- Fixed Armag Tomb's Test of Strength not opening all doors when solving the problem "the intended way" (with the 25 Athletics checks after passing the 18 Intelligence check)
- Fixed Mim's "Three Wishes" quest not raising artisan tier on completion (not backward compatible)

### Kingdom

- Can now build Temple of Abadar (with proper event), Assassin's Guild (with proper event and alignment), and Thieves Guild and Gambling Den (with proper alignment and town size)
- [Opt-out] Black Market now requires Thieves Guild and non-Lawful, non-Good alignment, as described
- "Erastil's Holy Place" now properly grants a kingdom buff that gives a +5 sacred bonus to Lore (Nature)
- [Modify Save] "It's a Magical Place" now properly grants a kingdom buff that gives +5 competence bonus to Knowledge (Arcana) and Lore (Religion). It also properly gives +5 bonus to resolve any situation with the Magister or High Priest in the region. Modify Save only required if you already have the upgrade
- [Opt-out, Modify Save] "The Magic of Candlemere Tower" now grants +3 in the regions around South Narlmarches, rather than +18 everywhere in the barony. Modify Save only required if you already have the upgrade
- [Opt-out] Embassy Row now only works in the region where it is built. Now also applies to Opportunities in addition to Problems
- [Modify Save] Fixed Embassy Row adjacency bonuses to include School, Aviary, Thieves Guild, and Black Market. Updating the bonus requires Building, Selling, or Upgrading a building in the settlement, or you can use the "Modify Current Save" button while in the settlement
- The "Wigmold System" from the Stability Rank 10 event now also applies to Opportunities in addition to Problems
- Fixed "Improving Cultural Development" not including Theater

### Text and UI

- Saving Throw "overtips" show "Roll vs Needed Roll" instead of "Roll vs DC" in Turn-Based mode, like attack rolls
- Added missing Name and Description for Mimic Ooze "Spit" ability, Giant Slug "Tongue" attack, Bulette attack, Mite rock throw, and Shocker Lizard "Touch" attack
- Fixed "Amethyst Ring" and "Garnet Ring" to have the proper intended name (the base game has two rings named Topaz Ring, and none named Amethyst Ring)
- Fixed "Cold Iron Rapier +3" displaying as "Cold Iron Rapier +1" in text logs

### Changes

Not quite "fixes", but should be a universal improvement for everyone

- Darts are now Light (Thrown) weapons, like in tabletop
- [Opt-out] Spell resistance is ignored when casting spells on allies outside of combat

### Stability

- Fixed data leak in InitiativeTrackerUnitVM
- Fixed null reference exception in BugReportCanvas
- Fixed null reference exception in WeatherSystemBehaviour

## Known Issues

These are some known issues introduced by the mod. These are generally accepted as better than what the base game had, but worth acknowledging here

- Successful Datura Sleep procs (Saving Throw fail) will not show above the target, only in logs

## Will Not Fix

These are issues that have been investigated and have been determined to be too large to fix through Harmony patching. People are free to investigate alternative solutions, but otherwise, there's no use in requesting for these issues to be fixed.

- Feral Mutagen will lose weapon enchantments (ex: Magic Fang) when reloading a save that previously had such enchantments
- All items with "conditional" buffs to stats (ex: Bracers of Archery buffing attack and damage if you're using a bow) do not track "descriptors" like "Competence bonus"

## Settings

List of settings:
- Fix Area of Effect Double Trigger: Without this fix, all lasting area of effects will trigger twice when a unit enters it (or when it's cast on top of them). For example, Stinking Cloud will require two saving throws
- Fix Nauseated Poison Descriptor: Without this fix, poison immunity makes you immune to the Nauseated condition. With this fix, you can use Cacophonous Call on poison immune enemies, but swarms can also apply Nauseated through your Delay Poison
- Fix Bane Living: Without this fix, Bane of the Living (and the Penalty scythe) is a bane of everything. Now, it is not a bane of undead or constructs
- Fix Candlemere Tower Research: Without this fix, Candlemere Tower Research applies the buff to all six regions, and applies globally (6 times +3). Without this fix, the buff only applies to the South Narlmarches region, and applies to it and adjacent regions (1 time +3). If you already have the six buffs on your save, you need to "Modify Current Save" to fix it
- Fix Kingdom Building Access: Without this fix, it is possible to build a Black Market without having a Thieves Guild or the correct alignment. With this fix, only non-Lawful, non-Good baronies can build one (after building a Thieves Guild)
- Fix Embassy Row Grand Diplomat Bonus: Without this fix, Embassy Row's Grand Diplomat bonus applies globally. With it, it only applies in the region where the Embassy Row is built
- Fix Necklace of Double Crosses: Without this fix, the Necklace gives +2d6 sneak attack to all weapon attacks, including ranged, and the ally attack mechanic is not implemented. With it, the bonus only applies to melee attacks, and the wielder will attempt attacks of opportunity on allies that leave its threatening range
- Fix Shatter Defenses: Without this fix, Shatter Defenses always applies on shaken/frightened targets, even if a hit was never landed. With it, a hit on a shaken/frightened will apply a buff on the enemy for 2 turns that makes it flat footed to the attacker (even if it's no longer shaken/frightened next turn). Does not apply when using Call of the Wild.
- Fix Arcane Trickster Alignment Requirement: With this fix, only non-lawful characters can be an Arcane Trickster, as described in the in-game glossary - Fix Crane Wing Requirements: With this fix, you cannot use Crane Wing with a shield (not even a buckler). Still allows for using a weapon with two-hands, since there's almost no way to wield a weapon in one hand in Kingmaker
- Fix Controlled Fireball: With this fix, Controlled Fireball applies minimal damage to allies instead of none. Watch out for Arcane Trickster sneak attacks
- Fix Weapon Enhancement Damage Reduction: Without this fix, any "trait" on a weapon makes it magical for the purpose of bypassing Damage Reduction (ex: 10/magic), including Composite or Thrown (but not masterwork). With this fix, only the proper "+1" (or more) enchantment makes a weapon bypass DR, as described in the rules
- Fix Free Ezvanki Temple: Without this fix, you always get a free temple from Ezvanki when starting out your barony, even if you've never talked to him. With this fix, you need to pass the Diplomacy check, as intended
- Bypass Spell Resistance for Out of Combat Buffs: In tabletop, a unit can always choose to remove its spell resistance by using a standard action, which removes it for a turn. This feature is not implemented in Kingmaker, but this setting allows units to ignore spell resistance when casting spells on allies while out of combat, as a shorthand.

## Contributors

Contributions are welcome! 

Programmers: consider taking a look at the [Github wiki page for issues](https://www.github.com/KABoissonneault/KingmakerBugfixes/wiki/Issues-to-fix) and sending a pull request.

For others, you can help by documenting issues on the [Pathfinder: Kingmaker Fandom wiki](https://pathfinderkingmaker.fandom.com/). The site has a bug documentation macro that looks like this:

```
{{bug||Relations 3, tier 3 reward is given from [[Kimo Tavon]] item list.|ver=2.1.7b|g=km|modfix=kmbf}}
```

`g=km` refers to a Kingmaker issue, and `modfix=kmbf` means this mod has fixed the issue. 

Feel free to add all the issues you find on this page over there, or add the `modfix=kmbf` if already documented.

### Localization

The mod supports localization, usings a `String_<locale>.json` file under the Localization folder. If you can make one for a locale other than enGB, feel free to send it here (through Pull Request or other means).

## Acknowledgement

- Whiterock for bug reporting and investigation
- silv7k for testing
- hambeard and Micro for support
- Vek17 for letting me port Wrath of the Righteous "Tabletop Tweaks" fixes back to Kingmaker
