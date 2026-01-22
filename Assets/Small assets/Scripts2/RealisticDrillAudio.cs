using UnityEngine;
using UnityEngine.XR;

public class RealisticDrillController : MonoBehaviour
{
    [Header("Audio Setup")]
    public AudioSource drillAudio;
    public float loopStart = 1.5f; // Where the "Spinning" sound begins
    public float loopEnd = 2.6f;   // Where the "Spinning" sound ends
    public float cutOffTime = 4.1f; // The absolute end of the file

    [Header("Effects")]
    public ParticleSystem dustEffect;
    public XRNode controllerNode = XRNode.RightHand;
    public float vibrationStrength = 0.5f;

    private bool isTriggerPressed = false;
    private bool isTouchingDecay = false;
    private bool isWindDown = false; // Are we currently playing the stop sound?

    void Start()
    {
        if (drillAudio)
        {
            drillAudio.loop = false;
            drillAudio.playOnAwake = false;
            drillAudio.Stop();
        }
        if (dustEffect) dustEffect.Stop();
    }

    void Update()
    {
        CheckInput(); // 1. Read Button
        UpdateAudio(); // 2. Manage Sound
        UpdateEffects(); // 3. Manage Dust/Vibration
    }

    void CheckInput()
    {
        // Support both VR Trigger AND Mouse Click (for testing)
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool vrClick = false;
        if (device.isValid)
        {
            device.TryGetFeatureValue(CommonUsages.trigger, out float val);
            vrClick = val > 0.1f;
        }

        isTriggerPressed = vrClick || Input.GetMouseButton(0);
    }

    void UpdateAudio()
    {
        if (isTriggerPressed)
        {
            // --- BUTTON IS HELD ---
            
            isWindDown = false; // We are definitely not stopping
            drillAudio.volume = 1.0f; // Ensure volume is up

            // A. If silent, START from 0
            if (!drillAudio.isPlaying)
            {
                drillAudio.time = 0f; 
                drillAudio.Play();
            }

            // B. If we passed the loop point, rewind to loop start
            // This creates the "Infinite Spin" effect
            if (drillAudio.time >= loopEnd)
            {
                drillAudio.time = loopStart;
            }
        }
        else
        {
            // --- BUTTON RELEASED ---

            if (drillAudio.isPlaying)
            {
                // A. First frame of release: Jump to "Wind Down" section
                if (!isWindDown)
                {
                    // Only jump if we haven't reached the wind-down part yet
                    if (drillAudio.time < loopEnd)
                    {
                        drillAudio.time = loopEnd; 
                    }
                    isWindDown = true;
                }

                // B. Stop if we reach the cut-off
                if (drillAudio.time >= cutOffTime)
                {
                    drillAudio.Stop();
                    drillAudio.time = 0; // Reset for next click
                    isWindDown = false;
                }
            }
        }
    }

    void UpdateEffects()
    {
        if (isTriggerPressed && isTouchingDecay)
        {
            // Drilling into tooth
            drillAudio.pitch = Mathf.Lerp(drillAudio.pitch, 0.9f, Time.deltaTime * 5); // Pitch drop
            if (!dustEffect.isPlaying) dustEffect.Play();
            TriggerHaptic(vibrationStrength);
        }
        else
        {
            // Air spinning
            drillAudio.pitch = Mathf.Lerp(drillAudio.pitch, 1.0f, Time.deltaTime * 5); // Normal pitch
            if (dustEffect.isPlaying) dustEffect.Stop();
        }
    }

    // --- COLLISION ---
    void OnTriggerEnter(Collider other) { if(other.CompareTag("Decay")) isTouchingDecay = true; }
    void OnTriggerExit(Collider other) { if(other.CompareTag("Decay")) isTouchingDecay = false; }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Decay"))
        {
            isTouchingDecay = true;
            if (isTriggerPressed) Destroy(other.gameObject);
        }
    }

    void TriggerHaptic(float amp)
    {
        InputDevice d = InputDevices.GetDeviceAtXRNode(controllerNode);
        if (d.isValid) d.SendHapticImpulse(0, amp, 0.1f);
    }
}