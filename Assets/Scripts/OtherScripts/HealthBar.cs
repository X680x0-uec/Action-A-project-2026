using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color emptyHealthColor = Color.red;
    [SerializeField] private bool followTarget;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f);

    private RectTransform fillRect;

    void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        if (fillImage == null && slider != null && slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }

        if (slider != null)
        {
            slider.interactable = false;
            if (slider.handleRect != null)
            {
                slider.handleRect.gameObject.SetActive(false);
            }

            slider.enabled = false;
        }

        if (fillImage == null)
        {
            Debug.LogError("HealthBar requires a fill Image.", this);
            enabled = false;
            return;
        }

        if (health == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                health = player.GetComponent<Health>();
            }
        }

        if (health == null)
        {
            Debug.LogError("HealthBar could not find a Health component to display.", this);
            enabled = false;
            return;
        }

        fillRect = fillImage.rectTransform;
        fillImage.sprite = null;
        fillImage.type = Image.Type.Simple;
        health.HealthChanged += Refresh;
        health.Depleted += OnDepleted;
        Refresh(health);
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.HealthChanged -= Refresh;
            health.Depleted -= OnDepleted;
        }
    }

    void LateUpdate()
    {
        if (!followTarget || health == null || Camera.main == null)
        {
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(health.transform.position + worldOffset);
    }

    void Refresh(Health target)
    {
        float ratio = target.MaxHP > 0 ? (float)target.CurrentHP / target.MaxHP : 0f;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(ratio, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        fillImage.color = Color.Lerp(emptyHealthColor, fullHealthColor, ratio);
    }

    void OnDepleted(Health target)
    {
        if (target.CompareTag("Player"))
        {
            Debug.Log("Player HP reached zero.", target);
        }
    }
}
