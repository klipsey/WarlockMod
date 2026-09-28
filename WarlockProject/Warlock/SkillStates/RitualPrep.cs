using EntityStates;
using RoR2;
using WarlockMod.Modules.BaseStates;

namespace WarlockMod.Warlock.SkillStates
{
    public class RitualPrep : BaseWarlockSkillState
    {
        private bool releasedOpeningPress;
        private bool converting;

        public override void OnEnter()
        {
            base.OnEnter();
            warlockController.OpenRitualMenu();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (!inputBank.skill4.down)
                releasedOpeningPress = true;
            else if (releasedOpeningPress)
                outer.SetNextState(new Empower { activatorSkillSlot = skillLocator.special });
        }

        public override void ModifyNextState(EntityState nextState)
        {
            base.ModifyNextState(nextState);
            converting = nextState is Empower;
        }

        public override void OnExit()
        {
            if (!converting) warlockController.CloseRitualMenu();
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
