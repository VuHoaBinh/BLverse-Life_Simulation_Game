using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private ApiClient apiClient;

    void Start()
    {
        ApiClient apiClient = gameObject.AddComponent<ApiClient>();
        StartCoroutine(apiClient.GetPlayers()); 
    }
}
