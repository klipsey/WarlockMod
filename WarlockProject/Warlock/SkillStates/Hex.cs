using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using WarlockMod.Warlock.Components;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class Hex : BaseMetamagicCharge
    {
        private static readonly int HexAnimation = Animator.StringToHash("Hex");
        private HurtBox victim;
        private bool empowered;
        private bool hasAppliedHex;
        private bool continuingHex;
        private WarlockTracker tracker;
        private CameraTargetParams.AimRequest aimRequest;
        private bool transferringTarget;

        protected override bool IsHeld => inputBank && inputBank.skill2.down;
        protected override GameObject ChargeEffectPrefab => WarlockAssets.hexChargeEffect;
        protected override float InitialChargeDuration => 0.5f;
        protected override bool FinishOnRelease => true;
        protected override bool CanCharge => base.CanCharge && IsTargetValid(victim, characterBody, tracker);

        internal static bool IsTargetValid(HurtBox target, CharacterBody caster, WarlockTracker tracker)
        {
            if (!tracker || !caster || !caster.teamComponent || !caster.inputBank ||
                !target || !target.healthComponent || !target.healthComponent.alive) return false;
            var targetBody = target.healthComponent.body;
            if (!targetBody || !targetBody.teamComponent ||
                targetBody.teamComponent.teamIndex == caster.teamComponent.teamIndex) return false;
            float maxDistance = tracker.maxTrackingDistance + targetBody.radius;
            return maxDistance >= 0f &&
                (caster.inputBank.aimOrigin - targetBody.corePosition).sqrMagnitude <= maxDistance * maxDistance;
        }

        public override void OnEnter()
        {
            tracker = GetComponent<WarlockTracker>();
            if (!continuingHex && isAuthority) victim = tracker.GetTrackingTarget();
            base.OnEnter();
            if (continuingHex && NetworkServer.active && !hasAppliedHex && CanCharge)
                ApplyHex(false);
            UpdateAnimation();
            if (victim)
            {
                tracker.SetChargeTarget(victim);
                if (cameraTargetParams) aimRequest = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (modelAnimator && modelAnimator.isActiveAndEnabled && CanPlayGestureAnimation(false) &&
                GetAnimationStateHash("Gesture, Override") != HexAnimation)
            {
                modelAnimator.SetFloat("Hex.playbackRate", Mathf.Max(0.01f, attackSpeedStat));
                PlayCrossfade("Gesture, Override", "Hex", 0.05f);
            }
        }

        protected override void BeginCharge()
        {
            if (!CanCharge) return;
            StartAimMode(0.5f);
        }

        protected override void ApplyConsumedStack() => ApplyHex(true);

        private void ApplyHex(bool metamagic)
        {
            if (!hasAppliedHex)
                empowered = warlockController.TryConsumeEmpowerment(WarlockBuffs.warlockEmpoweredM2Buff);
            hasAppliedHex = true;
            ApplyHex(victim, empowered, metamagic);
        }

        internal static void ApplyHex(HurtBox target, bool empowered, bool metamagic)
        {
            var targetBody = target.healthComponent.body;
            targetBody.AddTimedBuff(empowered ? WarlockBuffs.warlockHexxedEmpoweredDebuff : WarlockBuffs.warlockHexxedDebuff,
                WarlockConfig.HexDuration);
            if (metamagic)
                targetBody.AddTimedBuff(WarlockBuffs.warlockHexxedMetaMagicDebuff, WarlockConfig.HexDuration);
            EffectManager.SpawnEffect(WarlockAssets.warlockHexConsume, new EffectData
            {
                origin = targetBody.corePosition,
                scale = 1.5f
            }, true);
        }

        protected override BaseMetamagicCharge NextStep() => new Hex();

        protected override EntityState FinishCharge() => consumedStacks > 0 || hasAppliedHex
            ? (EntityState)new Idle()
            : new HexFire { target = victim, activatorSkillSlot = activatorSkillSlot };

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            if (nextState is Hex next)
            {
                transferringTarget = true;
                next.continuingHex = true;
                next.victim = victim;
                next.empowered = empowered;
                next.hasAppliedHex = hasAppliedHex;
            }
        }

        public override void OnExit()
        {
            if (!transferringTarget && tracker) tracker.ClearChargeTarget();
            aimRequest?.Dispose();
            base.OnExit();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(HurtBoxReference.FromHurtBox(victim));
            writer.Write(empowered);
            writer.Write(hasAppliedHex);
            writer.Write(continuingHex);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            victim = reader.ReadHurtBoxReference().ResolveHurtBox();
            empowered = reader.ReadBoolean();
            hasAppliedHex = reader.ReadBoolean();
            continuingHex = reader.ReadBoolean();
        }
    }
}
