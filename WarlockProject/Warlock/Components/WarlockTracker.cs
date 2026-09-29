using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using WarlockMod.Warlock;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.Components

{
    [RequireComponent(typeof(TeamComponent))]
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(InputBankTest))]
    public class WarlockTracker : MonoBehaviour
    {
        public float maxTrackingDistance = 100f;

        public float maxTrackingAngle = 10f;

        public float trackerUpdateFrequency = 10f;

        private HurtBox trackingTarget;

        private CharacterBody characterBody;

        private TeamComponent teamComponent;

        private InputBankTest inputBank;

        private float trackerUpdateStopwatch;

        private Indicator indicator;

        private bool chargeLocked;
        private HurtBox chargeTarget;

        internal void SetChargeTarget(HurtBox target)
        {
            chargeLocked = true;
            chargeTarget = target;
        }

        internal void ClearChargeTarget()
        {
            chargeLocked = false;
            chargeTarget = null;
        }

        private readonly BullseyeSearch search = new BullseyeSearch();

        private void Awake()
        {
            indicator = new Indicator(gameObject, WarlockAssets.telekinesisTracker);
        }

        private void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
            teamComponent = GetComponent<TeamComponent>();
        }

        public HurtBox GetTrackingTarget()
        {
            return chargeLocked ? chargeTarget : trackingTarget;
        }
        private void OnEnable()
        {
            indicator.active = false;
        }

        private void OnDisable()
        {
            indicator.active = false;
        }

        private void FixedUpdate()
        {
            if (!characterBody.hasEffectiveAuthority)
            {
                trackingTarget = null;
                indicator.active = false;
                return;
            }
            if (chargeLocked)
            {
                indicator.active = true;
                indicator.targetTransform = chargeTarget ? chargeTarget.transform : null;
                return;
            }
            if (characterBody.skillLocator.secondary.stock <= 0 &&
                !characterBody.HasBuff(WarlockBuffs.warlockCrimsonManaFullStack))
            {
                // A pending Hex must still read its target after the last stock is spent.
                indicator.active = false;
                indicator.targetTransform = null;
                trackerUpdateStopwatch = 1f / trackerUpdateFrequency;
                return;
            }
            indicator.active = true;
            trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                trackerUpdateStopwatch %= 1f / trackerUpdateFrequency;
                Ray aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                SearchForTarget(aimRay);
                indicator.targetTransform = trackingTarget ? trackingTarget.transform : null;
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            search.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(gameObject);
            trackingTarget = null;
            foreach (var candidate in search.GetResults())
            {
                if (!candidate || !candidate.healthComponent || !candidate.healthComponent.alive) continue;
                trackingTarget = candidate;
                break;
            }
        }
    }
}
