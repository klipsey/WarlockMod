using EntityStates;
using RoR2;
using UnityEngine;
using WarlockMod.Modules.BaseStates;
using EntityStates.Wisp1Monster;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class CrimsonSurgePrep : BaseWarlockSkillState
    {
		public string enterSoundString;
		public float baseDuration = 0.5f;
		private float duration;
		public string chargeMuzzle = "EldritchMuzzle";
		public GameObject ChargeUpPrefab = WarlockAssets.spawnPrefab;
		private Animator animator;
		private GameObject portal;

		public override void OnEnter()
		{
			RefreshState();
			base.OnEnter();
            EffectManager.SpawnEffect(ChargeUpPrefab, new EffectData
            {
                origin = FindModelChild(chargeMuzzle).position,
                scale = 0.2f,
				genericFloat = this.baseDuration / base.attackSpeedStat,
				rotation = Quaternion.LookRotation(GetAimRay().direction),
            }, transmit: true);
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
			GameObject.Destroy(portal);
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}