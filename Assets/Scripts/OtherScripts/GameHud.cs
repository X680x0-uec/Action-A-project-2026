using UnityEngine;

[DisallowMultipleComponent]
public class GameHud : MonoBehaviour
{
    public const int ClearScore = 10000;

    public static GameHud Instance { get; private set; }

    private int currentScore;
    private long bulletDamage;
    private long swordDamage;
    private float elapsedTime;
    private bool isGameClear;
    private Health playerHealth;

    private GUIStyle leftStyle;
    private GUIStyle clearStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateGameHud()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject hudObject = new GameObject("GameHud");
        hudObject.AddComponent<GameHud>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (!isGameClear)
        {
            elapsedTime += Time.deltaTime;
        }

        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }
    }

    public void AddScore(int amount)
    {
        if (isGameClear || amount <= 0)
        {
            return;
        }

        currentScore = Mathf.Min(ClearScore, currentScore + amount);
        if (currentScore >= ClearScore)
        {
            isGameClear = true;
            Time.timeScale = 0f;
        }
    }

    public void AddBulletDamage(int amount)
    {
        if (amount > 0)
        {
            bulletDamage = AddSaturating(bulletDamage, amount);
        }
    }

    public void AddSwordDamage(int amount)
    {
        if (amount > 0)
        {
            swordDamage = AddSaturating(swordDamage, amount);
        }
    }

    void OnGUI()
    {
        if (leftStyle == null)
        {
            leftStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            leftStyle.normal.textColor = Color.white;
        }

        int respawnCount = playerHealth != null ? playerHealth.RespawnCount : 0;
        string elapsed = FormatElapsedTime(elapsedTime);
        string score = $"{currentScore}/{ClearScore}";
        float hudWidth = Mathf.Max(260f, Screen.width - 32f);

        GUI.Label(new Rect(16f, 12f, hudWidth, 32f), "Respawns: " + respawnCount, leftStyle);
        GUI.Label(new Rect(16f, 44f, hudWidth, 32f), "Time: " + elapsed, leftStyle);
        GUI.Label(new Rect(16f, 76f, hudWidth, 32f), "Score: " + score, leftStyle);
        GUI.Label(new Rect(16f, 108f, hudWidth, 32f), "Bullet Damage: " + bulletDamage, leftStyle);
        GUI.Label(new Rect(16f, 140f, hudWidth, 32f), "Sword Damage: " + swordDamage, leftStyle);

        if (!isGameClear)
        {
            return;
        }

        if (clearStyle == null)
        {
            clearStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48,
                fontStyle = FontStyle.Bold
            };
            clearStyle.normal.textColor = Color.white;
        }

        GUI.Label(
            new Rect(0f, Screen.height * 0.5f - 36f, Screen.width, 72f),
            "Game Clear!",
            clearStyle);
    }

    static long AddSaturating(long total, int amount)
    {
        if (total > long.MaxValue - amount)
        {
            return long.MaxValue;
        }

        return total + amount;
    }

    static string FormatElapsedTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int hours = totalSeconds / 3600;
        int minutes = totalSeconds / 60 % 60;
        int remainingSeconds = totalSeconds % 60;

        return hours > 0
            ? string.Format("{0}:{1:00}:{2:00}", hours, minutes, remainingSeconds)
            : string.Format("{0:00}:{1:00}", minutes, remainingSeconds);
    }
}
