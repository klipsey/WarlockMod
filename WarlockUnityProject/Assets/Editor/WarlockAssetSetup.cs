using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class WarlockAssetSetup
{
    private const string Root = "Assets/Warlock/";
    private static readonly string[] PhysicsRoots = { "cloak.l", "cloak.r", "cloak.x", "hood.x", "c_feeler_00.l", "c_feeler_00.r" };
    private static readonly string[] SkillIconNames =
    {
        "texWarlockPassive", "texWarlockPrimary", "texWarlockPrimaryEmpowered", "texWarlockSecondary",
        "texWarlockSecondaryEmpowered", "texWarlockSpecial", "texWarlockUtility", "texWarlockUtilityEmpowered"
    };

    [MenuItem("Tools/Warlock/Setup and Build")]
    public static void SetupAndBuild()
    {
        SetupTexturesAndMaterials();
        var model = (GameObject)PrefabUtility.InstantiatePrefab(Require<GameObject>("Assets/FBX/mdlWarlock.fbx"));
        PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        try
        {
            model.name = "mdlWarlock";
            model.transform.localScale = Vector3.one * 1.5f;
            Find(model, "dagger.x").SetParent(Find(model, "hand.l"), true);
            var idle = CreateIdle(model);
            SetupMasks(model);
            SetupControllers(idle);
            var animator = model.GetComponent<Animator>();
            if (!animator) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = Require<AnimatorController>(Root + "Animations/animWarlock.controller");
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                string material = renderer.name == "meshBook" ? "matBook" : renderer.name == "meshDagger" ? "matDagger" : "matWarlock";
                renderer.sharedMaterial = Require<Material>(Root + "Materials/" + material + ".mat");
                renderer.localBounds = new Bounds(renderer.localBounds.center, renderer.localBounds.size * 1.5f);
            }
            SetupChildLocator(model);
            SetupDynamicBones(model);
            PrefabUtility.SaveAsPrefabAsset(model, Root + "mdlWarlock.prefab");
            model.name = "WarlockDisplay";
            animator.runtimeAnimatorController = Require<AnimatorController>(Root + "Animations/animWarlockCSS.controller");
            PrefabUtility.SaveAsPrefabAsset(model, Root + "WarlockDisplay.prefab");
        }
        finally
        {
            Object.DestroyImmediate(model);
        }
        SetupEmoteSkeleton();
        SetupScene();
        RenderPreviews();
        AssetDatabase.SaveAssets();
        Validate();
        BuildBundle();
    }

    private static T Require<T>(string path) where T : Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (!asset) throw new InvalidOperationException("Missing " + typeof(T).Name + ": " + path);
        return asset;
    }

    private static Transform Find(GameObject model, string name)
    {
        var bone = model.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == name);
        if (!bone) throw new InvalidOperationException("Missing bone: " + name);
        return bone;
    }

    private static void SetupTexturesAndMaterials()
    {
        foreach (string path in AssetDatabase.FindAssets("t:Texture2D", new[] { Root + "Textures" }).Select(AssetDatabase.GUIDToAssetPath))
        {
            if (!Path.GetFileName(path).StartsWith("texWarlock") && !Path.GetFileName(path).StartsWith("texDagger")) continue;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            bool normal = path.Contains("Normals");
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normal;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.alphaIsTransparency = false;
            importer.SaveAndReimport();
        }
        SetupMaterial("matWarlock", "texWarlockDiffuse", null, null);
        SetupMaterial("matBook", "texWarlockBookDiffuse", null, "texWarlockBookEmission");
        SetupMaterial("matDagger", "texDaggerDiffuse", "texDaggerNormals", null);
    }

    private static void SetupMaterial(string name, string diffuse, string normal, string emission)
    {
        var material = new Material(Require<Shader>(Root + "Materials/WarlockDoubleSided.shader")) { name = name };
        material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        material.doubleSidedGI = true;
        material.color = Color.white;
        material.mainTexture = Require<Texture2D>(Root + "Textures/" + diffuse + ".png");
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Glossiness", name == "matDagger" ? 0.35f : 0.05f);
        material.SetTexture("_BumpMap", normal == null ? null : Require<Texture2D>(Root + "Textures/" + normal + ".png"));
        material.SetTexture("_EmissionMap", emission == null ? null : Require<Texture2D>(Root + "Textures/" + emission + ".png"));
        material.SetColor("_EmissionColor", emission == null ? Color.black : Color.white);
        material.globalIlluminationFlags = emission == null ? MaterialGlobalIlluminationFlags.EmissiveIsBlack : MaterialGlobalIlluminationFlags.BakedEmissive;
        if (normal != null) material.EnableKeyword("_NORMALMAP");
        if (emission != null) material.EnableKeyword("_EMISSION");
        SaveAsset(material, Root + "Materials/" + name + ".mat");
    }

    [MenuItem("Tools/Warlock/Refresh Materials and Build")]
    public static void RefreshMaterialsAndBuild()
    {
        SetupTexturesAndMaterials();
        AssetDatabase.SaveAssets();
        RenderPreviews();
        BuildBundle();
    }

    private static AnimationClip CreateIdle(GameObject model)
    {
        var source = AssetDatabase.LoadAllAssetsAtPath("Assets/FBX/mdlWarlock_Idle.fbx").OfType<AnimationClip>().Single(c => !c.name.StartsWith("__preview__"));
        var clip = Object.Instantiate(source);
        clip.name = "WarlockIdle";
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (binding.path.StartsWith("root/dagger.x", StringComparison.Ordinal) || !model.transform.Find(binding.path))
                AnimationUtility.SetEditorCurve(clip, binding, null);
        }
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        settings.loopBlend = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        return SaveAsset(clip, Root + "Animations/WarlockIdle.anim");
    }

    private static T SaveAsset<T>(T asset, string path) where T : Object
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing)
        {
            EditorUtility.CopySerialized(asset, existing);
            Object.DestroyImmediate(asset);
            return existing;
        }
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void SetupMasks(GameObject model)
    {
        foreach (string name in new[] { "Aim", "Gesture", "Impact", "LeftArm" })
        {
            var mask = Require<AvatarMask>(Root + "Animations/maskWarlock" + name + ".mask");
            mask.transformCount = 0;
            mask.AddTransformPath(model.transform, true);
            for (int i = 0; i < mask.transformCount; i++)
            {
                string path = mask.GetTransformPath(i);
                bool active = name == "LeftArm" ? path.Contains("/shoulder.l") : path.Contains("/spine_01.x");
                if (PhysicsRoots.Any(b => path.Split('/').Contains(b))) active = false;
                mask.SetTransformActive(i, active);
            }
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, name == "LeftArm"
                    ? i == (int)AvatarMaskBodyPart.LeftArm || i == (int)AvatarMaskBodyPart.LeftFingers
                    : i == (int)AvatarMaskBodyPart.Body || i == (int)AvatarMaskBodyPart.Head || i == (int)AvatarMaskBodyPart.LeftArm || i == (int)AvatarMaskBodyPart.RightArm || i == (int)AvatarMaskBodyPart.LeftFingers || i == (int)AvatarMaskBodyPart.RightFingers);
            EditorUtility.SetDirty(mask);
        }
    }

    private static void SetupControllers(AnimationClip idle)
    {
        var empty = SaveAsset(new AnimationClip { name = "WarlockEmpty" }, Root + "Animations/WarlockEmpty.anim");
        foreach (string name in new[] { "animWarlock", "animWarlockCSS", "animWarlockEmotes" })
        {
            string path = Root + "Animations/" + name + ".controller";
            var controller = Require<AnimatorController>(path);
            controller.layers = Array.Empty<AnimatorControllerLayer>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path).Where(a => a != controller))
                Object.DestroyImmediate(asset, true);
            controller.AddLayer("Body");
            var body = controller.layers[0].stateMachine;
            body.defaultState = body.AddState("Idle");
            body.defaultState.motion = name == "animWarlockEmotes" ? empty : idle;
            if (name == "animWarlock")
            {
                foreach (string stateName in new[] { "Jump", "SprintJump", "BonusJump", "Ascend", "Descend", "Land", "Run", "Sprint", "IdleToRun", "RunToIdle", "BufferEmpty" })
                    body.AddState(stateName).motion = idle;
                foreach (string layerName in new[] { "Gesture, Override", "FullBody, Override", "LeftArm, Override", "AimPitch", "AimYaw", "Impact" })
                {
                    controller.AddLayer(layerName);
                    var layers = controller.layers;
                    var layer = layers[layers.Length - 1];
                    layer.defaultWeight = 1f;
                    string maskName = layerName.StartsWith("Aim") ? "Aim" : layerName.StartsWith("LeftArm") ? "LeftArm" : layerName == "Impact" ? "Impact" : "Gesture";
                    layer.avatarMask = Require<AvatarMask>(Root + "Animations/maskWarlock" + maskName + ".mask");
                    layer.stateMachine.defaultState = layer.stateMachine.AddState("BufferEmpty");
                    layer.stateMachine.defaultState.motion = empty;
                    if (layerName == "Gesture, Override")
                    {
                        foreach (string stateName in new[] { "Point", "SwapToGun", "SwapToBat" })
                        {
                            var state = layer.stateMachine.AddState(stateName);
                            state.motion = empty;
                            var transition = state.AddTransition(layer.stateMachine.defaultState);
                            transition.hasExitTime = true;
                            transition.exitTime = 1f;
                            transition.duration = 0.1f;
                        }
                    }
                    controller.layers = layers;
                }
            }
            EditorUtility.SetDirty(controller);
        }
    }

    private static Transform Child(Transform parent, string name, Vector3 position)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent, false);
        child.localPosition = position;
        return child;
    }

    private static void SetupChildLocator(GameObject model)
    {
        var pairs = new List<ChildLocator.NameTransformPair>();
        Action<string, Transform> add = (name, bone) => pairs.Add(new ChildLocator.NameTransformPair { name = name, transform = bone });
        foreach (var pair in new Dictionary<string, string>
        {
            { "Model", "meshBody" }, { "CloakModel", "meshCloak" }, { "RopeModel", "meshRope" }, { "BookModel", "meshBook" }, { "DaggerModel", "meshDagger" },
            { "Base", "base" }, { "Pelvis", "root.x" }, { "Chest", "spine_03.x" }, { "Stomach", "spine_01.x" }, { "Head", "head.x" },
            { "UpperArmL", "arm_stretch.l" }, { "UpperArmR", "arm_stretch.r" }, { "LowerArmL", "forearm_stretch.l" }, { "LowerArmR", "forearm_stretch.r" },
            { "HandL", "hand.l" }, { "HandR", "hand.r" }, { "ThighL", "thigh_stretch.l" }, { "ThighR", "thigh_stretch.r" },
            { "CalfL", "leg_stretch.l" }, { "CalfR", "leg_stretch.r" }, { "FootL", "foot.l" }, { "FootR", "foot.r" }, { "Gun", "book.x" }
        }) add(pair.Key, Find(model, pair.Value));
        var main = Child(model.transform, "MainHurtbox", new Vector3(0f, 0.61f, 0f));
        var capsule = main.gameObject.AddComponent<CapsuleCollider>();
        capsule.radius = 0.23f;
        capsule.height = 1.2f;
        capsule.isTrigger = true;
        add("MainHurtbox", main);
        var head = Child(Find(model, "head.x"), "HeadHurtbox", new Vector3(0f, 0.17f, 0f));
        var sphere = head.gameObject.AddComponent<SphereCollider>();
        sphere.radius = 0.16f / Mathf.Abs(head.lossyScale.x) * model.transform.lossyScale.x;
        sphere.isTrigger = true;
        add("HeadHurtbox", head);
        var melee = Child(model.transform, "MeleeHitbox", new Vector3(0f, 0.65f, 0.5f));
        melee.localScale = new Vector3(0.8f, 0.8f, 1f);
        add("MeleeHitbox", melee);
        add("Muzzle", Find(model, "book.x"));
        add("MuzzleHandL", Find(model, "hand.l"));
        add("MuzzleHandR", Find(model, "hand.r"));
        model.AddComponent<ChildLocator>().TransformPairs = pairs.ToArray();
    }

    private static void SetupDynamicBones(GameObject model)
    {
        var colliders = new List<DynamicBoneCollider>();
        foreach (var item in new[] { Tuple.Create("spine_02.x", 0.15f), Tuple.Create("head.x", 0.12f) })
        {
            var collider = Find(model, item.Item1).gameObject.AddComponent<DynamicBoneCollider>();
            collider.m_Radius = item.Item2 / Mathf.Abs(collider.transform.lossyScale.x) * model.transform.lossyScale.x;
            colliders.Add(collider);
        }
        foreach (string root in PhysicsRoots)
        {
            var dynamicBone = model.AddComponent<DynamicBone>();
            dynamicBone.m_Root = Find(model, root);
            dynamicBone.m_Damping = root.StartsWith("cloak") ? 0.25f : 0.2f;
            dynamicBone.m_Elasticity = 0.08f;
            dynamicBone.m_Stiffness = root == "hood.x" ? 0.3f : 0.15f;
            dynamicBone.m_Inert = 0.15f;
            dynamicBone.m_Radius = 0.015f;
            dynamicBone.m_Colliders = colliders;
            dynamicBone.m_DistantDisable = true;
            dynamicBone.m_DistanceToObject = 35f;
        }
    }

    private static void SetupEmoteSkeleton()
    {
        const string path = "Assets/FBX/mdlWarlock_aapose.fbx";
        var model = Object.Instantiate(Require<GameObject>(path));
        model.name = "mdlWarlock_aapose";
        try
        {
            var mapping = new Dictionary<string, string>
            {
                { "Hips", "base" }, { "Spine", "spine_01.x" }, { "Chest", "spine_02.x" }, { "UpperChest", "spine_03.x" }, { "Neck", "neck.x" }, { "Head", "head.x" }
            };
            foreach (string side in new[] { "Left", "Right" })
            {
                string suffix = side == "Left" ? ".l" : ".r";
                foreach (var pair in new Dictionary<string, string>
                {
                    { "Shoulder", "shoulder" }, { "UpperArm", "arm_stretch" }, { "LowerArm", "forearm_stretch" }, { "Hand", "hand" },
                    { "UpperLeg", "thigh_stretch" }, { "LowerLeg", "leg_stretch" }, { "Foot", "foot" }, { "Toes", "toes_01" }
                }) mapping.Add(side + pair.Key, pair.Value + suffix);
                foreach (string finger in new[] { "Thumb", "Index", "Middle", "Ring", "Little" })
                {
                    string boneName = finger == "Little" ? "pinky" : finger.ToLowerInvariant();
                    if (!model.GetComponentsInChildren<Transform>().Any(t => t.name == boneName + "1" + suffix)) continue;
                    mapping.Add(side + " " + finger + " Proximal", boneName + "1" + suffix);
                    mapping.Add(side + " " + finger + " Intermediate", boneName + "2" + suffix);
                    mapping.Add(side + " " + finger + " Distal", boneName + "3" + suffix);
                }
            }
            // Use the same pose solver as Configure Avatar > Enforce T-Pose in Unity 2021.
            var tool = typeof(Editor).Assembly.GetType("UnityEditor.AvatarSetupTool", true);
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            if (importer.humanDescription.skeleton.Length > 0)
                tool.GetMethod("TransferDescriptionToPose", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { new SerializedObject(importer), model.transform });
            var wrapper = tool.GetNestedType("BoneWrapper", BindingFlags.NonPublic);
            var bones = Array.CreateInstance(wrapper, HumanTrait.BoneCount);
            for (int i = 0; i < HumanTrait.BoneCount; i++)
            {
                string boneName;
                Transform bone = mapping.TryGetValue(HumanTrait.BoneName[i], out boneName) ? Find(model, boneName) : null;
                bones.SetValue(Activator.CreateInstance(wrapper, new object[] { HumanTrait.BoneName[i], bone }), i);
            }
            var isPoseValid = tool.GetMethod("IsPoseValid", BindingFlags.Public | BindingFlags.Static);
            if (!(bool)isPoseValid.Invoke(null, new object[] { bones }))
                tool.GetMethod("MakePoseValid", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { bones });
            if (!(bool)isPoseValid.Invoke(null, new object[] { bones }))
                throw new InvalidOperationException("Unity could not enforce a valid Warlock T-pose. Error: " + tool.GetMethod("GetPoseError", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { bones }));
            var description = importer.humanDescription;
            description.human = mapping.Select(p => new HumanBone { humanName = p.Key, boneName = p.Value, limit = new HumanLimit { useDefaultValues = true } }).ToArray();
            description.skeleton = (SkeletonBone[])tool.GetMethod("GetSkeletonBones", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { model.transform });
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.humanDescription = description;
            importer.importAnimation = false;
            importer.optimizeGameObjects = false;
            importer.SaveAndReimport();
            var avatar = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().Single();
            if (!avatar.isValid || !avatar.isHuman) throw new InvalidOperationException("Warlock emote avatar is invalid.");
            model.name = "warlock_emoteskeleton";
            model.transform.localScale = Vector3.one * 1.5f;
            var animator = model.GetComponent<Animator>();
            if (!animator) animator = model.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = Require<AnimatorController>(Root + "Animations/animWarlockEmotes.controller");
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                string material = renderer.name == "meshBook" ? "matBook" : renderer.name == "meshDagger" ? "matDagger" : "matWarlock";
                renderer.sharedMaterial = Require<Material>(Root + "Materials/" + material + ".mat");
                renderer.enabled = false;
            }
            PrefabUtility.SaveAsPrefabAsset(model, Root + "warlock_emoteskeleton.prefab");
        }
        finally
        {
            Object.DestroyImmediate(model);
        }
    }

    private static void SetupScene()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Warlock.unity");
        if (!scene.GetRootGameObjects().Any(g => PrefabUtility.GetCorrespondingObjectFromSource(g) == Require<GameObject>(Root + "mdlWarlock.prefab")))
            PrefabUtility.InstantiatePrefab(Require<GameObject>(Root + "mdlWarlock.prefab"), scene);
        if (!scene.GetRootGameObjects().Any(g => g.name == "warlock_emoteskeleton"))
        {
            var skeleton = (GameObject)PrefabUtility.InstantiatePrefab(Require<GameObject>(Root + "warlock_emoteskeleton.prefab"), scene);
            skeleton.transform.position = Vector3.right * 3f;
        }
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Tools/Warlock/Refresh Emote Skeleton and Build")]
    public static void RefreshEmoteSkeletonAndBuild()
    {
        SetupEmoteSkeleton();
        AssetDatabase.SaveAssets();
        BuildBundle();
    }

    [MenuItem("Tools/Warlock/Validate Assets")]
    public static void Validate()
    {
        foreach (string name in SkillIconNames)
            Require<Sprite>(Root + "Icons/Skill/" + name + ".png");
        var model = Require<GameObject>(Root + "mdlWarlock.prefab");
        if (model.GetComponentsInChildren<SkinnedMeshRenderer>().Length != 5) throw new InvalidOperationException("Expected five Warlock renderers.");
        if (model.GetComponents<DynamicBone>().Length != PhysicsRoots.Length) throw new InvalidOperationException("Missing secondary-motion chains.");
        foreach (var dynamicBone in model.GetComponents<DynamicBone>())
            if (!dynamicBone.m_Root || dynamicBone.m_Root.childCount == 0) throw new InvalidOperationException("Invalid dynamic bone root.");
        foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            if (!renderer.sharedMesh || !renderer.sharedMaterial || !renderer.sharedMaterial.mainTexture || renderer.bones.Any(b => !b))
                throw new InvalidOperationException("Incomplete renderer: " + renderer.name);
        var bookMaterial = Require<Material>(Root + "Materials/matBook.mat");
        if (!bookMaterial.IsKeywordEnabled("_EMISSION") || !bookMaterial.GetTexture("_EmissionMap"))
            throw new InvalidOperationException("Book emission is not enabled.");
        var daggerMaterial = Require<Material>(Root + "Materials/matDagger.mat");
        if (!daggerMaterial.IsKeywordEnabled("_NORMALMAP") || !daggerMaterial.GetTexture("_BumpMap"))
            throw new InvalidOperationException("Dagger normal map is not enabled.");
        ValidateDoubleSidedMaterials();
        var idle = Require<AnimationClip>(Root + "Animations/WarlockIdle.anim");
        foreach (string name in new[] { "Aim", "Gesture", "Impact", "LeftArm" })
        {
            var mask = Require<AvatarMask>(Root + "Animations/maskWarlock" + name + ".mask");
            for (int i = 0; i < mask.transformCount; i++)
            {
                string path = mask.GetTransformPath(i);
                if (path.Length > 0 && !model.transform.Find(path)) throw new InvalidOperationException("Unbound mask path: " + path);
                if (mask.GetTransformActive(i) && PhysicsRoots.Any(b => path.Split('/').Contains(b)))
                    throw new InvalidOperationException("Gesture mask includes dynamic chain: " + path);
            }
        }
        var instance = Object.Instantiate(model);
        try
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(idle))
                if (binding.path.Length > 0 && !instance.transform.Find(binding.path)) throw new InvalidOperationException("Unbound idle curve: " + binding.path);
            var head = Find(instance, "head.x");
            idle.SampleAnimation(instance, 0f);
            Vector3 first = head.position;
            idle.SampleAnimation(instance, idle.length * 0.5f);
            if ((first - head.position).sqrMagnitude < 0.000001f) throw new InvalidOperationException("Idle does not animate the Warlock model.");
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
        var emote = Require<GameObject>(Root + "warlock_emoteskeleton.prefab").GetComponent<Animator>();
        if (!emote.avatar || !emote.avatar.isHuman || !emote.avatar.isValid) throw new InvalidOperationException("Invalid emote avatar.");
        if (emote.cullingMode != AnimatorCullingMode.AlwaysAnimate)
            throw new InvalidOperationException("The hidden emote skeleton must animate even when its renderers are invisible.");
        ValidatePhysics(model);
        Debug.Log("Warlock validation passed: five textured renderers, six dynamic chains, bound animated idle, valid humanoid avatar.");
    }

    private static void ValidateDoubleSidedMaterials()
    {
        var shader = Require<Shader>(Root + "Materials/WarlockDoubleSided.shader");
        if (ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Warlock double-sided shader failed to compile.");
        foreach (string prefabName in new[] { "mdlWarlock", "WarlockDisplay", "warlock_emoteskeleton" })
        {
            foreach (var renderer in Require<GameObject>(Root + prefabName + ".prefab").GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                    if (!material || material.shader != shader || material.GetInt("_Cull") != 0)
                        throw new InvalidOperationException("Backface culling is enabled on " + prefabName + "/" + renderer.name);
            }
        }
        var scene = EditorSceneManager.NewPreviewScene();
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(quad, scene);
        var camera = new GameObject("BackfaceValidationCamera").AddComponent<Camera>();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
        camera.scene = scene;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.orthographicSize = 1f;
        var target = RenderTexture.GetTemporary(16, 16, 24);
        var image = new Texture2D(16, 16, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            foreach (string name in new[] { "matWarlock", "matBook", "matDagger" })
            {
                var material = Object.Instantiate(Require<Material>(Root + "Materials/" + name + ".mat"));
                try
                {
                    material.SetTexture("_EmissionMap", Texture2D.whiteTexture);
                    material.SetColor("_EmissionColor", Color.white);
                    material.EnableKeyword("_EMISSION");
                    quad.GetComponent<Renderer>().sharedMaterial = material;
                    foreach (float side in new[] { -1f, 1f })
                    {
                        camera.transform.position = new Vector3(0f, 0f, side * 2f);
                        camera.transform.LookAt(Vector3.zero);
                        camera.Render();
                        RenderTexture.active = target;
                        image.ReadPixels(new Rect(0, 0, 16, 16), 0, 0);
                        image.Apply();
                        if (image.GetPixel(8, 8).grayscale < 0.5f)
                            throw new InvalidOperationException(name + " failed front/back rendering on side " + side);
                    }
                }
                finally
                {
                    Object.DestroyImmediate(material);
                }
            }
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Object.DestroyImmediate(image);
            EditorSceneManager.ClosePreviewScene(scene);
        }
        Debug.Log("Warlock materials render from both sides; all model, display and emote materials have culling disabled.");
    }

    private static void ValidatePhysics(GameObject prefab)
    {
        var model = Object.Instantiate(prefab);
        try
        {
            var chains = model.GetComponents<DynamicBone>();
            var setup = typeof(DynamicBone).GetMethod("SetupParticles", BindingFlags.Instance | BindingFlags.NonPublic);
            var prepare = typeof(DynamicBone).GetMethod("PreUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            var simulate = typeof(DynamicBone).GetMethod("UpdateDynamicBones", BindingFlags.Instance | BindingFlags.NonPublic);
            var apply = typeof(DynamicBone).GetMethod("ApplyParticlesToTransforms", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var chain in chains) setup.Invoke(chain, null);
            var tips = chains.Select(c => c.m_Root.GetComponentsInChildren<Transform>().Last()).ToArray();
            var starts = tips.Select(t => t.position).ToArray();
            for (int frame = 0; frame < 90; frame++)
            {
                foreach (var chain in chains) prepare.Invoke(chain, null);
                model.transform.position = new Vector3(Mathf.Sin(frame * 0.08f) * 0.2f, 0f, 0f);
                foreach (var chain in chains)
                {
                    simulate.Invoke(chain, new object[] { 1f / 60f });
                    apply.Invoke(chain, null);
                }
                for (int i = 0; i < tips.Length; i++)
                {
                    float displacement = (tips[i].position - starts[i]).magnitude;
                    if (float.IsNaN(displacement) || float.IsInfinity(displacement) || displacement > 3f)
                        throw new InvalidOperationException("Unstable dynamic chain: " + chains[i].m_Root.name);
                }
            }
            for (int i = 0; i < tips.Length; i++)
                if ((tips[i].position - model.transform.position - starts[i]).sqrMagnitude < 0.000001f)
                    throw new InvalidOperationException("Unresponsive dynamic chain: " + chains[i].m_Root.name);
        }
        finally
        {
            Object.DestroyImmediate(model);
        }
    }

    [MenuItem("Tools/Warlock/Render Previews")]
    public static void RenderPreviews()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        var model = Object.Instantiate(Require<GameObject>(Root + "mdlWarlock.prefab"));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(model, scene);
        var camera = new GameObject("PreviewCamera").AddComponent<Camera>();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
        camera.scene = scene;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.06f, 0.09f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 1.35f;
        camera.transform.position = new Vector3(2f, 1.5f, 4f);
        camera.transform.LookAt(new Vector3(0f, 0.9f, 0f));
        var light = new GameObject("PreviewLight").AddComponent<Light>();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(light.gameObject, scene);
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        light.transform.rotation = Quaternion.Euler(30f, -30f, 0f);
        var fill = new GameObject("PreviewFill").AddComponent<Light>();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(fill.gameObject, scene);
        fill.type = LightType.Directional;
        fill.intensity = 0.7f;
        fill.transform.rotation = Quaternion.Euler(340f, 160f, 0f);
        try
        {
            var idle = Require<AnimationClip>(Root + "Animations/WarlockIdle.anim");
            idle.SampleAnimation(model, 0f);
            RenderImage(camera, "Logs/WarlockPreview.png", 768);
            camera.transform.position = new Vector3(-2f, 1.5f, -4f);
            camera.transform.LookAt(new Vector3(0f, 0.9f, 0f));
            RenderImage(camera, "Logs/WarlockBackPreview.png", 768);
            camera.transform.position = new Vector3(0.7f, 1.65f, 4f);
            camera.transform.LookAt(new Vector3(0f, 1.5f, 0f));
            camera.orthographicSize = 0.57f;
            string portrait = Root + "Icons/texWarlockIcon.png";
            RenderImage(camera, portrait, 256);
            AssetDatabase.ImportAsset(portrait);
            var importer = (TextureImporter)AssetImporter.GetAtPath(portrait);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.SaveAndReimport();
            Object.DestroyImmediate(model);
            model = Object.Instantiate(Require<GameObject>(Root + "warlock_emoteskeleton.prefab"));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(model, scene);
            foreach (var renderer in model.GetComponentsInChildren<Renderer>()) renderer.enabled = true;
            camera.orthographicSize = 1.35f;
            camera.transform.position = new Vector3(0f, 1.3f, 4f);
            camera.transform.LookAt(new Vector3(0f, 0.9f, 0f));
            RenderImage(camera, "Logs/WarlockTPosePreview.png", 768);
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static void RenderImage(Camera camera, string path, int size)
    {
        var target = RenderTexture.GetTemporary(size, size, 24);
        var image = new Texture2D(size, size, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            image.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Object.DestroyImmediate(image);
        }
    }

    [MenuItem("Tools/Warlock/Build Bundle")]
    public static void BuildBundle()
    {
        Validate();
        string output = "AssetBundles/StandaloneWindows";
        Directory.CreateDirectory(output);
        var assets = new[]
        {
            "mdlWarlock.prefab", "WarlockDisplay.prefab", "warlock_emoteskeleton.prefab",
            "Icons/texWarlockIcon.png", "Textures/texMetaMagicStackingBuff.png", "Textures/texMetaMagicBuff.png", "Textures/texEmpoweredMetaMagicBuff.png",
            "VFX/Grab.png"
        }.Concat(SkillIconNames.Select(name => "Icons/Skill/" + name + ".png")).Select(p => Root + p).ToArray();
        foreach (string asset in assets) Require<Object>(asset);
        foreach (string dependency in AssetDatabase.GetDependencies(assets, true))
        {
            if (!dependency.EndsWith(".prefab", StringComparison.Ordinal)) continue;
            var prefab = Require<GameObject>(dependency);
            foreach (var transform in prefab.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                    throw new InvalidOperationException("Missing script in bundle dependency: " + dependency);
        }
        var manifest = BuildPipeline.BuildAssetBundles(output, new[] { new AssetBundleBuild { assetBundleName = "warlock", assetNames = assets } },
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows64);
        if (!manifest) throw new InvalidOperationException("Warlock asset bundle build failed.");
        Debug.Log("Warlock bundle built: " + Path.GetFullPath(Path.Combine(output, "warlock")));
    }

    public static void Audit()
    {
        var report = new StringBuilder();
        foreach (string path in new[] { "Assets/FBX/mdlWarlock.fbx", "Assets/FBX/mdlWarlock_Idle.fbx", "Assets/FBX/mdlWarlock_aapose.fbx" })
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!model) throw new InvalidOperationException("Missing model: " + path);
            report.AppendLine(path);
            foreach (var bone in model.GetComponentsInChildren<Transform>(true))
                report.AppendLine(AnimationUtility.CalculateTransformPath(bone, model.transform) + " pos=" + bone.position.ToString("F4") + " local=" + bone.localEulerAngles.ToString("F2"));
            foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                report.AppendLine("MESH " + renderer.name + " bounds=" + renderer.bounds + " materials=" + string.Join(",", renderer.sharedMaterials.Select(m => m ? m.name : "MISSING")));
            foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")))
                report.AppendLine("CLIP " + clip.name + " duration=" + clip.length + " curves=" + AnimationUtility.GetCurveBindings(clip).Length);
        }
        foreach (string path in AssetDatabase.FindAssets("t:AnimatorController", new[] { Root.TrimEnd('/') }).Select(AssetDatabase.GUIDToAssetPath))
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            report.AppendLine("CONTROLLER " + path);
            foreach (var parameter in controller.parameters)
                report.AppendLine("PARAM " + parameter.name + " " + parameter.type);
            foreach (var layer in controller.layers)
                report.AppendLine("LAYER " + layer.name + " weight=" + layer.defaultWeight + " mask=" + (layer.avatarMask ? layer.avatarMask.name : "none"));
        }
        File.WriteAllText(Path.Combine(Application.dataPath, "../Logs/WarlockAssetAudit.txt"), report.ToString());
        Debug.Log("Warlock asset audit completed.");
    }
}
