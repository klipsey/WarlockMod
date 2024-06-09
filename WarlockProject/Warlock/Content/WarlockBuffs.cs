using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockBuffs
    {
        public static BuffDef warlockBloodMagicStack;
        public static BuffDef warlockBloodMagicFullStack;
        public static BuffDef warlockHexxedDebuff;
        public static BuffDef warlockHexxedEmpoweredDebuff;
        public static BuffDef warlockEmpoweredM1Buff;
        public static BuffDef warlockEmpoweredM2Buff;
        public static BuffDef warlockEmpoweredUtilityBuff;
        public static void Init(AssetBundle assetBundle)
        {
            warlockBloodMagicStack = Modules.Content.CreateAndAddBuff("WarlockBloodMagicStack", Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/VoidSurvivor/texBuffVoidSurvivorCorruptionIcon.tif").WaitForCompletion(),
                WarlockAssets.warlockSpecialRed, true, false, false);

            warlockBloodMagicFullStack = Modules.Content.CreateAndAddBuff("WarlockBloodMagicFullStack", Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/VoidSurvivor/texBuffVoidSurvivorCorruptionIcon.tif").WaitForCompletion(),
                Color.red, true, false, false);

            warlockHexxedDebuff = Modules.Content.CreateAndAddBuff("WarlockHexxed", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/DeathMark/texBuffDeathMarkIcon.tif").WaitForCompletion(),
                WarlockAssets.warlockSpecialRed, true, true, false);

            warlockHexxedEmpoweredDebuff = Modules.Content.CreateAndAddBuff("WarlockHexxedEmpowered", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/DeathMark/texBuffDeathMarkIcon.tif").WaitForCompletion(),
                Color.red, true, true, false);

            warlockEmpoweredM1Buff = Modules.Content.CreateAndAddBuff("WarlockQuickenPrimary", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion(),
                Color.red, true, false, false);

            warlockEmpoweredM2Buff = Modules.Content.CreateAndAddBuff("WarlockEmpoweredSecondary", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/DeathMark/texBuffDeathMarkIcon.tif").WaitForCompletion(),
                Color.red, true, false, false);

            warlockEmpoweredUtilityBuff = Modules.Content.CreateAndAddBuff("WarlockQuickenUtility", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texMovespeedBuffIcon.tif").WaitForCompletion(),
                Color.red, true, false, false);
        }
    }
}
