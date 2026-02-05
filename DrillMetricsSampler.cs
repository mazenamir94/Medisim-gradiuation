using UnityEngine;

public class DrillMetricsSampler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Tip of the drill (a child transform at the bur tip).")]
    public Transform drillTip;

    [Tooltip("A reference transform on the tooth. Its +up should represent the occlusal normal (or your chosen normal).")]
    public Transform toothReference;

    [Tooltip("LayerMask for the tooth collider(s). Put your tooth mesh collider on this layer.")]
    public LayerMask toothLayer;

    [Header("Depth Settings")]
    [Tooltip("How far we raycast into the tooth to estimate penetration depth.")]
    public float maxDepthMm = 6f;

    [Tooltip("Ray direction is opposite of drillTip.forward by default (drilling direction).")]
    public bool useTipForwardAsDirection = true;

    [Header("Debug")]
    public bool drawDebug = true;

    public float CurrentDepthMm { get; private set; }
    public float CurrentAngleDeg { get; private set; }
    public bool IsContactingTooth { get; private set; }

    // Call this each frame or on a timer; it updates properties
    public void UpdateMetrics()
    {
        if (drillTip == null || toothReference == null)
        {
            CurrentDepthMm = 0;
            CurrentAngleDeg = 0;
            IsContactingTooth = false;
            return;
        }

        // --- Angle ---
        // Define "ideal drilling axis" as toothReference.up (occlusal normal).
        // Define drill axis as drillTip.forward.
        Vector3 ideal = toothReference.up.normalized;
        Vector3 drillAxis = drillTip.forward.normalized;

        // Angle between axes in degrees
        CurrentAngleDeg = Vector3.Angle(drillAxis, ideal);

        // --- Depth ---
        // We raycast from tip backwards into the tooth along drilling direction.
        Vector3 dir = useTipForwardAsDirection ? drillTip.forward : -drillTip.forward;
        dir = dir.normalized;

        // Convert mm to meters if your Unity world units are meters.
        // If 1 Unity unit = 1 meter (default), then 1mm = 0.001 units.
        float maxDistWorld = maxDepthMm * 0.001f;

        Ray ray = new Ray(drillTip.position, dir);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistWorld, toothLayer, QueryTriggerInteraction.Ignore))
        {
            IsContactingTooth = true;
            float distWorld = hit.distance;

            // Penetration depth is how far we travelled before hitting.
            // If your drill tip starts outside and hits tooth surface, hit.distance is surface distance.
            // For a simple v1: treat depth as (maxDepth - hit.distance) * 1000 => mm.
            // But that assumes your ray is cast from inside out; here it's from tip into tooth.
            // Better v1: define depth as hit.distance in mm when tip is at surface contact.
            // We'll instead interpret depth as "how far into tooth the ray goes before encountering surface", which is limited.

            // Practical v1 for drilling evaluation:
            // Use hit.distance as "contact distance" and map it to depth.
            // If tip is on surface, hit.distance ~0 -> depth ~0.
            // If tip is inside, you often need collider inside checks.
            // So we also add a fallback: if tip is inside a tooth collider, estimate depth by raycast outward.

            float depthMm = distWorld * 1000f;

            // If very small, likely at surface
            CurrentDepthMm = Mathf.Max(0f, depthMm);

            if (drawDebug)
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
        }
        else
        {
            // Might still be "inside" tooth collider; check overlap
            // We do a small sphere overlap at the tip to detect "inside".
            float radius = 0.0008f; // ~0.8mm
            Collider[] overlaps = Physics.OverlapSphere(drillTip.position, radius, toothLayer, QueryTriggerInteraction.Ignore);

            if (overlaps != null && overlaps.Length > 0)
            {
                IsContactingTooth = true;

                // Estimate depth by raycasting outwards opposite direction
                Vector3 outDir = -dir;
                Ray outRay = new Ray(drillTip.position, outDir);
                if (Physics.Raycast(outRay, out RaycastHit outHit, maxDistWorld, toothLayer, QueryTriggerInteraction.Ignore))
                {
                    float distOutMm = outHit.distance * 1000f;
                    CurrentDepthMm = Mathf.Clamp(distOutMm, 0f, maxDepthMm);
                    if (drawDebug)
                        Debug.DrawRay(outRay.origin, outRay.direction * outHit.distance, Color.yellow);
                }
                else
                {
                    CurrentDepthMm = 0f;
                }
            }
            else
            {
                IsContactingTooth = false;
                CurrentDepthMm = 0f;
                if (drawDebug)
                    Debug.DrawRay(ray.origin, ray.direction * maxDistWorld, Color.red);
            }
        }
    }
}
