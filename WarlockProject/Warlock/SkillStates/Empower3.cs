using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;
using EntityStates.ImpBossMonster;
using UnityEngine.Networking;

namespace WarlockMod.Warlock.SkillStates
{
    public class Empower3 : BaseWarlockSkillState
	{
		public string enterSoundString;
		public float baseDuration = 3f;
		public float dampingCoefficient = 1.2f;
		private float duration;
		private Animator animator;

		public static GameObject areaIndicatorPrefab;
		private GameObject areaIndicatorInstance;
		public static float indicatorStartingRadius = 2.5f;
		public static float indicatorScalingCoefficient = .01f;

		public CharacterCameraParams cameraParams;
		private CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            this.warlockController.PlaySound();
            //return to special
            PlayAnimation("Gesture, Override", "SwapToGun", "Grab.playbackRate", 0.5f / base.characterBody.attackSpeed);
            this.skillLocator.primary.UnsetSkillOverride(this.gameObject, WarlockSurvivor.m1EmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.secondary.UnsetSkillOverride(this.gameObject, WarlockSurvivor.m2EmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.utility.UnsetSkillOverride(this.gameObject, WarlockSurvivor.utilityEmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.special.UnsetSkillOverride(this.gameObject, WarlockSurvivor.cancelSkillDef, GenericSkill.SkillOverridePriority.Network);

            if (base.isAuthority)
            {
                this.warlockController.ReturnSavedStocks();
            }

            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(WarlockBuffs.warlockBloodMagicFullStack);
                characterBody.AddTimedBuff(WarlockBuffs.warlockEmpoweredUtilityBuff, WarlockStaticValues.utilityDuration);
            }

            skillLocator.utility.stock = skillLocator.utility.maxStock;

            warlockController.jamTimer = 0f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= 0.25f)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
