using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class BloodDashPrep : BaseMetamagicCharge
    {
        private static readonly int ChargeBlink = Animator.StringToHash("ChargeBlink");
        private static readonly int ChargeBlinkLoop = Animator.StringToHash("ChargeBlinkLoop");
        private Vector3 destination;
        private bool hasDestination;
        private bool transferring;
        private bool refunding;
        private float chargeAge;
        private Vector3 frozenPosition;
        private bool movementLocked;
        private bool motorWasEnabled;
        private Vector3 initialCameraPosition;
        private GameObject preview;
        private CameraTargetParams.CameraParamsOverrideHandle cameraOverride;

        internal float MaxDistance => 25f + 5f * consumedStacks;
        protected override bool IsHeld => inputBank && inputBank.skill3.down;
        protected override bool CanCharge => base.CanCharge && characterMotor;
        protected override GameObject ChargeEffectPrefab => WarlockAssets.hexChargeEffect;

        protected override void BeginCharge()
        {
            StartAimMode(0.5f);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            UpdateAnimation();
            if (characterMotor && !movementLocked)
            {
                movementLocked = true;
                frozenPosition = transform.position;
                motorWasEnabled = characterMotor.enabled;
                characterMotor.enabled = false;
                HoldPosition();
                if (isAuthority && cameraTargetParams)
                    initialCameraPosition = cameraTargetParams.currentCameraParamsData.idealLocalCameraPos.value;
            }
            if (!isAuthority) return;
            if (!preview)
            {
                preview = Object.Instantiate(WarlockAssets.dashDestinationPreview);
                preview.transform.localScale = Vector3.one * BloodDash.blastAttackRadius;
                preview.SetActive(false);
            }
            UpdateDestination();
        }

        public override void Update()
        {
            base.Update();
            UpdateAnimation();
            if (!isAuthority) return;
            chargeAge += Time.deltaTime;
            if (cameraTargetParams)
            {
                var data = new CharacterCameraParamsData
                {
                    idealLocalCameraPos = initialCameraPosition + new Vector3(0f, chargeAge, -4f * chargeAge)
                };
                cameraTargetParams.RemoveParamsOverride(cameraOverride, 0f);
                cameraOverride = cameraTargetParams.AddParamsOverride(new CameraTargetParams.CameraParamsOverrideRequest
                {
                    cameraParamsData = data,
                    priority = 0.1f
                }, 0f);
            }
        }

        private void UpdateAnimation()
        {
            if (!modelAnimator || !modelAnimator.isActiveAndEnabled) return;
            modelAnimator.SetBool("chargingBlink", true);
            int state = GetAnimationStateHash("FullBody, Override");
            if (state != ChargeBlink && state != ChargeBlinkLoop)
                PlayCrossfade("FullBody, Override", "ChargeBlink", 0.05f);
        }

        public override void FixedUpdate()
        {
            HoldPosition();
            if (isAuthority) UpdateDestination();
            base.FixedUpdate();
        }

        private void HoldPosition()
        {
            if (!characterMotor || !movementLocked) return;
            characterMotor.velocity = Vector3.zero;
            characterMotor.rootMotion = Vector3.zero;
            characterMotor.Motor.SetPosition(frozenPosition);
        }

        private void UpdateDestination()
        {
            if (!characterMotor) return;
            Ray aim = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), gameObject, out float cameraDistance);
            float rayDistance = MaxDistance + cameraDistance;
            Vector3 point = Physics.Raycast(aim, out var hit, rayDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore)
                ? hit.point : aim.GetPoint(rayDistance);
            if (TryGetSafePosition(point, out var safePosition))
            {
                destination = safePosition;
                hasDestination = true;
            }
            if (preview)
            {
                preview.SetActive(hasDestination);
                if (hasDestination) preview.transform.position = destination;
            }
        }

        private bool TryGetSafePosition(Vector3 targetPosition, out Vector3 position)
        {
            Vector3 scale = transform.lossyScale;
            float radius = characterMotor.capsuleRadius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float height = Mathf.Max(radius * 2f, characterMotor.capsuleHeight * Mathf.Abs(scale.y));
            Vector3 center = transform.TransformPoint(Vector3.up * characterMotor.capsuleYOffset);
            Vector3 halfSegment = Vector3.up * (height * 0.5f - radius);
            Vector3 bottom = center - halfSegment;
            Vector3 top = center + halfSegment;
            Vector3 offset = Vector3.ClampMagnitude(targetPosition - transform.position, MaxDistance);
            // Leave a small skin so existing ground contact does not block the sweep.
            float castRadius = Mathf.Max(0.01f, radius - 0.02f);
            if (offset.sqrMagnitude > 0f && Physics.CapsuleCast(bottom, top, castRadius, offset.normalized,
                out var hit, offset.magnitude, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                offset = offset.normalized * Mathf.Max(0f, hit.distance - 0.05f);
            position = transform.position + offset;
            return !Physics.CheckCapsule(bottom + offset, top + offset, castRadius,
                LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
        }

        protected override BaseMetamagicCharge NextStep() => new BloodDashPrep();

        protected override EntityState FinishCharge()
        {
            if (consumedStacks == 0)
                return new BloodDash
                {
                    activatorSkillSlot = activatorSkillSlot
                };
            if (!hasDestination)
            {
                Log.Warning("Blood Dash found no unobstructed destination; refunding its charge and metamagic.");
                return new Idle();
            }
            return new BloodDash
            {
                targeted = true,
                destination = destination,
                metamagicStacks = consumedStacks,
                activatorSkillSlot = activatorSkillSlot
            };
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            if (nextState is BloodDashPrep next)
            {
                transferring = true;
                next.destination = destination;
                next.hasDestination = hasDestination;
                next.chargeAge = chargeAge;
                next.frozenPosition = frozenPosition;
                next.movementLocked = movementLocked;
                next.motorWasEnabled = motorWasEnabled;
                next.initialCameraPosition = initialCameraPosition;
                next.preview = preview;
                next.cameraOverride = cameraOverride;
            }
            if (nextState is BloodDash dash && (NetworkServer.active || isAuthority))
            {
                dash.metamagicStacks = consumedStacks;
            }
            refunding = nextState is Idle;
        }

        public override void OnExit()
        {
            if (!transferring)
            {
                if (modelAnimator)
                {
                    modelAnimator.SetBool("chargingBlink", false);
                    if (modelAnimator.isActiveAndEnabled && GetAnimationStateHash("FullBody, Override") == ChargeBlink)
                        PlayCrossfade("FullBody, Override", "BufferEmpty", 0.05f);
                }
                if (preview) Destroy(preview);
                if (cameraTargetParams) cameraTargetParams.RemoveParamsOverride(cameraOverride, 0.25f);
                if (characterMotor && movementLocked) characterMotor.enabled = motorWasEnabled;
            }
            if (refunding && characterBody.healthComponent && characterBody.healthComponent.alive)
            {
                if (NetworkServer.active && consumedStacks > 0)
                    characterBody.SetBuffCount(WarlockBuffs.warlockMetaMagicBuff.buffIndex,
                        characterBody.GetBuffCount(WarlockBuffs.warlockMetaMagicBuff) + consumedStacks);
                if (isAuthority) skillLocator.utility.AddOneStock();
            }
            base.OnExit();
        }

    }
}
