using UnityEngine;

public class DrillMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of the drill movement.")]
    public float moveSpeed = 5.0f;

    [Header("Collision Settings")]
    [Tooltip("Layers that block the drill (e.g., Default, Default except Drill).")]
    public LayerMask collisionMask;
    [Tooltip("Radius of the drill tip for collision checks.")]
    public float drillRadius = 0.1f;

    void Update()
    {
        // 1. Gather Input
        float moveX = 0f;
        float moveY = 0f;
        float moveZ = 0f;

        // A / D = Left / Right
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;

        // W / S = Up / Down
        if (Input.GetKey(KeyCode.W)) moveY = 1f;
        if (Input.GetKey(KeyCode.S)) moveY = -1f;

        // Q / E = Forward / Backward
        if (Input.GetKey(KeyCode.Q)) moveZ = 1f; // Forward
        if (Input.GetKey(KeyCode.E)) moveZ = -1f; // Backward

        Vector3 inputDir = new Vector3(moveX, moveY, moveZ).normalized;

        if (inputDir.magnitude > 0.01f)
        {
            MoveDrill(inputDir);
        }
    }

    void MoveDrill(Vector3 direction)
    {
        float distance = moveSpeed * Time.deltaTime;

        // 2. Collision Check (SphereCast)
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, drillRadius, direction, out hit, distance, collisionMask))
        {
            // CHECK TAG
            if (hit.collider.CompareTag("Decay"))
            {
                // DECAY: Allow slight penetration to trigger the shrink effect (DrillTip.cs)
                // If the distance to the hit is super small, we are already touching.
                // We allow a very small overlapping penetration (e.g. 0.05f).
                float allowedMove = Mathf.Min(distance, hit.distance + 0.05f);
                transform.position += direction * allowedMove;
            }
            else
            {
                // HEALTHY TOOTH / WALL: SLIDE
                // We do NOT want to penetrate. instead, we slide along the surface.
                
                // 1. Calculate the slide direction (project our move direction onto the wall's plane)
                Vector3 slideDir = Vector3.ProjectOnPlane(direction, hit.normal).normalized;

                // 2. Move along the slide direction (if we aren't moving directly into it/stuck)
                // We assume the slide path is clear for this frame (simple sliding).
                if (slideDir != Vector3.zero)
                {
                    transform.position += slideDir * distance;
                }
            }
        }
        else
        {
            // No barrier, move freely
            transform.position += direction * distance;
        }
    }
}
