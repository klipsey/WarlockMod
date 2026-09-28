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
            if (modelAnimator && modelAnimator.isActiveAndEnabled)
            {
                modelAnimator.SetFloat("Mana.playbackRate", attackSpeedStat);
                PlayCrossfade("Gesture, Override", "UseMana", 0.05f);
            }
            warlockController.CloseRitualMenu();
            if (warlockController.TryConsumeCrimsonMana())
                warlockController.ApplyPrimaryEmpowerment();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= 0.1f) outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
