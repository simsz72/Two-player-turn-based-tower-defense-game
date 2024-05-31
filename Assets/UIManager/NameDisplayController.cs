using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameDisplayController : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    private Transform target;

    void Start()
    {
        nameText = GetComponent<TextMeshProUGUI>();
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
            transform.position = target.position + Vector3.up * 1.2f;
        }
    }
}