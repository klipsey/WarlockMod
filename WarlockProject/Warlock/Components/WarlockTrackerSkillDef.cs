using RoR2;

namespace WarlockMod.Warlock.Components
{
    public class WarlockTrackerSkillDef : CrimsonManaSkillDef
    {
        protected override bool HasTarget(GenericSkill skillSlot)
        {
            var tracker = ((InstanceData)skillSlot.skillInstanceData).tracker;
            var target = tracker ? tracker.GetTrackingTarget() : null;
            return target && target.healthComponent && target.healthComponent.alive;
        }
    }
}
