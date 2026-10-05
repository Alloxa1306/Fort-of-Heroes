using UnityEngine;

public class ConfigSelector : MonoBehaviour
{
    [SerializeField] private ScriptableObject selectedConfig;

    public ScriptableObject SelectedConfig => selectedConfig;
}