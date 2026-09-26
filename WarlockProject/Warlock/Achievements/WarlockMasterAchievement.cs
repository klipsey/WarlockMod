using RoR2;
using WarlockMod.Modules.Achievements;
using WarlockMod.Warlock;

namespace WarlockMod.Warlock.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 10, null)]
    public class WarlockMasterAchievement : BaseMasteryAchievement
    {
        public const string identifier = WarlockSurvivor.WARLOCK_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = WarlockSurvivor.WARLOCK_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => WarlockSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}