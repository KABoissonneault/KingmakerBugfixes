using UnityModManagerNet;

#pragma warning disable 0649 // Lots of serialization magic in here

namespace kmbf
{
    [DrawFields(DrawFieldMask.Public)]
    class BalanceSettings
    {
        public bool FixAreaOfEffectDoubleTrigger = true;
    }

    internal class UMMSettings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Balance")] public BalanceSettings BalanceSettings = new BalanceSettings();

        // Not necessary to override after 0.31.1
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {

        }
    }
}
