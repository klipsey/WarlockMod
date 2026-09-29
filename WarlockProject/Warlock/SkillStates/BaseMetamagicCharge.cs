using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public abstract class BaseMetamagicCharge : BaseWarlockSkillState
    {
        protected int consumedStacks;
        protected bool hadMetaMagic;
        private int remainingStacks;
        private float interval;
        private float nextConsumption;
        private bool continuing;
        private bool transferringEffect;
        private bool releasedDuringWindup;
        private GameObject chargeEffect;

        protected bool IsContinuingCharge => transferringEffect;
        protected abstract bool IsHeld { get; }
        protected abstract GameObject ChargeEffectPrefab { get; }
        protected virtual float ChargeSpeed => attackSpeedStat;
        protected virtual float InitialChargeDuration => interval;
        protected virtual bool WaitForInitialCharge => false;
        protected virtual bool FinishOnRelease => false;
        protected virtual bool CanCharge => characterBody && characterBody.healthComponent && characterBody.healthComponent.alive;
        protected virtual void BeginCharge() { }
        protected virtual void ApplyConsumedStack() { }
        protected abstract BaseMetamagicCharge NextStep();
        protected abstract EntityState FinishCharge();

        public override void OnEnter()
        {
            base.OnEnter();
            if (!continuing)
            {
                if (NetworkServer.active || isAuthority)
                {
                    consumedStacks = 0;
                    remainingStacks = characterBody.GetBuffCount(WarlockBuffs.warlockMetaMagicBuff);
                    hadMetaMagic = remainingStacks > 0;
                    interval = Mathf.Max(0.05f, 0.5f / Mathf.Max(0.01f, ChargeSpeed));
                    nextConsumption = InitialChargeDuration;
                }
                BeginCharge();
            }
            if (CanCharge && !chargeEffect)
                chargeEffect = WarlockAssets.CreateChargeEffect(ChargeEffectPrefab, FindModelChild("Muzzle"), GetAimRay().direction);

            if (continuing && (NetworkServer.active || isAuthority) && CanCharge)
            {
                bool consumed = NetworkServer.active
                    ? warlockController.TryConsumeMetaMagic()
                    : remainingStacks > 0;
                if (consumed)
                {
                    consumedStacks++;
                    remainingStacks--;
                    if (NetworkServer.active) ApplyConsumedStack();
                }
                if (NetworkServer.active)
                    remainingStacks = characterBody.GetBuffCount(WarlockBuffs.warlockMetaMagicBuff);
            }
        }

        public override void Update()
        {
            base.Update();
            if (chargeEffect) chargeEffect.transform.rotation = Util.QuaternionSafeLookRotation(GetAimRay().direction);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (!CanCharge)
            {
                outer.SetNextStateToMain();
                return;
            }
            bool released = !IsHeld;
            if (WaitForInitialCharge && !continuing)
            {
                releasedDuringWindup |= released;
                if (fixedAge < nextConsumption) return;
                released = releasedDuringWindup;
            }
            if ((FinishOnRelease && released) || (hadMetaMagic && (released || remainingStacks <= 0)) ||
                (!hadMetaMagic && fixedAge >= nextConsumption))
            {
                outer.SetNextState(FinishCharge());
                return;
            }
            if (hadMetaMagic && fixedAge >= nextConsumption)
                outer.SetNextState(NextStep());
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            if (nextState is BaseMetamagicCharge next && next.GetType() == GetType())
            {
                transferringEffect = true;
                next.continuing = true;
                next.chargeEffect = chargeEffect;
                next.activatorSkillSlot = activatorSkillSlot;
                if (NetworkServer.active || isAuthority)
                {
                    next.consumedStacks = consumedStacks;
                    next.remainingStacks = remainingStacks;
                    next.hadMetaMagic = hadMetaMagic;
                    next.interval = Mathf.Max(0.05f, interval * 0.5f);
                    next.nextConsumption = Mathf.Max(0f, next.interval - Mathf.Max(0f, fixedAge - nextConsumption));
                }
            }
        }

        public override void OnExit()
        {
            if (!transferringEffect) WarlockAssets.StopChargeEffect(chargeEffect);
            base.OnExit();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(continuing);
            writer.Write(consumedStacks);
            writer.Write(remainingStacks);
            writer.Write(hadMetaMagic);
            writer.Write(interval);
            writer.Write(nextConsumption);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            continuing = reader.ReadBoolean();
            consumedStacks = reader.ReadInt32();
            remainingStacks = reader.ReadInt32();
            hadMetaMagic = reader.ReadBoolean();
            interval = reader.ReadSingle();
            nextConsumption = reader.ReadSingle();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
