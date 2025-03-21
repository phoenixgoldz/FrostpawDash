using UnityEngine;

public class ButtonPulse : MonoBehaviour
{
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.05f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }
}
