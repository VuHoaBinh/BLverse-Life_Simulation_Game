using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ApiClient : MonoBehaviour
{
    private const string ApiUrl = "http://localhost:3000/api/players";  // Đảm bảo URL đúng

    // Gọi API để lấy danh sách player
    public IEnumerator GetPlayers()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(ApiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Players fetched: " + request.downloadHandler.text);
                Debug.Log("Players fetched successfully from Backend");
            }
            else
            {
                Debug.LogError("Error fetching players: " + request.error); // In lỗi nếu API không thành công
            }
        }
    }
}
