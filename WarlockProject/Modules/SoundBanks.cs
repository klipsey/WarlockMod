using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            initialized = true;
            AKRESULT akResult = AkSoundEngine.AddBasePath(SoundBankDirectory);

            AkSoundEngine.LoadBank("interrogator_bank.bnk", out _);
        }
    }
}
