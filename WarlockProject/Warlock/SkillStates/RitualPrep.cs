using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using WarlockMod.Modules.BaseStates;
using EntityStates.ImpBossMonster;

namespace WarlockMod.Warlock.SkillStates
{
    public class RitualPrep : BaseWarlockSkillState
	{
        private float baseDuration = 0.15f;
		public string enterSoundString;
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
            Util.PlaySound("sfx_scout_swap_weapon", this.gameObject);
            //return to gun
            PlayAnimation("Gesture, Override", "SwapToBat", "Grab.playbackRate", 0.5f / base.characterBody.attackSpeed);

            this.warlockController.jamTimer = 1f / this.attackSpeedStat;

			this.warlockController.maxSecondaryStock = this.skillLocator.secondary.maxStock;
			this.warlockController.currentSecondaryStock = this.skillLocator.secondary.stock;
			this.warlockController.maxUtilityStock = this.skillLocator.utility.maxStock;
			this.warlockController.currentUtilityStock = this.skillLocator.utility.stock;

            this.skillLocator.primary.SetSkillOverride(this.gameObject, WarlockSurvivor.m1EmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.secondary.SetSkillOverride(this.gameObject, WarlockSurvivor.m2EmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.utility.SetSkillOverride(this.gameObject, WarlockSurvivor.utilityEmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.isAuthority && IsKeyDownAuthority() && this.fixedAge >= this.baseDuration)
            {
                outer.SetNextState(new Empower());
            }
        }

        public override void OnExit()
        {
            this.skillLocator.primary.UnsetSkillOverride(this.gameObject, WarlockSurvivor.m1EmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.secondary.UnsetSkillOverride(this.gameObject, WarlockSurvivor.m2EmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.utility.UnsetSkillOverride(this.gameObject, WarlockSurvivor.utilityEmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);

            if (base.isAuthority)
            {
                this.warlockController.ReturnSavedStocks();
            }

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}


