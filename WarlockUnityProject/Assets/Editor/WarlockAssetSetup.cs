using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RoR2;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;

public static class WarlockAssetSetup
{
    private const string Root = "Assets/Warlock/";
    private static readonly string[] PhysicsRoots = { "cloak.l", "cloak.r", "cloak.x", "hood.x", "c_feeler_00.l", "c_feeler_00.r", "scarf.l", "scarf.r" };
    private const string BookRoot = "root/base/spine_01.x/spine_02.x/spine_03.x/shoulder.r/arm_stretch.r/forearm_stretch.r/hand.r/c_bookroot.x";
    private static readonly Dictionary<string, (string hand, string grip)> PropHands = new Dictionary<string, (string hand, string grip)>
    {
        { "c_bookroot.x", ("hand.r", "BookGrip") },
        { "dagger.x", ("hand.l", "DaggerGrip") }
    };
    private static readonly Dictionary<string, string> RagdollProps = new Dictionary<string, string>
    {
        { "c_bookroot.x", "meshBook" },
        { "dagger.x", "meshDagger" }
    };
    private static readonly (string bone, string parent, string tip, float radius)[] RagdollBones =
    {
        ("root.x", null, "spine_01.x", 0.09f),
        ("spine_01.x", "root.x", "neck.x", 0.11f),
        ("head.x", "spine_01.x", null, 0.14f),
        ("arm_stretch.l", "spine_01.x", "forearm_stretch.l", 0.045f),
        ("forearm_stretch.l", "arm_stretch.l", "hand.l", 0.04f),
        ("hand.l", "forearm_stretch.l", "middle1.l", 0.045f),
        ("arm_stretch.r", "spine_01.x", "forearm_stretch.r", 0.045f),
        ("forearm_stretch.r", "arm_stretch.r", "hand.r", 0.04f),
        ("hand.r", "forearm_stretch.r", "middle1.r", 0.045f),
        ("thigh_stretch.l", "root.x", "leg_stretch.l", 0.055f),
        ("leg_stretch.l", "thigh_stretch.l", "foot.l", 0.04f),
        ("foot.l", "leg_stretch.l", "toes_01.l", 0.035f),
        ("thigh_stretch.r", "root.x", "leg_stretch.r", 0.055f),
        ("leg_stretch.r", "thigh_stretch.r", "foot.r", 0.04f),
        ("foot.r", "leg_stretch.r", "toes_01.r", 0.035f)
    };
    private static readonly string[] SkillIconNames =
    {
        "texWarlockPassive", "texWarlockPrimary", "texWarlockPrimaryEmpowered", "texWarlockSecondary",
        "texWarlockSecondaryEmpowered", "texWarlockSpecial", "texWarlockUtility", "texWarlockUtilityEmpowered"
    };

    [MenuItem("Tools/Warlock/Setup and Build")]
    public static void SetupAndBuild()
    {
        RebuildModelVariant();
        RebuildDisplayVariant();
        SetupEmoteSkeleton();
        SetupMasks(Require<GameObject>(Root + "mdlWarlock.prefab"));
        SetupEyeAiming();
        var controller = Require<AnimatorController>(Root + "Animations/animWarlock.controller");
        var layers = controller.layers;
        var bookLayer = layers.Single(layer => layer.name == "Book, Override");
        bookLayer.avatarMask = Require<AvatarMask>(Root + "Animations/maskWarlockBook.mask");
        controller.layers = layers;
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        BuildBundle();
    }

    private static void RebuildModelVariant()
    {
        var existing = Require<GameObject>(Root + "mdlWarlock.prefab");
        var model = (GameObject)PrefabUtility.InstantiatePrefab(Require<GameObject>("Assets/FBX/mdlWarlock.fbx"));
        try
        {
            model.name = "mdlWarlock";
            model.transform.localScale = existing.transform.localScale;
            CopyAnimator(existing, model);
            SetupRenderers(model);
            SetupChildLocator(model);
            SetupDynamicBones(model, existing);
            SetupRagdoll(model);
            SaveVariant(model, Root + "mdlWarlock.prefab", "Assets/FBX/mdlWarlock.fbx");
        }
        finally
        {
            Object.DestroyImmediate(model);
        }
    }

    private static void RebuildDisplayVariant()
    {
        var existing = Require<GameObject>(Root + "WarlockDisplay.prefab");
        var display = (GameObject)PrefabUtility.InstantiatePrefab(Require<GameObject>(Root + "mdlWarlock.prefab"));
        try
        {
            display.name = "WarlockDisplay";
            display.transform.localScale = existing.transform.localScale;
            var animator = CopyAnimator(existing, display);
            animator.runtimeAnimatorController = Require<AnimatorController>(Root + "Animations/animWarlockCSS.controller");
            PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
            SetupDynamicBones(display, existing);
            SaveVariant(display, Root + "WarlockDisplay.prefab", Root + "mdlWarlock.prefab");
        }
        finally
        {
            Object.DestroyImmediate(display);
        }
    }

    private static Animator CopyAnimator(GameObject existing, GameObject target)
    {
        var source = existing.GetComponent<Animator>();
        if (!source) throw new InvalidOperationException("Missing configured Animator on " + existing.name);
        var animator = target.GetComponent<Animator>();
        if (!animator) animator = target.AddComponent<Animator>();
        EditorUtility.CopySerialized(source, animator);
        PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
        return animator;
    }

    private static void SetupRenderers(GameObject model, bool hidden = false)
    {
        if (hidden)
        {
            foreach (var bone in model.GetComponentsInChildren<Transform>(true))
            {
                bone.gameObject.SetActive(true);
                PrefabUtility.RecordPrefabInstancePropertyModifications(bone.gameObject);
            }
        }
        foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            string material = renderer.name == "meshBook" ? "matBook" : renderer.name == "meshDagger" ? "matDagger" : "matWarlock";
            renderer.sharedMaterial = Require<Material>(Root + "Materials/" + material + ".mat");
            renderer.gameObject.SetActive(true);
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer.gameObject);
            renderer.enabled = !hidden;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
        if (hidden)
        {
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
        }
    }

    private static void SaveVariant(GameObject instance, string path, string parentPath)
    {
        if (!PrefabUtility.SaveAsPrefabAsset(instance, path))
            throw new InvalidOperationException("Could not save prefab variant: " + path);
        var saved = Require<GameObject>(path);
        if (PrefabUtility.GetPrefabAssetType(saved) != PrefabAssetType.Variant ||
            AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(saved)) != parentPath)
            throw new InvalidOperationException("Incorrect prefab ancestry: " + path);
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

    [MenuItem("Tools/Warlock/Refresh Prop Constraints")]
    public static void RefreshPropConstraints()
    {
        foreach (string name in new[] { "mdlWarlock", "WarlockDisplay" })
        {
            string path = Root + name + ".prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (name == "mdlWarlock") SetupPropConstraints(contents);
                else DisablePropConstraints(contents);
                SaveVariant(contents, path, name == "mdlWarlock" ? "Assets/FBX/mdlWarlock.fbx" : Root + "mdlWarlock.prefab");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
        AssetDatabase.SaveAssets();
        ValidatePropConstraints();
    }

    private static void SetupPropConstraints(GameObject model)
    {
        var reference = Object.Instantiate(model);
        reference.hideFlags = HideFlags.HideAndDontSave;
        try
        {
            foreach (var constraint in reference.GetComponentsInChildren<ParentConstraint>(true))
                constraint.enabled = false;
            LoadImportedIdle().SampleAnimation(reference, 0f);
            foreach (var pair in PropHands)
            {
                var prop = Find(model, pair.Key);
                var referenceProp = Find(reference, pair.Key);
                var referenceHand = Find(reference, pair.Value.hand);
                var hand = Find(model, pair.Value.hand);
                var grip = hand.Find(pair.Value.grip);
                if (!grip) grip = Child(hand, pair.Value.grip, Vector3.zero);
                grip.localPosition = referenceHand.InverseTransformPoint(referenceProp.position);
                grip.localRotation = Quaternion.Inverse(referenceHand.rotation) * referenceProp.rotation;
                grip.localScale = Vector3.one;
                PrefabUtility.RecordPrefabInstancePropertyModifications(grip);
                var constraint = prop.GetComponent<ParentConstraint>();
                if (!constraint) constraint = prop.gameObject.AddComponent<ParentConstraint>();
                constraint.constraintActive = false;
                constraint.locked = false;
                constraint.SetSources(new List<ConstraintSource>
                {
                    new ConstraintSource { sourceTransform = grip, weight = 1f }
                });
                constraint.translationAtRest = prop.localPosition;
                constraint.rotationAtRest = prop.localEulerAngles;
                constraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
                constraint.rotationAxis = Axis.X | Axis.Y | Axis.Z;
                // Grip transforms scale with the rig; ParentConstraint offsets do not.
                constraint.SetTranslationOffset(0, Vector3.zero);
                constraint.SetRotationOffset(0, Vector3.zero);
                constraint.weight = 1f;
                constraint.locked = true;
                constraint.constraintActive = true;
                constraint.enabled = true;
                PrefabUtility.RecordPrefabInstancePropertyModifications(constraint);
            }
        }
        finally
        {
            Object.DestroyImmediate(reference);
        }
    }

    private static void DisablePropConstraints(GameObject display)
    {
        foreach (string name in PropHands.Keys)
        {
            var constraint = Find(display, name).GetComponent<ParentConstraint>();
            if (!constraint) throw new InvalidOperationException("Missing prop constraint: " + name);
            constraint.enabled = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(constraint);
        }
    }

    [MenuItem("Tools/Warlock/Validate Prop Constraints")]
    public static void ValidatePropConstraints()
    {
        var source = Require<GameObject>("Assets/FBX/mdlWarlock.fbx");
        foreach (string name in new[] { "mdlWarlock", "WarlockDisplay" })
        {
            var prefab = Require<GameObject>(Root + name + ".prefab");
            int constraintCount = prefab.GetComponentsInChildren<ParentConstraint>(true).Length;
            if (constraintCount != 0 && constraintCount != PropHands.Count)
                throw new InvalidOperationException("Expected two prop constraints on " + name);
            foreach (var pair in PropHands)
            {
                var prop = Find(prefab, pair.Key);
                if (AnimationUtility.CalculateTransformPath(prop, prefab.transform) !=
                    AnimationUtility.CalculateTransformPath(Find(source, pair.Key), source.transform))
                    throw new InvalidOperationException("Prop hierarchy was changed: " + name + "/" + pair.Key);
                if (constraintCount == 0) continue;
                var grip = Find(prefab, pair.Value.grip);
                var constraint = prop.GetComponent<ParentConstraint>();
                if (!constraint || constraint.enabled != (name == "mdlWarlock") || !constraint.constraintActive ||
                    !constraint.locked || constraint.weight != 1f || constraint.sourceCount != 1 ||
                    constraint.GetSource(0).sourceTransform != grip || constraint.GetSource(0).weight != 1f ||
                    constraint.GetTranslationOffset(0) != Vector3.zero || constraint.GetRotationOffset(0) != Vector3.zero ||
                    grip.parent != Find(prefab, pair.Value.hand) || grip.localScale != Vector3.one ||
                    constraint.translationAxis != (Axis.X | Axis.Y | Axis.Z) || constraint.rotationAxis != (Axis.X | Axis.Y | Axis.Z))
                    throw new InvalidOperationException("Invalid prop constraint: " + name + "/" + pair.Key);
            }
        }
        Debug.Log("Warlock prop hierarchy and any optional hand constraints validated.");
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
        string path = Root + "Materials/" + name + ".mat";
        // Existing materials contain artist-authored settings, not setup defaults.
        if (AssetDatabase.LoadAssetAtPath<Material>(path)) return;

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
        SaveAsset(material, path);
    }

    [MenuItem("Tools/Warlock/Refresh Materials and Build")]
    public static void RefreshMaterialsAndBuild()
    {
        SetupTexturesAndMaterials();
        AssetDatabase.SaveAssets();
        RenderPreviews();
        BuildBundle();
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
        foreach (string name in new[] { "Aim", "Gesture", "Impact", "LeftArm", "Book" })
        {
            string path = Root + "Animations/maskWarlock" + name + ".mask";
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
            if (!mask)
            {
                mask = new AvatarMask { name = "maskWarlock" + name };
                AssetDatabase.CreateAsset(mask, path);
            }
            mask.transformCount = 0;
            mask.AddTransformPath(model.transform, true);
            for (int i = 0; i < mask.transformCount; i++)
            {
                string bonePath = mask.GetTransformPath(i);
                bool active = name == "Book" ? bonePath.StartsWith(BookRoot + "/", StringComparison.Ordinal) :
                    name == "LeftArm" ? bonePath.Contains("/shoulder.l") : bonePath.Contains("/spine_01.x");
                if (PhysicsRoots.Any(b => bonePath.Split('/').Contains(b))) active = false;
                mask.SetTransformActive(i, active);
            }
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, name != "Book" && (name == "LeftArm"
                    ? i == (int)AvatarMaskBodyPart.LeftArm || i == (int)AvatarMaskBodyPart.LeftFingers
                    : i == (int)AvatarMaskBodyPart.Body || i == (int)AvatarMaskBodyPart.Head || i == (int)AvatarMaskBodyPart.LeftArm || i == (int)AvatarMaskBodyPart.RightArm || i == (int)AvatarMaskBodyPart.LeftFingers || i == (int)AvatarMaskBodyPart.RightFingers));
            EditorUtility.SetDirty(mask);
        }
    }

    [MenuItem("Tools/Warlock/Setup Eye Aiming")]
    public static void SetupEyeAiming()
    {
        var model = Require<GameObject>(Root + "mdlWarlock.prefab");
        var eyePaths = new HashSet<string>(new[] { "eye.x", "eye_look.x" }
            .Select(name => AnimationUtility.CalculateTransformPath(Find(model, name), model.transform)));
        var controller = Require<AnimatorController>(Root + "Animations/animWarlock.controller");
        var parameters = controller.parameters.ToList();
        foreach (string name in new[] { "eyePitch", "eyeYaw", "eyeWeight" })
        {
            var existing = parameters.SingleOrDefault(parameter => parameter.name == name);
            if (existing != null)
            {
                if (existing.type != AnimatorControllerParameterType.Float)
                    throw new InvalidOperationException(name + " must be a Float Animator parameter.");
                continue;
            }
            parameters.Add(new AnimatorControllerParameter
            {
                name = name,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = name == "eyeWeight" ? 0f : 0.5f
            });
        }
        controller.parameters = parameters.ToArray();
        const string maskPath = Root + "Animations/maskWarlockEye.mask";
        var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);
        if (!mask)
        {
            mask = new AvatarMask { name = "maskWarlockEye" };
            AssetDatabase.CreateAsset(mask, maskPath);
        }
        mask.transformCount = 0;
        mask.AddTransformPath(model.transform, true);
        for (int i = 0; i < mask.transformCount; i++)
            mask.SetTransformActive(i, eyePaths.Contains(mask.GetTransformPath(i)));
        for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
            mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
        EditorUtility.SetDirty(mask);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Warlock eye setup: eyePitch/eyeYaw neutral at 0.5, eyeWeight defaults to 0 (animation control); eye-only mask and existing layers preserved.");
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

    private static void SetupDynamicBones(GameObject model, GameObject existing)
    {
        var colliders = new List<DynamicBoneCollider>();
        foreach (var item in new[] { Tuple.Create("spine_02.x", 0.15f), Tuple.Create("head.x", 0.12f) })
        {
            var bone = Find(model, item.Item1);
            var collider = bone.GetComponent<DynamicBoneCollider>();
            if (!collider) collider = bone.gameObject.AddComponent<DynamicBoneCollider>();
            var oldCollider = Find(existing, item.Item1).GetComponent<DynamicBoneCollider>();
            if (oldCollider) EditorUtility.CopySerialized(oldCollider, collider);
            else collider.m_Radius = item.Item2 / Mathf.Abs(collider.transform.lossyScale.x) * model.transform.lossyScale.x;
            PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
            colliders.Add(collider);
        }
        foreach (string root in PhysicsRoots)
        {
            var dynamicBone = model.GetComponents<DynamicBone>().SingleOrDefault(b => b.m_Root && b.m_Root.name == root);
            if (!dynamicBone) dynamicBone = model.AddComponent<DynamicBone>();
            var previous = existing.GetComponents<DynamicBone>().SingleOrDefault(b => b.m_Root && b.m_Root.name == root);
            if (previous) EditorUtility.CopySerialized(previous, dynamicBone);
            else
            {
                dynamicBone.m_Damping = root.StartsWith("cloak") ? 0.25f : 0.2f;
                dynamicBone.m_Elasticity = 0.08f;
                dynamicBone.m_Stiffness = root == "hood.x" ? 0.3f : 0.15f;
                dynamicBone.m_Inert = 0.15f;
                dynamicBone.m_Radius = 0.015f;
                dynamicBone.m_DistantDisable = true;
                dynamicBone.m_DistanceToObject = 35f;
            }
            dynamicBone.m_Root = Find(model, root);
            dynamicBone.m_Colliders = colliders;
            dynamicBone.m_Exclusions = previous && previous.m_Exclusions != null
                ? previous.m_Exclusions.Where(t => t).Select(t => Find(model, t.name)).ToList()
                : new List<Transform>();
            dynamicBone.m_ReferenceObject = previous && previous.m_ReferenceObject ? Find(model, previous.m_ReferenceObject.name) : null;
            PrefabUtility.RecordPrefabInstancePropertyModifications(dynamicBone);
        }
    }

    [MenuItem("Tools/Warlock/Refresh Ragdoll and Build")]
    public static void RefreshRagdollAndBuild()
    {
        string path = Root + "mdlWarlock.prefab";
        var model = PrefabUtility.LoadPrefabContents(path);
        try
        {
            SetupRagdoll(model);
            SaveVariant(model, path, "Assets/FBX/mdlWarlock.fbx");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(model);
        }
        AssetDatabase.SaveAssets();
        ValidateRagdoll();
        BuildBundleAssets();
    }

    private static void SetupRagdoll(GameObject model)
    {
        var controller = model.GetComponent<RagdollController>();
        if (!controller) controller = model.AddComponent<RagdollController>();
        controller.bones = RagdollBones.Select(spec => Find(model, spec.bone))
            .Concat(RagdollProps.Keys.Select(name => Find(model, name))).ToArray();
        controller.componentsToDisableOnRagdoll = Array.Empty<MonoBehaviour>();
        PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
        foreach (var spec in RagdollBones)
        {
            var bone = Find(model, spec.bone);
            var body = bone.GetComponent<Rigidbody>();
            if (!body) body = bone.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = true;
            var collider = bone.GetComponent<CapsuleCollider>();
            if (!collider)
            {
                collider = bone.gameObject.AddComponent<CapsuleCollider>();
                collider.radius = spec.radius * Mathf.Abs(model.transform.lossyScale.x / bone.lossyScale.x);
                var tip = spec.tip == null ? Vector3.up * collider.radius * 2f :
                    bone.InverseTransformPoint(Find(model, spec.tip).position);
                collider.center = tip * 0.5f;
                collider.height = Mathf.Max(Mathf.Abs(tip.y) * 0.9f, collider.radius * 2f);
                collider.direction = 1;
            }
            collider.enabled = false;
            collider.isTrigger = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(body);
            PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
        }
        foreach (var pair in RagdollProps)
        {
            var prop = Find(model, pair.Key);
            var body = prop.GetComponent<Rigidbody>();
            if (!body) body = prop.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = true;
            var collider = prop.GetComponent<BoxCollider>();
            if (!collider)
            {
                var renderer = Find(model, pair.Value).GetComponent<SkinnedMeshRenderer>();
                var mesh = new Mesh();
                try
                {
                    renderer.BakeMesh(mesh);
                    var vertices = mesh.vertices;
                    if (vertices.Length == 0) throw new InvalidOperationException("Empty ragdoll prop mesh: " + pair.Value);
                    var bounds = new Bounds(prop.InverseTransformPoint(renderer.transform.TransformPoint(vertices[0])), Vector3.zero);
                    foreach (var vertex in vertices)
                        bounds.Encapsulate(prop.InverseTransformPoint(renderer.transform.TransformPoint(vertex)));
                    collider = prop.gameObject.AddComponent<BoxCollider>();
                    collider.center = bounds.center;
                    collider.size = bounds.size;
                }
                finally
                {
                    Object.DestroyImmediate(mesh);
                }
            }
            collider.enabled = false;
            collider.isTrigger = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(body);
            PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
        }
        foreach (var spec in RagdollBones.Where(spec => spec.parent != null))
        {
            var bone = Find(model, spec.bone);
            var joint = bone.GetComponent<CharacterJoint>();
            if (!joint)
            {
                joint = bone.gameObject.AddComponent<CharacterJoint>();
                joint.lowTwistLimit = new SoftJointLimit { limit = -20f };
                joint.highTwistLimit = new SoftJointLimit { limit = 70f };
                joint.swing1Limit = new SoftJointLimit { limit = 40f };
                joint.swing2Limit = new SoftJointLimit { limit = 40f };
            }
            // The spine and pelvis are siblings in this rig, so ancestry alone cannot connect the torso.
            joint.connectedBody = Find(model, spec.parent).GetComponent<Rigidbody>();
            joint.enableCollision = false;
            joint.enableProjection = true;
            joint.projectionDistance = 0.025f;
            PrefabUtility.RecordPrefabInstancePropertyModifications(joint);
        }
    }

    [MenuItem("Tools/Warlock/Validate Ragdoll")]
    public static void ValidateRagdoll()
    {
        foreach (string name in new[] { "mdlWarlock", "WarlockDisplay" })
        {
            var model = Require<GameObject>(Root + name + ".prefab");
            var controller = model.GetComponent<RagdollController>();
            if (!controller || !controller.enabled || controller.bones == null ||
                !controller.bones.SequenceEqual(RagdollBones.Select(spec => Find(model, spec.bone))
                    .Concat(RagdollProps.Keys.Select(prop => Find(model, prop)))) ||
                controller.componentsToDisableOnRagdoll == null ||
                controller.componentsToDisableOnRagdoll.Any(component => !component))
                throw new InvalidOperationException("Missing or invalid ragdoll controller: " + name);
            foreach (var spec in RagdollBones)
            {
                var bone = Find(model, spec.bone);
                var body = bone.GetComponent<Rigidbody>();
                var collider = bone.GetComponent<CapsuleCollider>();
                var joint = bone.GetComponent<CharacterJoint>();
                if (!body || !body.isKinematic || !body.useGravity || !collider || collider.enabled ||
                    collider.isTrigger || collider.radius <= 0f || collider.height < collider.radius * 2f)
                    throw new InvalidOperationException("Invalid ragdoll physics: " + name + "/" + spec.bone);
                if (spec.parent == null ? joint != null :
                    !joint || joint.connectedBody != Find(model, spec.parent).GetComponent<Rigidbody>() || joint.enableCollision)
                    throw new InvalidOperationException("Invalid ragdoll joint: " + name + "/" + spec.bone);
            }
            foreach (string propName in RagdollProps.Keys)
            {
                var prop = Find(model, propName);
                var body = prop.GetComponent<Rigidbody>();
                var collider = prop.GetComponent<BoxCollider>();
                if (!body || !body.isKinematic || !body.useGravity || !collider || collider.enabled ||
                    collider.isTrigger || collider.size.x <= 0f || collider.size.y <= 0f || collider.size.z <= 0f ||
                    prop.GetComponent<Joint>() || prop.GetComponent<ParentConstraint>())
                    throw new InvalidOperationException("Ragdoll prop must be free to drop: " + name + "/" + propName);
            }
        }
    }

    private static void SetupEmoteSkeleton()
    {
        const string path = "Assets/FBX/mdlWarlock_aapose.fbx";
        var existing = Require<GameObject>(Root + "warlock_emoteskeleton.prefab");
        var model = (GameObject)PrefabUtility.InstantiatePrefab(Require<GameObject>(path));
        try
        {
            model.name = "warlock_emoteskeleton";
            model.transform.localScale = existing.transform.localScale;
            CopyAnimator(existing, model);
            var bodyRenderer = Find(Require<GameObject>(Root + "mdlWarlock.prefab"), "meshBody").GetComponent<SkinnedMeshRenderer>();
            var mapperRenderer = model.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(renderer => renderer.name == bodyRenderer.name);
            if (!mapperRenderer)
            {
                var meshObject = Child(model.transform, bodyRenderer.name, bodyRenderer.transform.localPosition);
                meshObject.localRotation = bodyRenderer.transform.localRotation;
                meshObject.localScale = bodyRenderer.transform.localScale;
                mapperRenderer = meshObject.gameObject.AddComponent<SkinnedMeshRenderer>();
            }
            mapperRenderer.sharedMesh = bodyRenderer.sharedMesh;
            mapperRenderer.bones = bodyRenderer.bones.Select(bone => Find(model, bone.name)).ToArray();
            mapperRenderer.rootBone = Find(model, bodyRenderer.rootBone.name);
            mapperRenderer.localBounds = bodyRenderer.localBounds;
            SetupRenderers(model, true);
            SaveVariant(model, Root + "warlock_emoteskeleton.prefab", path);
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

    [MenuItem("Tools/Warlock/Refresh Emote Skeleton")]
    public static void RefreshEmoteSkeleton()
    {
        SetupEmoteSkeleton();
        AssetDatabase.SaveAssets();
        ValidateEmoteSkeleton();
    }

    [MenuItem("Tools/Warlock/Refresh Emote Skeleton and Build")]
    public static void RefreshEmoteSkeletonAndBuild()
    {
        RefreshEmoteSkeleton();
        BuildBundle();
    }

    [MenuItem("Tools/Warlock/Validate Assets")]
    public static void Validate()
    {
        ValidatePropConstraints();
        ValidateRagdoll();
        foreach (string name in SkillIconNames)
            Require<Sprite>(Root + "Icons/Skill/" + name + ".png");
        foreach (var pair in new Dictionary<string, string>
        {
            { "mdlWarlock", "Assets/FBX/mdlWarlock.fbx" },
            { "WarlockDisplay", Root + "mdlWarlock.prefab" },
            { "warlock_emoteskeleton", "Assets/FBX/mdlWarlock_aapose.fbx" }
        })
        {
            var prefab = Require<GameObject>(Root + pair.Key + ".prefab");
            if (PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.Variant ||
                AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(prefab)) != pair.Value)
                throw new InvalidOperationException("Invalid prefab variant ancestry: " + pair.Key);
            var source = Require<GameObject>(pair.Value);
            foreach (var sourceRenderer in source.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var renderer = Find(prefab, sourceRenderer.name).GetComponent<SkinnedMeshRenderer>();
                if (renderer.sharedMesh != sourceRenderer.sharedMesh || renderer.bones.Length != sourceRenderer.bones.Length)
                    throw new InvalidOperationException("Variant does not use its source mesh/rig: " + pair.Key + "/" + renderer.name);
            }
            foreach (string bone in PhysicsRoots) Find(prefab, bone);
        }
        var model = Require<GameObject>(Root + "mdlWarlock.prefab");
        if (model.GetComponentsInChildren<SkinnedMeshRenderer>().Length != 5) throw new InvalidOperationException("Expected five Warlock renderers.");
        if (model.GetComponents<DynamicBone>().Length != PhysicsRoots.Length) throw new InvalidOperationException("Missing secondary-motion chains.");
        foreach (var dynamicBone in model.GetComponents<DynamicBone>())
            if (!dynamicBone.m_Root || dynamicBone.m_Root.childCount == 0) throw new InvalidOperationException("Invalid dynamic bone root.");
        var display = Require<GameObject>(Root + "WarlockDisplay.prefab");
        if (display.GetComponents<DynamicBone>().Length != PhysicsRoots.Length)
            throw new InvalidOperationException("Display is missing secondary-motion chains.");
        if (display.GetComponent<Animator>().runtimeAnimatorController != Require<AnimatorController>(Root + "Animations/animWarlockCSS.controller"))
            throw new InvalidOperationException("WarlockDisplay must use animWarlockCSS.");
        foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            if (!renderer.sharedMesh || !renderer.sharedMaterial || !renderer.sharedMaterial.mainTexture || renderer.bones.Any(b => !b))
                throw new InvalidOperationException("Incomplete renderer: " + renderer.name);
        var bookMaterial = Require<Material>(Root + "Materials/matBook.mat");
        if (!bookMaterial.GetTexture("_EmissionMap") || bookMaterial.GetColor("_EmissionColor").maxColorComponent <= 0f)
            throw new InvalidOperationException("Book emission is not enabled.");
        var daggerMaterial = Require<Material>(Root + "Materials/matDagger.mat");
        if (!daggerMaterial.IsKeywordEnabled("_NORMALMAP") || !daggerMaterial.GetTexture("_BumpMap"))
            throw new InvalidOperationException("Dagger normal map is not enabled.");
        ValidateDoubleSidedMaterials();
        var idle = LoadImportedIdle();
        foreach (string name in new[] { "Aim", "Gesture", "Impact", "LeftArm", "Book" })
        {
            var mask = Require<AvatarMask>(Root + "Animations/maskWarlock" + name + ".mask");
            for (int i = 0; i < mask.transformCount; i++)
            {
                string path = mask.GetTransformPath(i);
                if (path.Length > 0 && !model.transform.Find(path)) throw new InvalidOperationException("Unbound mask path: " + path);
                if (mask.GetTransformActive(i) && PhysicsRoots.Any(b => path.Split('/').Contains(b)))
                    throw new InvalidOperationException("Gesture mask includes dynamic chain: " + path);
                if (name == "Book" && mask.GetTransformActive(i) != path.StartsWith(BookRoot + "/", StringComparison.Ordinal))
                    throw new InvalidOperationException("Book mask must include only descendants of c_bookroot.x: " + path);
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
        ValidateEmoteSkeleton();
        ValidatePhysics(model);
        ValidatePhysics(display);
        Debug.Log("Warlock validation passed: three linked prefab variants, eight dynamic chains on model/display, current rig and masks, CSS controller and valid humanoid avatar.");
    }

    [MenuItem("Tools/Warlock/Validate Emote Skeleton")]
    public static void ValidateEmoteSkeleton()
    {
        var emote = Require<GameObject>(Root + "warlock_emoteskeleton.prefab").GetComponent<Animator>();
        if (!emote || !emote.avatar || !emote.avatar.isHuman || !emote.avatar.isValid) throw new InvalidOperationException("Invalid emote avatar.");
        if (emote.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0 ||
            emote.GetComponentsInChildren<Renderer>(true).Any(renderer => renderer.enabled))
            throw new InvalidOperationException("Emote renderer objects must be active for bone discovery while their renderers stay hidden.");
        var model = Require<GameObject>(Root + "mdlWarlock.prefab");
        var mappingRenderer = Find(emote.gameObject, "meshBody").GetComponent<SkinnedMeshRenderer>();
        var bodyRenderer = Find(model, "meshBody").GetComponent<SkinnedMeshRenderer>();
        if (!mappingRenderer || !bodyRenderer || !bodyRenderer.sharedMesh || mappingRenderer.sharedMesh != bodyRenderer.sharedMesh ||
            mappingRenderer.bones.Length == 0 || mappingRenderer.bones.Any(bone => !bone) || bodyRenderer.bones.Any(bone => !bone) ||
            !mappingRenderer.bones.Select(bone => AnimationUtility.CalculateTransformPath(bone, emote.transform))
                .SequenceEqual(bodyRenderer.bones.Select(bone => AnimationUtility.CalculateTransformPath(bone, model.transform))))
            throw new InvalidOperationException("Emote bone mapping is stale. Refresh the emote skeleton variant after reimporting the rig.");
        Find(emote.gameObject, "dagger.x");
        if (emote.cullingMode != AnimatorCullingMode.AlwaysAnimate)
            throw new InvalidOperationException("The hidden emote skeleton must animate even when its renderers are invisible.");
        Debug.Log("Warlock emote skeleton validated: humanoid avatar, hidden renderers and bone mapping matching the current model hierarchy.");
    }

    private static AnimationClip LoadImportedIdle() => AssetDatabase.LoadAllAssetsAtPath("Assets/FBX/mdlWarlock_Idle.fbx")
        .OfType<AnimationClip>().Single(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal));

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
                    material.DisableKeyword("_EMISSION");
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
            var idle = LoadImportedIdle();
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
        BuildBundleAssets();
    }

    private static void BuildBundleAssets()
    {
        string output = "AssetBundles/StandaloneWindows";
        Directory.CreateDirectory(output);
        var assets = new[]
        {
            "mdlWarlock.prefab", "WarlockDisplay.prefab", "warlock_emoteskeleton.prefab",
            "Animations/maskWarlockBook.mask", "Animations/maskWarlockEye.mask",
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
