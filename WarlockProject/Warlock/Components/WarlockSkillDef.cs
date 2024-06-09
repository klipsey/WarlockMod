using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.Components
{
    public class WarlockSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public WarlockTracker tracker;
            public CharacterBody characterBody;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                tracker = skillSlot.GetComponent<WarlockTracker>(),
                characterBody = skillSlot.gameObject.GetComponent<CharacterBody>()
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            if (!(((InstanceData)skillSlot.skillInstanceData).characterBody.HasBuff(WarlockBuffs.warlockBloodMagicFullStack)))
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


