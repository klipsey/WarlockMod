using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockBuffs
    {
        public static BuffDef warlockCrimsonManaStack;
        public static BuffDef warlockCrimsonManaFullStack;
        public static BuffDef warlockHexxedDebuff;
        public static BuffDef warlockHexxedEmpoweredDebuff;
        public static BuffDef warlockHexxedMetaMagicDebuff;
        public static BuffDef warlockEmpoweredM1Buff;
        public static BuffDef warlockEmpoweredM2Buff;
        public static BuffDef warlockEmpoweredUtilityBuff;
        public static BuffDef warlockMetaMagicBuff;
        public static void Init(AssetBundle assetBundle)
        {
            warlockCrimsonManaStack = Modules.Content.CreateAndAddBuff("WarlockCrimsonManaStack", assetBundle.LoadAsset<Sprite>("texMetaMagicStackingBuff"),
                Color.white, true, false, false);

            warlockCrimsonManaFullStack = Modules.Content.CreateAndAddBuff("WarlockCrimsonManaFullStack", assetBundle.LoadAsset<Sprite>("texMetaMagicBuff"),
                Color.white, true, false, false);

            warlockHexxedDebuff = Modules.Content.CreateAndAddBuff("WarlockHexxed", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/DeathMark/texBuffDeathMarkIcon.tif").WaitForCompletion(),
                WarlockAssets.warlockSpecialRed, true, true, false);

            warlockHexxedEmpoweredDebuff = Modules.Content.CreateAndAddBuff("WarlockHexxedEmpowered", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/DeathMark/texBuffDeathMarkIcon.tif").WaitForCompletion(),
                WarlockAssets.warlockColor, true, true, false);

            warlockHexxedMetaMagicDebuff = Modules.Content.CreateAndAddBuff("WarlockHexxedMetaMagic", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/LaserTurbine/texLaserTurbineKillChargeBuffIcon.tif").WaitForCompletion(),
                WarlockAssets.warlockColor, true, false, false);

            warlockEmpoweredM1Buff = Modules.Content.CreateAndAddBuff("WarlockQuickenPrimary", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion(),
                WarlockAssets.warlockColor, true, false, false);

            warlockEmpoweredM2Buff = Modules.Content.CreateAndAddBuff("WarlockEmpoweredSecondary", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/DeathMark/texBuffDeathMarkIcon.tif").WaitForCompletion(),
                WarlockAssets.warlockColor, true, false, false);

            warlockEmpoweredUtilityBuff = Modules.Content.CreateAndAddBuff("WarlockQuickenUtility", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texMovespeedBuffIcon.tif").WaitForCompletion(),
                WarlockAssets.warlockColor, true, false, false);

            warlockMetaMagicBuff = Modules.Content.CreateAndAddBuff("WarlockMetaMagicEmpower", assetBundle.LoadAsset<Sprite>("texEmpoweredMetaMagicBuff"),
                Color.white, true, false, false);
        }
    }
}
