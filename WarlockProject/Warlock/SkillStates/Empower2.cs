using EntityStates;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class Empower2 : BaseWarlockSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            warlockController.CloseRitualMenu();
            if (warlockController.TryConsumeCrimsonMana())
                warlockController.ApplySecondaryEmpowerment(skillLocator.secondary.maxStock / 2);
            if (isAuthority) skillLocator.secondary.stock = skillLocator.secondary.maxStock;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= 0.1f) outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
