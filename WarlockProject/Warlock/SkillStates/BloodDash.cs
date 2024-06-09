using EntityStates;
using RoR2;
using UnityEngine;
using EntityStates.ImpMonster;
using EntityStates.ImpBossMonster;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class BloodDash : BaseWarlockSkillState
    {
		private Transform modelTransform;
		private float stopwatch;
		private Vector3 blinkVector = Vector3.zero;

        public Material destealthMaterial = WarlockAssets.destealthMaterial;
        // for AOE stun
        public static float blastAttackRadius = 10f;
		public static float blastAttackDamageCoefficient = 1.5f;
		public static float blastAttackProcCoefficient = 1f;
		public static float blastAttackForce = 1;

		[SerializeField]
		public float duration = 0.15f;

		[SerializeField]
		public float speedCoefficient = 10f;

		[SerializeField]
		private CharacterModel characterModel;
		private HurtBoxGroup hurtboxGroup;

		public override void OnEnter()
		{
			RefreshState();
			base.OnEnter();
			Util.PlaySound(EntityStates.ImpMonster.BlinkState.beginSoundString, base.gameObject);
			FireAOEStun();
			modelTransform = GetModelTransform();
			if ((bool)modelTransform)
			{
				characterModel = modelTransform.GetComponent<CharacterModel>();
				hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
			}
			if ((bool)characterModel)
			{
				characterModel.invisibilityCount++;
			}
			if ((bool)hurtboxGroup)
			{
				HurtBoxGroup hurtBoxGroup = hurtboxGroup;
				int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
				hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			blinkVector = GetBlinkVector();
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		}

		protected virtual Vector3 GetBlinkVector()
		{
			return ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(blinkVector);
			effectData.origin = origin;
			EffectManager.SpawnEffect(EntityStates.ImpMonster.BlinkState.blinkPrefab, effectData, transmit: false);
		}

		private void FireAOEStun()
		{
			if (base.isAuthority)
			{
				BlastAttack obj = new BlastAttack
				{
					radius = blastAttackRadius,
					procCoefficient = blastAttackProcCoefficient,
					position = base.transform.position,
					attacker = base.gameObject,
					crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
					baseDamage = base.characterBody.damage * blastAttackDamageCoefficient,
					falloffModel = BlastAttack.FalloffModel.None,
					damageType = DamageType.Stun1s,
					baseForce = blastAttackForce
				};
				obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
				obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
				obj.Fire();
			}
			if (GroundPound.slamEffectPrefab)
			{
				EffectData effectData = new EffectData();
				effectData.rotation = Util.QuaternionSafeLookRotation(blinkVector);
				effectData.origin = Util.GetCorePosition(base.gameObject);
				EffectManager.SpawnEffect(GroundPound.slamEffectPrefab, effectData, transmit: false);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.fixedDeltaTime;
			if ((bool)base.characterMotor && (bool)base.characterDirection)
			{
				base.characterMotor.velocity = Vector3.zero;
				base.characterMotor.rootMotion += blinkVector * (moveSpeedStat * speedCoefficient * Time.fixedDeltaTime);
			}
			if (stopwatch >= duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
            if (this.utilityEmpowered)
            {
				FireAOEStun();
            }
            if (!outer.destroying)
			{
				Util.PlaySound(EntityStates.ImpMonster.BlinkState.endSoundString, base.gameObject);
				CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
                modelTransform = GetModelTransform();
                if (modelTransform && this.destealthMaterial)
                {
                    TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = 1f;
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = this.destealthMaterial;
                    temporaryOverlay.inspectorCharacterModel = modelTransform.gameObject.GetComponent<CharacterModel>();
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.animateShaderAlpha = true;
                }
            }
			if ((bool)characterModel)
			{
				characterModel.invisibilityCount--;
			}
			if ((bool)hurtboxGroup)
			{
				HurtBoxGroup hurtBoxGroup = hurtboxGroup;
				int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
				hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			if ((bool)base.characterMotor)
			{
				base.characterMotor.disableAirControlUntilCollision = false;
			}
			base.OnExit();
		}
    }
}