using RoR2;
using WarlockMod.Warlock.Achievements;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            characterUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(WarlockUnlockAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(WarlockUnlockAchievement.identifier),
                WarlockAssets.CreateUnlockIcon());
        }
    }
}
