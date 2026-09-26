using R2API;
using System.IO;

namespace WarlockMod.Modules
{
    internal static class SoundBanks
    {
        private static bool initialized = false;
        public static string SoundBankDirectory
        {
            get
            {
                return Path.Combine(Path.Combine(Path.GetDirectoryName(WarlockPlugin.instance.Info.Location)), "SoundBanks");
            }
        }

        public static void Init()
        {
            if (initialized) return;
            string path = Path.Combine(SoundBankDirectory, "interrogator_bank.bnk");
            if (!File.Exists(path))
                throw new FileNotFoundException("Warlock soundbank is missing. Deploy the complete Build/plugins folder.", path);
            SoundAPI.SoundBanks.Add(path);
            initialized = true;
        }
    }
}
