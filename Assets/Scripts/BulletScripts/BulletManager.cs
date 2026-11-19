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

    // プレイヤーの周囲を振って攻撃する剣のPrefab
    public GameObject swordPrefab;

    public Key swordAttackKey = Key.E;

    [Min(0), InspectorName("Bullet Damage Increase Per Upgrade")]
    public int bulletDamagePerUpgrade = 5;

    [Min(0), InspectorName("Attack Damage Increase Per Respawn")]
    public int attackDamageIncreasePerRespawn = 5;

    [Min(0.01f), InspectorName("Sword Base Swing Duration (seconds)")]
    public float swordSwingDuration = 0.35f;

    [Min(0f), InspectorName("Sword Speed Increase Per Upgrade (%)")]
    public float swordSpeedIncreasePerUpgrade = 20f;

    [Range(0f, 360f)]
    public float swordSwingArc = 180f;

    [Min(0f)]
    public float swordOrbitRadius = 2f;

    [Min(0.01f), InspectorName("Sword Base Size")]
    public float swordBaseSize = 1f;

    [Min(0f), InspectorName("Sword Size Increase Per Upgrade (%)")]
    public float swordSizeIncreasePerUpgrade = 20f;

    [Min(1), InspectorName("Sword Base Damage")]
    public int swordBaseDamage = 10;

    [Min(0), InspectorName("Sword Damage Increase Per Upgrade")]
    public int swordDamagePerUpgrade = 5;

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

    // Bullet Prefabのdamageを基準値として保存する
    private int bulletBaseDamage = 10;
    private SwordAttack swordAttack;
    private readonly int[] upgradedBulletsPerKey = new int[4];
    private readonly Text[] upgradeButtonLabels = new Text[5];
    private readonly Image[] upgradeButtonImages = new Image[5];
    private readonly GameObject[] upgradeButtonObjects = new GameObject[5];
    private readonly Queue<WeaponUpgradeType> queuedUpgrades = new Queue<WeaponUpgradeType>();
    private Canvas upgradeCanvas;
    private Text upgradeTitle;
    private Text upgradeHint;
    private WeaponUpgradeType currentUpgradeType;
    private int selectedUpgradeIndex;
    private int bulletDamageUpgradeLevel;
    private int swordDamageUpgradeLevel;
    private int swordSizeUpgradeLevel;
    private int swordSpeedUpgradeLevel;
    private int respawnAttackDamageBonus;
    private int killAttackDamageBonus;
    private bool isSwordUnlocked;
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

        CreateSwordAttack();
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
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;
        if (keyboard == null && gamepad == null)
        {
            return;
        }

        if (isChoosingUpgrade)
        {
            HandleUpgradeSelectionInput();
            return;
        }

        Vector2 swordFacing = Vector2.zero;
        if (keyboard != null && keyboard.wKey.wasPressedThisFrame)
        {
            SpawnBullets(upgradedBulletsPerKey[0]);
            swordFacing += Vector2.up;
        }
        if (keyboard != null && keyboard.aKey.wasPressedThisFrame)
        {
            SpawnBullets(upgradedBulletsPerKey[1]);
            swordFacing += Vector2.left;
        }
        if (keyboard != null && keyboard.sKey.wasPressedThisFrame)
        {
            SpawnBullets(upgradedBulletsPerKey[2]);
            swordFacing += Vector2.down;
        }
        if (keyboard != null && keyboard.dKey.wasPressedThisFrame)
        {
            SpawnBullets(1 + upgradedBulletsPerKey[3]);
            swordFacing += Vector2.right;
        }

        if (isSwordUnlocked && swordFacing != Vector2.zero && swordAttack != null)
        {
            BeginSwordSwing(swordFacing);
        }
        else if (isSwordUnlocked && swordAttack != null &&
            ((keyboard != null && keyboard[swordAttackKey].wasPressedThisFrame) ||
                (gamepad != null && gamepad.rightShoulder.wasPressedThisFrame)))
        {
            BeginSwordSwing(GetPlayerFacing());
        }
    }


    void HandleUpgradeSelectionInput()
    {
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;
        if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
        {
            HighlightUpgrade((selectedUpgradeIndex + 1) % GetUpgradeOptionCount());
        }
        else if (keyboard != null && keyboard.upArrowKey.wasPressedThisFrame)
        {
            HighlightUpgrade(0);
        }
        else if (keyboard != null && keyboard.leftArrowKey.wasPressedThisFrame)
        {
            HighlightUpgrade(1);
        }
        else if (keyboard != null && keyboard.downArrowKey.wasPressedThisFrame)
        {
            HighlightUpgrade(2);
        }
        else if (keyboard != null && keyboard.rightArrowKey.wasPressedThisFrame)
        {
            HighlightUpgrade(currentUpgradeType == WeaponUpgradeType.Sword ? 2 : 3);
        }
        else if ((keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            || (gamepad != null && gamepad.aButton.wasPressedThisFrame))
        {
            ConfirmUpgrade(selectedUpgradeIndex);
        }
    }


    public void OpenUpgradeSelection()
    {
        OpenUpgradeSelection(WeaponUpgradeType.Bullet);
    }


    public void OpenUpgradeSelection(WeaponUpgradeType upgradeType)
    {
        if (upgradeType == WeaponUpgradeType.Sword && !isSwordUnlocked)
        {
            isSwordUnlocked = true;
            Debug.Log("剣が解放されました。初期値で使用できます。", this);
            return;
        }

        queuedUpgrades.Enqueue(upgradeType);
        if (isChoosingUpgrade)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isChoosingUpgrade = true;
        CreateUpgradeSelectionUI();
        currentUpgradeType = queuedUpgrades.Dequeue();
        selectedUpgradeIndex = 0;
        upgradeCanvas.gameObject.SetActive(true);
        RefreshUpgradeButtonLabels();
    }


    void CreateSwordAttack()
    {
        if (swordPrefab == null)
        {
            Debug.LogWarning("BulletManager: swordPrefabが設定されていません。", this);
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("BulletManager: Playerタグのオブジェクトが見つかりません。", this);
            return;
        }

        GameObject swordObject = Instantiate(swordPrefab);
        swordAttack = swordObject.GetComponent<SwordAttack>();
        if (swordAttack == null)
        {
            swordAttack = swordObject.AddComponent<SwordAttack>();
        }

        swordAttack.Initialize(player.transform, swordBaseDamage, swordBaseSize);
        swordObject.SetActive(false);
    }


    void BeginSwordSwing(Vector2 facing)
    {
        float speedMultiplier = 1f + swordSpeedIncreasePerUpgrade * swordSpeedUpgradeLevel / 100f;
        float duration = swordSwingDuration / speedMultiplier;
        swordAttack.BeginSwing(facing, duration, swordSwingArc, swordOrbitRadius);
    }


    Vector2 GetPlayerFacing()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("BulletManager: Playerタグのオブジェクトが見つかりません。", this);
            return Vector2.down;
        }

        ObjectDirection objectDirection = player.GetComponent<ObjectDirection>();
        return objectDirection != null
            ? objectDirection.direction
            : Vector2.down;
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

        upgradeTitle = CreateLabel(
            "Title",
            "Choose a key to upgrade",
            panel.transform,
            new Vector2(0f, 130f),
            new Vector2(460f, 48f),
            28
        );

        CreateUpgradeButton(0, "W", panel.transform);
        CreateUpgradeButton(1, "A", panel.transform);
        CreateUpgradeButton(2, "S", panel.transform);
        CreateUpgradeButton(3, "D", panel.transform);
        CreateUpgradeButton(4, "Bullet Damage", panel.transform);

        upgradeHint = CreateLabel(
            "Hint",
            "Click a key or press its arrow key",
            panel.transform,
            new Vector2(0f, -150f),
            new Vector2(460f, 32f),
            18
        );
    }


    int GetUpgradeOptionCount()
    {
        return currentUpgradeType == WeaponUpgradeType.Sword ? 3 : 5;
    }


    void CreateUpgradeButton(int index, string keyLabel, Transform parent)
    {
        GameObject buttonObject = CreateUIObject(keyLabel + "UpgradeButton", parent);
        upgradeButtonObjects[index] = buttonObject;
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
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
        bool isSwordUpgrade = currentUpgradeType == WeaponUpgradeType.Sword;
        upgradeTitle.text = isSwordUpgrade
            ? "Choose a sword upgrade"
            : "Choose a bullet upgrade";
        upgradeHint.text = isSwordUpgrade
            ? "Click or use arrows / Tab, then Space"
            : "Arrows: W/A/S/D, Tab: Next, Space: Confirm";

        for (int i = 0; i < upgradeButtonLabels.Length; i++)
        {
            bool isVisible = i < GetUpgradeOptionCount();
            upgradeButtonObjects[i].SetActive(isVisible);
            upgradeButtonLabels[i].text = isSwordUpgrade
                ? GetSwordUpgradeLabel(i)
                : GetBulletUpgradeLabel(i, keyNames);
            upgradeButtonLabels[i].fontSize = isSwordUpgrade || i == 4 ? 18 : 22;

            RectTransform buttonRect = upgradeButtonObjects[i].GetComponent<RectTransform>();
            if (isSwordUpgrade)
            {
                buttonRect.anchoredPosition = i == 0
                    ? new Vector2(0f, 55f)
                    : i == 1
                        ? new Vector2(-100f, -30f)
                        : new Vector2(100f, -30f);
                buttonRect.sizeDelta = new Vector2(160f, 76f);
            }
            else
            {
                buttonRect.anchoredPosition = i == 0
                    ? new Vector2(0f, 60f)
                    : i == 1
                        ? new Vector2(-135f, -15f)
                        : i == 2
                            ? new Vector2(0f, -90f)
                            : i == 3
                                ? new Vector2(135f, -15f)
                                : new Vector2(0f, -15f);
                buttonRect.sizeDelta = i == 4
                    ? new Vector2(150f, 70f)
                    : new Vector2(110f, 64f);
            }

            upgradeButtonImages[i].color = i == selectedUpgradeIndex
                ? new Color(0.95f, 0.65f, 0.2f, 1f)
                : new Color(0.25f, 0.38f, 0.58f, 1f);
        }
    }


    string GetBulletUpgradeLabel(int index, string[] keyNames)
    {
        if (index < upgradedBulletsPerKey.Length)
        {
            return keyNames[index] + "\nBullets: " + upgradedBulletsPerKey[index];
        }

        int currentDamage = bulletBaseDamage +
            bulletDamageUpgradeLevel * bulletDamagePerUpgrade +
            respawnAttackDamageBonus +
            killAttackDamageBonus;
        return "Damage\nNow: " + currentDamage + "\n+" + bulletDamagePerUpgrade;
    }


    string GetSwordUpgradeLabel(int index)
    {
        if (index == 0)
        {
            int currentDamage = swordBaseDamage +
                swordDamageUpgradeLevel * swordDamagePerUpgrade +
                respawnAttackDamageBonus +
                killAttackDamageBonus;
            return "Damage\nNow: " + currentDamage + "\n+" + swordDamagePerUpgrade;
        }
        if (index == 1)
        {
            float currentSize = swordBaseSize *
                (1f + swordSizeUpgradeLevel * swordSizeIncreasePerUpgrade / 100f);
            return "Size\nNow: " + currentSize.ToString("0.##") + "x\n+" +
                swordSizeIncreasePerUpgrade + "%";
        }

        float currentSpeed = 100f + swordSpeedUpgradeLevel * swordSpeedIncreasePerUpgrade;
        return "Speed\nNow: " + currentSpeed.ToString("0.#") + "%\n+" +
            swordSpeedIncreasePerUpgrade + "%";
    }


    void HighlightUpgrade(int keyIndex)
    {
        if (!isChoosingUpgrade || keyIndex < 0 || keyIndex >= GetUpgradeOptionCount())
        {
            return;
        }

        selectedUpgradeIndex = keyIndex;
        RefreshUpgradeButtonLabels();
    }


    void ConfirmUpgrade(int keyIndex)
    {
        if (!isChoosingUpgrade || keyIndex < 0 || keyIndex >= GetUpgradeOptionCount())
        {
            return;
        }

        if (currentUpgradeType == WeaponUpgradeType.Bullet)
        {
            if (keyIndex < upgradedBulletsPerKey.Length)
            {
                upgradedBulletsPerKey[keyIndex]++;
            }
            else
            {
                bulletDamageUpgradeLevel++;
            }
        }
        else
        {
            if (keyIndex == 0)
            {
                swordDamageUpgradeLevel++;
            }
            else if (keyIndex == 1)
            {
                swordSizeUpgradeLevel++;
            }
            else
            {
                swordSpeedUpgradeLevel++;
            }
            ApplySwordUpgrades();
        }

        if (queuedUpgrades.Count > 0)
        {
            currentUpgradeType = queuedUpgrades.Dequeue();
            selectedUpgradeIndex = 0;
            RefreshUpgradeButtonLabels();
            return;
        }

        isChoosingUpgrade = false;
        upgradeCanvas.gameObject.SetActive(false);
        Time.timeScale = previousTimeScale;
    }


    void ApplySwordUpgrades()
    {
        if (swordAttack == null)
        {
            return;
        }

        int damage = swordBaseDamage +
            swordDamageUpgradeLevel * swordDamagePerUpgrade +
            respawnAttackDamageBonus +
            killAttackDamageBonus;
        float sizeMultiplier = swordBaseSize *
            (1f + swordSizeUpgradeLevel * swordSizeIncreasePerUpgrade / 100f);
        swordAttack.SetDamage(damage);
        swordAttack.SetSizeMultiplier(sizeMultiplier);
    }

    public void AddRespawnAttackDamage()
    {
        respawnAttackDamageBonus = (int)System.Math.Min(
            int.MaxValue,
            (long)respawnAttackDamageBonus + Mathf.Max(0, attackDamageIncreasePerRespawn));
        ApplySwordUpgrades();
    }

    public void AddKillAttackDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        killAttackDamageBonus = (int)System.Math.Min(
            int.MaxValue,
            (long)killAttackDamageBonus + amount);
        ApplySwordUpgrades();
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
            bullet.damage = Mathf.Max(
                1,
                bulletBaseDamage +
                    bulletDamageUpgradeLevel * bulletDamagePerUpgrade +
                    respawnAttackDamageBonus +
                    killAttackDamageBonus
            );
            bullet.Launch(direction, player.transform.position);
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

        if (bullets.Count == 1)
        {
            bulletBaseDamage = Mathf.Max(1, bullet.damage);
        }

        bullet.SetAlive(false);
        return bullet;
    }
}