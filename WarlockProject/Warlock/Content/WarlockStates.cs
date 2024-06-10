using WarlockMod.Modules.BaseStates;
using WarlockMod.Warlock.SkillStates;
using WarlockMod.Warlock.SkillStates;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(BaseWarlockSkillState));
            Modules.Content.AddEntityState(typeof(MainState));
            Modules.Content.AddEntityState(typeof(BaseWarlockState));
            Modules.Content.AddEntityState(typeof(BloodDash));
            Modules.Content.AddEntityState(typeof(CrimsonSurgeFire));
            Modules.Content.AddEntityState(typeof(CrimsonSurgePrep));
            Modules.Content.AddEntityState(typeof(Hex));
            Modules.Content.AddEntityState(typeof(RitualPrep));

            Modules.Content.AddEntityState(typeof(Empower));

            Modules.Content.AddEntityState(typeof(Empower1));

            Modules.Content.AddEntityState(typeof(Empower2));

            Modules.Content.AddEntityState(typeof(Empower3));
        }
    }
}
