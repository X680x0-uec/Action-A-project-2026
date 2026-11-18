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

    void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        if (slider == null)
        {
            Debug.LogError("HealthBar requires a Slider component.", this);
            enabled = false;
            return;
        }

        if (fillImage == null && slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
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

        health.HealthChanged += Refresh;
        health.Depleted += OnDepleted;
        slider.minValue = 0;
        slider.maxValue = health.MaxHP;
        slider.wholeNumbers = true;
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
        slider.maxValue = target.MaxHP;
        slider.value = target.CurrentHP;

        if (fillImage != null)
        {
            float ratio = target.MaxHP > 0 ? (float)target.CurrentHP / target.MaxHP : 0f;
            fillImage.color = Color.Lerp(emptyHealthColor, fullHealthColor, ratio);
        }
    }

    void OnDepleted(Health target)
    {
        if (target.CompareTag("Player"))
        {
            Debug.Log("Player HP reached zero.", target);
        }
    }
}
