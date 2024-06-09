using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace WarlockMod.Warlock.Components
{
    public class WarlockPassive : MonoBehaviour
    {
        public SkillDef interrogatorPassive;

        public GenericSkill passiveSkillSlot;

        public bool isJump
        {
            get
            {
                if (interrogatorPassive && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == interrogatorPassive;
                }

                return false;
            }
        }
    }
}