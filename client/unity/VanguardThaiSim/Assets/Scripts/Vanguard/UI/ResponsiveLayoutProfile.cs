using UnityEngine;
using UnityEngine.UI;

namespace VanguardThaiSim.UI
{
    public enum ResponsiveDeviceClass
    {
        PhonePortrait,
        PhoneLandscape,
        Tablet,
        Desktop
    }

    public readonly struct ResponsiveLayoutProfile
    {
        public ResponsiveLayoutProfile(
            ResponsiveDeviceClass deviceClass,
            Vector2 referenceResolution,
            float canvasMatchWidthOrHeight,
            float toolbarHeight,
            float playToolbarHeight,
            float touchTargetHeight,
            float controlWidthScale,
            int cardGridColumns,
            Vector2 cardGridCellSize,
            Vector2 cardTileImageSize,
            float detailPanelWidth,
            float deckPanelWidth,
            float detailImageWidth,
            float playSidePanelWidth,
            float playFieldRowHeight,
            float playResourceRowHeight,
            float playHandHeight)
        {
            DeviceClass = deviceClass;
            ReferenceResolution = referenceResolution;
            CanvasMatchWidthOrHeight = canvasMatchWidthOrHeight;
            ToolbarHeight = toolbarHeight;
            PlayToolbarHeight = playToolbarHeight;
            TouchTargetHeight = touchTargetHeight;
            ControlWidthScale = controlWidthScale;
            CardGridColumns = cardGridColumns;
            CardGridCellSize = cardGridCellSize;
            CardTileImageSize = cardTileImageSize;
            DetailPanelWidth = detailPanelWidth;
            DeckPanelWidth = deckPanelWidth;
            DetailImageWidth = detailImageWidth;
            PlaySidePanelWidth = playSidePanelWidth;
            PlayFieldRowHeight = playFieldRowHeight;
            PlayResourceRowHeight = playResourceRowHeight;
            PlayHandHeight = playHandHeight;
        }

        public ResponsiveDeviceClass DeviceClass { get; }

        public Vector2 ReferenceResolution { get; }

        public float CanvasMatchWidthOrHeight { get; }

        public float ToolbarHeight { get; }

        public float PlayToolbarHeight { get; }

        public float TouchTargetHeight { get; }

        public float ControlWidthScale { get; }

        public int CardGridColumns { get; }

        public Vector2 CardGridCellSize { get; }

        public Vector2 CardTileImageSize { get; }

        public float DetailPanelWidth { get; }

        public float DeckPanelWidth { get; }

        public float DetailImageWidth { get; }

        public float PlaySidePanelWidth { get; }

        public float PlayFieldRowHeight { get; }

        public float PlayResourceRowHeight { get; }

        public float PlayHandHeight { get; }

        public bool IsCompact
        {
            get { return DeviceClass == ResponsiveDeviceClass.PhonePortrait || DeviceClass == ResponsiveDeviceClass.PhoneLandscape; }
        }

        public float ScaleControlWidth(float baseWidth)
        {
            return Mathf.Max(32f, Mathf.Round(baseWidth * ControlWidthScale));
        }

        public static ResponsiveLayoutProfile ForScreen(float width, float height)
        {
            if (width <= 0f || height <= 0f)
            {
                width = 1280f;
                height = 720f;
            }

            bool portrait = height >= width;
            if (portrait && width < 740f)
            {
                return new ResponsiveLayoutProfile(
                    ResponsiveDeviceClass.PhonePortrait,
                    new Vector2(720f, 1280f),
                    0f,
                    112f,
                    124f,
                    48f,
                    0.50f,
                    2,
                    new Vector2(118f, 188f),
                    new Vector2(96f, 134f),
                    190f,
                    210f,
                    132f,
                    150f,
                    270f,
                    68f,
                    128f);
            }

            if (!portrait && height < 560f)
            {
                return new ResponsiveLayoutProfile(
                    ResponsiveDeviceClass.PhoneLandscape,
                    new Vector2(1280f, 720f),
                    1f,
                    88f,
                    96f,
                    48f,
                    0.72f,
                    3,
                    new Vector2(122f, 190f),
                    new Vector2(98f, 136f),
                    240f,
                    240f,
                    170f,
                    156f,
                    280f,
                    68f,
                    128f);
            }

            if (width < 1180f)
            {
                return new ResponsiveLayoutProfile(
                    ResponsiveDeviceClass.Tablet,
                    new Vector2(1180f, 760f),
                    0.5f,
                    82f,
                    82f,
                    48f,
                    0.86f,
                    3,
                    new Vector2(132f, 204f),
                    new Vector2(112f, 154f),
                    300f,
                    280f,
                    218f,
                    160f,
                    300f,
                    68f,
                    140f);
            }

            return new ResponsiveLayoutProfile(
                ResponsiveDeviceClass.Desktop,
                new Vector2(1280f, 720f),
                0.5f,
                74f,
                64f,
                42f,
                0.92f,
                4,
                new Vector2(132f, 204f),
                new Vector2(112f, 154f),
                340f,
                300f,
                250f,
                168f,
                330f,
                68f,
                140f);
        }
    }

    internal sealed class ResponsiveLayoutBinding
    {
        private readonly LayoutElement layout;
        private readonly float baseWidth;
        private readonly float baseHeight;
        private readonly bool scaleWidth;
        private readonly bool touchHeight;

        public ResponsiveLayoutBinding(LayoutElement layout, bool scaleWidth, bool touchHeight)
        {
            this.layout = layout;
            this.baseWidth = layout.preferredWidth;
            this.baseHeight = layout.preferredHeight;
            this.scaleWidth = scaleWidth;
            this.touchHeight = touchHeight;
        }

        public bool IsAlive
        {
            get { return layout != null; }
        }

        public void Apply(ResponsiveLayoutProfile profile)
        {
            if (layout == null)
            {
                return;
            }

            if (scaleWidth && baseWidth > 0f)
            {
                layout.preferredWidth = profile.ScaleControlWidth(baseWidth);
            }

            if (touchHeight)
            {
                layout.preferredHeight = Mathf.Max(baseHeight, profile.TouchTargetHeight);
            }
        }
    }
}
