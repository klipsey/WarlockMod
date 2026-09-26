using R2API;

namespace WarlockMod.Modules
{
    internal static class SoundBanks
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            SoundAPI.SoundBanks.Add(Assets.ReadEmbeddedResource("WarlockMod.interrogator_bank.bnk"));
            initialized = true;
        }
    }
}
