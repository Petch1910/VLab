using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.Smoke;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VanguardThaiSim.UI
{
    public sealed class CardBrowserBootstrap : MonoBehaviour
    {
        private const int PageSize = 24;
        private static CardBrowserScreenMode pendingScreenMode = CardBrowserScreenMode.DeckBuilder;

        private ICardRepository repository;
        private CardPackManifest manifest;
        private CardPackValidationStatus packValidationStatus;
        private VanguardDeck activeDeck;
        private DeckValidator deckValidator;
        private DeckStorage deckStorage;
        private CardImageCache imageCache;
        private string packDirectory;
        private Font font;
        private CanvasScaler canvasScaler;
        private RectTransform canvasRect;
        private RectTransform toolbarRect;
        private RectTransform mainRect;
        private HorizontalLayoutGroup toolbarLayout;
        private HorizontalLayoutGroup mainLayout;
        private GridLayoutGroup cardGrid;
        private LayoutElement detailPanelLayout;
        private LayoutElement deckPanelLayout;
        private LayoutElement detailImageLayout;
        private LayoutElement detailActionsLayout;
        private LayoutElement deckActionsLayout;
        private ResponsiveLayoutProfile activeLayoutProfile;
        private ResponsiveDeviceClass activeDeviceClass;
        private bool hasResponsiveProfile;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;
        private float cardTileImageWidth = 112f;
        private float cardTileImageHeight = 154f;
        private float cardTileNameWidth = 116f;

        private InputField searchInput;
        private Dropdown seriesDropdown;
        private Dropdown clanDropdown;
        private Text statusText;
        private Text pageText;
        private RectTransform gridContent;
        private RawImage detailImage;
        private AspectRatioFitter detailImageAspect;
        private Text screenSummaryText;
        private Text detailTitle;
        private Text detailBody;
        private Texture2D detailTexture;
        private CardDetail selectedDetail;
        private Text deckStatusText;
        private Text deckRuleBadgeText;
        private Text deckIssuesText;
        private Text deckListText;
        private Text deckMessageText;
        private GameObject deckToolsDialog;
        private GameObject deckAccessoriesDialog;
        private InputField deckCodeInput;
        private Text deckToolsStatusText;
        private Text deckAccessoriesStatusText;

        private readonly List<string> seriesValues = new List<string>();
        private readonly List<CardTaxonomyFilterOption> taxonomyValues = new List<CardTaxonomyFilterOption>();
        private readonly List<ResponsiveLayoutBinding> responsiveBindings = new List<ResponsiveLayoutBinding>();
        private int currentPage;
        private IReadOnlyList<CardSummary> currentCards = Array.Empty<CardSummary>();
        private string repositoryLoadError;
        private CardBrowserScreenMode screenMode;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnLoad()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (PlayerSmokeFlowBootstrap.IsSmokeRequested(args) ||
                VisualEvidenceBootstrap.IsVisualEvidenceRequested(args))
            {
                return;
            }

            HomeLobbyBootstrap.Show();
        }

        public static void Show()
        {
            ShowDeckBuilder();
        }

        public static void ShowDeckBuilder()
        {
            Show(CardBrowserScreenMode.DeckBuilder);
        }

        public static void ShowBrowser()
        {
            Show(CardBrowserScreenMode.Browser);
        }

        private static void Show(CardBrowserScreenMode mode)
        {
            if (FindAnyObjectByType<CardBrowserBootstrap>() != null)
            {
                return;
            }

            pendingScreenMode = mode;
            GameObject host = new GameObject("Vanguard Card Browser");
            DontDestroyOnLoad(host);
            host.AddComponent<CardBrowserBootstrap>();
        }

        private void Awake()
        {
            screenMode = pendingScreenMode;
        }

        private void Start()
        {
            font = LoadFont();
            EnsureEventSystem();
            BuildUi();
            if (!TryLoadRepository())
            {
                return;
            }

            CreateActiveDeck();
            PopulateFilters();
            RefreshCards(0);
            UpdateDeckUi();
        }

        private void Update()
        {
            if (!hasResponsiveProfile)
            {
                return;
            }

            if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
            {
                return;
            }

            ResponsiveDeviceClass previousClass = activeDeviceClass;
            bool changed = ApplyResponsiveLayout();
            if (changed && previousClass != activeDeviceClass && repository != null)
            {
                RefreshCards(currentPage);
                UpdateDeckUi();
            }
        }

        private void OnDestroy()
        {
            ClearDetailTexture();
            if (imageCache != null)
            {
                imageCache.Dispose();
                imageCache = null;
            }

            if (repository != null)
            {
                DisposeRepository();
                repository = null;
            }
        }

        private void LoadRepository()
        {
            packDirectory = CardPackFileSystem.DefaultPackDirectory;
            manifest = CardPackFileSystem.LoadManifest(packDirectory);
            imageCache = new CardImageCache(CardPackFileSystem.GetImageRootPath(manifest));
            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
            packValidationStatus = CardPackValidationStatusBuilder.FromManifest(
                manifest,
                File.Exists(databasePath),
                File.Exists(CardPackFileSystem.GetAssetIndexPath(packDirectory, manifest)));
            CardRepositoryLoadResult loadResult = CardRepositoryFactory.LoadDefault(packDirectory, manifest);
            repository = loadResult.Repository;
            if (!string.IsNullOrEmpty(loadResult.Warning))
            {
                Debug.LogWarning("Card repository loaded with warning: " + loadResult.Warning);
            }
        }

        private bool TryLoadRepository()
        {
            try
            {
                LoadRepository();
                repositoryLoadError = null;
                return true;
            }
            catch (Exception exception)
            {
                repositoryLoadError = exception.GetType().Name + ": " + exception.Message;
                Debug.LogError("Card browser failed to load runtime card pack: " + exception);
                DisposeRuntimeResources();
                ShowRepositoryLoadFailure();
                return false;
            }
        }

        private void CreateActiveDeck()
        {
            if (DeckBuilderSandboxState.DraftDeck != null)
            {
                activeDeck = DeckBuilderSandboxState.DraftDeck;
            }
            else
            {
                activeDeck = VanguardDeck.Create("New Deck", "D", manifest.pack_id, manifest.source_version);
                DeckBuilderSandboxState.DraftDeck = activeDeck;
            }
            deckValidator = new DeckValidator(repository);
            deckStorage = new DeckStorage();
        }

        private void PopulateFilters()
        {
            seriesValues.Clear();
            taxonomyValues.Clear();

            List<Dropdown.OptionData> seriesOptions = new List<Dropdown.OptionData>();
            seriesOptions.Add(new Dropdown.OptionData("All series"));
            seriesValues.Add(null);
            foreach (SeriesOption option in repository.ListSeries())
            {
                seriesOptions.Add(new Dropdown.OptionData(option.Series));
                seriesValues.Add(option.Series);
            }

            List<Dropdown.OptionData> clanOptions = new List<Dropdown.OptionData>();
            clanOptions.Add(new Dropdown.OptionData(VanguardAreaClanTaxonomy.AllGroupsLabel));
            taxonomyValues.Add(CardTaxonomyFilterOption.Empty);
            foreach (CardTaxonomyFilterOption option in VanguardAreaClanTaxonomy.BuildFilterOptions(
                         repository.ListClans(),
                         ListNations(repository)))
            {
                clanOptions.Add(new Dropdown.OptionData(option.DisplayLabel));
                taxonomyValues.Add(option);
            }

            seriesDropdown.options = seriesOptions;
            clanDropdown.options = clanOptions;
            seriesDropdown.RefreshShownValue();
            clanDropdown.RefreshShownValue();
        }

        private void RefreshCards(int page)
        {
            if (repository == null || manifest == null)
            {
                ShowRepositoryLoadFailure();
                return;
            }

            currentPage = Math.Max(0, page);
            ClearGrid();

            CardTaxonomyFilterOption taxonomyFilter =
                taxonomyValues.Count > clanDropdown.value ? taxonomyValues[clanDropdown.value] : CardTaxonomyFilterOption.Empty;

            CardQueryOptions options = new CardQueryOptions
            {
                SearchText = string.IsNullOrWhiteSpace(searchInput.text) ? null : searchInput.text.Trim(),
                Series = seriesValues.Count > seriesDropdown.value ? seriesValues[seriesDropdown.value] : null,
                Clan = taxonomyFilter.IsClan ? taxonomyFilter.Value : null,
                Nation = taxonomyFilter.IsNation ? taxonomyFilter.Value : null,
                Limit = PageSize,
                Offset = currentPage * PageSize
            };

            currentCards = repository.QueryCards(options);
            foreach (CardSummary card in currentCards)
            {
                CreateCardTile(card);
            }

            pageText.text = "Page " + (currentPage + 1);
            int totalCards = repository.CountCards();
            string groupStatus = taxonomyFilter.IsEmpty ? null : taxonomyFilter.StatusLabel;
            statusText.text = CardWorkshopToolbarFormatter.FormatCompactStatus(
                totalCards,
                currentCards.Count,
                HasActiveFilters(options.SearchText, options.Series, groupStatus));
            UpdateCardWorkshopSummary(totalCards, currentCards.Count, options.SearchText, options.Series, groupStatus);

            if (currentCards.Count > 0)
            {
                ShowDetail(currentCards[0]);
            }
            else
            {
                ShowEmptyDetail(options);
            }
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("Card Browser Canvas");
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280, 720);
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasRect = canvasObject.GetComponent<RectTransform>();

            GameObject background = CreatePanel("Background", canvasRect, new Color(0.09f, 0.1f, 0.12f, 1f));
            Stretch(background.GetComponent<RectTransform>(), 0, 0, 0, 0);

            RectTransform toolbar = CreatePanel("Toolbar", canvasRect, new Color(0.15f, 0.16f, 0.18f, 1f)).GetComponent<RectTransform>();
            toolbarRect = toolbar;
            AnchorTop(toolbar, 74f);
            toolbarLayout = toolbar.gameObject.AddComponent<HorizontalLayoutGroup>();
            toolbarLayout.padding = new RectOffset(12, 12, 10, 10);
            toolbarLayout.spacing = 8;
            toolbarLayout.childAlignment = TextAnchor.MiddleLeft;
            toolbarLayout.childControlHeight = true;
            toolbarLayout.childControlWidth = false;

            CreateButton(toolbar, "Back", 60, GoBackToHome);
            searchInput = CreateInput(toolbar, "Search Thai/name/code/text", 190);
            seriesDropdown = CreateDropdown(toolbar, 210);
            clanDropdown = CreateDropdown(toolbar, 170);
            seriesDropdown.onValueChanged.AddListener(delegate { RefreshCards(0); });
            clanDropdown.onValueChanged.AddListener(delegate { RefreshCards(0); });
            CreateLabel(toolbar, CardBrowserModeFormatter.FormatTitle(screenMode), 110, 18, TextAnchor.MiddleCenter);
            CreateButton(toolbar, "Search", 75, delegate { RefreshCards(0); });
            CreateButton(toolbar, "Clear", 65, delegate
            {
                searchInput.text = "";
                seriesDropdown.value = 0;
                clanDropdown.value = 0;
                RefreshCards(0);
            });
            CreateSecondaryButton(toolbar, "Cache", 64, ClearImageCache);
            CreateButton(toolbar, "<", 36, delegate { RefreshCards(Math.Max(0, currentPage - 1)); });
            CreateButton(toolbar, ">", 36, delegate
            {
                if (currentCards.Count == PageSize)
                {
                    RefreshCards(currentPage + 1);
                }
            });
            pageText = CreateLabel(toolbar, "Page 1", 70, 16, TextAnchor.MiddleCenter);
            statusText = CreateLabel(toolbar, UiStateMessageFormatter.CardPoolPreparing, 180, 15, TextAnchor.MiddleLeft);
            statusText.gameObject.name = "Card Workshop Toolbar Status Text";

            RectTransform main = CreatePanel("Main", canvasRect, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            mainRect = main;
            Stretch(main, 0, 0, 0, 74);
            mainLayout = main.gameObject.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = new RectOffset(12, 12, 12, 12);
            mainLayout.spacing = 12;
            mainLayout.childControlHeight = true;
            mainLayout.childControlWidth = true;

            RectTransform detailPanel = CreatePanel("Detail Panel", main, new Color(0.16f, 0.17f, 0.19f, 1f)).GetComponent<RectTransform>();
            detailPanelLayout = detailPanel.gameObject.AddComponent<LayoutElement>();
            detailPanelLayout.preferredWidth = 340;
            detailPanelLayout.flexibleWidth = 0;
            VerticalLayoutGroup detailVertical = detailPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            detailVertical.padding = new RectOffset(14, 14, 14, 14);
            detailVertical.spacing = 10;
            detailVertical.childControlHeight = false;
            detailVertical.childControlWidth = true;
            detailVertical.childAlignment = TextAnchor.UpperCenter;

            screenSummaryText = CreateMultilineLabel(
                detailPanel,
                CardWorkshopReadinessFormatter.FormatPreparing(screenMode),
                300,
                76,
                13);
            screenSummaryText.gameObject.name = "Card Workshop Summary Text";
            detailImage = CreateDetailImageFrame(detailPanel, 250, 350);
            detailTitle = CreateLabel(detailPanel, "Select a card", 300, 20, TextAnchor.MiddleCenter);
            detailBody = CreateMultilineLabel(detailPanel, "", 300, 160, 15);

            if (screenMode == CardBrowserScreenMode.DeckBuilder)
            {
                RectTransform detailActions = CreatePanel("Detail Actions", detailPanel, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
                detailActionsLayout = detailActions.gameObject.AddComponent<LayoutElement>();
                detailActionsLayout.preferredWidth = 300;
                detailActionsLayout.preferredHeight = 42;
                HorizontalLayoutGroup actionRow = detailActions.gameObject.AddComponent<HorizontalLayoutGroup>();
                actionRow.spacing = 6;
                actionRow.childAlignment = TextAnchor.MiddleCenter;
                actionRow.childControlHeight = true;
                actionRow.childControlWidth = false;
                CreateButton(detailActions, "+Main", 86, delegate { AddSelectedCardToDeck(DeckZone.Main); });
                CreateButton(detailActions, "+Ride", 82, delegate { AddSelectedCardToDeck(DeckZone.Ride); });
                CreateButton(detailActions, "-Main", 86, RemoveSelectedMainCard);
            }

            RectTransform scrollHost = CreatePanel("Grid Host", main, new Color(0.12f, 0.13f, 0.15f, 1f)).GetComponent<RectTransform>();
            scrollHost.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            ScrollRect scrollRect = scrollHost.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            GameObject viewport = CreatePanel("Viewport", scrollHost, new Color(0, 0, 0, 0));
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect, 0, 0, 0, 0);
            scrollRect.viewport = viewportRect;

            GameObject content = new GameObject("Grid Content");
            content.transform.SetParent(viewport.transform, false);
            gridContent = content.AddComponent<RectTransform>();
            gridContent.anchorMin = new Vector2(0, 1);
            gridContent.anchorMax = new Vector2(1, 1);
            gridContent.pivot = new Vector2(0.5f, 1);
            gridContent.offsetMin = new Vector2(8, 0);
            gridContent.offsetMax = new Vector2(-8, 0);
            cardGrid = content.AddComponent<GridLayoutGroup>();
            cardGrid.cellSize = new Vector2(132, 204);
            cardGrid.spacing = new Vector2(10, 12);
            cardGrid.padding = new RectOffset(8, 8, 8, 8);
            cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cardGrid.constraintCount = 4;
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = gridContent;

            if (screenMode == CardBrowserScreenMode.DeckBuilder)
            {
                RectTransform deckPanel = CreatePanel("Deck Panel", main, new Color(0.14f, 0.15f, 0.17f, 1f)).GetComponent<RectTransform>();
                deckPanelLayout = deckPanel.gameObject.AddComponent<LayoutElement>();
                deckPanelLayout.preferredWidth = 300;
                deckPanelLayout.flexibleWidth = 0;
                VerticalLayoutGroup deckVertical = deckPanel.gameObject.AddComponent<VerticalLayoutGroup>();
                deckVertical.padding = new RectOffset(12, 12, 12, 12);
                deckVertical.spacing = 8;
                deckVertical.childControlHeight = false;
                deckVertical.childControlWidth = true;
                deckVertical.childAlignment = TextAnchor.UpperCenter;

                CreateLabel(deckPanel, "Deck Builder", 260, 20, TextAnchor.MiddleCenter);
                deckRuleBadgeText = CreateLabel(deckPanel, "", 260, 15, TextAnchor.MiddleCenter);
                deckStatusText = CreateMultilineLabel(deckPanel, "", 260, 42, 13);
                deckIssuesText = CreateMultilineLabel(deckPanel, "", 260, 58, 12);
                deckMessageText = CreateMultilineLabel(deckPanel, "", 260, 30, 12);

                CreateButton(deckPanel, "Deck Tools", 260, ShowDeckToolsDialog);
                CreateButton(deckPanel, "Deck Type / Accessories", 260, ShowDeckAccessoriesDialog);
                CreateButton(deckPanel, "Start Game", 260, StartGame);
                CreateButton(deckPanel, "Online Room", 260, OpenOnlineRoom);

                deckListText = CreateMultilineLabel(deckPanel, "", 260, 194, 12);
            }
            ApplyResponsiveLayout();
        }

        private bool ApplyResponsiveLayout()
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(Screen.width, Screen.height);
            bool deviceClassChanged = !hasResponsiveProfile || profile.DeviceClass != activeDeviceClass;
            activeLayoutProfile = profile;
            activeDeviceClass = profile.DeviceClass;
            hasResponsiveProfile = true;

            canvasScaler.referenceResolution = profile.ReferenceResolution;
            canvasScaler.matchWidthOrHeight = profile.CanvasMatchWidthOrHeight;
            AnchorTop(toolbarRect, profile.ToolbarHeight);
            Stretch(mainRect, 0, 0, 0, profile.ToolbarHeight);

            toolbarLayout.padding = profile.IsCompact ? new RectOffset(8, 8, 8, 8) : new RectOffset(12, 12, 10, 10);
            toolbarLayout.spacing = profile.IsCompact ? 4f : 8f;
            mainLayout.padding = profile.IsCompact ? new RectOffset(8, 8, 8, 8) : new RectOffset(12, 12, 12, 12);
            mainLayout.spacing = profile.IsCompact ? 8f : 12f;

            cardGrid.cellSize = profile.CardGridCellSize;
            cardGrid.spacing = profile.IsCompact ? new Vector2(8f, 10f) : new Vector2(10f, 12f);
            cardGrid.constraintCount = screenMode == CardBrowserScreenMode.DeckBuilder
                ? profile.CardGridColumns
                : Math.Max(profile.CardGridColumns, profile.IsCompact ? 2 : 5);
            detailPanelLayout.preferredWidth = profile.DetailPanelWidth;
            if (deckPanelLayout != null)
            {
                deckPanelLayout.preferredWidth = profile.DeckPanelWidth;
            }

            cardTileImageWidth = profile.CardTileImageSize.x;
            cardTileImageHeight = profile.CardTileImageSize.y;
            cardTileNameWidth = Mathf.Max(92f, profile.CardGridCellSize.x - 16f);

            if (detailImageLayout != null)
            {
                detailImageLayout.preferredWidth = profile.DetailImageWidth;
                detailImageLayout.preferredHeight = Mathf.Round(profile.DetailImageWidth * 1.4f);
            }

            if (detailActionsLayout != null)
            {
                detailActionsLayout.preferredWidth = Mathf.Max(160f, profile.DetailPanelWidth - 20f);
                detailActionsLayout.preferredHeight = profile.TouchTargetHeight;
            }

            if (deckActionsLayout != null)
            {
                deckActionsLayout.preferredWidth = Mathf.Max(178f, profile.DeckPanelWidth - 20f);
                deckActionsLayout.preferredHeight = profile.TouchTargetHeight;
            }

            responsiveBindings.RemoveAll(binding => !binding.IsAlive);
            foreach (ResponsiveLayoutBinding binding in responsiveBindings)
            {
                binding.Apply(profile);
            }

            return deviceClassChanged;
        }

        private void CreateCardTile(CardSummary card)
        {
            GameObject tile = CreatePanel("Card " + card.CardId, gridContent, new Color(0.19f, 0.2f, 0.23f, 1f));
            Button button = tile.AddComponent<Button>();
            button.onClick.AddListener(delegate { ShowDetail(card); });

            VerticalLayoutGroup layout = tile.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 4;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            RawImage image = CreateRawImage(tile.GetComponent<RectTransform>(), cardTileImageWidth, cardTileImageHeight);
            Texture2D thumbnail = imageCache.LoadThumbnail(card.ImageRelativePath);
            image.texture = thumbnail;
            bool usesFallback = !card.ImageExists ||
                                string.IsNullOrWhiteSpace(card.ImageRelativePath) ||
                                imageCache.IsFallbackTexture(thumbnail);

            Text name = CreateMultilineLabel(
                tile.GetComponent<RectTransform>(),
                CardImageFallbackStatusFormatter.FormatTileLabel(card.NameTh ?? card.CardId, usesFallback),
                cardTileNameWidth,
                34,
                12);
            name.alignment = TextAnchor.MiddleCenter;
        }

        private void ShowDetail(CardSummary summary)
        {
            CardDetail detail = repository.GetCard(summary.CardId);
            if (detail == null)
            {
                ShowEmptyDetail(null);
                return;
            }

            selectedDetail = detail;
            ClearDetailTexture();
            detailTexture = imageCache.LoadFullImage(detail.ImageRelativePath);
            detailImage.texture = detailTexture;
            UpdateDetailImageAspect(detailTexture);
            bool usesFallback = !detail.ImageExists ||
                                string.IsNullOrWhiteSpace(detail.ImageRelativePath) ||
                                imageCache.IsFallbackTexture(detailTexture);
            string imageStatus = CardImageFallbackStatusFormatter.FormatDetailStatusWithTip(
                usesFallback,
                detail.ImageRelativePath);

            detailTitle.text = detail.NameTh + "\n" + detail.CardId;
            detailBody.text =
                "Series: " + detail.Series + "\n" +
                "Clan: " + detail.Clan + "\n" +
                "Grade: " + NullableText(detail.Grade) + "  Power: " + NullableText(detail.Power) + "\n\n" +
                (string.IsNullOrEmpty(imageStatus) ? string.Empty : imageStatus + "\n\n") +
                detail.TextTh;
        }

        private void ShowEmptyDetail(CardQueryOptions options = null)
        {
            selectedDetail = null;
            ClearDetailTexture();
            detailImage.texture = null;
            UpdateDetailImageAspect(null);
            detailTitle.text = CardBrowserSearchPanelFormatter.EmptyTitle;
            detailBody.text = CardBrowserSearchPanelFormatter.FormatEmptyResult(options);
        }

        private void ClearImageCache()
        {
            if (repository == null)
            {
                ShowRepositoryLoadFailure();
                return;
            }

            if (imageCache != null)
            {
                imageCache.ClearMemory();
            }

            RefreshCards(currentPage);
            statusText.text = statusText.text + " | cache cleared";
        }

        private void AddSelectedCardToDeck(DeckZone zone)
        {
            if (selectedDetail == null || activeDeck == null)
            {
                return;
            }

            activeDeck.AddCard(zone, selectedDetail.CardId, 1);
            SetDeckMessage("Added " + selectedDetail.CardId + " to " + zone + ".");
            UpdateDeckUi();
        }

        private void RemoveSelectedMainCard()
        {
            if (selectedDetail == null || activeDeck == null)
            {
                return;
            }

            activeDeck.RemoveCard(DeckZone.Main, selectedDetail.CardId, 1);
            SetDeckMessage("Removed " + selectedDetail.CardId + " from main.");
            UpdateDeckUi();
        }

        private void ClearDeck()
        {
            if (activeDeck == null)
            {
                return;
            }

            activeDeck.ClearAll();
            SetDeckMessage("Deck cleared.");
            SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResult("Clear", true, "Current deck contents cleared."));
            UpdateDeckUi();
        }

        private void SaveDeck()
        {
            if (activeDeck == null || deckStorage == null)
            {
                return;
            }

            string path = deckStorage.Save(activeDeck);
            SetDeckMessage("Saved " + Path.GetFileName(path));
            SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResult("Save", true, Path.GetFileName(path)));
            UpdateDeckUi();
        }

        private void LoadLatestDeck()
        {
            if (deckStorage == null)
            {
                return;
            }
 
            VanguardDeck loaded = deckStorage.LoadLatest();
            if (loaded == null)
            {
                SetDeckMessage("No saved deck found.");
                SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResultWithTip(
                    "Load",
                    false,
                    "No saved deck found.",
                    LoadingTipCatalog.DeckLoad));
                return;
            }
 
            activeDeck = loaded;
            DeckBuilderSandboxState.DraftDeck = loaded;
            SetDeckMessage("Loaded " + activeDeck.name + ".");
            SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResultWithTip(
                "Load",
                true,
                activeDeck.name,
                LoadingTipCatalog.DeckLoad));
            UpdateDeckUi();
        }
 
        private void CopyDeckCode()
        {
            if (activeDeck == null)
            {
                return;
            }
 
            string code = DeckCodeCodec.Export(activeDeck);
            GUIUtility.systemCopyBuffer = code;
            if (deckCodeInput != null)
            {
                deckCodeInput.text = code;
            }
 
            SetDeckMessage("Copied deck code (" + code.Length + " chars).");
            SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResult("Copy", true, code.Length + " chars copied."));
        }

        private void CopyDeckText()
        {
            if (activeDeck == null)
            {
                return;
            }

            string text = CountLineDeckCodec.Export(
                activeDeck,
                manifest == null ? null : manifest.definition_hash);
            GUIUtility.systemCopyBuffer = text;
            if (deckCodeInput != null)
            {
                deckCodeInput.text = text;
            }

            SetDeckMessage("Copied count-line deck text.");
            SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResult(
                "Copy Text",
                true,
                text.Split('\n').Length + " lines copied."));
        }
 
        private void ApplyDeckCode()
        {
            if (deckCodeInput == null)
            {
                return;
            }
 
            try
            {
                VanguardDeck imported = DeckCodeCodec.Import(deckCodeInput.text);
                activeDeck = imported;
                DeckBuilderSandboxState.DraftDeck = imported;
                SetDeckMessage("Applied deck code: " + activeDeck.name + ".");
                SetDeckToolsStatus(FormatImportCompatibilityStatus("Apply", activeDeck, null));
                UpdateDeckUi();
            }
            catch (Exception exception)
            {
                SetDeckMessage("Deck code rejected.");
                SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResult("Apply", false, exception.Message));
            }
        }

        private void ApplyDeckText()
        {
            if (deckCodeInput == null)
            {
                return;
            }

            try
            {
                CountLineDeckImportResult importResult = CountLineDeckCodec.ImportDetailed(deckCodeInput.text);
                VanguardDeck imported = importResult.deck;
                activeDeck = imported;
                DeckBuilderSandboxState.DraftDeck = imported;
                SetDeckMessage("Applied deck text: " + activeDeck.name + ".");
                SetDeckToolsStatus(FormatImportCompatibilityStatus(
                    "Apply Text",
                    activeDeck,
                    importResult.pack_definition_hash));
                UpdateDeckUi();
            }
            catch (Exception exception)
            {
                SetDeckMessage("Deck text rejected.");
                SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResult("Apply Text", false, exception.Message));
            }
        }

        private string FormatImportCompatibilityStatus(
            string operation,
            VanguardDeck importedDeck,
            string importedPackDefinitionHash)
        {
            DeckValidationResult validation = deckValidator == null || importedDeck == null
                ? null
                : deckValidator.Validate(importedDeck);
            DeckImportCompatibilityReport compatibility = DeckImportCompatibilityAnalyzer.Analyze(
                importedDeck,
                validation,
                manifest,
                importedPackDefinitionHash);
            return DeckToolsDialogFormatter.FormatOperationResult(
                       operation,
                       true,
                       importedDeck == null ? "Imported deck" : importedDeck.name) +
                   "\n" +
                   DeckImportCompatibilityFormatter.Format(compatibility);
        }
 
        private void DeleteCurrentDeck()
        {
            if (activeDeck == null || deckStorage == null)
            {
                return;
            }
 
            string deletedDeckName = activeDeck.name;
            bool deleted = deckStorage.Delete(activeDeck.deck_id);
            activeDeck = VanguardDeck.Create("New Deck", "D", manifest.pack_id, manifest.source_version);
            DeckBuilderSandboxState.DraftDeck = activeDeck;
            SetDeckMessage(deleted ? "Deleted " + deletedDeckName + "." : "No saved file for current deck.");
            SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResult(
                "Delete",
                deleted,
                deleted ? deletedDeckName : "Current deck file was not found."));
            UpdateDeckUi();
        }

        private void ShowDeckToolsDialog()
        {
            if (screenMode != CardBrowserScreenMode.DeckBuilder || canvasRect == null)
            {
                return;
            }

            if (deckToolsDialog != null)
            {
                deckToolsDialog.SetActive(true);
                SetDeckToolsStatus(DeckToolsDialogFormatter.FormatDialogStatus(activeDeck));
                return;
            }

            deckToolsDialog = CreatePanel("Deck Tools Dialog", canvasRect, new Color(0.02f, 0.025f, 0.03f, 0.88f));
            Stretch(deckToolsDialog.GetComponent<RectTransform>(), 0, 0, 0, 0);

            RectTransform panel = CreatePanel("Deck Tools Panel", deckToolsDialog.transform, new Color(0.13f, 0.15f, 0.18f, 1f)).GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(640, 560);
            panel.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            CreateLabel(panel, "Deck Tools", 600, 24, TextAnchor.MiddleCenter);
            deckToolsStatusText = CreateMultilineLabel(panel, DeckToolsDialogFormatter.FormatDialogStatus(activeDeck), 600, 96, 13);
            deckCodeInput = CreateInput(panel, "Paste deck code or count-line deck text", 600);
            deckCodeInput.lineType = InputField.LineType.MultiLineNewline;
            LayoutElement inputLayout = deckCodeInput.GetComponent<LayoutElement>();
            if (inputLayout != null)
            {
                inputLayout.preferredHeight = 120f;
            }

            RectTransform rowOne = CreateDialogRow(panel);
            CreateButton(rowOne, "Save Current", 140, SaveDeck);
            CreateButton(rowOne, "Load Latest", 140, LoadLatestDeck);
            CreateButton(rowOne, "Copy Code", 120, CopyDeckCode);

            RectTransform rowTwo = CreateDialogRow(panel);
            CreateButton(rowTwo, "Apply Code", 130, ApplyDeckCode);
            CreateButton(rowTwo, "Copy Text", 120, CopyDeckText);
            CreateButton(rowTwo, "Apply Text", 125, ApplyDeckText);

            RectTransform rowThree = CreateDialogRow(panel);
            CreateButton(rowThree, "Clear Deck", 105, ClearDeck);
            CreateButton(rowThree, "Delete", 96, DeleteCurrentDeck);
            CreateButton(rowThree, "Export Image", 125, ExportDeckImage);
            CreateButton(rowThree, "Pack Check", 112, ShowPackValidationStatus);
            CreateButton(rowThree, "Close", 80, CloseDeckToolsDialog);
        }

        private RectTransform CreateDialogRow(Transform parent)
        {
            RectTransform row = CreatePanel("Dialog Row", parent, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            LayoutElement layoutElement = row.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 44f;
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            return row;
        }

        private void CloseDeckToolsDialog()
        {
            if (deckToolsDialog != null)
            {
                deckToolsDialog.SetActive(false);
            }
        }

        private void ShowPackValidationStatus()
        {
            string input = deckCodeInput == null ? null : deckCodeInput.text;
            SetDeckToolsStatus(PackValidationUiFormatter.FormatFromDeckToolsInput(
                input,
                manifest,
                packValidationStatus));
            SetDeckMessage("Pack validation status shown.");
        }

        private void ExportDeckImage()
        {
            DeckImageExportPlan plan = DeckImageExportPlanner.CreatePlan(
                activeDeck,
                Path.Combine(Application.persistentDataPath, "deck_exports"),
                DateTime.Now);
            SetDeckToolsStatus(DeckImageExportPlanner.FormatPlanStatus(plan));
            if (plan == null || !plan.accepted)
            {
                SetDeckMessage("Deck image export rejected.");
                return;
            }

            SetDeckMessage("Exporting deck image...");
            CloseDeckToolsDialog();
            StartCoroutine(CaptureDeckImageAtEndOfFrame(plan));
        }

        private IEnumerator CaptureDeckImageAtEndOfFrame(DeckImageExportPlan plan)
        {
            yield return new WaitForEndOfFrame();

            try
            {
                string directory = Path.GetDirectoryName(plan.file_path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                ScreenCapture.CaptureScreenshot(plan.file_path);
                SetDeckMessage("Deck image export requested: " + plan.file_name);
            }
            catch (Exception exception)
            {
                SetDeckMessage("Deck image export failed: " + exception.Message);
                SetDeckToolsStatus(DeckToolsDialogFormatter.FormatOperationResult(
                    "Export Image",
                    false,
                    exception.Message));
            }
        }

        private void ShowDeckAccessoriesDialog()
        {
            if (screenMode != CardBrowserScreenMode.DeckBuilder)
            {
                return;
            }

            if (activeDeck == null)
            {
                SetDeckMessage("No deck loaded.");
                return;
            }

            EnsureDeckAppearance();
            if (deckAccessoriesDialog != null)
            {
                deckAccessoriesDialog.SetActive(true);
                SetDeckAccessoriesStatus();
                return;
            }

            deckAccessoriesDialog = CreatePanel("Deck Type / Accessories Dialog", canvasRect, new Color(0.02f, 0.025f, 0.03f, 0.88f));
            Stretch(deckAccessoriesDialog.GetComponent<RectTransform>(), 0, 0, 0, 0);

            RectTransform panel = CreatePanel("Deck Accessories Panel", deckAccessoriesDialog.transform, new Color(0.13f, 0.15f, 0.18f, 1f)).GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(680, 520);
            panel.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            CreateLabel(panel, "Deck Type / Accessories", 640, 24, TextAnchor.MiddleCenter);
            deckAccessoriesStatusText = CreateMultilineLabel(panel, DeckAccessoriesDialogFormatter.FormatSummary(activeDeck), 640, 104, 13);

            RectTransform rowOne = CreateDialogRow(panel);
            CreateButton(rowOne, "Format", 112, CycleDeckFormat);
            CreateButton(rowOne, "Sleeve", 112, CycleDeckSleeve);
            CreateButton(rowOne, "Card Back", 120, CycleDeckCardBack);
            CreateButton(rowOne, "Playmat", 112, CycleDeckPlaymat);

            RectTransform rowTwo = CreateDialogRow(panel);
            CreateButton(rowTwo, "Crest", 112, CycleDeckCrest);
            CreateButton(rowTwo, "Persona", 112, CycleDeckPersonaShield);
            CreateButton(rowTwo, "Gift", 112, CycleDeckGiftMarker);
            CreateButton(rowTwo, "Quick", 112, CycleDeckQuickShield);

            RectTransform rowThree = CreateDialogRow(panel);
            CreateButton(rowThree, "Close", 120, CloseDeckAccessoriesDialog);
        }

        private void CloseDeckAccessoriesDialog()
        {
            if (deckAccessoriesDialog != null)
            {
                deckAccessoriesDialog.SetActive(false);
            }
        }

        private void SetDeckToolsStatus(string message)
        {
            if (deckToolsStatusText != null)
            {
                deckToolsStatusText.text = message;
            }
        }

        private void SetDeckAccessoriesStatus()
        {
            if (deckAccessoriesStatusText != null)
            {
                deckAccessoriesStatusText.text = DeckAccessoriesDialogFormatter.FormatSummary(activeDeck);
            }
        }

        private void CycleDeckFormat()
        {
            if (activeDeck == null)
            {
                return;
            }

            activeDeck.format = DeckAccessoriesDialogFormatter.NextFormat(activeDeck.format);
            SetDeckMessage("Deck format: " + activeDeck.format);
            SetDeckAccessoriesStatus();
            UpdateDeckUi();
        }

        private void CycleDeckSleeve()
        {
            EnsureDeckAppearance();
            activeDeck.appearance.sleeve_key = DeckAccessoriesDialogFormatter.NextSleeve(activeDeck.appearance.sleeve_key);
            SetDeckMessage("Sleeve: " + activeDeck.appearance.sleeve_key);
            SetDeckAccessoriesStatus();
        }

        private void CycleDeckCardBack()
        {
            EnsureDeckAppearance();
            activeDeck.appearance.card_back_key = DeckAccessoriesDialogFormatter.NextCardBack(activeDeck.appearance.card_back_key);
            SetDeckMessage("Card back: " + activeDeck.appearance.card_back_key);
            SetDeckAccessoriesStatus();
        }

        private void CycleDeckPlaymat()
        {
            EnsureDeckAppearance();
            activeDeck.appearance.playmat_key = DeckAccessoriesDialogFormatter.NextPlaymat(activeDeck.appearance.playmat_key);
            SetDeckMessage("Playmat: " + activeDeck.appearance.playmat_key);
            SetDeckAccessoriesStatus();
        }

        private void CycleDeckCrest()
        {
            EnsureDeckAppearance();
            activeDeck.appearance.crest_key = DeckAccessoriesDialogFormatter.NextCrest(activeDeck.appearance.crest_key);
            SetDeckMessage("Crest: " + activeDeck.appearance.crest_key);
            SetDeckAccessoriesStatus();
        }

        private void CycleDeckPersonaShield()
        {
            EnsureDeckAppearance();
            activeDeck.appearance.persona_shield_key = DeckAccessoriesDialogFormatter.NextPersonaShield(activeDeck.appearance.persona_shield_key);
            SetDeckMessage("Persona shield: " + activeDeck.appearance.persona_shield_key);
            SetDeckAccessoriesStatus();
        }

        private void CycleDeckGiftMarker()
        {
            EnsureDeckAppearance();
            activeDeck.appearance.gift_marker_key = DeckAccessoriesDialogFormatter.NextGiftMarker(activeDeck.appearance.gift_marker_key);
            SetDeckMessage("Gift marker: " + activeDeck.appearance.gift_marker_key);
            SetDeckAccessoriesStatus();
        }

        private void CycleDeckQuickShield()
        {
            EnsureDeckAppearance();
            activeDeck.appearance.quick_shield_key = DeckAccessoriesDialogFormatter.NextQuickShield(activeDeck.appearance.quick_shield_key);
            SetDeckMessage("Quick shield: " + activeDeck.appearance.quick_shield_key);
            SetDeckAccessoriesStatus();
        }

        private void EnsureDeckAppearance()
        {
            if (activeDeck == null)
            {
                return;
            }

            activeDeck.appearance = DeckAppearanceMetadata.Normalize(activeDeck.appearance);
        }

        private void StartGame()
        {
            DeckValidationResult validation = activeDeck != null && deckValidator != null
                ? deckValidator.Validate(activeDeck)
                : null;
            PlayTableSetupReadinessResult readiness = PlayTableSetupReadiness.Evaluate(activeDeck, validation);
            if (!readiness.can_start)
            {
                SetDeckMessage(readiness.status_message);
                UpdateDeckUi();
                return;
            }

            VanguardDeck playerDeck = VanguardDeck.FromJson(activeDeck.ToJson(false));
            VanguardDeck opponentDeck = VanguardDeck.FromJson(activeDeck.ToJson(false));
            PlayTableBootstrap.Show(playerDeck, opponentDeck);
            SetDeckMessage("Started manual table.");
        }

        private void OpenOnlineRoom()
        {
            if (activeDeck == null || manifest == null)
            {
                SetDeckMessage("Deck or pack is not ready.");
                return;
            }

            MultiplayerLobbyBootstrap.Show(activeDeck, manifest);
            SetDeckMessage("Opened online room panel.");
        }

        private void SetDeckMessage(string message)
        {
            if (deckMessageText != null)
            {
                deckMessageText.text = message;
            }
        }

        private void UpdateDeckUi()
        {
            if (deckStatusText == null || deckIssuesText == null || deckListText == null || activeDeck == null || deckValidator == null)
            {
                return;
            }

            DeckValidationResult result = deckValidator.Validate(activeDeck);
            if (deckRuleBadgeText != null)
            {
                deckRuleBadgeText.text = DeckBuilderFilterPanelFormatter.FormatRuleBadge(activeDeck, result);
            }

            deckStatusText.text = DeckBuilderFilterPanelFormatter.FormatDeckCounters(result);
            deckIssuesText.text = DeckBuilderFilterPanelFormatter.FormatIssues(result);
            deckListText.text = BuildDeckListText();
        }

        private string BuildDeckListText()
        {
            StringBuilder builder = new StringBuilder();
            AppendDeckZone(builder, "Main", DeckZone.Main);
            AppendDeckZone(builder, "Ride", DeckZone.Ride);
            AppendDeckZone(builder, "G", DeckZone.G);
            return builder.Length == 0 ? "Deck is empty." : builder.ToString();
        }

        private void AppendDeckZone(StringBuilder builder, string label, DeckZone zone)
        {
            IReadOnlyList<DeckCardEntry> entries = activeDeck.GetEntries(zone);
            if (entries.Count == 0)
            {
                return;
            }

            builder.AppendLine(label);
            int shown = 0;
            foreach (DeckCardEntry entry in entries)
            {
                if (shown >= 18)
                {
                    builder.AppendLine("...");
                    break;
                }

                CardDetail card = repository.GetCard(entry.card_id);
                string name = card == null || string.IsNullOrWhiteSpace(card.NameTh) ? entry.card_id : card.NameTh;
                builder.Append("x");
                builder.Append(entry.quantity);
                builder.Append(" ");
                builder.Append(entry.card_id);
                builder.Append(" ");
                builder.AppendLine(name);
                shown++;
            }
        }

        private void ShowRepositoryLoadFailure()
        {
            string message = string.IsNullOrEmpty(repositoryLoadError)
                ? "Card pack is not ready."
                : repositoryLoadError;

            currentCards = Array.Empty<CardSummary>();

            if (seriesDropdown != null)
            {
                SetSingleDropdownOption(seriesDropdown, "Unavailable");
            }

            if (clanDropdown != null)
            {
                SetSingleDropdownOption(clanDropdown, "Unavailable");
            }

            if (pageText != null)
            {
                pageText.text = "Page -";
            }

            if (statusText != null)
            {
                statusText.text = "Card data load failed";
            }

            if (screenSummaryText != null)
            {
                screenSummaryText.text = CardWorkshopReadinessFormatter.FormatFailure(
                    screenMode,
                    message,
                    CardPackFileSystem.DefaultPackRelativePath);
            }

            if (gridContent != null)
            {
                ClearGrid();
            }

            if (detailTitle != null)
            {
                detailTitle.text = "Card data unavailable";
            }

            if (detailBody != null)
            {
                detailBody.text = UiStateMessageFormatter.FormatCardPackLoadFailure(
                    message,
                    CardPackFileSystem.DefaultPackRelativePath);
            }

            if (deckStatusText != null)
            {
                deckStatusText.text = "Deck system unavailable until card data loads.";
            }

            if (deckRuleBadgeText != null)
            {
                deckRuleBadgeText.text = "Rule: unavailable";
            }

            if (deckIssuesText != null)
            {
                deckIssuesText.text = "";
            }

            if (deckListText != null)
            {
                deckListText.text = "";
            }
        }

        private static void SetSingleDropdownOption(Dropdown dropdown, string label)
        {
            dropdown.options = new List<Dropdown.OptionData> { new Dropdown.OptionData(label) };
            dropdown.value = 0;
            dropdown.RefreshShownValue();
        }

        private void DisposeRuntimeResources()
        {
            if (imageCache != null)
            {
                imageCache.Dispose();
                imageCache = null;
            }

            if (repository != null)
            {
                DisposeRepository();
                repository = null;
            }

            manifest = null;
            packValidationStatus = null;
        }

        private void DisposeRepository()
        {
            IDisposable disposable = repository as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private static IReadOnlyList<NationOption> ListNations(ICardRepository source)
        {
            INationCardRepository nationRepository = source as INationCardRepository;
            return nationRepository == null ? Array.Empty<NationOption>() : nationRepository.ListNations();
        }

        private void ClearGrid()
        {
            for (int i = gridContent.childCount - 1; i >= 0; i--)
            {
                Destroy(gridContent.GetChild(i).gameObject);
            }
        }

        private void UpdateCardWorkshopSummary(
            int totalCards,
            int showingCards,
            string searchText,
            string series,
            string group)
        {
            if (screenSummaryText == null)
            {
                return;
            }

            screenSummaryText.text = CardWorkshopReadinessFormatter.FormatReady(
                screenMode,
                totalCards,
                showingCards,
                searchText,
                series,
                group);
        }

        private static bool HasActiveFilters(string searchText, string series, string group)
        {
            return DeckBuilderFilterPanelFormatter.FormatFilters(searchText, series, group) !=
                   DeckBuilderFilterPanelFormatter.NoFiltersLabel;
        }

        private void UpdateDetailImageAspect(Texture2D texture)
        {
            if (detailImageAspect == null)
            {
                return;
            }

            detailImageAspect.aspectRatio = CardImageAspectRatioHelper.Resolve(texture);
        }

        private void ClearDetailTexture()
        {
            detailTexture = null;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private Font LoadFont()
        {
            Font loaded = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (loaded == null)
            {
                loaded = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return loaded;
        }

        private GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private void RegisterResponsiveBinding(LayoutElement layout, bool scaleWidth, bool touchHeight)
        {
            responsiveBindings.Add(new ResponsiveLayoutBinding(layout, scaleWidth, touchHeight));
            if (hasResponsiveProfile)
            {
                responsiveBindings[responsiveBindings.Count - 1].Apply(activeLayoutProfile);
            }
        }

        private InputField CreateInput(Transform parent, string placeholder, float width)
        {
            GameObject root = CreatePanel("Search Input", parent, Color.white);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 42;
            RegisterResponsiveBinding(layout, true, true);

            InputField input = root.AddComponent<InputField>();
            Text text = CreateText("Text", root.transform, "", 16, TextAnchor.MiddleLeft, Color.black);
            Stretch(text.rectTransform, 10, 10, 6, 6);
            Text placeholderText = CreateText("Placeholder", root.transform, placeholder, 16, TextAnchor.MiddleLeft, new Color(0.45f, 0.45f, 0.45f, 1f));
            Stretch(placeholderText.rectTransform, 10, 10, 6, 6);
            input.textComponent = text;
            input.placeholder = placeholderText;
            return input;
        }

        private Dropdown CreateDropdown(Transform parent, float width)
        {
            GameObject root = CreatePanel("Dropdown", parent, Color.white);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 42;
            RegisterResponsiveBinding(layout, true, true);

            Dropdown dropdown = root.AddComponent<Dropdown>();
            dropdown.targetGraphic = root.GetComponent<Image>();
            Text label = CreateText("Label", root.transform, "", 15, TextAnchor.MiddleLeft, Color.black);
            Stretch(label.rectTransform, 10, 34, 6, 6);
            dropdown.captionText = label;

            Text arrow = CreateText("Arrow", root.transform, "v", 15, TextAnchor.MiddleCenter, new Color(0.2f, 0.2f, 0.2f, 1f));
            arrow.rectTransform.anchorMin = new Vector2(1, 0);
            arrow.rectTransform.anchorMax = new Vector2(1, 1);
            arrow.rectTransform.pivot = new Vector2(1, 0.5f);
            arrow.rectTransform.sizeDelta = new Vector2(28, 0);
            arrow.rectTransform.anchoredPosition = new Vector2(-4, 0);

            GameObject template = CreatePanel("Template", root.transform, Color.white);
            RectTransform templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.sizeDelta = new Vector2(0, 220);
            templateRect.anchoredPosition = new Vector2(0, -2);

            ScrollRect scrollRect = template.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewport = CreatePanel("Viewport", template.transform, new Color(1, 1, 1, 1));
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect, 0, 0, 0, 0);
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            scrollRect.viewport = viewportRect;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = 1;
            ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRect;

            GameObject item = CreatePanel("Item", content.transform, new Color(0.98f, 0.98f, 0.98f, 1f));
            LayoutElement itemLayout = item.AddComponent<LayoutElement>();
            itemLayout.preferredHeight = 34;
            Toggle toggle = item.AddComponent<Toggle>();
            toggle.targetGraphic = item.GetComponent<Image>();

            Text checkmark = CreateText("Item Checkmark", item.transform, "x", 14, TextAnchor.MiddleCenter, new Color(0.78f, 0.24f, 0.64f, 1f));
            checkmark.rectTransform.anchorMin = new Vector2(0, 0);
            checkmark.rectTransform.anchorMax = new Vector2(0, 1);
            checkmark.rectTransform.pivot = new Vector2(0, 0.5f);
            checkmark.rectTransform.sizeDelta = new Vector2(28, 0);
            checkmark.rectTransform.anchoredPosition = Vector2.zero;
            toggle.graphic = checkmark;

            Text itemText = CreateText("Item Label", item.transform, "", 14, TextAnchor.MiddleLeft, Color.black);
            Stretch(itemText.rectTransform, 30, 8, 4, 4);
            dropdown.itemText = itemText;
            dropdown.template = templateRect;
            dropdown.options = new List<Dropdown.OptionData> { new Dropdown.OptionData(UiStateMessageFormatter.FilterPreparing) };
            template.SetActive(false);
            return dropdown;
        }

        private Button CreateButton(Transform parent, string label, float width, UnityEngine.Events.UnityAction action)
        {
            return CreateButton(parent, label, width, action, new Color(0.78f, 0.24f, 0.64f, 1f));
        }

        private Button CreateSecondaryButton(Transform parent, string label, float width, UnityEngine.Events.UnityAction action)
        {
            return CreateButton(parent, label, width, action, new Color(0.24f, 0.27f, 0.3f, 1f));
        }

        private Button CreateButton(
            Transform parent,
            string label,
            float width,
            UnityEngine.Events.UnityAction action,
            Color color)
        {
            GameObject root = CreatePanel(label + " Button", parent, color);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 42;
            RegisterResponsiveBinding(layout, true, true);
            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(action);
            Text text = CreateText("Label", root.transform, label, 16, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 0, 0, 0, 0);
            return button;
        }

        private Text CreateLabel(Transform parent, string value, float width, int size, TextAnchor alignment)
        {
            Text text = CreateText("Label", parent, value, size, alignment, Color.white);
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 42;
            RegisterResponsiveBinding(layout, true, false);
            return text;
        }

        private Text CreateMultilineLabel(Transform parent, string value, float width, float height, int size)
        {
            Text text = CreateText("Multiline Label", parent, value, size, TextAnchor.UpperLeft, Color.white);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            return text;
        }

        private RawImage CreateRawImage(Transform parent, float width, float height)
        {
            GameObject root = new GameObject("Card Image");
            root.transform.SetParent(parent, false);
            RawImage image = root.AddComponent<RawImage>();
            image.color = Color.white;
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            return image;
        }

        private RawImage CreateDetailImageFrame(Transform parent, float width, float height)
        {
            GameObject frame = CreatePanel("Card Detail Image Frame", parent, new Color(0, 0, 0, 0));
            detailImageLayout = frame.AddComponent<LayoutElement>();
            detailImageLayout.preferredWidth = width;
            detailImageLayout.preferredHeight = height;

            GameObject imageObject = new GameObject("Card Detail Image");
            imageObject.transform.SetParent(frame.transform, false);
            RectTransform imageRect = imageObject.AddComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.sizeDelta = new Vector2(width, height);

            RawImage image = imageObject.AddComponent<RawImage>();
            image.color = Color.white;
            detailImageAspect = imageObject.AddComponent<AspectRatioFitter>();
            detailImageAspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            detailImageAspect.aspectRatio = CardImageAspectRatioHelper.DefaultCardAspectRatio;
            return image;
        }

        private Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment, Color color)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            Text text = root.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            return text;
        }

        private static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void AnchorTop(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.offsetMin = new Vector2(0, -height);
            rect.offsetMax = new Vector2(0, 0);
        }

        private static string NullableText(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "-";
        }

        private void GoBackToHome()
        {
            Destroy(gameObject);
            HomeLobbyBootstrap.Show();
        }
    }
}
