using EntityStates;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class CrimsonSurgePrep : BaseMetamagicCharge
    {
        private bool empoweredAtStart;
        protected override bool IsHeld => inputBank && inputBank.skill1.down;
        protected override GameObject ChargeEffectPrefab => WarlockAssets.spawnPrefab;
        protected override float ChargeSpeed => attackSpeedStat / (primaryEmpowered ? 0.85f : 1f);

        protected override void BeginCharge()
        {
            empoweredAtStart = primaryEmpowered;
            StartAimMode(0.5f);
            Util.PlayAttackSpeedSound("Play_imp_overlord_attack2_tell", gameObject, attackSpeedStat);
        }

        protected override BaseMetamagicCharge NextStep() => new CrimsonSurgePrep();

        protected override EntityState FinishCharge() => new CrimsonSurgeFire();

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            if (nextState is CrimsonSurgePrep next)
                next.empoweredAtStart = empoweredAtStart;
            if (nextState is CrimsonSurgeFire fire && (NetworkServer.active || isAuthority))
            {
                fire.maxShots = consumedStacks + 1;
                fire.empoweredShot = empoweredAtStart;
                if (isAuthority) fire.initialAimRay = GetAimRay();
                fire.activatorSkillSlot = activatorSkillSlot;
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(empoweredAtStart);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            empoweredAtStart = reader.ReadBoolean();
        }
    }
}
