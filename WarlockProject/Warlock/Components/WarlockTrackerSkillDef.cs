using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;

namespace WarlockMod.Warlock.Components
{
    public class WarlockTrackerSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public WarlockTracker tracker;
            public SkillLocator skillLocator;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                tracker = skillSlot.GetComponent<WarlockTracker>(),
                skillLocator = skillSlot.gameObject.GetComponent<SkillLocator>()
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            if (!(((InstanceData)skillSlot.skillInstanceData).tracker?.GetTrackingTarget() || (((InstanceData)skillSlot.skillInstanceData).skillLocator.secondary.stock <= 0)))
            {
                return false;
            }
            return true;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            if (!HasTarget(skillSlot))
            {
                return false;
            }
            return base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            if (base.IsReady(skillSlot))
            {
                return HasTarget(skillSlot);
            }
            return false;
        }
    }
}


