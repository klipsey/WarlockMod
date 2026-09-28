using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace WarlockMod.Modules
{
    internal static class SoundBanks
    {
        internal const string BloodExplosionEvent = "Play_deathsquelch";
        internal const string OrbConsumeEvent = "Play_warlockconsume";
        internal const string HexEvent = "Play_warlockhex";
        private static NetworkSoundEventDef openMenuSound;
        private static NetworkSoundEventDef useManaSound;
        private static NetworkSoundEventDef consumeSound;
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            SoundAPI.SoundBanks.Add(Assets.ReadEmbeddedResource("WarlockMod.warlock_bank.bnk"));
            openMenuSound = Content.CreateAndAddNetworkSoundEventDef("Play_warlock_open_menu");
            useManaSound = Content.CreateAndAddNetworkSoundEventDef("Play_warlock_use_mana");
            consumeSound = Content.CreateAndAddNetworkSoundEventDef(OrbConsumeEvent);
            initialized = true;
        }

        internal static void PlayOpenMenu(Vector3 position)
        {
            if (NetworkServer.active)
                EffectManager.SimpleSoundEffect(openMenuSound.index, position, true);
        }

        internal static void PlayUseMana(Vector3 position)
        {
            if (NetworkServer.active)
                EffectManager.SimpleSoundEffect(useManaSound.index, position, true);
        }

        internal static void PlayConsume(Vector3 position)
        {
            if (NetworkServer.active)
                EffectManager.SimpleSoundEffect(consumeSound.index, position, true);
        }
    }
}
