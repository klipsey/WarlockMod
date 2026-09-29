using EntityStates;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Components;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class CrimsonManaRefill : BaseWarlockSkillState
    {
        internal uint requestId;
        internal bool secondary;
        internal HurtBox target;
        private bool responseReceived;
        private bool accepted;
        private bool reservedMana;
        private bool castCommitted;
        private WarlockTracker tracker;
        private RoR2.Skills.SkillDef originalSkill;
        private GenericSkill Slot => secondary ? skillLocator.secondary : skillLocator.utility;

        private bool TargetValid => !secondary || Hex.IsTargetValid(target, characterBody, tracker);

        public override void OnEnter()
        {
            base.OnEnter();
            tracker = GetComponent<WarlockTracker>();
            originalSkill = Slot.skillDef;
            if (secondary) tracker.SetChargeTarget(target);
            if (NetworkServer.active)
            {
                reservedMana = characterBody.healthComponent && characterBody.healthComponent.alive &&
                    TargetValid && originalSkill is CrimsonManaSkillDef && warlockController.TryConsumeCrimsonMana(false);
                ReceiveResult(reservedMana);
                if (!isAuthority)
                    new SyncCrimsonManaRefill(GetComponent<NetworkIdentity>().netId, requestId, reservedMana).Send(NetworkDestination.Clients);
            }
        }

        internal void ReceiveResult(bool success)
        {
            if (responseReceived) return;
            responseReceived = true;
            accepted = success;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (!responseReceived)
            {
                if (fixedAge >= 3f)
                {
                    Log.Warning("Crimson Mana refill timed out waiting for the server.");
                    outer.SetNextStateToMain();
                }
                return;
            }
            if (!accepted || !characterBody.healthComponent.alive || !TargetValid || Slot.skillDef != originalSkill)
            {
                outer.SetNextStateToMain();
                return;
            }
            if (outer.HasPendingState()) return;
            int previousStock = Slot.stock;
            Slot.stock = Slot.maxStock;
            if (!Slot.ExecuteIfReady())
            {
                Slot.stock = previousStock;
                Log.Warning("Crimson Mana refill could not execute the requested skill.");
                outer.SetNextStateToMain();
            }
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            castCommitted = accepted && (secondary ? nextState is Hex : nextState is BloodDash || nextState is BloodDashPrep);
            if (!castCommitted) return;
            if (NetworkServer.active)
            {
                reservedMana = false;
                Modules.SoundBanks.PlayUseMana(characterBody.corePosition);
                if (secondary)
                    warlockController.ApplySecondaryEmpowerment(Mathf.Max(1, Slot.maxStock / 2));
                else
                    warlockController.ApplyUtilityEmpowerment();
                if (!isAuthority) Slot.stock = Mathf.Max(0, Slot.maxStock - 1);
            }
        }

        public override void OnExit()
        {
            if (NetworkServer.active && reservedMana)
                characterBody.AddBuff(WarlockBuffs.warlockCrimsonManaFullStack);
            if (secondary && !castCommitted && tracker) tracker.ClearChargeTarget();
            base.OnExit();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(requestId);
            writer.Write(secondary);
            writer.Write(HurtBoxReference.FromHurtBox(target));
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            requestId = reader.ReadUInt32();
            secondary = reader.ReadBoolean();
            target = reader.ReadHurtBoxReference().ResolveHurtBox();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }
}
