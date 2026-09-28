using WarlockMod.Modules;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockConfig
    {
        public static int KillsPerCrimsonMana { get; private set; }
        public static float CrimsonSurgeDamage { get; private set; }
        public static float CrimsonSurgeProc { get; private set; }
        public static float PrimaryEmpowerDuration { get; private set; }
        public static float BloodDashDamage { get; private set; }
        public static float BloodDashProc { get; private set; }
        public static float UtilityEmpowerDuration { get; private set; }
        public static float HexDamage { get; private set; }
        public static float HexProcMultiplier { get; private set; }
        public static float HexDuration { get; private set; }
        public static float EmpoweredHexDamage { get; private set; }
        public static float EmpoweredHexProc { get; private set; }
        public static float BleedDamageMultiplier { get; private set; }
        public static float BleedProc { get; private set; }
        public static float BleedDuration { get; private set; }

        public static void Init()
        {
            KillsPerCrimsonMana = Stack("01 - Crimson Mana", "Kills per Crimson Mana", 2, "Number of kills required to gain one stack of Crimson Mana.");
            CrimsonSurgeDamage = Damage("02 - Crimson Surge", "Damage coefficient", 4f, "Damage per shot, as a multiple of base damage.");
            CrimsonSurgeProc = Proc("02 - Crimson Surge", "Proc coefficient", 1f, "Proc coefficient per shot.");
            PrimaryEmpowerDuration = Duration("02 - Crimson Surge", "Empower duration", 7f, "Duration in seconds of Crimson Mana's primary fire-rate empowerment.");
            BloodDashDamage = Damage("03 - Blood Dash", "Damage coefficient", 2f, "Damage per blast, including the empowered exit blast, as a multiple of base damage.");
            BloodDashProc = Proc("03 - Blood Dash", "Proc coefficient", 1f, "Proc coefficient of each dash blast.");
            UtilityEmpowerDuration = Duration("03 - Blood Dash", "Empower duration", 7f, "Duration in seconds of Crimson Mana's empowerment that adds an arrival blast.");
            HexDamage = Damage("04 - Hex", "Damage coefficient", 0.5f, "Bonus damage per Hex stack, as a multiple of the triggering hit's damage.");
            HexProcMultiplier = Proc("04 - Hex", "Proc coefficient multiplier", 1f, "Multiplies the triggering hit's proc coefficient for Hex bonus damage. Does not add new on-hit item callbacks.");
            HexDuration = Duration("04 - Hex", "Duration", 7f, "Duration in seconds of normal Hex, empowered Hex and its metamagic bleed mark.");
            EmpoweredHexDamage = Damage("05 - Empowered Hex", "Damage coefficient", 0.5f, "Delayed blast damage per empowered Hex stack, as a multiple of the triggering hit's damage.");
            EmpoweredHexProc = Proc("05 - Empowered Hex", "Proc coefficient", 1f, "Proc coefficient of the delayed area blast, independent of the triggering hit.");
            BleedDamageMultiplier = Damage("06 - Metamagic Bleed", "Damage multiplier", 0.2f,
                "Multiplies vanilla bleed damage and the Hex hit's proc coefficient per metamagic stack. Vanilla bleed ticks for 0.2 base damage every 0.25 seconds; duration is configured separately.");
            BleedProc = Proc("06 - Metamagic Bleed", "Proc coefficient", 0f,
                "Zero preserves vanilla non-proccing bleed. Positive values enable on-hit callbacks on a separate Warlock dot. Its proc chains cannot create more metamagic bleed; other bleeds are unchanged.");
            BleedDuration = Duration("06 - Metamagic Bleed", "Duration", 2f, "Duration in seconds of each metamagic bleed stack, with or without on-hit procs.");
        }

        private static int Stack(string section, string name, int value, string description) =>
            Config.BindInt(section, name, value, 1, 100, description).Value;

        private static float Damage(string section, string name, float value, string description) =>
            Config.BindFloat(section, name, value, 100f, description).Value;

        private static float Proc(string section, string name, float value, string description) =>
            Config.BindFloat(section, name, value, 10f, description).Value;

        private static float Duration(string section, string name, float value, string description) =>
            Config.BindFloat(section, name, value, 300f, description).Value;
    }
}
