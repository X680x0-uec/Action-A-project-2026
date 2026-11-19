using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float damageNumberDuration = 3f;
    [SerializeField, Min(0.001f)] private float damageNumberSize = 0.1f;
    [SerializeField, Min(0f)] private float verticalOffset = 0.7f;
    [SerializeField, Min(0f)] private float riseDistance = 0.35f;

    private static EffectManager instance;

    public static EffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<EffectManager>();
                if (instance == null)
                {
                    GameObject managerObject = new GameObject(nameof(EffectManager));
                    instance = managerObject.AddComponent<EffectManager>();
                }
            }

            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void ShowDamageNumber(Health target, int amount)
    {
        if (target == null || amount <= 0)
        {
            return;
        }

        GameObject numberObject = new GameObject("Damage Number");
        Vector3 startPosition = target.transform.position + Vector3.up * verticalOffset;
        numberObject.transform.position = startPosition;

        TextMesh textMesh = numberObject.AddComponent<TextMesh>();
        textMesh.text = amount.ToString();
        textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textMesh.fontSize = 48;
        textMesh.characterSize = damageNumberSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = target.CompareTag("Player") ? Color.red : Color.yellow;

        MeshRenderer meshRenderer = numberObject.GetComponent<MeshRenderer>();
        SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
        if (targetRenderer != null)
        {
            meshRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            meshRenderer.sortingOrder = targetRenderer.sortingOrder + 1;
        }
        else
        {
            meshRenderer.sortingOrder = 100;
        }

        StartCoroutine(AnimateDamageNumber(numberObject, textMesh, startPosition));
    }

    IEnumerator AnimateDamageNumber(GameObject numberObject, TextMesh textMesh, Vector3 startPosition)
    {
        float duration = Mathf.Max(0.01f, damageNumberDuration);
        float elapsed = 0f;
        Color startColor = textMesh.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            numberObject.transform.position = startPosition + Vector3.up * (riseDistance * progress);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 1f - progress);
            yield return null;
        }

        Destroy(numberObject);
    }
}
