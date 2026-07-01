using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VanguardThaiSim.UI
{
    public static class ManualScreenOverlay
    {
        public static GameObject Create(Transform parent, Font font, Action onClose)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            Font resolvedFont = ResolveFont(font);
            GameObject root = CreatePanel("Manual Screen", parent, new Color(0.04f, 0.05f, 0.06f, 0.96f));
            Stretch(root.GetComponent<RectTransform>(), 0, 0, 0, 0);

            GameObject panel = CreatePanel("Manual Panel", root.transform, new Color(0.1f, 0.12f, 0.14f, 1f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Stretch(panelRect, 96, 96, 64, 64);
            VerticalLayoutGroup panelLayout = panel.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(22, 22, 18, 18);
            panelLayout.spacing = 12;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            RectTransform header = CreatePanel("Manual Header", panel.transform, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            LayoutElement headerLayout = header.gameObject.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 46;
            HorizontalLayoutGroup headerRow = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerRow.spacing = 10;
            headerRow.childAlignment = TextAnchor.MiddleLeft;
            headerRow.childControlWidth = true;
            headerRow.childControlHeight = true;
            headerRow.childForceExpandWidth = false;
            headerRow.childForceExpandHeight = false;

            Text title = CreateText("Manual Title", header, "Manual", resolvedFont, 24, TextAnchor.MiddleLeft, Color.white);
            LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1;
            titleLayout.preferredHeight = 42;

            Button closeButton = CreateButton(header, "Manual Close", "Close", resolvedFont, delegate
            {
                if (onClose != null)
                {
                    onClose();
                }
            });
            closeButton.gameObject.name = "Manual Close Button";

            IReadOnlyList<string> categories = ManualContentFilter.Categories();
            int categoryIndex = 0;
            RectTransform filterRow = CreatePanel("Manual Filter Row", panel.transform, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            LayoutElement filterRowLayout = filterRow.gameObject.AddComponent<LayoutElement>();
            filterRowLayout.preferredHeight = 44;
            HorizontalLayoutGroup filterLayout = filterRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            filterLayout.spacing = 10;
            filterLayout.childAlignment = TextAnchor.MiddleLeft;
            filterLayout.childControlWidth = true;
            filterLayout.childControlHeight = true;
            filterLayout.childForceExpandWidth = false;
            filterLayout.childForceExpandHeight = false;

            InputField searchInput = CreateInputField(filterRow, "Manual Search Input", "Search manual", resolvedFont);
            Button categoryButton = CreateButton(filterRow, "Manual Category", ManualContentFilter.FormatCategoryButtonLabel(categories[categoryIndex]), resolvedFont, delegate { });
            categoryButton.gameObject.name = "Manual Category Button";
            Text categoryButtonLabel = categoryButton.GetComponentInChildren<Text>();

            GameObject scrollObject = CreatePanel("Manual Scroll View", panel.transform, new Color(0.07f, 0.08f, 0.09f, 1f));
            LayoutElement scrollLayout = scrollObject.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1;
            scrollLayout.minHeight = 360;
            scrollLayout.preferredHeight = 580;
            ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();

            GameObject viewport = CreatePanel("Manual Viewport", scrollObject.transform, new Color(0, 0, 0, 0));
            Stretch(viewport.GetComponent<RectTransform>(), 10, 10, 10, 10);
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject content = new GameObject("Manual Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = new Vector2(0, -1600);
            contentRect.offsetMax = Vector2.zero;

            Text body = CreateText(
                "Manual Body",
                content.transform,
                string.Empty,
                resolvedFont,
                15,
                TextAnchor.UpperLeft,
                new Color(0.88f, 0.9f, 0.94f, 1f));
            body.lineSpacing = 1.08f;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            Stretch(body.rectTransform, 2, 2, 0, 0);

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            Action refreshBody = delegate
            {
                string category = categories[categoryIndex];
                if (categoryButtonLabel != null)
                {
                    categoryButtonLabel.text = ManualContentFilter.FormatCategoryButtonLabel(category);
                }

                body.text = ManualContentFilter.FormatSections(
                    ManualContentFilter.Filter(searchInput.text, category));
            };
            searchInput.onValueChanged.AddListener(delegate { refreshBody(); });
            categoryButton.onClick.AddListener(delegate
            {
                categoryIndex = (categoryIndex + 1) % categories.Count;
                refreshBody();
            });
            refreshBody();

            return root;
        }

        private static InputField CreateInputField(Transform parent, string name, string placeholder, Font font)
        {
            GameObject root = CreatePanel(name, parent, new Color(0.16f, 0.18f, 0.2f, 1f));
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = 360;
            layout.preferredHeight = 42;

            InputField input = root.AddComponent<InputField>();
            Text text = CreateText("Text", root.transform, string.Empty, font, 15, TextAnchor.MiddleLeft, Color.white);
            Stretch(text.rectTransform, 12, 8, 3, 3);
            Text placeholderText = CreateText(
                "Placeholder",
                root.transform,
                placeholder,
                font,
                15,
                TextAnchor.MiddleLeft,
                new Color(0.62f, 0.64f, 0.68f, 1f));
            Stretch(placeholderText.rectTransform, 12, 8, 3, 3);
            input.textComponent = text;
            input.placeholder = placeholderText;
            input.lineType = InputField.LineType.SingleLine;
            return input;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Font font,
            UnityEngine.Events.UnityAction action)
        {
            GameObject root = CreatePanel(name + " Button", parent, new Color(0.78f, 0.24f, 0.64f, 1f));
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = 108;
            layout.preferredHeight = 42;
            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(action);
            Text text = CreateText("Label", root.transform, label, font, 15, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 6, 6, 3, 3);
            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            Font font,
            int size,
            TextAnchor alignment,
            Color color)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            RectTransform rect = root.AddComponent<RectTransform>();
            Text text = root.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            return text;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.AddComponent<RectTransform>();
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Font ResolveFont(Font font)
        {
            if (font != null)
            {
                return font;
            }

            Font loaded = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loaded == null)
            {
                loaded = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return loaded;
        }

        private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
