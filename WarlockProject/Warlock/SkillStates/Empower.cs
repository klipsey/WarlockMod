using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class Empower : BaseWarlockSkillState
    {
        private bool repeating;
        private bool continuing;
        private int remainingMana;
        private float nextConversion = 0.5f;

        internal bool IsCreatingMetaMagic => remainingMana > 0 && (!isAuthority || inputBank.skill4.down);

        public override void OnEnter()
        {
            base.OnEnter();
            if (!repeating)
            {
                warlockController.OpenRitualMenu();
                if (NetworkServer.active || isAuthority)
                    remainingMana = characterBody.GetBuffCount(WarlockBuffs.warlockCrimsonManaFullStack);
            }
            if (NetworkServer.active)
            {
                if (warlockController.TryConsumeCrimsonMana(false))
                {
                    characterBody.AddBuff(WarlockBuffs.warlockMetaMagicBuff);
                    Modules.SoundBanks.PlayConsume(characterBody.corePosition);
                }
                remainingMana = characterBody.GetBuffCount(WarlockBuffs.warlockCrimsonManaFullStack);
            }
            else if (isAuthority)
                remainingMana = Mathf.Max(0, remainingMana - 1);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (!inputBank.skill4.down)
                outer.SetNextStateToMain();
            else if (remainingMana > 0 && fixedAge >= nextConversion)
                outer.SetNextState(new Empower());
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            if (nextState is Empower next)
            {
                continuing = true;
                next.repeating = true;
                next.activatorSkillSlot = activatorSkillSlot;
                if (NetworkServer.active || isAuthority)
                {
                    next.remainingMana = remainingMana;
                    next.nextConversion = Mathf.Max(0f, 0.1f - Mathf.Max(0f, fixedAge - nextConversion));
                }
            }
        }

        public override void OnExit()
        {
            if (!continuing) warlockController.CloseRitualMenu();
            base.OnExit();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(repeating);
            writer.Write(remainingMana);
            writer.Write(nextConversion);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            repeating = reader.ReadBoolean();
            remainingMana = reader.ReadInt32();
            nextConversion = reader.ReadSingle();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
