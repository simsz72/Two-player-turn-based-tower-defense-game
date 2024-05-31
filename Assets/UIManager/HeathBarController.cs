using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public RectTransform rectTransform;
    private Transform target;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public Transform GetTarget()
    {
        return target;
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + Vector3.up * 1.0f;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        float healthPercentage = currentHealth / maxHealth;

        float newWidth = Mathf.Lerp(rectTransform.sizeDelta.x, 0f, rectTransform.sizeDelta.x - healthPercentage);
        rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
    }
}