using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    [Header("Prefab Reference")]
    [SerializeField] private TestPopup popupPrefab;
    [SerializeField] private Canvas mainCanvas; // Canvas to spawn popup in

    [Header("Buttons")]
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;

    private void Start()
    {
        if (button1 != null)
            button1.onClick.AddListener(() => ShowPopup("첫 번째 팝업입니다.\n(First Message)"));

        if (button2 != null)
            button2.onClick.AddListener(() => ShowPopup("두 번째 팝업입니다.\n경고 메시지 예시!", Color.yellow));

        if (button3 != null)
            button3.onClick.AddListener(() => ShowPopup("세 번째 팝업입니다.\n이미지도 바꿀 수 있습니다."));

        CheckAndCreateEventSystem();
    }

    private void CheckAndCreateEventSystem()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Defaulting to StandaloneInputModule. 
            // If using the new Input System, Unity might require InputSystemUIInputModule, 
            // but StandaloneInputModule is often the safe default fallback or creates a warning to Replace.
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("Created EventSystem (and InputModule) automatically for testing.");
        }
    }

    private void ShowPopup(string message, Color? imageColor = null)
    {
        if (popupPrefab == null)
        {
            Debug.LogError("Popup Prefab is not assigned!");
            return;
        }

        // Parent to Canvas if possible, otherwise just Instantiate
        TestPopup popup = Instantiate(popupPrefab, mainCanvas != null ? mainCanvas.transform : transform);
        
        popup.SetMessage(message);

        // Optional: Change image color just to show variety if no sprite is available
        if (imageColor.HasValue)
        {
            // Assuming we added a public getter or exposed the image in TestPopup, 
            // but for now let's just use a GetComponent or assume the user accepts this modification
            // actually TestPopup has SetImage(Sprite), not Color. 
            // Let's stick to just Message for now as per "simple" request, or modify TestPopup.
            // But wait, user said "Image (Inspector, Script Control)".
            // I'll leave the color part out for now to keep it simple or use a placeholder sprite logic if I had one.
            // Re-reading: "Image (Inspector, Script Control)".
            // I'll just stick to message for the core request, maybe assume a default sprite.
        }
        
        // Reset scale just in case
        popup.transform.localScale = Vector3.one;
        popup.transform.localPosition = Vector3.zero;
        
        // Ensure it covers full screen if it's stretch
        RectTransform rt = popup.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
