using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [System.Serializable]
    public struct SceneSpawnPoint
    {
        public string sceneName;
        public Vector3 playerPosition;
        public Vector3 cameraPosition;
    }

    [Header("References")]
    public Transform player;
    public Transform mainCamera;

    [Header("Spawn Points")]
    public SceneSpawnPoint[] spawnPoints;

    private void Start()
    {
        string previousScene = PlayerPrefs.GetString("LastScene");

        foreach (var spawnPoint in spawnPoints)
        {
            if (previousScene == spawnPoint.sceneName)
            {
                if (player != null)
                    player.position = spawnPoint.playerPosition;

                if (mainCamera != null)
                    mainCamera.position = spawnPoint.cameraPosition;

                break;
            }
        }

        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
    }
}