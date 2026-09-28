using EntityStates;
using RoR2;
using UnityEngine;
using EntityStates.ImpMonster;
using EntityStates.ImpBossMonster;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;
using UnityEngine.Networking;

namespace WarlockMod.Warlock.SkillStates
{
    public class BloodDash : BaseWarlockSkillState
    {
        public bool crimsonManaEmpowered;
        public bool targeted;
        public Vector3 destination;
        public int metamagicStacks;
        private Vector3 blinkStart;
        private bool arrived;
        private bool motorWasEnabled;
        private int originalLayer;
        private GameObject destinationEffect;
		private Transform modelTransform;
		private float stopwatch;
		private Vector3 blinkVector = Vector3.zero;

        public Material destealthMaterial = WarlockAssets.destealthMaterial;
        // for AOE stun
        public static float blastAttackRadius = 15f;
		public static float blastAttackDamageCoefficient => WarlockConfig.BloodDashDamage;
		public static float blastAttackProcCoefficient => WarlockConfig.BloodDashProc;
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
            utilityEmpowered |= crimsonManaEmpowered;
			Util.PlaySound(targeted ? "Play_imp_overlord_teleport_start" : EntityStates.ImpMonster.BlinkState.beginSoundString, base.gameObject);
			if (!targeted || utilityEmpowered) FireAOEStun(transform.position);
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
            blinkStart = transform.position;
			blinkVector = targeted ? (destination - blinkStart).normalized : GetBlinkVector();
            if (targeted && characterMotor)
            {
                duration = 0.1f;
                motorWasEnabled = characterMotor.enabled;
                characterMotor.enabled = false;
                originalLayer = gameObject.layer;
                gameObject.layer = LayerIndex.GetAppropriateFakeLayerForTeam(teamComponent.teamIndex).intVal;
                characterMotor.Motor.RebuildCollidableLayers();
                destinationEffect = Object.Instantiate(WarlockAssets.dashBlinkDestinationEffect, destination, Quaternion.identity);
                destinationEffect.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
            }
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		}

		protected virtual Vector3 GetBlinkVector()
		{
			return ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
		}

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(crimsonManaEmpowered);
            writer.Write(targeted);
            writer.Write(destination);
            writer.Write(metamagicStacks);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            crimsonManaEmpowered = reader.ReadBoolean();
            targeted = reader.ReadBoolean();
            destination = reader.ReadVector3();
            metamagicStacks = Mathf.Max(0, reader.ReadInt32());
        }

		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(blinkVector);
			effectData.origin = origin;
			EffectManager.SpawnEffect(targeted ? WarlockAssets.dashBlinkEffect : EntityStates.ImpMonster.BlinkState.blinkPrefab,
                effectData, transmit: false);
		}

		private void FireAOEStun(Vector3 position)
		{
			if (targeted ? NetworkServer.active : base.isAuthority)
			{
				DamageTypeCombo damageType = DamageType.Stun1s;
				damageType.damageSource = DamageSource.Utility;
                BlastAttack obj = new BlastAttack
				{
					radius = blastAttackRadius,
					procCoefficient = blastAttackProcCoefficient,
					position = position,
					attacker = base.gameObject,
                    inflictor = base.gameObject,
					crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
					baseDamage = base.characterBody.damage * (blastAttackDamageCoefficient + (targeted ? metamagicStacks : 0)),
					falloffModel = BlastAttack.FalloffModel.None,
					damageType = damageType,
					baseForce = blastAttackForce
				};
				obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
				obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
				obj.Fire();
			}
			if (!targeted && GroundPound.slamEffectPrefab)
			{
				EffectData effectData = new EffectData();
				effectData.rotation = Util.QuaternionSafeLookRotation(blinkVector);
				effectData.origin = Util.GetCorePosition(base.gameObject);
				EffectManager.SpawnEffect(GroundPound.slamEffectPrefab, effectData, transmit: false);
			}
		}

        private void CompleteTargetedBlink()
        {
            if (arrived) return;
            arrived = true;
            if (characterMotor)
            {
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion = Vector3.zero;
                characterMotor.Motor.SetPosition(destination);
            }
            if (isAuthority) TeleportHelper.TeleportGameObject(gameObject, destination);
            if (destinationEffect) Destroy(destinationEffect);
            destinationEffect = null;
            Util.PlaySound("Play_imp_overlord_teleport_end", gameObject);
            CreateBlinkEffect(destination);
            if (characterBody.healthComponent && characterBody.healthComponent.alive) FireAOEStun(destination);
        }

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.fixedDeltaTime;
            if (targeted)
            {
                if (characterMotor)
                {
                    characterMotor.velocity = Vector3.zero;
                    characterMotor.rootMotion = Vector3.zero;
                    characterMotor.Motor.SetPosition(Vector3.Lerp(blinkStart, destination, stopwatch / duration));
                }
                if (stopwatch >= duration && !arrived)
                    CompleteTargetedBlink();
                if (arrived && isAuthority) outer.SetNextStateToMain();
                return;
            }
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
            if (targeted && !arrived && !outer.destroying &&
                characterBody.healthComponent && characterBody.healthComponent.alive)
                CompleteTargetedBlink();
            if (!targeted && this.utilityEmpowered)
            {
				FireAOEStun(transform.position);
            }
            if (!outer.destroying)
			{
                if (!targeted)
                {
                    Util.PlaySound(EntityStates.ImpMonster.BlinkState.endSoundString, base.gameObject);
                    CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
                }
                modelTransform = GetModelTransform();
                if (modelTransform && this.destealthMaterial)
                {
                    TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlay.duration = 1f;
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = this.destealthMaterial;
                    temporaryOverlay.inspectorCharacterModel = modelTransform.gameObject.GetComponent<CharacterModel>();
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.animateShaderAlpha = true;
                }
            }
            if (destinationEffect) Destroy(destinationEffect);
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
                if (targeted)
                {
                    gameObject.layer = originalLayer;
                    characterMotor.Motor.RebuildCollidableLayers();
                    characterMotor.enabled = motorWasEnabled;
                }
			}
			base.OnExit();
		}

        public override InterruptPriority GetMinimumInterruptPriority() =>
            targeted ? InterruptPriority.PrioritySkill : base.GetMinimumInterruptPriority();
    }
}