using EntityStates;
using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.SkillStates
{
    public class Empower1 : BaseWarlockSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            warlockController.CloseRitualMenu();
            warlockController.PlaySound();
            if (warlockController.TryConsumeCrimsonMana())
                characterBody.AddTimedBuff(WarlockBuffs.warlockEmpoweredM1Buff, WarlockConfig.PrimaryEmpowerDuration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= 0.1f) outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
