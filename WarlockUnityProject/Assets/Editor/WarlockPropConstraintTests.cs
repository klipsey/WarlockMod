using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.TestTools;

public class WarlockPropConstraintTests
{
    private GameObject model;
    private GameObject reference;

    [Test]
    public void PrefabsKeepTheirHierarchyAndCharacterSelectAnimations()
    {
        WarlockAssetSetup.ValidatePropConstraints();
    }

    [UnityTest]
    public IEnumerator PropsKeepIdleGripsAcrossBlendedAim()
    {
        yield return new EnterPlayMode();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Warlock/mdlWarlock.prefab");
        model = Object.Instantiate(prefab);
        reference = Object.Instantiate(prefab);
        reference.GetComponent<Animator>().enabled = false;
        foreach (var constraint in reference.GetComponentsInChildren<ParentConstraint>())
            constraint.enabled = false;
        Clip("Idle").SampleAnimation(reference, 0f);
        var constraints = model.GetComponentsInChildren<ParentConstraint>();
        Assert.That(constraints.Length, Is.EqualTo(2));
        var offsets = new Vector3[constraints.Length];
        var rotations = new Quaternion[constraints.Length];
        for (int i = 0; i < constraints.Length; i++)
        {
            Assert.That(constraints[i].sourceCount, Is.EqualTo(1));
            var hand = Find(reference, constraints[i].GetSource(0).sourceTransform.parent.name);
            var prop = Find(reference, constraints[i].name);
            offsets[i] = hand.InverseTransformPoint(prop.position);
            rotations[i] = Quaternion.Inverse(hand.rotation) * prop.rotation;
        }
        var animator = model.GetComponent<Animator>();
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        int pitchLayer = animator.GetLayerIndex("AimPitch");
        int yawLayer = animator.GetLayerIndex("AimYaw");
        Assert.That(pitchLayer, Is.GreaterThanOrEqualTo(0));
        Assert.That(yawLayer, Is.GreaterThanOrEqualTo(0));
        yield return null;
        yield return null;
        var initialHand = model.transform.InverseTransformPoint(constraints[0].GetSource(0).sourceTransform.parent.position);
        float handMovement = 0f;
        foreach (float weight in new[] { 0f, 0.5f, 1f })
        {
            animator.SetLayerWeight(pitchLayer, weight);
            animator.SetLayerWeight(yawLayer, weight);
            foreach (float pitch in new[] { 0f, 0.25f, 0.5f, 0.75f, 1f })
            {
                animator.SetFloat("aimPitchCycle", pitch);
                foreach (float yaw in new[] { 0f, 0.25f, 0.5f, 0.75f, 1f })
                {
                    animator.SetFloat("aimYawCycle", yaw);
                    model.transform.SetPositionAndRotation(new Vector3(2f, 1f, -3f), Quaternion.Euler(0f, 37f, 0f));
                    yield return null;
                    yield return null;
                    AssertGrips(constraints, offsets, rotations);
                    handMovement = Mathf.Max(handMovement, Vector3.Distance(initialHand,
                        model.transform.InverseTransformPoint(constraints[0].GetSource(0).sourceTransform.parent.position)));
                }
            }
        }
        Assert.That(handMovement, Is.GreaterThan(0.01f), "The aim layers must actually move the source hand.");
        foreach (float scale in new[] { 0.5f, 2f })
        {
            model.transform.localScale = prefab.transform.localScale * scale;
            yield return null;
            yield return null;
            AssertGrips(constraints, offsets, rotations);
        }
        animator.enabled = false;
        var book = Find(model, "c_bookroot.x");
        var bones = book.GetComponentsInChildren<Transform>();
        var opening = Clip("BookOpen");
        opening.SampleAnimation(model, 0f);
        yield return null;
        yield return null;
        var closedRotations = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++) closedRotations[i] = bones[i].localRotation;
        opening.SampleAnimation(model, opening.length);
        yield return null;
        yield return null;
        AssertGrips(constraints, offsets, rotations);
        bool bookAnimated = false;
        for (int i = 0; i < bones.Length; i++)
            if (bones[i] != book && Quaternion.Angle(closedRotations[i], bones[i].localRotation) > 1f) bookAnimated = true;
        Assert.That(bookAnimated, Is.True, "The book's internal opening animation must remain free to move.");
    }

    private static void AssertGrips(ParentConstraint[] constraints, Vector3[] offsets, Quaternion[] rotations)
    {
        for (int i = 0; i < constraints.Length; i++)
        {
            var hand = constraints[i].GetSource(0).sourceTransform.parent;
            Assert.That(Vector3.Distance(hand.TransformPoint(offsets[i]), constraints[i].transform.position),
                Is.LessThan(0.0001f), constraints[i].name + " grip position");
            Assert.That(Quaternion.Angle(hand.rotation * rotations[i], constraints[i].transform.rotation),
                Is.LessThan(0.1f), constraints[i].name + " grip rotation");
        }
    }

    private static Transform Find(GameObject target, string name) =>
        target.GetComponentsInChildren<Transform>(true).Single(t => t.name == name);

    private static AnimationClip Clip(string name) =>
        AssetDatabase.LoadAllAssetsAtPath("Assets/FBX/mdlWarlock_" + name + ".fbx")
            .OfType<AnimationClip>().Single(c => !c.name.StartsWith("__preview__"));

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (model) Object.DestroyImmediate(model);
        if (reference) Object.DestroyImmediate(reference);
        if (Application.isPlaying) yield return new ExitPlayMode();
    }
}
