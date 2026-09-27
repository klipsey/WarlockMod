using EntityStates;
using RoR2;
using RoR2.Skills;
using WarlockMod.Warlock.Content;
using WarlockMod.Warlock.SkillStates;

namespace WarlockMod.Warlock.Components
{
    public class CrimsonManaSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public CharacterBody body;
            public WarlockTracker tracker;
        }

        public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot) => new InstanceData
        {
            body = skillSlot.GetComponent<CharacterBody>(),
            tracker = skillSlot.GetComponent<WarlockTracker>()
        };

        protected virtual bool HasTarget(GenericSkill skillSlot) => true;

        public override bool IsReady(GenericSkill skillSlot)
        {
            var data = (InstanceData)skillSlot.skillInstanceData;
            return base.IsReady(skillSlot) && HasTarget(skillSlot) &&
                (skillSlot.stock >= stockToConsume ||
                 (skillSlot.stock == 0 && data.body && data.body.HasBuff(WarlockBuffs.warlockCrimsonManaFullStack)));
        }

        public override void OnExecute(GenericSkill skillSlot)
        {
            if (!CanExecute(skillSlot)) return;
            if (skillSlot.stock > 0)
            {
                base.OnExecute(skillSlot);
                return;
            }
            var controller = skillSlot.GetComponent<WarlockController>();
            var data = (InstanceData)skillSlot.skillInstanceData;
            bool secondary = skillSlot == data.body.skillLocator.secondary;
            skillSlot.stateMachine.SetInterruptState(new CrimsonManaRefill
            {
                activatorSkillSlot = skillSlot,
                requestId = controller.NextRefillRequestId(),
                secondary = secondary,
                target = secondary ? data.tracker.GetTrackingTarget() : null
            }, interruptPriority);
        }
    }
}
