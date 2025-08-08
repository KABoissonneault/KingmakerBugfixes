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
    }

    [DrawFields(DrawFieldMask.Public)]
    public class EventSettings
    {
        public bool FixFreeEzvankiTemple = true;
    }

    public class UMMSettings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Balance")] public BalanceSettings BalanceSettings = new();
        [Draw("Event")] public EventSettings EventSettings = new();

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
