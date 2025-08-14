using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Stats;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;

using static kmbf.Blueprint.Builder.ElementBuilder;

namespace kmbf.Patch
{
    static class EventFixes
    {
        public static void Apply()
        {
            FixShrewishGulch();
            FixCandlemere();
            FixUnrestInTheStreets();
            FixTestOfStrength();
            FixMimThreeWishes();
        }

        // Shrewish Gulch Last Stage "Two Actions" and "Three Actions" checks have "Lore (Nature)" instead of "Athletics" as skill check, unlike the One Action variant
        static void FixShrewishGulch()
        {
            BlueprintCheckConfigurator.From(BlueprintCheckGuid.ShrewishGulchLastStageTwoActions)
                .SetSkillType(StatType.SkillAthletics)
                .Configure();

            BlueprintCheckConfigurator.From(BlueprintCheckGuid.ShrewishGulchLastStageThreeActions)
                .SetSkillType(StatType.SkillAthletics)
                .Configure();
        }

        // Fix the "Magic of the Candlemere Tower" region upgrade not getting unlocked when delaying the Duke Dazzleflare fight
        static void FixCandlemere()
        {            
            BlueprintCueConfigurator.From(BlueprintCueGuid.CandlemereRismelDelayedStartFight)
                .AddOnStopAction(UnlockFlagConfigurator.New(BlueprintUnlockableFlagGuid.SouthNarlmarches_MagicalUpgrade, 1).Configure())
                .Configure();
        }

        // Unrest in the Streets Angry First Check DC goes from DC23 at 0, to 18 at -1, to 23 at -2 and -3, to -22 at -4
        // Fix the modifiers to actually check for -2 and -3 instead of all three checking for -4, giving the intended DC progression
        static void FixUnrestInTheStreets()
        {            
            BlueprintCheckConfigurator.From(BlueprintCheckGuid.Unrest_AngryMob_FirstCheck_Diplomacy)
                .EditDCModifierAt(4, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -2)))
                .EditDCModifierAt(5, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -3)))
                .EditDCModifierAt(6, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -4)))
                .Configure();

            BlueprintCheckConfigurator.From(BlueprintCheckGuid.Unrest_AngryMob_FirstCheck_Intimidate)
                .EditDCModifierAt(4, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -2)))
                .EditDCModifierAt(5, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -3)))
                .EditDCModifierAt(6, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -4)))
                .Configure();
        }

        // Test of Strength has four conclusions
        // 1. Push the block successfully a few times after passing the Intelligence 18 check (Athletics 25)
        // 2. Use a clever workaround by standing on the switch yourself (Perception 35)
        // 3. Successfully break the ice walls to brute force the puzzle (Athletics 35)
        // 4. Break the ice walls to brute force the puzzle after having failed once (Athletics 35)
        //
        // It is intended that using 2 to solve the problem only opens one door (Linzi mentions so), while using 3 and 4 opens all the doors
        // But while 1 is clearly the intended solution, it only opens one door, like 2, and Linzi makes no mention of this
        // This fix allows the normal intended solution to open all doors, while keeping the locked doors for the "clever workaround" solution
        static void FixTestOfStrength()
        {
            if (!BlueprintAnswerGuid.TestOfStrength_BreakWallsSolution_Conclusion.GetBlueprint(out BlueprintAnswer breakWallsAnswer)) return;

            BlueprintAnswerConfigurator.From(BlueprintAnswerGuid.TestOfStrength_PushSolution_Conclusion)
                .EditOnSelectActions(actions =>
                {
                    // Remove the current open doors and add the ones from the "Break walls" solution
                    actions.Actions = actions.Actions.Where(a => !(a is OpenDoor)).Concat(breakWallsAnswer.OnSelect.Actions.OfType<OpenDoor>()).ToArray();
                })
                .Configure();
        }

        // Should raise Artisan tier by 1 on completion, but fails to do so
        static void FixMimThreeWishes()
        {
            // Obj5_LeadToTalonPeak finishes parent by default, when the quest has a Finish objective already to handle completion, as well as completion effects
            BlueprintQuestObjectiveConfigurator.From(new BlueprintQuestObjectiveGuid("798ca1c73a57a864fbf127e4cd27bfe5"))
                .SetFinishParent(false)
                .Configure();
        }
    }
}
