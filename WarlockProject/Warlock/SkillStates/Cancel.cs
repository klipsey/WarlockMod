using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using WarlockMod.Modules.BaseStates;
using EntityStates.ImpBossMonster;

namespace WarlockMod.Warlock.SkillStates
{
    public class Cancel : BaseWarlockSkillState
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
            Util.PlaySound("sfx_scout_swap_weapon", this.gameObject);
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

            if (base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
	}
}
