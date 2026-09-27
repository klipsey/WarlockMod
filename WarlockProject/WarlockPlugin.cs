using BepInEx;
using R2API.Utils;
using WarlockMod.Modules;
using WarlockMod.Warlock.Content;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using R2API.Networking;
using ShaderSwapper;
using R2API;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//rename this namespace
namespace WarlockMod
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [BepInDependency(NetworkingAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(DamageAPI.PluginGUID)]
    [BepInDependency(DotAPI.PluginGUID, DotAPI.PluginVersion)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(SoundAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    public class WarlockPlugin : BaseUnityPlugin
    {
        // if you do not change this, you are giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.kenko.Warlock";
        public const string MODNAME = "Warlock";
        public const string MODVERSION = "1.0.0";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "KENKO";

        public static WarlockPlugin instance;
        private ContentPacks contentPacks;

        public static bool emotesInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI");
        public static bool riskOfOptionsInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");

        public static bool scepterInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter");
        void Awake()
        {
            instance = this;

            NetworkingAPI.RegisterMessageType<Warlock.Components.SyncBloodExplosion>();
            NetworkingAPI.RegisterMessageType<Warlock.Components.SyncOrbWarlock>();
            NetworkingAPI.RegisterMessageType<Warlock.Components.SyncCrimsonManaRefill>();

            //easy to use logger
            Log.Init(Logger);
            SoundBanks.Init();
            WarlockConfig.Init();

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            WarlockAssets.Init(Modules.Assets.LoadAssetBundle("warlock"));
            StartCoroutine(WarlockAssets.mainAssetBundle.UpgradeStubbedShadersAsync());

            new WarlockMod.Warlock.WarlockSurvivor().Initialize();

            // make a content pack and add it. this has to be last
            contentPacks = new ContentPacks();
            contentPacks.Initialize();

            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
        }

        private void OnDestroy()
        {
            Warlock.WarlockSurvivor.instance?.RemoveHooks();
            contentPacks?.Shutdown();
            DamageTypes.Unhook();
            Dots.Unhook();
            if (emotesInstalled) Warlock.Compatibility.EmotesCompat.Shutdown();
        }
    }
}
