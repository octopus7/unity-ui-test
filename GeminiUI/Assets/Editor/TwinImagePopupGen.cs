using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Generated.Editor
{
    [InitializeOnLoad]
    public class TwinImagePopupGen
    {
        static TwinImagePopupGen()
        {
            EditorApplication.delayCall += RunGenerate;
        }

        private static void RunGenerate()
        {
            // 키 변경으로 재실행 유도 (v2)
            if (SessionState.GetBool("TwinImagePopupGen_Done_v2", false)) return;
            SessionState.SetBool("TwinImagePopupGen_Done_v2", true);

            Debug.Log("[TwinImagePopupGen] Start generating prefab with sprites...");

            // 리소스 로드
            Sprite duckSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Duck.png");
            Sprite gooseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Goose.png");

            if (duckSprite == null) Debug.LogError("Duck sprite not found at Assets/Textures/Duck.png");
            if (gooseSprite == null) Debug.LogError("Goose sprite not found at Assets/Textures/Goose.png");

            // 1. Root 생성
            GameObject root = new GameObject("TwinImageTestPopup");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            
            // 2. Main Script 추가
            TwinImagePopup logic = root.AddComponent<TwinImagePopup>();

            // 3. UI 구조 생성
            // Background (Dimmer)
            GameObject bgObj = CreateUIObject("Background", root);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.5f);
            Stretch(bgObj.GetComponent<RectTransform>());

            // Panel (Window)
            GameObject panelObj = CreateUIObject("Panel", root);
            Image panelImg = panelObj.AddComponent<Image>();
            panelImg.color = Color.white;
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(500, 300);

            // Left Image (Duck)
            GameObject leftObj = CreateUIObject("DuckImage", panelObj);
            Image leftImg = leftObj.AddComponent<Image>();
            leftImg.sprite = duckSprite; // Sprite 할당
            leftImg.color = Color.white; // Sprite가 있으면 흰색이어야 제대로 보임
            RectTransform leftRect = leftObj.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.2f, 0.5f);
            leftRect.anchorMax = new Vector2(0.2f, 0.5f);
            leftRect.sizeDelta = new Vector2(150, 150); // 크기 조정
            leftRect.anchoredPosition = Vector2.zero;

            // Right Image (Goose)
            GameObject rightObj = CreateUIObject("GooseImage", panelObj);
            Image rightImg = rightObj.AddComponent<Image>();
            rightImg.sprite = gooseSprite; // Sprite 할당
            rightImg.color = Color.white;
            RectTransform rightRect = rightObj.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.8f, 0.5f);
            rightRect.anchorMax = new Vector2(0.8f, 0.5f);
            rightRect.sizeDelta = new Vector2(150, 150); // 크기 조정
            rightRect.anchoredPosition = Vector2.zero;

            // Close Button
            GameObject btnObj = CreateUIObject("CloseButton", panelObj);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = Color.red;
            Button btn = btnObj.AddComponent<Button>();
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(1, 1);
            btnRect.sizeDelta = new Vector2(30, 30);
            btnRect.anchoredPosition = new Vector2(-10, -10);

            // 4. Binding
            logic.leftImage = leftImg;
            logic.rightImage = rightImg;
            logic.closeButton = btn;

            // 5. Prefab 저장
            string folder = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Prefabs");
            
            string path = "Assets/Prefabs/TwinImageTestPopup.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log($"[TwinImagePopupGen] Prefab created with sprites at: {path}");

            // 6. Cleanup
            Object.DestroyImmediate(root);
        }

        private static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
