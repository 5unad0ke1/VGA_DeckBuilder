using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DeckBuilder.Data;
using DeckBuilder.UI;

namespace DeckBuilder.EditorTools
{
    /// <summary>
    /// シーン内に自動でUI構造を構築し、コンポーネントをアタッチするスクリプト。
    /// 空のシーンでこのコンポーネントをGameObjectに追加して実行すると、簡易的なUIが自動生成されます。
    /// </summary>
    public class DeckBuilderAutoSetup : MonoBehaviour
    {
        [ContextMenu("Setup Deck Builder UI")]
        public void Setup()
        {
            // 1. マネージャーの準備
            var inventoryManager = FindFirstObjectByType<InventoryManager>() ?? new GameObject("InventoryManager").AddComponent<InventoryManager>();
            var deckBuilderUI = FindFirstObjectByType<DeckBuilderUI>() ?? inventoryManager.gameObject.AddComponent<DeckBuilderUI>();

            // 2. Canvasの作成
            var canvasObj = new GameObject("DeckBuilderCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // EventSystemの確認
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>().gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 3. 背景
            var bg = CreateUIObject("Background", canvas.transform);
            bg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f);
            Stretch(bg.GetComponent<RectTransform>());

            // 4. デッキエリア (上部)
            var deckArea = CreateUIObject("DeckArea", canvas.transform);
            var deckRT = deckArea.GetComponent<RectTransform>();
            deckRT.anchorMin = new Vector2(0, 0.7f);
            deckRT.anchorMax = new Vector2(1, 0.95f);
            deckRT.sizeDelta = Vector2.zero;
            deckArea.AddComponent<HorizontalLayoutGroup>().spacing = 20;
            deckArea.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            deckArea.GetComponent<HorizontalLayoutGroup>().childControlHeight = true;
            deckArea.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;

            DeckSlot[] slots = new DeckSlot[4];
            for (int i = 0; i < 4; i++)
            {
                var slotObj = CreateUIObject($"Slot_{i}", deckArea.transform);
                slotObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
                var slot = slotObj.AddComponent<DeckSlot>();
                slot.SetSlotIndex(i);
                slots[i] = slot;
            }

            // 5. インベントリ (中央)
            var scrollView = CreateUIObject("InventoryScrollView", canvas.transform);
            var scrollRT = scrollView.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRT.anchorMax = new Vector2(0.95f, 0.65f);
            scrollRT.sizeDelta = Vector2.zero;
            scrollView.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f);
            var scrollRect = scrollView.AddComponent<ScrollRect>();

            var viewport = CreateUIObject("Viewport", scrollView.transform);
            viewport.AddComponent<Mask>();
            viewport.AddComponent<Image>();
            Stretch(viewport.GetComponent<RectTransform>());
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            var content = CreateUIObject("Content", viewport.transform);
            var contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            var glg = content.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 250);
            glg.spacing = new Vector2(20, 20);
            glg.padding = new RectOffset(20, 20, 20, 20);
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRT;

            // 6. コスト表示 & メッセージ
            var infoArea = CreateUIObject("InfoArea", canvas.transform);
            var infoRT = infoArea.GetComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0, 0.65f);
            infoRT.anchorMax = new Vector2(1, 0.7f);
            infoRT.sizeDelta = Vector2.zero;

            var costText = CreateText("TotalCostText", infoArea.transform, "Total Cost: 0 / 10", 36);
            var statusText = CreateText("StatusText", infoArea.transform, "", 30);
            statusText.rectTransform.anchoredPosition = new Vector2(0, -40);

            // 7. 操作ボタン (下部)
            var buttonArea = CreateUIObject("ButtonArea", canvas.transform);
            var btnRT = buttonArea.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0, 0.02f);
            btnRT.anchorMax = new Vector2(1, 0.12f);
            btnRT.sizeDelta = Vector2.zero;
            var hlg = buttonArea.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 10;
            hlg.childControlWidth = true;

            var btnAcq = CreateButton("Btn_SortAcq", buttonArea.transform, "入手順");
            var btnCost = CreateButton("Btn_SortCost", buttonArea.transform, "コスト順");
            var btnHP = CreateButton("Btn_SortHP", buttonArea.transform, "HP順");
            var btnConfirm = CreateButton("Btn_Confirm", canvas.transform, "デッキ決定");
            var confirmRT = btnConfirm.GetComponent<RectTransform>();
            confirmRT.anchorMin = new Vector2(0.3f, 0.12f);
            confirmRT.anchorMax = new Vector2(0.7f, 0.14f);
            confirmRT.sizeDelta = Vector2.zero;

            // 8. キャラクタープレハブの簡易作成 (実行時に生成するための元データ)
            var charPrefab = CreateCharacterPrefab();
            charPrefab.SetActive(false); // テンプレートとして非表示に

            // 9. DeckBuilderUIへの紐付け (リフレクションを使用してプライベートフィールドをセット)
            SetPrivateField(deckBuilderUI, "inventoryContainer", content.transform);
            SetPrivateField(deckBuilderUI, "characterPrefab", charPrefab);
            SetPrivateField(deckBuilderUI, "deckSlots", slots);
            SetPrivateField(deckBuilderUI, "totalCostText", costText);
            SetPrivateField(deckBuilderUI, "statusMessageText", statusText);
            SetPrivateField(deckBuilderUI, "confirmButton", btnConfirm.GetComponent<Button>());
            SetPrivateField(deckBuilderUI, "sortAcquisitionButton", btnAcq.GetComponent<Button>());
            SetPrivateField(deckBuilderUI, "sortCostButton", btnCost.GetComponent<Button>());
            SetPrivateField(deckBuilderUI, "sortHPButton", btnHP.GetComponent<Button>());

            Debug.Log("Deck Builder UI Setup Complete! 実行ボタンを押して確認してください。");
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string text, float size)
        {
            var obj = CreateUIObject(name, parent);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            Stretch(tmp.rectTransform);
            return tmp;
        }

        private GameObject CreateButton(string name, Transform parent, string label)
        {
            var obj = CreateUIObject(name, parent);
            obj.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f);
            obj.AddComponent<Button>();
            var text = CreateText("Label", obj.transform, label, 24);
            Stretch(text.rectTransform);
            return obj;
        }

        private GameObject CreateCharacterPrefab()
        {
            var obj = new GameObject("CharacterItemTemplate");
            obj.AddComponent<RectTransform>().sizeDelta = new Vector2(200, 250);
            obj.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.45f);
            
            var icon = CreateUIObject("Icon", obj.transform);
            icon.AddComponent<Image>();
            var iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f, 0.4f);
            iconRT.anchorMax = new Vector2(0.9f, 0.9f);
            iconRT.sizeDelta = Vector2.zero;

            var costText = CreateText("CostText", obj.transform, "Cost: 0", 20);
            costText.rectTransform.anchoredPosition = new Vector2(0, -70);
            
            var hpText = CreateText("HPText", obj.transform, "HP: 0", 20);
            hpText.rectTransform.anchoredPosition = new Vector2(0, -100);

            var cg = obj.AddComponent<CanvasGroup>();
            var uiItem = obj.AddComponent<CharacterUIItem>();
            
            // UIItem内部のフィールドをセット
            SetPrivateField(uiItem, "costText", costText);
            SetPrivateField(uiItem, "hpText", hpText);
            SetPrivateField(uiItem, "iconImage", icon.GetComponent<Image>());
            SetPrivateField(uiItem, "canvasGroup", cg);

            return obj;
        }

        private void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(target, value);
        }
    }
}
