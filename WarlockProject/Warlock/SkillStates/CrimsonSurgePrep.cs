using EntityStates;
using RoR2;
using UnityEngine;
using WarlockMod.Modules.BaseStates;
using EntityStates.Wisp1Monster;

namespace WarlockMod.Warlock.SkillStates
{
    public class CrimsonSurgePrep : BaseWarlockSkillState
    {
		public string enterSoundString;
		public float baseDuration = 0.5f;
		private float duration;
		private Animator animator;

		public override void OnEnter()
		{
			RefreshState();
			base.OnEnter();
			this.duration = this.baseDuration / base.attackSpeedStat;
			if (this.primaryEmpowered) this.duration *= 0.85f;
            //PlayAnimation("Gesture, Additive", "MainToSide", "MainToSide.playbackRate", duration);
            Util.PlayAttackSpeedSound("Play_imp_overlord_attack2_tell", gameObject, attackSpeedStat);
        }

        public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge > this.duration && warlockController.jamTimer <= 0f)
			{
				CrimsonSurgeFire FireState = new CrimsonSurgeFire();
				outer.SetNextState(FireState);
			}
		}

		public override void OnExit()
		{
			base.OnExit();
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}