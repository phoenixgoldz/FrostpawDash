using UnityEngine;

public class ButtonWobble : MonoBehaviour
{
    public float wobbleSpeed = 2f;
    public float wobbleAngle = 5f;

    private RectTransform rectTransform;
    private float originalZ;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalZ = rectTransform.eulerAngles.z;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAngle;
        rectTransform.rotation = Quaternion.Euler(0, 0, originalZ + angle);
    }
}
