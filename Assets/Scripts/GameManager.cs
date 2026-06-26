using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    PlayerManager playerRef;

    [Header("Coin Model")]
    public Transform coinModel;
    public float coinSpinSpeed = 180f;

    void Start()
    {
        playerRef = FindFirstObjectByType<PlayerManager>();
    }

    void Update()
    {
        if (coinModel != null)
            coinModel.Rotate(Vector3.up, coinSpinSpeed * Time.deltaTime);
    }

    void Awake()
    {
        playerRef = FindFirstObjectByType<PlayerManager>();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public T GetComponentInPlayer<T>() where T : Component
    {
        if (playerRef != null)
        {
            return playerRef.GetComponent<T>();
        }
        return null;
    }
}
