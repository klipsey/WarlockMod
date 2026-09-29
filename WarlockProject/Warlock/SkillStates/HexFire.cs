using EntityStates;
using RoR2;
using UnityEngine.Networking;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Components;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class HexFire : BaseWarlockSkillState
    {
        internal HurtBox target;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active && characterBody.healthComponent && characterBody.healthComponent.alive &&
                Hex.IsTargetValid(target, characterBody, GetComponent<WarlockTracker>()))
            {
                bool empowered = warlockController.TryConsumeEmpowerment(WarlockBuffs.warlockEmpoweredM2Buff);
                Hex.ApplyHex(target, empowered, false);
            }
            if (isAuthority) outer.SetNextStateToMain();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(HurtBoxReference.FromHurtBox(target));
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            target = reader.ReadHurtBoxReference().ResolveHurtBox();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
