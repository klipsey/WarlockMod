using EntityStates;
using RoR2;
using WarlockMod.Warlock.Content;
using WarlockMod.Warlock.SkillStates;

namespace WarlockMod.Warlock.Components
{
    public class BloodDashSkillDef : CrimsonManaSkillDef
    {
        public override EntityState InstantiateNextState(GenericSkill skillSlot)
        {
            if (skillSlot.characterBody.HasBuff(WarlockBuffs.warlockMetaMagicBuff))
                return new BloodDashPrep { activatorSkillSlot = skillSlot };
            return base.InstantiateNextState(skillSlot);
        }
    }
}
