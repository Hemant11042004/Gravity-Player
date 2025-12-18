using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Timer")]
    public float timeLimit = 120f;
    private float currentTime;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;

    [Header("Collectibles")]
    public int totalCollectibles;
    private int collectedCount;

    [Header("Player")]
    public Transform player;
    [SerializeField] private GravityController gravityController;
    public float fallCheckDelay = 1.0f;

    private float fallTimer;
    private bool isGameOver;
    private float fallIgnoreTimer;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        currentTime = timeLimit;
        gameOverPanel.SetActive(false);

        if (!gravityController)
            gravityController = FindObjectOfType<GravityController>();
    }

    void Update()
    {
        if (isGameOver) return;

        UpdateTimer();
        CheckFreeFall();
    }

    // -----------------------
    // TIMER
    // -----------------------
    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(0, currentTime);

        UpdateTimerUI();

        if (currentTime <= 0f)
            GameOver("Time Up!");
    }

    void UpdateTimerUI()
    {
        if (!timerText) return;

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // -----------------------
    // COLLECTIBLES
    // -----------------------
    public void CollectItem()
    {
        collectedCount++;

        if (collectedCount >= totalCollectibles)
            WinGame();
    }

    // -----------------------
    // FREE FALL CHECK
    // -----------------------
    void CheckFreeFall()
    {
        if (fallIgnoreTimer > 0f)
        {
            fallIgnoreTimer -= Time.deltaTime;
            return;
        }

        if (!gravityController || !player) return;

        // ✅ THIS IS WHERE gravityDir BELONGS
        Vector3 gravityDir = gravityController.CurrentGravityDirection.normalized;

        Ray ray = new Ray(player.position, gravityDir);

        bool grounded = Physics.Raycast(
            ray,
            1.1f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (!grounded)
        {
            fallTimer += Time.deltaTime;

            if (fallTimer >= fallCheckDelay)
                GameOver("You Fell!");
        }
        else
        {
            fallTimer = 0f;
        }
    }

    public void IgnoreFallCheck(float duration)
    {
        fallIgnoreTimer = duration;
    }

    // -----------------------
    // GAME STATES
    // -----------------------
    void GameOver(string reason)
    {
        isGameOver = true;
        Debug.Log(reason);
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void WinGame()
    {
        isGameOver = true;
        Debug.Log("All Collectibles Collected!");
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
