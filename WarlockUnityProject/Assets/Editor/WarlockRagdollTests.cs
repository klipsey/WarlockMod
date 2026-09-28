using System.Collections;
using System.Linq;
using NUnit.Framework;
using RoR2;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.TestTools;

public class WarlockRagdollTests
{
    private GameObject model;
    private GameObject floor;
    private bool? autoSimulation;
    private Vector3? gravity;

    [Test]
    public void ModelAndDisplayHaveConnectedDormantRagdolls()
    {
        WarlockAssetSetup.ValidateRagdoll();
    }

    [UnityTest]
    public IEnumerator RagdollFallsWithoutSeparatingAndDropsProps()
    {
        yield return new EnterPlayMode();
        model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Warlock/mdlWarlock.prefab"));
        model.transform.position = Vector3.up * 2f;
        var animator = model.GetComponent<Animator>();
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        yield return null;
        yield return null;

        var controller = model.GetComponent<RagdollController>();
        Assert.That(controller, Is.Not.Null);
        var bones = controller.bones;
        Assert.That(bones.Length, Is.EqualTo(17));
        foreach (var bone in bones)
        {
            Assert.That(bone.GetComponent<Rigidbody>().isKinematic, Is.True, bone.name);
            Assert.That(bone.GetComponent<Collider>().enabled, Is.False, bone.name);
        }

        floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.layer = 11;
        floor.transform.position = Vector3.down * 0.5f;
        floor.transform.localScale = new Vector3(20f, 1f, 20f);
        var constraints = model.GetComponentsInChildren<ParentConstraint>();
        Assert.That(constraints, Is.Empty);
        var props = bones.Where(bone => bone.name == "c_bookroot.x" || bone.name == "dagger.x").ToArray();
        Assert.That(props.Length, Is.EqualTo(2));
        var propStarts = props.Select(prop => prop.position).ToArray();
        var start = bones[0].position;
        autoSimulation = Physics.autoSimulation;
        Physics.autoSimulation = false;
        gravity = Physics.gravity;
        Physics.gravity = Vector3.down * 30f;
        animator.enabled = false;
        // The editor component is a serialization stub; reproduce RoR2's handoff to real Unity physics.
        foreach (var bone in bones)
        {
            bone.gameObject.layer = 17;
            bone.SetParent(model.transform, true);
            bone.GetComponent<Collider>().enabled = true;
            var body = bone.GetComponent<Rigidbody>();
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.AddForce(Vector3.right + Vector3.up * 3f, ForceMode.VelocityChange);
        }
        foreach (var component in controller.componentsToDisableOnRagdoll) component.enabled = false;
        Physics.SyncTransforms();

        for (int frame = 0; frame < 150; frame++)
        {
            Physics.Simulate(Time.fixedDeltaTime);
            yield return null;
            foreach (var bone in bones)
            {
                Assert.That(float.IsNaN(bone.position.sqrMagnitude) || float.IsInfinity(bone.position.sqrMagnitude),
                    Is.False, bone.name);
                Assert.That(bone.position.y, Is.GreaterThan(-0.3f), bone.name + " fell through the floor");
                var joint = bone.GetComponent<CharacterJoint>();
                if (joint)
                    Assert.That(Vector3.Distance(bone.TransformPoint(joint.anchor),
                        joint.connectedBody.transform.TransformPoint(joint.connectedAnchor)),
                        Is.LessThan(0.1f), bone.name + " joint separated at frame " + frame +
                        ", connected anchor " + joint.connectedAnchor);
            }
        }
        yield return null;
        Assert.That(bones[0].position.y, Is.LessThan(start.y - 1f), "Ragdoll must fall.");
        Assert.That(bones[0].position.x, Is.GreaterThan(start.x + 0.1f), "Death force must move the ragdoll.");
        Assert.That(animator.enabled, Is.False);
        for (int i = 0; i < props.Length; i++)
        {
            Assert.That(props[i].parent, Is.EqualTo(model.transform));
            Assert.That(props[i].GetComponent<Joint>(), Is.Null, props[i].name);
            Assert.That(props[i].GetComponent<Rigidbody>().isKinematic, Is.False, props[i].name);
            Assert.That(props[i].position.y, Is.LessThan(propStarts[i].y - 1f), props[i].name + " did not drop");
        }
        Assert.That(model.GetComponents<DynamicBone>().All(chain => chain.enabled), Is.True);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (model) Object.DestroyImmediate(model);
        if (floor) Object.DestroyImmediate(floor);
        if (autoSimulation.HasValue) Physics.autoSimulation = autoSimulation.Value;
        if (gravity.HasValue) Physics.gravity = gravity.Value;
        if (Application.isPlaying) yield return new ExitPlayMode();
    }
}
