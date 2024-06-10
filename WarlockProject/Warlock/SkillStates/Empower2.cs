using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using WarlockMod.Modules.BaseStates;
using EntityStates.ImpBossMonster;
using UnityEngine.Networking;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class Empower2 : BaseWarlockSkillState
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

            if (base.isAuthority)
            {
                this.warlockController.ReturnSavedStocks();
            }

            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(WarlockBuffs.warlockCrimsonManaFullStack);
                characterBody.SetBuffCount(WarlockBuffs.warlockEmpoweredM2Buff.buffIndex, characterBody.GetBuffCount(WarlockBuffs.warlockEmpoweredM2Buff) + Mathf.RoundToInt(skillLocator.secondary.maxStock / 2));
            }

            warlockController.jamTimer = 0f;

            EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "MetaMenu");
            if (entityStateMachine)
            {
                entityStateMachine.SetNextStateToMain();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= 0.1f)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            this.skillLocator.primary.UnsetSkillOverride(this.gameObject, WarlockSurvivor.m1EmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.secondary.UnsetSkillOverride(this.gameObject, WarlockSurvivor.m2EmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);
            this.skillLocator.utility.UnsetSkillOverride(this.gameObject, WarlockSurvivor.utilityEmpowerSkillDef, GenericSkill.SkillOverridePriority.Network);

            if (base.isAuthority)
            {
                this.warlockController.ReturnSavedStocks();
            }

            skillLocator.secondary.stock = skillLocator.secondary.maxStock;

            warlockController.jamTimer = 0f;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}


