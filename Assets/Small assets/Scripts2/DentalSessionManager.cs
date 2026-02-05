using UnityEngine;
using System.IO;

public class DentalSessionManager : MonoBehaviour
{
    [Header("Optimization References")]
     // Drag 'FullRoom_Container' here
    public GameObject resultsUI;       // Drag your 'AI Chat' Canvas here
    
    [Header("Components")]
    public LLMConnector llmConnector; // Drag your LLMConnector script here
    
    [Header("Tools (Optional: For reliable connection)")]
    public DrillTip drillTool;      // Drag your Drill Tip here
    public FillingTip fillingTool;  // Drag your Filling Tip here

    [Header("Session Settings")]
    public string currentProcedureName = "ClassI_Cavity"; // Matches JSON Key

    // --- JSON DATA STRUCTURES ---
    [System.Serializable]
    public class Wrapper { public KnowledgeBase procedures; }

    [System.Serializable]
    public class KnowledgeBase {
        public ProcedureData ClassI_Cavity; 
        public ProcedureData Amalgam_Filling;
    }

    [System.Serializable]
    public class ProcedureData {
        public float min_accuracy;
        public float min_time_seconds;
        public BookRef book_reference;
    }

    [System.Serializable]
    public class BookRef {
        public string title;
        public string chapter;
        public string page; // Added Page for better citation
        public string quote; // Paste your book text here in the JSON!
    }
    
    private KnowledgeBase database;
    // ----------------------------

    // --- SCORING VARIABLES ---
    private int removedDecay = 0;
    private int totalDecayVoxels = 0; 
    private int gumHits = 0;
    private int restoredVoxels = 0;
    private int overfillHits = 0;
    public bool isSessionActive = false;
    
    // Time Tracking
    public float sessionTime = 0f;

    void Start()
    {
        StartSession();
        LoadDentalProcedures();
    }

    void LoadDentalProcedures()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "DentalProcedures.json");
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            // Handle the nested "procedures" key
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(jsonContent);
            if (wrapper != null) database = wrapper.procedures;
            Debug.Log("Dental Knowledge Loaded from: " + path);
        }
        else
        {
            Debug.LogError("Could not find dental book at: " + path);
        }
    }

    void Update()
    {
        if (isSessionActive)
        {
            sessionTime += Time.deltaTime;
        }
    }

    public void StartSession()
    {
        isSessionActive = true;
        sessionTime = 0f;
        removedDecay = 0;
        gumHits = 0;
        restoredVoxels = 0;
        overfillHits = 0;
        
        // AUTO-CONNECT TOOLS
        // If you dragged them in Inspector, we tell them "Hey, I am your Manager!"
        if (drillTool != null) drillTool.dentalManager = this;
        if (fillingTool != null) fillingTool.dentalManager = this;
        
        GameObject[] decayObjects = GameObject.FindGameObjectsWithTag("Decay");
        totalDecayVoxels = decayObjects.Length;
        Debug.Log($"Medisem: Session Started. Total Decay Voxels found: {totalDecayVoxels}");

        // --- XR ERROR FIX ---
        // Automatically fix the Canvas Camera to prevent "KeyNotFoundException"
        if (resultsUI != null)
        {
            Canvas canvas = resultsUI.GetComponent<Canvas>();
            if (canvas != null && canvas.worldCamera == null)
            {
                if (Camera.main != null)
                {
                    canvas.worldCamera = Camera.main;
                    Debug.Log("Medisem: Fixed Results UI Canvas Camera.");
                }
            }
        }
    }

    // --- EVENT HANDLERS (Called by Drill/Syringe) ---
    public void OnDecayRemoved(int amount)
    {
        if (!isSessionActive) return;
        removedDecay += amount;
        if (removedDecay % 50 == 0) Debug.Log($"Decay Removed: {removedDecay}/{totalDecayVoxels}");
    }

    // Default overload
    public void OnDecayRemoved() => OnDecayRemoved(1);

    public void OnGumHit()
    {
        if (!isSessionActive) return;
        gumHits++;
        Debug.LogWarning("Gum Hit!");
    }

    public void OnFillingAdded()
    {
        if (!isSessionActive) return;
        // Cap the count as requested to prevent overflow visuals
        if (restoredVoxels < 2032) 
            restoredVoxels++;
    }
    
    // Overload for compatibility
    public void OnFillingAdded(int amount) => OnFillingAdded();

    public void OnOverfillHit()
    {
        if (!isSessionActive) return;
        overfillHits++;
    }

    public void FinishSession()
    {
        Debug.Log("Medisem: Session Finished. Asking AI...");
        isSessionActive = false;

        // --- ROBUST SCORING FIX ---
        // 1. Calculate Removed Decay (What is NOT in the scene relative to start)
        GameObject[] remainingDecayObjects = GameObject.FindGameObjectsWithTag("Decay");
        int remainingDecayCount = remainingDecayObjects.Length;
        removedDecay = totalDecayVoxels - remainingDecayCount;
        if (removedDecay < 0) removedDecay = 0;

        // 2. Calculate Filled Voxels (Visual Check)
        // Since both Drill and FillingTip might use the "Filling" tag, strictly check visibility.
        // - Drill makes it "Filling" + Invisible (Hole)
        // - FillingTip makes it "Filling" + Visible (Restored)
        GameObject[] potentialFillings = GameObject.FindGameObjectsWithTag("Filling");
        restoredVoxels = 0;
        foreach (GameObject voxel in potentialFillings)
        {
            if (voxel.TryGetComponent<MeshRenderer>(out MeshRenderer renderer) && renderer.enabled)
            {
                restoredVoxels++;
            }
        }
        // Also check "Untagged" or "Filled" if user added them, just in case
        GameObject[] explicitFilled = GameObject.FindGameObjectsWithTag("Untagged"); // Old fallback
        foreach (GameObject voxel in explicitFilled)
        {
             // Only count if white? No, too complex. Just assume if it was processed it's good.
             // Actually, let's stick to the main "Filling" tag logic which is consistent with current scripts.
        }

        // 3. Score Calculations
        float drillAccuracy = 0f;
        if (totalDecayVoxels > 0)
            drillAccuracy = Mathf.Clamp(((float)removedDecay / (float)totalDecayVoxels) * 100f - (gumHits * 5f), 0, 100);
        else 
            drillAccuracy = 100f; 

        // Fill accuracy: Target matches exactly what you drilled (removedDecay)!!!
        float targetFill = (float)removedDecay;
        if (targetFill <= 0) targetFill = 1f; // Prevent division by zero

        float fillAccuracy = Mathf.Clamp(((float)restoredVoxels / targetFill) * 100f - (overfillHits * 5f), 0, 100);

        // --- GET BOOK CONTEXT ---
        string bookContext = "";
        if (database != null)
        {
            ProcedureData rules = null;
            if (currentProcedureName == "ClassI_Cavity") rules = database.ClassI_Cavity;
            else if (currentProcedureName == "Amalgam_Filling") rules = database.Amalgam_Filling; // JSON key match
            
            if (rules != null && rules.book_reference != null)
            {
                // Expanded Context for Proper Citation
                bookContext = $"[REFERENCE TEXTBOOK]\n" +
                              $"Title: {rules.book_reference.title}\n" +
                              $"Chapter: {rules.book_reference.chapter}\n" +
                              $"Page: {rules.book_reference.page}\n" +
                              $"Excerpt: {rules.book_reference.quote}";
            }
        }

        string surgerySummary = $"Student Performance Report:\n" +
                                $"Time Taken: {sessionTime:F1} seconds.\n" +
                                $"Drilling Accuracy: {drillAccuracy:F1}% ({removedDecay}/{totalDecayVoxels}).\n" +
                                $"Filling Accuracy: {fillAccuracy:F1}% ({restoredVoxels}/{removedDecay}).\n" +
                                $"Mistakes: {gumHits} gum hits, {overfillHits} overfills.\n" +
                                $"{bookContext}\n" + 
                                $"INSTRUCTION: Analyze the student based ONLY on the above Reference Textbook rules.";
        
        Debug.Log("Sending to AI: " + surgerySummary);

        if (llmConnector != null)
        {
            // Optional: Show immediate feedback on UI while waiting?
            // llmConnector.chatText.text = $"Calculating...\nDrill: {drillAccuracy:F1}%\nFill: {fillAccuracy:F1}%";
            llmConnector.SendToAI(surgerySummary);
        }

        else 
        {
            Debug.LogError("Medisem: LLMConnector is missing!");
        }
    }
}