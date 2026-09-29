using RoR2;
using UnityEngine;

namespace WarlockMod.Warlock.Components
{
    public sealed class WarlockEyeController : MonoBehaviour
    {
        private static readonly int EyePitch = Animator.StringToHash("eyePitch");
        private static readonly int EyeYaw = Animator.StringToHash("eyeYaw");
        private static readonly int EyeWeight = Animator.StringToHash("eyeWeight");
        private const float SearchInterval = 0.1f;
        private const float SmoothTime = 0.1f;
        private const float YawRange = 48.3f;
        private const float PitchRange = 20f;

        private readonly BullseyeSearch search = new BullseyeSearch
        {
            filterByLoS = false,
            sortMode = BullseyeSearch.SortMode.Distance,
            maxAngleFilter = 180f
        };

        private CharacterBody body;
        private Animator animator;
        private Transform head;
        private Transform eye;
        private HurtBox target;
        private int pitchLayer;
        private int yawLayer;
        private float searchTimer;
        private bool initialized;

        private void Start()
        {
            body = GetComponent<CharacterBody>();
            var locator = GetComponent<ModelLocator>();
            if (locator && locator.modelTransform)
            {
                animator = locator.modelTransform.GetComponent<Animator>();
                var children = locator.modelTransform.GetComponent<ChildLocator>();
                head = children ? children.FindChild("Head") : null;
                eye = head ? head.Find("eye.x") : null;
            }
            if (!body || !body.teamComponent || !animator || !head || !eye)
            {
                Log.Error("Warlock eye tracking requires its body, team, Animator, Head and eye.x bone.");
                enabled = false;
                return;
            }
            pitchLayer = animator.GetLayerIndex("EyePitch");
            yawLayer = animator.GetLayerIndex("EyeYaw");
            if (pitchLayer < 0 || yawLayer < 0 || !HasFloat(EyePitch) || !HasFloat(EyeYaw) || !HasFloat(EyeWeight))
            {
                Log.Error("Warlock's animator is missing its eye layers or eyePitch, eyeYaw and eyeWeight parameters.");
                enabled = false;
                return;
            }
            search.viewer = body;
            initialized = true;
            ResetEyes();
        }

        private bool HasFloat(int hash)
        {
            foreach (var parameter in animator.parameters)
                if (parameter.nameHash == hash && parameter.type == AnimatorControllerParameterType.Float) return true;
            return false;
        }

        private void Update()
        {
            if (!initialized || !body || !animator || !head || !eye) return;
            if (!animator.isActiveAndEnabled || !body.healthComponent || !body.healthComponent.alive ||
                (body.outOfCombat && body.outOfDanger))
            {
                target = null;
                searchTimer = 0f;
                ResetEyes();
                return;
            }

            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                searchTimer = SearchInterval;
                search.teamMaskFilter = TeamMask.allButNeutral;
                search.teamMaskFilter.RemoveTeam(body.teamComponent.teamIndex);
                search.searchOrigin = eye.position;
                search.searchDirection = head.forward;
                search.RefreshCandidates();
                target = null;
                foreach (var candidate in search.GetResults())
                {
                    if (!candidate || !candidate.healthComponent || !candidate.healthComponent.alive) continue;
                    target = candidate;
                    break;
                }
            }

            float pitch = 0.5f;
            float yaw = 0.5f;
            bool hasTarget = target && target.healthComponent && target.healthComponent.alive;
            if (hasTarget)
            {
                Vector3 direction = head.InverseTransformDirection(target.transform.position - eye.position);
                float yawAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float pitchAngle = Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg;
                yaw = Mathf.Clamp01(0.5f + yawAngle / (2f * YawRange));
                pitch = Mathf.Clamp01(0.5f + pitchAngle / (2f * PitchRange));
            }

            // These clips loop, so a cycle of exactly 1 would wrap to the opposite pose.
            animator.SetFloat(EyePitch, Mathf.Min(pitch, 0.999f), SmoothTime, Time.deltaTime);
            animator.SetFloat(EyeYaw, Mathf.Min(yaw, 0.999f), SmoothTime, Time.deltaTime);
            animator.SetFloat(EyeWeight, hasTarget ? 1f : 0f, SmoothTime, Time.deltaTime);
            float weight = Mathf.Clamp01(animator.GetFloat(EyeWeight));
            animator.SetLayerWeight(pitchLayer, weight);
            animator.SetLayerWeight(yawLayer, weight);
        }

        private void ResetEyes()
        {
            animator.SetFloat(EyePitch, 0.5f);
            animator.SetFloat(EyeYaw, 0.5f);
            animator.SetFloat(EyeWeight, 0f);
            animator.SetLayerWeight(pitchLayer, 0f);
            animator.SetLayerWeight(yawLayer, 0f);
        }

        private void OnDisable()
        {
            target = null;
            searchTimer = 0f;
            if (initialized && animator) ResetEyes();
        }
    }
}
