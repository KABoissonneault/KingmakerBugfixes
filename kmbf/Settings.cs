using UnityModManagerNet;

#pragma warning disable 0649 // Lots of serialization magic in here

namespace kmbf
{
    [DrawFields(DrawFieldMask.Public)]
    public class BalanceSettings
    {
        public bool FixAreaOfEffectDoubleTrigger = true;
        public bool FixNauseatedPoisonDescriptor = true;
    }

    public class UMMSettings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Balance")] public BalanceSettings BalanceSettings = new BalanceSettings();

        // Not necessary to override after UMM 0.31.1
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {

        }
    }
}
