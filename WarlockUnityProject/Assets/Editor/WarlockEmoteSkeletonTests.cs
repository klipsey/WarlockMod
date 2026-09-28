using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class WarlockEmoteSkeletonTests
{
    [Test]
    public void EmoteSkeletonMatchesCurrentModelHierarchy()
    {
        WarlockAssetSetup.ValidateEmoteSkeleton();
    }

    [Test]
    public void HumanoidPoseMovesEmoteArm()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Warlock/warlock_emoteskeleton.prefab");
        var skeleton = Object.Instantiate(prefab);
        try
        {
            var animator = skeleton.GetComponent<Animator>();
            animator.Rebind();
            animator.Update(0f);
            animator.enabled = false;
            var arm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Assert.That(arm, Is.Not.Null);
            int muscle = HumanTrait.MuscleFromBone((int)HumanBodyBones.LeftUpperArm, 2);
            Assert.That(muscle, Is.GreaterThanOrEqualTo(0));
            using (var handler = new HumanPoseHandler(animator.avatar, skeleton.transform))
            {
                var pose = new HumanPose();
                handler.GetHumanPose(ref pose);
                var before = arm.localRotation;
                pose.muscles[muscle] = pose.muscles[muscle] > 0f ? -0.5f : 0.5f;
                handler.SetHumanPose(ref pose);
                Assert.That(Quaternion.Angle(before, arm.localRotation), Is.GreaterThan(1f));
            }
        }
        finally
        {
            Object.DestroyImmediate(skeleton);
        }
    }
}
