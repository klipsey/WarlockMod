using RoR2;
using UnityEngine;
using WarlockMod.Warlock;
using WarlockMod.Warlock.Achievements;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            /*
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(SpyMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(SpyMasteryAchievement.unlockableIdentifier),
                InterrogatorSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMonsoonSkin"));
            */
            /*
            if (true == false)
            {
                characterUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(SpyUnlockAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(SpyUnlockAchievement.unlockableIdentifier),
                SpySurvivor.instance.assetBundle.LoadAsset<Sprite>("texSpyIcon"));
            }
            */
        }
    }
}
