using UnityEngine;
using UnityEngine.XR;

public class RealisticDrillController : MonoBehaviour
{
    [Header("Audio Setup")]
    public AudioSource drillAudio;
    public float loopStart = 1.5f; 
    public float loopEnd = 2.6f;   
    public float cutOffTime = 4.1f; 

    [Header("Effects")]
    public ParticleSystem dustEffect;
    public XRNode controllerNode = XRNode.RightHand;
    public float vibrationStrength = 0.5f;

    // LOGIC VARIABLES
    private bool isTriggerPressed = false;
    private float contactTimer = 0f; // <--- The Magic Fix
    private bool isWindDown = false;

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
        CheckInput();
        UpdateAudio();
        UpdateEffects();
        
        // Count down the contact timer every frame
        if (contactTimer > 0)
        {
            contactTimer -= Time.deltaTime;
        }
    }

    void CheckInput()
    {
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
            isWindDown = false; 
            drillAudio.volume = 1.0f; 

            if (!drillAudio.isPlaying)
            {
                drillAudio.time = 0f; 
                drillAudio.Play();
            }

            if (drillAudio.time >= loopEnd)
            {
                drillAudio.time = loopStart;
            }
        }
        else
        {
            if (drillAudio.isPlaying)
            {
                if (!isWindDown)
                {
                    if (drillAudio.time < loopEnd) drillAudio.time = loopEnd; 
                    isWindDown = true;
                }
                if (drillAudio.time >= cutOffTime)
                {
                    drillAudio.Stop();
                    drillAudio.time = 0; 
                    isWindDown = false;
                }
            }
        }
    }

    void UpdateEffects()
    {
        // CHANGE: Check the Timer, not a boolean
        bool isDrilling = isTriggerPressed && (contactTimer > 0);

        if (isDrilling)
        {
            // Drilling State
            drillAudio.pitch = Mathf.Lerp(drillAudio.pitch, 0.9f, Time.deltaTime * 10);
            
            if (!dustEffect.isPlaying) dustEffect.Play();
            
            TriggerHaptic(vibrationStrength);
        }
        else
        {
            // Air State
            drillAudio.pitch = Mathf.Lerp(drillAudio.pitch, 1.0f, Time.deltaTime * 5);
            
            if (dustEffect.isPlaying) dustEffect.Stop();
        }
    }

    // --- COLLISION LOGIC ---
    
    // We combine Enter and Stay to keep the timer full while pushing through cubes
    void OnTriggerEnter(Collider other) 
    { 
        HandleCollision(other); 
    }

    void OnTriggerStay(Collider other)
    {
        HandleCollision(other);
    }

    void HandleCollision(Collider other)
    {
        if (other.CompareTag("Decay"))
        {
            // Reset the timer to 0.15 seconds every time we touch ANY part of the decay
            contactTimer = 0.15f; 

            if (isTriggerPressed)
            {
                Destroy(other.gameObject);
            }
        }
    }

    void TriggerHaptic(float amp)
    {
        InputDevice d = InputDevices.GetDeviceAtXRNode(controllerNode);
        if (d.isValid) d.SendHapticImpulse(0, amp, 0.1f);
    }
}