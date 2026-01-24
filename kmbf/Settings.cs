using UnityModManagerNet;

#pragma warning disable 0649 // Lots of serialization magic in here

namespace kmbf
{
    [DrawFields(DrawFieldMask.Public)]
    public class BalanceSettings
    {
        public bool FixAreaOfEffectDoubleTrigger = true;
        public bool FixNauseatedPoisonDescriptor = true;
        public bool FixBaneLiving = true;
        public bool FixCandlemereTowerResearch = true;
        public bool FixKingdomBuildingAccess = true;
        public bool FixEmbassyRowGrandDiplomatBonus = true;
        public bool FixNecklaceOfDoubleCrosses = true;
        public bool FixShatterDefenses = true;
        public bool FixArcaneTricksterAlignmentRequirement = true;
        public bool FixCraneWingRequirements = true;
        public bool FixControlledFireball = true;
        public bool FixWeaponEnhancementDamageReduction = true;
        public bool FixTouchOfGlory = true;
        public bool FixConstructUndeadImmunities = true;
        public bool FixAnimalDomainPerceptionBonus = true;
        public bool FixAmiriOversizedSwordPlusShield = true;
    }

    [DrawFields(DrawFieldMask.Public)]
    public class EventSettings
    {
        public bool FixFreeEzvankiTemple = true;
    }

    [DrawFields(DrawFieldMask.Public)]
    public class QualityOfLifeSettings
    {
        public bool BypassSpellResistanceForOutOfCombatBuffs = true;
        public bool CombatExpertiseOffByDefault = true;
    }

    [DrawFields(DrawFieldMask.Public)]
    public class AddedContentSettings
    {
        public bool SlingsExperimental = false;
    }

    public class UMMSettings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Balance")] public BalanceSettings BalanceSettings = new();
        [Draw("Event")] public EventSettings EventSettings = new();
        [Draw("Quality of Life")] public QualityOfLifeSettings QualityOfLifeSettings = new();
        [Draw("Added Content")] public AddedContentSettings AddedContentSettings = new();


        bool fixesDirty = false;
        public bool FixesDirty { get => fixesDirty; }

        // Not necessary to override after UMM 0.31.1
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            fixesDirty = true;
        }
    }
}
