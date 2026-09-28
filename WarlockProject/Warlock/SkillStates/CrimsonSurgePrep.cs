using EntityStates;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class CrimsonSurgePrep : BaseMetamagicCharge
    {
        private static readonly int BlastCharge = Animator.StringToHash("BlastCharge");
        private bool firing;

        protected override bool IsHeld => inputBank && inputBank.skill1.down;
        protected override GameObject ChargeEffectPrefab => WarlockAssets.spawnPrefab;
        protected override bool WaitForInitialCharge => true;

        public override void OnEnter()
        {
            base.OnEnter();
            UpdateAnimation();
        }

        public override void Update()
        {
            base.Update();
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (!modelAnimator || !modelAnimator.isActiveAndEnabled) return;
            modelAnimator.SetBool("chargingBlast", true);
            modelAnimator.SetBool("isBlasting", consumedStacks > 0);
            if (CanPlayGestureAnimation(true) && GetAnimationStateHash("Gesture, Override") != BlastCharge)
            {
                modelAnimator.SetFloat("Blast.playbackRate", attackSpeedStat);
                PlayCrossfade("Gesture, Override", "BlastCharge", 0.05f);
            }
        }

        protected override void BeginCharge()
        {
            StartAimMode(0.5f);
            Util.PlayAttackSpeedSound("Play_voidDevastator_m1_chargeUp", gameObject, attackSpeedStat);
        }

        protected override BaseMetamagicCharge NextStep() => new CrimsonSurgePrep();

        protected override EntityState FinishCharge() => new CrimsonSurgeFire();

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            firing = nextState is CrimsonSurgeFire;
            if (nextState is CrimsonSurgeFire fire && (NetworkServer.active || isAuthority))
            {
                fire.maxShots = consumedStacks + 1;
                if (isAuthority) fire.initialAimRay = GetAimRay();
                fire.activatorSkillSlot = activatorSkillSlot;
            }
        }

        public override void OnExit()
        {
            if (!IsContinuingCharge && modelAnimator)
            {
                modelAnimator.SetBool("chargingBlast", false);
                modelAnimator.SetBool("isBlasting", false);
                if (!firing && modelAnimator.isActiveAndEnabled && GetAnimationStateHash("Gesture, Override") == BlastCharge)
                    PlayCrossfade("Gesture, Override", "BufferEmpty", 0.05f);
            }
            base.OnExit();
        }
    }
}
