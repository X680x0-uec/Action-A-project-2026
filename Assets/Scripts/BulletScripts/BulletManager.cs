using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 弾全体を管理するためのスクリプト
public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    // ==============================
    // Inspectorから設定する項目
    // ==============================

    // 生成する弾の元となるBullet
    // ProjectにあるBulletタグ付きのPrefabをInspectorから設定する
    public GameObject bulletPrefab;

    // あらかじめ生成しておく弾の数
    public int bulletCount = 100;

    // 弾が広がる角度（Inspectorから設定可能、最大360度）
    [Range(0f, 360f)]
    public float spreadAngle = 60f;

    // 生成した弾をまとめて入れる親オブジェクト
    public Transform bulletParent;


    // ==============================
    // 弾を保存しておくリスト
    // ==============================

    // 生成した弾をすべて保存する
    private List<GameObject> bullets = new List<GameObject>();

    // 発射時に再利用する未使用弾のリスト
    private List<Bullet> availableBullets = new List<Bullet>();

    private readonly int[] upgradedBulletsPerKey = new int[4];
    private readonly Text[] upgradeButtonLabels = new Text[4];
    private readonly Image[] upgradeButtonImages = new Image[4];
    private Canvas upgradeCanvas;
    private int queuedUpgradeCount;
    private int selectedUpgradeIndex;
    private bool isChoosingUpgrade;
    private float previousTimeScale;


    // ==============================
    // ゲーム開始時に1回だけ実行する処理
    // ==============================
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1回の発射に必要な数は最低限確保する
        int poolSize = Mathf.Max(bulletCount, 1);
        for (int i = 0; i < poolSize; i++)
        {
            CreateBullet();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // ==============================
    // 毎フレーム実行する処理
    // ==============================
    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (isChoosingUpgrade)
        {
            HandleUpgradeSelectionInput();
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            SpawnBullets(upgradedBulletsPerKey[0]);
        }
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            SpawnBullets(upgradedBulletsPerKey[1]);
        }
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            SpawnBullets(upgradedBulletsPerKey[2]);
        }
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            SpawnBullets(1 + upgradedBulletsPerKey[3]);
        }
    }


    void HandleUpgradeSelectionInput()
    {
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            HighlightUpgrade(0);
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            HighlightUpgrade(1);
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            HighlightUpgrade(2);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            HighlightUpgrade(3);
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ConfirmUpgrade(selectedUpgradeIndex);
        }
    }


    public void OpenUpgradeSelection()
    {
        queuedUpgradeCount++;
        if (isChoosingUpgrade)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isChoosingUpgrade = true;
        CreateUpgradeSelectionUI();
        upgradeCanvas.gameObject.SetActive(true);
        RefreshUpgradeButtonLabels();
    }


    void CreateUpgradeSelectionUI()
    {
        if (upgradeCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject(
            "UpgradeSelectionCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );
        upgradeCanvas = canvasObject.GetComponent<Canvas>();
        upgradeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        upgradeCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f);

        GameObject background = CreateUIObject("Background", canvasObject.transform);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        GameObject panel = CreateUIObject("UpgradePanel", background.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 360f);
        panel.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 1f);

        CreateLabel(
            "Title",
            "Choose a key to upgrade",
            panel.transform,
            new Vector2(0f, 130f),
            new Vector2(460f, 48f),
            28
        );

        CreateUpgradeButton(0, "W", panel.transform, new Vector2(0f, 60f));
        CreateUpgradeButton(1, "A", panel.transform, new Vector2(-135f, -15f));
        CreateUpgradeButton(2, "S", panel.transform, new Vector2(0f, -90f));
        CreateUpgradeButton(3, "D", panel.transform, new Vector2(135f, -15f));

        CreateLabel(
            "Hint",
            "Click a key or press its arrow key",
            panel.transform,
            new Vector2(0f, -150f),
            new Vector2(460f, 32f),
            18
        );
    }


    void CreateUpgradeButton(int index, string keyLabel, Transform parent, Vector2 position)
    {
        GameObject buttonObject = CreateUIObject(keyLabel + "UpgradeButton", parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(110f, 64f);

        Image image = buttonObject.AddComponent<Image>();
        upgradeButtonImages[index] = image;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        int selectedIndex = index;
        button.onClick.AddListener(() => ConfirmUpgrade(selectedIndex));

        EventTrigger eventTrigger = buttonObject.AddComponent<EventTrigger>();
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener(_ => HighlightUpgrade(selectedIndex));
        eventTrigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener(_ => RefreshUpgradeButtonLabels());
        eventTrigger.triggers.Add(pointerExit);

        upgradeButtonLabels[index] = CreateLabel(
            "Label",
            keyLabel,
            buttonObject.transform,
            Vector2.zero,
            new Vector2(110f, 64f),
            22
        );
    }


    Text CreateLabel(
        string objectName,
        string label,
        Transform parent,
        Vector2 position,
        Vector2 size,
        int fontSize
    )
    {
        GameObject labelObject = CreateUIObject(objectName, parent);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = position;
        labelRect.sizeDelta = size;

        Text text = labelObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = label;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }


    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }


    void RefreshUpgradeButtonLabels()
    {
        string[] keyNames = { "W", "A", "S", "D" };
        for (int i = 0; i < upgradeButtonLabels.Length; i++)
        {
            upgradeButtonLabels[i].text =
                keyNames[i] + "\nBullets: " + upgradedBulletsPerKey[i];
            upgradeButtonImages[i].color = i == selectedUpgradeIndex
                ? new Color(0.95f, 0.65f, 0.2f, 1f)
                : new Color(0.25f, 0.38f, 0.58f, 1f);
        }
    }


    void HighlightUpgrade(int keyIndex)
    {
        if (!isChoosingUpgrade || keyIndex < 0 || keyIndex >= upgradeButtonImages.Length)
        {
            return;
        }

        selectedUpgradeIndex = keyIndex;
        RefreshUpgradeButtonLabels();
    }


    void ConfirmUpgrade(int keyIndex)
    {
        if (!isChoosingUpgrade || keyIndex < 0 || keyIndex >= upgradedBulletsPerKey.Length)
        {
            return;
        }

        upgradedBulletsPerKey[keyIndex]++;
        queuedUpgradeCount--;

        if (queuedUpgradeCount > 0)
        {
            RefreshUpgradeButtonLabels();
            return;
        }

        isChoosingUpgrade = false;
        upgradeCanvas.gameObject.SetActive(false);
        Time.timeScale = previousTimeScale;
    }


    // ==============================
    // Bulletを散弾状に発射する処理
    // ==============================
    void SpawnBullets(int requestedShotCount)
    {
        if (requestedShotCount <= 0)
        {
            return;
        }

        // Playerタグを持つGameObjectを探す
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Playerが見つからなかった場合は処理を終了する
        if (player == null)
        {
            return;
        }

        ObjectDirection objectDirection = player.GetComponent<ObjectDirection>();
        Vector2 baseDirection = objectDirection != null
            ? objectDirection.direction.normalized
            : Vector2.down;
        int shotCount = requestedShotCount;
        float totalSpread = Mathf.Clamp(spreadAngle, 0f, 360f);
        availableBullets.Clear();

        // リストに入っているBulletを1個ずつ確認する
        foreach (GameObject bulletObject in bullets)
        {
            // Bulletスクリプトを取得する
            Bullet bullet = bulletObject.GetComponent<Bullet>();

            // Bulletスクリプトが見つからなかった場合は次のBulletを調べる
            if (bullet == null)
            {
                continue;
            }

            // このBulletが死んでいるか確認する
            if (!bullet.isAlive)
            {
                availableBullets.Add(bullet);
            }
        }

        while (availableBullets.Count < shotCount)
        {
            Bullet bullet = CreateBullet();
            if (bullet == null)
            {
                break;
            }
            availableBullets.Add(bullet);
        }

        int spawnedCount = Mathf.Min(shotCount, availableBullets.Count);
        for (int i = 0; i < spawnedCount; i++)
        {
            // 発射する弾を扇状に均等配置する
            float angle;
            if (spawnedCount == 1)
            {
                angle = 0f;
            }
            else if (totalSpread >= 360f)
            {
                // 全方位では端の弾を重複させず、向いている方向から一周に配置する
                angle = 360f * i / spawnedCount;
            }
            else
            {
                angle = -totalSpread / 2f + totalSpread * i / (spawnedCount - 1);
            }
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * baseDirection;

            Bullet bullet = availableBullets[i];
            bullet.transform.position = player.transform.position;
            bullet.Launch(direction);
        }
    }


    Bullet CreateBullet()
    {
        GameObject bulletObject = Instantiate(bulletPrefab, bulletParent);
        bullets.Add(bulletObject);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet == null)
        {
            Debug.LogError("Bullet PrefabにBulletスクリプトが設定されていません。", bulletObject);
            return null;
        }

        bullet.isAlive = false;
        return bullet;
    }
}