using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaController : MonoBehaviour
{
    private RectTransform panelSafeArea;
    private Rect currentSafeArea = new Rect(0, 0, 0, 0);
    private ScreenOrientation currentOrientation = ScreenOrientation.AutoRotation;

    void Awake()
    {
        panelSafeArea = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        if (currentSafeArea != Screen.safeArea || currentOrientation != Screen.orientation)
            ApplySafeArea();
    }

    void ApplySafeArea()
    {
        currentSafeArea = Screen.safeArea;
        currentOrientation = Screen.orientation;

        Vector2 anchorMin = currentSafeArea.position;
        Vector2 anchorMax = currentSafeArea.position + currentSafeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panelSafeArea.anchorMin = anchorMin;
        panelSafeArea.anchorMax = anchorMax;

        Debug.Log($"✅ SafeArea applied: {Screen.safeArea}");
    }
}
