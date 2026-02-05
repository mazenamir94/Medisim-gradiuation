using UnityEngine;

[CreateAssetMenu(menuName = "DentalTutor/Backend Config")]
public class BackendConfig : ScriptableObject
{
    [Header("Example: http://localhost:3001")]
    public string BaseUrl = "http://localhost:3001";
}
