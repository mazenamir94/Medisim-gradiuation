using UnityEngine;

public class MockProcedureDriver : MonoBehaviour
{
    public BackendSessionReporter reporter;
    public ProcedureStateMachine stateMachine;
    public ToolManager toolManager;

    [Header("Mock drill values")]
    public float mockDepthMm = 0.5f;
    public float mockAngleDeg = 10f;

    private void Update()
    {
        // Start / End
        if (Input.GetKeyDown(KeyCode.S))
            reporter.StartProcedure();

        if (Input.GetKeyDown(KeyCode.F))
            reporter.EndProcedure();

        // Steps
        if (Input.GetKeyDown(KeyCode.Alpha1))
            stateMachine.SetStep(ProcedureStateMachine.Step.DRILLING);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            stateMachine.SetStep(ProcedureStateMachine.Step.CLEANING);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            stateMachine.SetStep(ProcedureStateMachine.Step.FILLING);

        // Tools (fake IDs)
        if (Input.GetKeyDown(KeyCode.Q))
            toolManager.SetTool("HIGH_SPEED_BUR");   // correct for drilling

        if (Input.GetKeyDown(KeyCode.W))
            toolManager.SetTool("SCALER");          // wrong for drilling

        if (Input.GetKeyDown(KeyCode.E))
            toolManager.SetTool("COMPOSITE_GUN");   // filling tool

        // Mock drill samples:
        // Press D to send a "good" sample (under limits)
        if (Input.GetKeyDown(KeyCode.D))
            SendMockSample(depth: 1.7f, angle: 10f);

        // Press X to send a "too deep" sample
        if (Input.GetKeyDown(KeyCode.X))
            SendMockSample(depth: 3.2f, angle: 10f);

        // Press C to send a "too steep angle" sample
        if (Input.GetKeyDown(KeyCode.C))
            SendMockSample(depth: 1.7f, angle: 35f);
    }

    private void SendMockSample(float depth, float angle)
    {
        // We don't have a real drill sampler yet, so call the reporter’s backend method directly:
        // easiest: expose a public method in BackendSessionReporter (below)
        reporter.SendManualDrillSample(depth, angle);
    }
}
