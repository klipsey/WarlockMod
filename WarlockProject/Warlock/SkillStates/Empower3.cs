using EntityStates;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class Empower3 : BaseWarlockSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            warlockController.CloseRitualMenu();
            warlockController.PlaySound();
            if (warlockController.TryConsumeCrimsonMana())
                warlockController.ApplyUtilityEmpowerment();
            if (isAuthority) skillLocator.utility.stock = skillLocator.utility.maxStock;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= 0.1f) outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
