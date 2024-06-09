using BepInEx.Configuration;
using WarlockMod.Modules;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockConfig
    {
        public static ConfigEntry<bool> forceUnlock;
        public static ConfigEntry<float> stabDamageCoefficient;
        public static ConfigEntry<float> maxCloakDefault;
        public static ConfigEntry<float> cloakHealthCost;
        public static ConfigEntry<float> maxCloakDead;
        public static ConfigEntry<float> bigEarnerHealthPunishment;
        public static ConfigEntry<bool> bigEarnerFullyResets;
        public static ConfigEntry<float> sapperRange;
        public static void Init()
        {
            string section = "01 - General";
            string section2 = "02 - Stats";

            //add more here or else you're cringe
            forceUnlock = Config.BindAndOptions(
                section,
                "Unlock Interrogator",
                false,
                "Unlock Interrogator.", true);
        }
    }
}
