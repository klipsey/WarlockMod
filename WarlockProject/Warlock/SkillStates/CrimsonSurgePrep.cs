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
		public string chargeMuzzle = "Muzzle";
		public GameObject ChargeUpPrefab = WarlockAssets.spawnPrefab;
		private GameObject portal;
        private const float PortalExitDuration = 0.15f;

		public override void OnEnter()
		{
			RefreshState();
			base.OnEnter();
            Transform muzzle = FindModelChild(chargeMuzzle);
            if (muzzle)
            {
                portal = Object.Instantiate(ChargeUpPrefab, muzzle.position, Quaternion.LookRotation(GetAimRay().direction));
                portal.GetComponent<EffectComponent>().noEffectData = true;
                portal.transform.localScale = Vector3.one * 0.2f;
                portal.transform.SetParent(muzzle, true);
            }
            else
            {
                Log.Error($"Cannot attach Crimson Surge's portal: model child '{chargeMuzzle}' is missing.");
            }
            this.duration = this.baseDuration / base.attackSpeedStat;
			if (this.primaryEmpowered) this.duration *= 0.85f;
            //PlayAnimation("Gesture, Additive", "MainToSide", "MainToSide.playbackRate", duration);
            Util.PlayAttackSpeedSound("Play_imp_overlord_attack2_tell", gameObject, attackSpeedStat);
        }

        public override void FixedUpdate()
		{
			base.FixedUpdate();
            if (portal) portal.transform.rotation = Quaternion.LookRotation(GetAimRay().direction);
			if (base.isAuthority && base.fixedAge > this.duration && warlockController.jamTimer <= 0f)
			{
				CrimsonSurgeFire FireState = new CrimsonSurgeFire();
				outer.SetNextState(FireState);
			}
		}

		public override void OnExit()
		{
			base.OnExit();
            if (portal)
            {
                foreach (var particles in portal.GetComponentsInChildren<ParticleSystem>(true))
                    particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                Object.Destroy(portal, PortalExitDuration);
                portal = null;
            }
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}