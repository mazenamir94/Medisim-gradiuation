using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class AdminDashboardController : MonoBehaviour
{
    [Header("Dependencies")]
    public BackendClient Client; // Drag the BackendClient prefab here if needed, or we find it

    [Header("Tabs")]
    public GameObject UsersPanel;
    public GameObject ReportsPanel;

    [Header("User Tab UI")]
    public TMP_InputField SearchInput;
    public Transform UserListContent; // The Content object inside the Scroll View
    public GameObject UserRowPrefab; // Drag the UserRow prefab here

    [Header("Report Tab UI")]
    public Transform ReportListContent; // The Content object inside the Scroll View
    public GameObject ReportRowPrefab; // Drag the ReportRow prefab here

    [Header("Status")]
    public TMP_Text StatusText;

    private string _token;

    [Header("Popups")]
    public GameObject CreateUserPopup;
    public TMP_InputField CreateEmailInput;
    public TMP_InputField CreatePassInput;
    public TMP_Dropdown CreateRoleDropdown; // You'll need to create this in UI

    public GameObject EditUserPopup;
    public TMP_InputField EditEmailInput;
    public TMP_Dropdown EditRoleDropdown;
    private string _currentEditUserId; // To remember who we are editing

    private void Start()
    {
        // 1. Find Client
        if (Client == null) Client = FindFirstObjectByType<BackendClient>();

        // 2. Get Token & Role
        _token = PlayerPrefs.GetString("UserToken", "");
        string role = PlayerPrefs.GetString("UserRole", "user");

        // 3. Security Check
        if (string.IsNullOrEmpty(_token) || role != "admin")
        {
            Debug.LogError("Access Denied: Not an admin.");
            SceneManager.LoadScene("LoginScene");
            return;
        }

        Client.SetToken(_token);

        if (CreateUserPopup != null) CreateUserPopup.SetActive(false);
        if (EditUserPopup != null) EditUserPopup.SetActive(false);


        // 4. Default to Users Tab
        ShowUsersTab();
    }

    // --- TAB SWITCHING ---

    public void ShowUsersTab()
    {
        UsersPanel.SetActive(true);
        ReportsPanel.SetActive(false);
        StatusText.text = "Loading Users...";
        _ = LoadUsers(); // Fire and forget
    }

    public void ShowReportsTab()
    {
        UsersPanel.SetActive(false);
        ReportsPanel.SetActive(true);
        StatusText.text = "Loading Reports...";
        _ = LoadReports();
    }

    public void OnLogout()
    {
        PlayerPrefs.DeleteKey("UserToken");
        PlayerPrefs.DeleteKey("UserRole");
        SceneManager.LoadScene("LoginScene");
    }

    // --- LOGIC: USERS ---

    public async void OnSearchClicked()
    {
        await LoadUsers(SearchInput.text);
    }

    private async Task LoadUsers(string search = "")
    {
        ClearList(UserListContent);

        var resp = await Client.AdminGetUsers(search);

        if (!resp.ok)
        {
            StatusText.text = "Error: " + resp.error;
            return;
        }

        if (resp.users == null || resp.users.Length == 0)
        {
            StatusText.text = "No users found.";
            return;
        }

        foreach (var u in resp.users)
        {
            GameObject row = Instantiate(UserRowPrefab, UserListContent);
            AdminRowItem item = row.GetComponent<AdminRowItem>();
            item.SetupUser(u, OnDeleteUser, OpenEditPopup);
        }

        StatusText.text = $"Loaded {resp.users.Length} users.";
    }

    private async void OnDeleteUser(string userId)
    {
        StatusText.text = "Deleting...";
        var resp = await Client.AdminDeleteUser(userId);
        
        if (resp.ok)
        {
            StatusText.text = "User deleted.";
            // Refresh list
            await LoadUsers(SearchInput.text);
        }
        else
        {
            StatusText.text = "Delete failed: " + resp.error;
        }
    }

    // --- CREATE USER LOGIC ---

    public void OpenCreatePopup()
    {
        CreateUserPopup.SetActive(true);
        CreateEmailInput.text = "";
        CreatePassInput.text = "";
    }

    public void CloseCreatePopup() => CreateUserPopup.SetActive(false);

    public async void OnConfirmCreate()
    {
        StatusText.text = "Creating user...";
        string role = CreateRoleDropdown.options[CreateRoleDropdown.value].text.ToLower();
        
        var resp = await Client.AdminCreateUser(CreateEmailInput.text, CreatePassInput.text, role);
        
        if (resp.ok)
        {
            StatusText.text = "User created!";
            CloseCreatePopup();
            await LoadUsers(); // Refresh list
        }
        else
        {
            StatusText.text = "Failed: " + resp.error;
        }
    }

    // --- EDIT USER LOGIC ---

    public void OpenEditPopup(string userId, string currentEmail, string currentRole)
    {
        _currentEditUserId = userId;
        EditUserPopup.SetActive(true);
        EditEmailInput.text = currentEmail;
        
        // Set dropdown to current role
        int index = 0;
        for(int i=0; i<EditRoleDropdown.options.Count; i++) {
            if (EditRoleDropdown.options[i].text.ToLower() == currentRole.ToLower()) index = i;
        }
        EditRoleDropdown.value = index;
    }

    public void CloseEditPopup() => EditUserPopup.SetActive(false);

    public async void OnConfirmEdit()
    {
        StatusText.text = "Updating...";
        string role = EditRoleDropdown.options[EditRoleDropdown.value].text.ToLower();

        var resp = await Client.AdminUpdateUser(_currentEditUserId, EditEmailInput.text, role);

        if (resp.ok)
        {
            StatusText.text = "User updated!";
            CloseEditPopup();
            await LoadUsers();
        }
        else
        {
            StatusText.text = "Failed: " + resp.error;
        }
    }

    // --- LOGIC: REPORTS ---

    private async Task LoadReports()
    {
        ClearList(ReportListContent);

        var resp = await Client.AdminGetReports();

        if (!resp.ok)
        {
            StatusText.text = "Error: " + resp.error;
            return;
        }

        if (resp.sessions == null || resp.sessions.Length == 0)
        {
            StatusText.text = "No reports found.";
            return;
        }

        foreach (var r in resp.sessions)
        {
            GameObject row = Instantiate(ReportRowPrefab, ReportListContent);
            AdminRowItem item = row.GetComponent<AdminRowItem>();
            item.SetupReport(r);
        }

        StatusText.text = $"Loaded {resp.sessions.Length} reports.";
    }

    // --- HELPER ---
    private void ClearList(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}