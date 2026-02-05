using System;
using UnityEngine;

public class ProcedureStateMachine : MonoBehaviour
{
    public enum Step
    {
        START,
        DRILLING,
        CLEANING,
        FILLING,
        FINISH
    }

    [Header("Current State")]
    [SerializeField] private Step currentStep = Step.START;

    public Step CurrentStep => currentStep;

    public event Action<Step> OnStepChanged;

    public void SetStep(Step newStep)
    {
        if (newStep == currentStep) return;
        currentStep = newStep;
        OnStepChanged?.Invoke(currentStep);
    }

    // Optional: Simple keyboard test in Editor
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetStep(Step.DRILLING);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetStep(Step.CLEANING);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetStep(Step.FILLING);
        if (Input.GetKeyDown(KeyCode.Alpha0)) SetStep(Step.FINISH);
    }

    // Backend expects string like "DRILLING"
    public static string StepToBackendString(Step s) => s.ToString();
}
