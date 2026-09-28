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
        private bool continuingHex;
        private WarlockTracker tracker;
        private CameraTargetParams.AimRequest aimRequest;
        private bool transferringTarget;

        protected override bool IsHeld => inputBank && inputBank.skill2.down;
        protected override GameObject ChargeEffectPrefab => WarlockAssets.hexChargeEffect;
        protected override bool CanCharge => base.CanCharge && victim && victim.healthComponent &&
            victim.healthComponent.alive && victim.healthComponent.body &&
            victim.healthComponent.body.teamComponent.teamIndex != characterBody.teamComponent.teamIndex &&
            Vector3.Distance(inputBank.aimOrigin, victim.healthComponent.body.corePosition) <=
                tracker.maxTrackingDistance + victim.healthComponent.body.radius;

        public override void OnEnter()
        {
            tracker = GetComponent<WarlockTracker>();
            if (!continuingHex && isAuthority) victim = tracker.GetTrackingTarget();
            base.OnEnter();
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
            if (NetworkServer.active)
            {
                empowered = warlockController.TryConsumeEmpowerment(WarlockBuffs.warlockEmpoweredM2Buff);
                ApplyHex(false);
            }
        }

        protected override void ApplyConsumedStack() => ApplyHex(true);

        private void ApplyHex(bool metamagic)
        {
            var targetBody = victim.healthComponent.body;
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

        protected override EntityState FinishCharge() => new Idle();

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            if (nextState is Hex next)
            {
                transferringTarget = true;
                next.continuingHex = true;
                next.victim = victim;
                next.empowered = empowered;
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
            writer.Write(continuingHex);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            victim = reader.ReadHurtBoxReference().ResolveHurtBox();
            empowered = reader.ReadBoolean();
            continuingHex = reader.ReadBoolean();
        }
    }
}
