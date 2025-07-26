# KingmakerBugfixes

Bug fixing mod for Pathfinder: Kingmaker. Many long-standing bugs in Kingmaker are simple mistakes in the game data (Blueprints) that can easily be patched by a mod.

While some of these issues might be fixed in other large scale mods, this mod aims to stay "purist" and only have fixes that bring content the base game already has, rather than rebalancing or creating alternative content. When there is a mismatch between what the game says and what the data actually does, the data is fixed to match the description (ex: Datura Will Saving Throw DC should be 16 instead of 11).

Many of these are buffs to the player, since they make more abilities work, but a few might nerf them (ex: Bane of the Living actually working only on the living). Beware when relying on broken behavior.

## Fixes

### Core
- [Opt-out setting] "Area of Effect" with UnitEnter and Round events (ex: Stinking Cloud) do not double trigger on cast
- Polymorphed characters now preserve their natural weapon enchantments (ex: Magic Fang) through save and load
- Feral Mutagen characters now preserve their natural weapon enchantments (ex: Magic Fang) through save. Unfortunately, they still lose it on load

### Ability fix

#### Class & Spells

- Blight Druid's Darkness Domain's "Moonfire" damage now scales properly with Druid levels
- "Magic Vestment, Shield" gives Shield Enhancer AC instead of Shield AC
- Fixed Freedom of Movement's Immunity to Grapple
- Fixed Rogue "Double Debilitation" not applying two Debilitating Injuries
- Fixed Kineticist "Deadly Earth: Metal" (and Rare Metal) having half the damage scaling it should have
- Fixed Raise Dead not draining 2 levels on difficulty Core and above (turn Enemy Stats Adjustment down to disable, like in Wrath)

#### Feat & Traits

- Fixed Vital Strike not working on ranged weapons
- Fixed Ekundayo's Dog "Loyal Companion" and "Enraged Companion" not giving stats

#### Item
- Fixed issue where Datura would wake up targets immediately after sleeping them. Added the sleep enchantment to the second head. Also increased DC to 16, as described
- Fixed Bane of the Living being a bane of all creatures (properly excludes Undead and Constructs)
- Fixed Nature's Wrath (trident) not being a bane of aberrations, constructs, humanoids, outsiders and undead. Also, the "Electricity Vulnerability" debuff now applies to the struck target rather than the wielder, as described
- Scroll of Summon Nature's Ally V (Single) now properly summons a Manticore

#### Misc

- [Opt-out setting] "Poison" descriptor removed from Nauseated debuff, and thus can be applied by non-poison sources (ex: Swarm Distraction, Ooze Spit). Poison Immunity still blocks poison spells like Stinking Cloud

### Event

- Irlene's "Relations rank 3" tier 3 gift (when A Simple Favor or Coronation but not both are complete) properly uses Irlene items rather than Kimo Tavon's
- Shrewish Gulch illustrated book event now uses Athletics instead of Lore (Nature) on the last stage for the first option in all cases

### Text and UI

- Saving Throw "overtips" show "Roll vs Needed Roll" instead of "Roll vs DC" in Turn-Based mode, like attack rolls
- Added missing Name and Description for Mimic Ooze "Spit" ability

### Changes

Not quite "fixes", but should be a universal improvement for everyone

- Darts are now Light (Thrown) weapons, like in tabletop

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
