using System;
using System.Collections.Generic;
using System.IO;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VanguardThaiSim.UI
{
    public sealed class HomeLobbyBootstrap : MonoBehaviour
    {
        private Font font;
        private CanvasScaler canvasScaler;
        private CardPackManifest manifest;
        private ICardRepository repository;
        private DeckValidator deckValidator;
        private VanguardDeck latestDeck;
        private DeckValidationResult latestDeckValidation;
        private UserIconPackValidationResult iconPackValidation;
        private PlayerSettings playerSettings;
        private string loadError;
        private string lastActionMessage;
        private Text packStatusText;
        private Text deckStatusText;
        private Text modeStatusText;
        private Text iconStatusText;
        private GameObject settingsPanel;
        private GameObject manualScreen;
        private GameObject soloSetupPanel;
        private GameObject replayPanel;
        private Text settingsSummaryText;
        private Text soloSetupSummaryText;
        private Text soloDifficultyText;
        private Text soloOpponentDeckText;
        private Text replaySummaryText;
        private InputField replayPathInput;
        private string replaySummaryOverride;
        private Text replayPreviewText;
        private GameReplay loadedReplay;
        private GameReplayPlayer replayPlayer;
        private int soloDifficultyIndex;
        private int soloOpponentDeckChoiceIndex;

        public static void Show()
        {
            HomeLobbyBootstrap existing = FindAnyObjectByType<HomeLobbyBootstrap>();
            if (existing != null)
            {
                return;
            }

            GameObject host = new GameObject("Vanguard Home Lobby");
            DontDestroyOnLoad(host);
            host.AddComponent<HomeLobbyBootstrap>();
        }

        private void Start()
        {
            font = LoadFont();
            EnsureEventSystem();
            playerSettings = PlayerSettings.CreateDefault();
            LoadStartupData();
            BuildUi();
            RefreshStatus();
        }

        private void OnDestroy()
        {
            if (repository != null)
            {
                DisposeRepository();
                repository = null;
            }
        }

        private void LoadStartupData()
        {
            iconPackValidation = UserIconPackRuntime.LoadDefault();

            if (repository != null)
            {
                DisposeRepository();
                repository = null;
            }

            try
            {
                string packDirectory = CardPackFileSystem.DefaultPackDirectory;
                manifest = CardPackFileSystem.LoadManifest(packDirectory);
                string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
                string catalogPath = CardPackFileSystem.GetCatalogPath(packDirectory, manifest);
                if (File.Exists(databasePath) || File.Exists(catalogPath))
                {
                    CardRepositoryLoadResult loadResult = CardRepositoryFactory.LoadDefault(packDirectory, manifest);
                    repository = loadResult.Repository;
                    if (!string.IsNullOrEmpty(loadResult.Warning))
                    {
                        Debug.LogWarning("Home lobby card repository loaded with warning: " + loadResult.Warning);
                    }

                    deckValidator = new DeckValidator(repository);
                }

                if (DeckBuilderSandboxState.DraftDeck != null)
                {
                    latestDeck = DeckBuilderSandboxState.DraftDeck;
                }
                else
                {
                    latestDeck = new DeckStorage().LoadLatest();
                }
                latestDeckValidation = latestDeck != null && deckValidator != null
                    ? deckValidator.Validate(latestDeck)
                    : null;
                loadError = null;
            }
            catch (Exception exception)
            {
                loadError = exception.GetType().Name + ": " + exception.Message;
                latestDeck = null;
                latestDeckValidation = null;
                if (repository != null)
                {
                    DisposeRepository();
                    repository = null;
                }

                Debug.LogError("Home lobby failed to load startup data: " + exception);
            }
        }

        private void BuildUi()
        {
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(Screen.width, Screen.height);
            bool compact = profile.IsCompact;

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<GraphicRaycaster>();
            canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = profile.ReferenceResolution;
            canvasScaler.matchWidthOrHeight = profile.CanvasMatchWidthOrHeight;

            RectTransform root = gameObject.GetComponent<RectTransform>();
            GameObject background = CreatePanel("Home Background", root, new Color(0.05f, 0.06f, 0.07f, 1f));
            Stretch(background.GetComponent<RectTransform>(), 0, 0, 0, 0);

            RectTransform header = CreatePanel("Header", root, new Color(0.1f, 0.11f, 0.13f, 1f)).GetComponent<RectTransform>();
            AnchorTop(header, compact ? 64f : 72f);
            HorizontalLayoutGroup headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = compact ? new RectOffset(18, 18, 10, 10) : new RectOffset(22, 22, 12, 12);
            headerLayout.spacing = compact ? 10 : 12;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = false;

            CreateSingleLineLabel(header, compact ? "VG Thai Sim" : "Vanguard Thai Sim", compact ? 176 : 330, compact ? 20 : 24, TextAnchor.MiddleLeft, Color.white);
            CreatePill(header, "HOME", compact ? 70 : 88, new Color(0.72f, 0.18f, 0.58f, 1f));
            CreatePill(header, compact ? "D" : "D STANDARD", compact ? 56 : 128, new Color(0.16f, 0.45f, 0.66f, 1f));
            CreatePill(header, compact ? "AREA CLANS" : "VANGUARD AREA CLANS", compact ? 116 : 170, new Color(0.22f, 0.26f, 0.31f, 1f));
            Space(header, 1f);
            CreateSingleLineLabel(header, compact ? "Local DB" : "Local card database", compact ? 90 : 210, compact ? 14 : 16, TextAnchor.MiddleRight, new Color(0.82f, 0.84f, 0.88f, 1f));

            RectTransform main = CreatePanel("Home Main", root, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            Stretch(main, 0, 0, compact ? 64f : 72f, 0);
            HorizontalLayoutGroup mainLayout = main.gameObject.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = compact ? new RectOffset(18, 18, 18, 18) : new RectOffset(26, 26, 24, 24);
            mainLayout.spacing = compact ? 14 : 18;
            mainLayout.childControlHeight = true;
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = true;
            mainLayout.childForceExpandWidth = true;

            RectTransform leftPanel = CreateColumnPanel(main, "Fight Panel", 320, 1f, new Color(0.12f, 0.13f, 0.15f, 1f));
            CreateLabel(leftPanel, "Fight", 280, 24, TextAnchor.MiddleLeft, Color.white);
            CreatePrimaryButton(leftPanel, "Solo Play", 276, OpenSoloSetup);
            CreatePrimaryButton(leftPanel, "Online Room", 276, OpenOnlineRoom);
            CreateSecondaryButton(leftPanel, "CPU Fight", 276, ShowCpuLocked);
            CreateSecondaryButton(leftPanel, "Replay", 276, OpenReplay);
            CreateSecondaryButton(leftPanel, "Manual", 276, OpenManual);
            CreateSecondaryButton(leftPanel, "Settings", 276, OpenSettings);
            modeStatusText = CreateMultilineLabel(leftPanel, "", 276, 122, 14, new Color(0.86f, 0.88f, 0.92f, 1f));

            RectTransform centerPanel = CreateColumnPanel(main, "Center Panel", 440, 2f, new Color(0.09f, 0.11f, 0.13f, 1f));
            CreateLabel(centerPanel, "Quick Start", 380, 26, TextAnchor.MiddleLeft, Color.white);
            CreateMultilineLabel(
                centerPanel,
                "Manual table, deck builder, card browser, and trusted-client room flow start here.",
                380,
                54,
                15,
                new Color(0.78f, 0.8f, 0.84f, 1f));
            CreateDivider(centerPanel, 380);
            CreatePrimaryButton(centerPanel, "Deck Builder / Cards", 380, OpenDeckBuilder);
            CreateSecondaryButton(centerPanel, "Card Browser", 380, OpenReadOnlyCardBrowser);
            CreateSecondaryButton(centerPanel, "Reload Startup Data", 380, ReloadStartupData);
            CreateDivider(centerPanel, 380);
            CreateMultilineLabel(centerPanel, HomeLobbyStatusFormatter.TaxonomyBaseline, 380, 70, 14, new Color(0.82f, 0.84f, 0.88f, 1f));

            RectTransform rightPanel = CreateColumnPanel(main, "Status Panel", 350, 1f, new Color(0.12f, 0.13f, 0.15f, 1f));
            CreateLabel(rightPanel, "Status", 300, 24, TextAnchor.MiddleLeft, Color.white);
            packStatusText = CreateMultilineLabel(rightPanel, "", 300, 92, 14, new Color(0.86f, 0.88f, 0.92f, 1f));
            CreateDivider(rightPanel, 300);
            deckStatusText = CreateMultilineLabel(rightPanel, "", 300, 126, 14, new Color(0.86f, 0.88f, 0.92f, 1f));
            CreateDivider(rightPanel, 300);
            iconStatusText = CreateMultilineLabel(rightPanel, "", 300, 54, 13, new Color(0.82f, 0.84f, 0.88f, 1f));
            CreateDivider(rightPanel, 300);
            CreateLabel(rightPanel, "Trigger badges", 300, 18, TextAnchor.MiddleLeft, Color.white);
            RectTransform triggerRowOne = CreateRow(rightPanel, 40);
            CreateSymbolPill(triggerRowOne, UiGameSymbolKeys.TriggerCritical, 92, new Color(0.68f, 0.18f, 0.22f, 1f));
            CreateSymbolPill(triggerRowOne, UiGameSymbolKeys.TriggerDraw, 72, new Color(0.16f, 0.38f, 0.64f, 1f));
            CreateSymbolPill(triggerRowOne, UiGameSymbolKeys.TriggerFront, 76, new Color(0.24f, 0.48f, 0.34f, 1f));
            RectTransform triggerRowTwo = CreateRow(rightPanel, 40);
            CreateSymbolPill(triggerRowTwo, UiGameSymbolKeys.TriggerHeal, 72, new Color(0.36f, 0.5f, 0.22f, 1f));
            CreateSymbolPill(triggerRowTwo, UiGameSymbolKeys.TriggerOver, 72, new Color(0.52f, 0.32f, 0.72f, 1f));
            CreateSymbolPill(triggerRowTwo, UiGameSymbolKeys.TriggerStandLegacy, 78, new Color(0.62f, 0.46f, 0.16f, 1f));

            BuildSettingsPanel(root, compact);
            BuildSoloSetupPanel(root, compact);
            BuildReplayPanel(root, compact);
        }

        private void RefreshStatus()
        {
            if (packStatusText != null)
            {
                packStatusText.text = HomeLobbyStatusFormatter.FormatPackStatus(manifest, loadError);
            }

            if (deckStatusText != null)
            {
                deckStatusText.text = HomeLobbyStatusFormatter.FormatDeckStatus(latestDeck, latestDeckValidation);
            }

            if (modeStatusText != null)
            {
                modeStatusText.text = HomeLobbyStatusFormatter.FormatModeStatus(lastActionMessage);
            }

            if (iconStatusText != null)
            {
                iconStatusText.text = HomeLobbyStatusFormatter.FormatIconPackStatus(iconPackValidation);
            }

            RefreshSettingsSummary();
        }

        private void DisposeRepository()
        {
            IDisposable disposable = repository as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private void ReloadStartupData()
        {
            LoadStartupData();
            lastActionMessage = LoadingTipCatalog.AppendTip(
                "Startup data reloaded.",
                LoadingTipCatalog.DataReload);
            RefreshStatus();
        }

        private void OpenDeckBuilder()
        {
            Destroy(gameObject);
            CardBrowserBootstrap.ShowDeckBuilder();
        }

        private void OpenReadOnlyCardBrowser()
        {
            Destroy(gameObject);
            CardBrowserBootstrap.ShowBrowser();
        }

        private void OpenSoloSetup()
        {
            soloDifficultyIndex = SoloPlayEntryFlow.NormalizeDifficultyIndex(soloDifficultyIndex);
            soloOpponentDeckChoiceIndex = SoloPlayEntryFlow.NormalizeOpponentChoiceIndex(
                soloOpponentDeckChoiceIndex,
                LoadSavedDecksForSolo());
            if (soloSetupPanel != null)
            {
                soloSetupPanel.SetActive(true);
            }

            lastActionMessage = "Solo setup opened.";
            RefreshSoloSetupSummary();
            RefreshStatus();
        }

        private void OpenOnlineRoom()
        {
            if (latestDeck == null || latestDeck.TotalCards(DeckZone.Main) <= 0)
            {
                lastActionMessage = "No saved playable deck. Opening Deck Builder.";
                RefreshStatus();
                OpenDeckBuilder();
                return;
            }

            if (manifest == null)
            {
                lastActionMessage = "Card pack is not ready.";
                RefreshStatus();
                return;
            }

            VanguardDeck deckCopy = VanguardDeck.FromJson(latestDeck.ToJson(false));
            Destroy(gameObject);
            MultiplayerLobbyBootstrap.Show(deckCopy, manifest);
        }

        private void ShowCpuLocked()
        {
            lastActionMessage = "CPU fight unlocks after bot milestones.";
            RefreshStatus();
        }

        private void OpenReplay()
        {
            if (replayPanel != null)
            {
                replayPanel.SetActive(true);
            }

            RefreshReplaySummary();
            lastActionMessage = "Replay screen opened.";
            RefreshStatus();
        }

        private void CloseReplay()
        {
            if (replayPanel != null)
            {
                replayPanel.SetActive(false);
            }

            lastActionMessage = "Replay screen closed.";
            RefreshStatus();
        }

        private void OpenManual()
        {
            if (manualScreen == null)
            {
                manualScreen = ManualScreenOverlay.Create(transform, font, CloseManual);
            }
            else
            {
                manualScreen.SetActive(true);
            }

            lastActionMessage = "Manual opened.";
            RefreshStatus();
        }

        private void CloseManual()
        {
            if (manualScreen != null)
            {
                manualScreen.SetActive(false);
            }

            lastActionMessage = "Manual closed.";
            RefreshStatus();
        }

        private void OpenSettings()
        {
            playerSettings = PlayerSettings.Normalize(playerSettings);
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }

            lastActionMessage = "Settings opened.";
            RefreshStatus();
        }

        private void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            lastActionMessage = "Settings updated for this session.";
            RefreshStatus();
        }

        private void StartConfiguredSoloPlay()
        {
            IReadOnlyList<VanguardDeck> savedDecks = LoadSavedDecksForSolo();
            SoloPlayEntryFlowOptions options = SoloPlayEntryFlow.CreateOptionsFromUi(
                soloDifficultyIndex,
                soloOpponentDeckChoiceIndex,
                savedDecks,
                Environment.TickCount);
            SoloPlayEntryFlowStartResult result = SoloPlayEntryFlow.CreateStartRequest(
                latestDeck,
                latestDeckValidation,
                deckValidator,
                savedDecks,
                options);
            if (!result.can_start)
            {
                lastActionMessage = result.status_message;
                RefreshSoloSetupSummary();
                RefreshStatus();
                if (latestDeck == null || latestDeck.TotalCards(DeckZone.Main) <= 0)
                {
                    OpenDeckBuilder();
                }

                return;
            }

            Destroy(gameObject);
            PlayTableBootstrap.Show(result.player_deck, result.opponent_deck, result.playtable_mode_detail);
        }

        private void CloseSoloSetup()
        {
            if (soloSetupPanel != null)
            {
                soloSetupPanel.SetActive(false);
            }

            lastActionMessage = "Solo setup closed.";
            RefreshStatus();
        }

        private void CycleSoloDifficulty()
        {
            soloDifficultyIndex = SoloPlayEntryFlow.NextDifficultyIndex(soloDifficultyIndex);
            lastActionMessage = "Solo difficulty: " +
                                SoloPlayEntryFlow.FormatDifficultyLabel(
                                    SoloPlayEntryFlow.DifficultyFromIndex(soloDifficultyIndex));
            RefreshSoloSetupSummary();
            RefreshStatus();
        }

        private void CycleSoloOpponentDeck()
        {
            IReadOnlyList<VanguardDeck> savedDecks = LoadSavedDecksForSolo();
            soloOpponentDeckChoiceIndex = SoloPlayEntryFlow.NextOpponentChoiceIndex(
                soloOpponentDeckChoiceIndex,
                savedDecks);
            lastActionMessage = "Bot deck: " +
                                SoloPlayEntryFlow.FormatOpponentChoiceLabel(
                                    soloOpponentDeckChoiceIndex,
                                    savedDecks);
            RefreshSoloSetupSummary();
            RefreshStatus();
        }

        private void RefreshSoloSetupSummary()
        {
            if (soloSetupSummaryText == null)
            {
                return;
            }

            IReadOnlyList<VanguardDeck> savedDecks = LoadSavedDecksForSolo();
            soloDifficultyIndex = SoloPlayEntryFlow.NormalizeDifficultyIndex(soloDifficultyIndex);
            soloOpponentDeckChoiceIndex = SoloPlayEntryFlow.NormalizeOpponentChoiceIndex(
                soloOpponentDeckChoiceIndex,
                savedDecks);
            BotDifficulty difficulty = SoloPlayEntryFlow.DifficultyFromIndex(soloDifficultyIndex);
            string difficultyLabel = SoloPlayEntryFlow.FormatDifficultyLabel(difficulty);
            string opponentLabel = SoloPlayEntryFlow.FormatOpponentChoiceLabel(
                soloOpponentDeckChoiceIndex,
                savedDecks);
            SoloPlayEntryFlowOptions options = SoloPlayEntryFlow.CreateOptionsFromUi(
                soloDifficultyIndex,
                soloOpponentDeckChoiceIndex,
                savedDecks,
                0);
            SoloPlayEntryFlowStartResult result = SoloPlayEntryFlow.CreateStartRequest(
                latestDeck,
                latestDeckValidation,
                deckValidator,
                savedDecks,
                options);

            if (soloDifficultyText != null)
            {
                soloDifficultyText.text = "Difficulty: " + difficultyLabel;
            }

            if (soloOpponentDeckText != null)
            {
                soloOpponentDeckText.text = "Bot deck: " + opponentLabel;
            }

            soloSetupSummaryText.text = result.status_message;
        }

        private IReadOnlyList<VanguardDeck> LoadSavedDecksForSolo()
        {
            List<VanguardDeck> decks = new List<VanguardDeck>();
            DeckStorage storage = new DeckStorage();
            foreach (string deckId in storage.ListDeckIds())
            {
                try
                {
                    VanguardDeck deck = storage.Load(deckId);
                    if (deck != null)
                    {
                        decks.Add(deck);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Failed to load saved solo deck " + deckId + ": " + exception.Message);
                }
            }

            return decks;
        }

        private void CyclePreferredFormat()
        {
            playerSettings = PlayerSettings.Normalize(playerSettings);
            playerSettings.preferred_format = HomeSettingsPanelFormatter.NextPreferredFormat(playerSettings.preferred_format);
            lastActionMessage = "Preferred format: " + playerSettings.preferred_format;
            RefreshStatus();
        }

        private void CycleImageCacheMode()
        {
            playerSettings = PlayerSettings.Normalize(playerSettings);
            playerSettings.image_cache_mode = HomeSettingsPanelFormatter.NextImageCacheMode(playerSettings.image_cache_mode);
            lastActionMessage = "Image cache: " + playerSettings.image_cache_mode;
            RefreshStatus();
        }

        private void RefreshSettingsSummary()
        {
            if (settingsSummaryText != null)
            {
                settingsSummaryText.text = HomeSettingsPanelFormatter.Format(playerSettings);
            }
        }

        private void BuildSettingsPanel(RectTransform root, bool compact)
        {
            settingsPanel = CreatePanel("Settings Screen", root, new Color(0.06f, 0.07f, 0.09f, 0.98f));
            RectTransform panel = settingsPanel.GetComponent<RectTransform>();
            Stretch(panel, compact ? 22f : 140f, compact ? 22f : 140f, compact ? 86f : 110f, compact ? 22f : 90f);

            VerticalLayoutGroup layout = settingsPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = compact ? new RectOffset(18, 18, 18, 18) : new RectOffset(28, 28, 26, 26);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            float contentWidth = compact ? 360f : 520f;
            CreateLabel(settingsPanel.transform, "Settings", contentWidth, compact ? 22 : 26, TextAnchor.MiddleLeft, Color.white);
            settingsSummaryText = CreateMultilineLabel(
                settingsPanel.transform,
                "",
                contentWidth,
                compact ? 132f : 150f,
                compact ? 14 : 16,
                new Color(0.86f, 0.88f, 0.92f, 1f));

            RectTransform row = CreateRow(settingsPanel.transform, 52f);
            LayoutElement rowLayout = row.GetComponent<LayoutElement>();
            if (rowLayout != null)
            {
                rowLayout.preferredWidth = contentWidth;
            }

            CreateSecondaryButton(row, "Format", compact ? 106f : 128f, CyclePreferredFormat);
            CreateSecondaryButton(row, "Cache", compact ? 106f : 128f, CycleImageCacheMode);
            CreatePrimaryButton(row, "Close", compact ? 106f : 128f, CloseSettings);
            settingsPanel.SetActive(false);
            RefreshSettingsSummary();
        }

        private void BuildSoloSetupPanel(RectTransform root, bool compact)
        {
            soloSetupPanel = CreatePanel("Solo Play Setup Screen", root, new Color(0.06f, 0.07f, 0.09f, 0.98f));
            RectTransform panel = soloSetupPanel.GetComponent<RectTransform>();
            Stretch(panel, compact ? 22f : 140f, compact ? 22f : 140f, compact ? 86f : 110f, compact ? 22f : 90f);

            VerticalLayoutGroup layout = soloSetupPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = compact ? new RectOffset(18, 18, 18, 18) : new RectOffset(28, 28, 26, 26);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            float contentWidth = compact ? 360f : 560f;
            CreateLabel(soloSetupPanel.transform, "Solo Practice", contentWidth, compact ? 22 : 26, TextAnchor.MiddleLeft, Color.white);
            soloSetupSummaryText = CreateMultilineLabel(
                soloSetupPanel.transform,
                "",
                contentWidth,
                compact ? 118f : 132f,
                compact ? 14 : 16,
                new Color(0.86f, 0.88f, 0.92f, 1f));
            soloDifficultyText = CreateSingleLineLabel(
                soloSetupPanel.transform,
                "",
                contentWidth,
                compact ? 14 : 16,
                TextAnchor.MiddleLeft,
                new Color(0.86f, 0.88f, 0.92f, 1f));
            soloOpponentDeckText = CreateSingleLineLabel(
                soloSetupPanel.transform,
                "",
                contentWidth,
                compact ? 14 : 16,
                TextAnchor.MiddleLeft,
                new Color(0.86f, 0.88f, 0.92f, 1f));

            RectTransform rowOne = CreateRow(soloSetupPanel.transform, 52f);
            LayoutElement rowOneLayout = rowOne.GetComponent<LayoutElement>();
            if (rowOneLayout != null)
            {
                rowOneLayout.preferredWidth = contentWidth;
            }

            CreateSecondaryButton(rowOne, "Difficulty", compact ? 112f : 136f, CycleSoloDifficulty);
            CreateSecondaryButton(rowOne, "Bot Deck", compact ? 112f : 136f, CycleSoloOpponentDeck);

            RectTransform rowTwo = CreateRow(soloSetupPanel.transform, 52f);
            LayoutElement rowTwoLayout = rowTwo.GetComponent<LayoutElement>();
            if (rowTwoLayout != null)
            {
                rowTwoLayout.preferredWidth = contentWidth;
            }

            CreatePrimaryButton(rowTwo, "Start Solo", compact ? 126f : 150f, StartConfiguredSoloPlay);
            CreateSecondaryButton(rowTwo, "Close", compact ? 106f : 128f, CloseSoloSetup);
            soloSetupPanel.SetActive(false);
            RefreshSoloSetupSummary();
        }

        private void BuildReplayPanel(RectTransform root, bool compact)
        {
            replayPanel = CreatePanel("Replay Screen", root, new Color(0.06f, 0.07f, 0.09f, 0.98f));
            RectTransform panel = replayPanel.GetComponent<RectTransform>();
            Stretch(panel, compact ? 22f : 140f, compact ? 22f : 140f, compact ? 86f : 110f, compact ? 22f : 90f);

            VerticalLayoutGroup layout = replayPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = compact ? new RectOffset(18, 18, 18, 18) : new RectOffset(28, 28, 26, 26);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            float contentWidth = compact ? 360f : 560f;
            CreateLabel(replayPanel.transform, "Replay", contentWidth, compact ? 22 : 26, TextAnchor.MiddleLeft, Color.white);
            replaySummaryText = CreateMultilineLabel(
                replayPanel.transform,
                "",
                contentWidth,
                compact ? 126f : 142f,
                compact ? 14 : 16,
                new Color(0.86f, 0.88f, 0.92f, 1f));
            replaySummaryText.gameObject.name = "Replay Summary Text";
            replayPathInput = CreateInput(replayPanel.transform, "Replay JSON path", "", contentWidth);
            replayPreviewText = CreateMultilineLabel(
                replayPanel.transform,
                "",
                contentWidth,
                compact ? 106f : 118f,
                compact ? 14 : 15,
                new Color(0.86f, 0.88f, 0.92f, 1f));
            replayPreviewText.gameObject.name = "Replay Preview Text";

            RectTransform row = CreateRow(replayPanel.transform, 52f);
            LayoutElement rowLayout = row.GetComponent<LayoutElement>();
            if (rowLayout != null)
            {
                rowLayout.preferredWidth = contentWidth;
            }

            CreateSecondaryButton(row, "Load Path", compact ? 76f : 126f, LoadReplayPath);
            CreateSecondaryButton(row, "Start Preview", compact ? 94f : 142f, StartReplayPreview);
            CreateSecondaryButton(row, "Step Replay", compact ? 88f : 136f, StepReplayPreview);
            CreateSecondaryButton(row, "End Replay", compact ? 78f : 126f, EndReplayPreview);
            RectTransform closeRow = CreateRow(replayPanel.transform, 52f);
            CreatePrimaryButton(closeRow, "Close", compact ? 106f : 128f, CloseReplay);
            replayPanel.SetActive(false);
            RefreshReplaySummary();
            RefreshReplayPreview();
        }

        private void LoadReplayPath()
        {
            string path = replayPathInput == null ? string.Empty : (replayPathInput.text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                loadedReplay = null;
                replayPlayer = null;
                replaySummaryOverride = HomeReplayPanelFormatter.FormatError("Replay path is empty.");
                lastActionMessage = "Replay path is empty.";
                RefreshStatus();
                RefreshReplaySummary();
                RefreshReplayPreview();
                return;
            }

            if (!File.Exists(path))
            {
                loadedReplay = null;
                replayPlayer = null;
                replaySummaryOverride = HomeReplayPanelFormatter.FormatError("Replay file not found.");
                lastActionMessage = "Replay file not found.";
                RefreshStatus();
                RefreshReplaySummary();
                RefreshReplayPreview();
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                GameReplay replay = GameReplay.FromJson(json);
                replay.CreateInitialState();
                loadedReplay = replay;
                replayPlayer = new GameReplayPlayer(loadedReplay);
                int eventCount = replay.events == null ? 0 : replay.events.Count;
                replaySummaryOverride = HomeReplayPanelFormatter.FormatLoadedReplay(replay.replay_id, eventCount, path);
                lastActionMessage = "Replay loaded.";
            }
            catch (Exception exception)
            {
                loadedReplay = null;
                replayPlayer = null;
                replaySummaryOverride = HomeReplayPanelFormatter.FormatError(exception.Message);
                lastActionMessage = "Replay load failed.";
            }

            RefreshStatus();
            RefreshReplaySummary();
            RefreshReplayPreview();
        }

        private void StartReplayPreview()
        {
            if (!EnsureReplayPlayer())
            {
                return;
            }

            replayPlayer.JumpToStart();
            lastActionMessage = "Replay preview started.";
            RefreshStatus();
            RefreshReplayPreview();
        }

        private void StepReplayPreview()
        {
            if (!EnsureReplayPlayer())
            {
                return;
            }

            if (replayPlayer.StepForward())
            {
                lastActionMessage = "Replay stepped.";
            }
            else
            {
                lastActionMessage = "Replay is already at the end.";
            }

            RefreshStatus();
            RefreshReplayPreview();
        }

        private void EndReplayPreview()
        {
            if (!EnsureReplayPlayer())
            {
                return;
            }

            replayPlayer.JumpToEnd();
            lastActionMessage = "Replay jumped to end.";
            RefreshStatus();
            RefreshReplayPreview();
        }

        private bool EnsureReplayPlayer()
        {
            if (loadedReplay == null)
            {
                replaySummaryOverride = HomeReplayPanelFormatter.FormatError("Load a replay before previewing.");
                lastActionMessage = "Load a replay before previewing.";
                RefreshStatus();
                RefreshReplaySummary();
                RefreshReplayPreview();
                return false;
            }

            if (replayPlayer == null)
            {
                replayPlayer = new GameReplayPlayer(loadedReplay);
            }

            return true;
        }

        private void RefreshReplaySummary()
        {
            if (replaySummaryText != null)
            {
                replaySummaryText.text = string.IsNullOrEmpty(replaySummaryOverride)
                    ? HomeReplayPanelFormatter.FormatEmptyLibrary()
                    : replaySummaryOverride;
            }
        }

        private void RefreshReplayPreview()
        {
            if (replayPreviewText == null)
            {
                return;
            }

            if (replayPlayer == null || replayPlayer.CurrentState == null)
            {
                replayPreviewText.text = HomeReplayPanelFormatter.EmptyPreviewMessage;
                return;
            }

            GameState state = replayPlayer.CurrentState;
            replayPreviewText.text = HomeReplayPanelFormatter.FormatPreview(
                replayPlayer.CurrentIndex,
                replayPlayer.EventCount,
                state.turn_number,
                state.phase.ToString(),
                replayPlayer.IsAtEnd);
        }

        private InputField CreateInput(Transform parent, string placeholder, string value, float width)
        {
            GameObject root = CreatePanel(placeholder + " Input", parent, Color.white);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 46f;
            InputField input = root.AddComponent<InputField>();
            Text text = CreateText("Text", root.transform, value ?? string.Empty, 15, TextAnchor.MiddleLeft, Color.black);
            Stretch(text.rectTransform, 10, 10, 4, 4);
            Text placeholderText = CreateText("Placeholder", root.transform, placeholder, 15, TextAnchor.MiddleLeft, new Color(0.55f, 0.55f, 0.55f, 1f));
            Stretch(placeholderText.rectTransform, 10, 10, 4, 4);
            input.textComponent = text;
            input.placeholder = placeholderText;
            input.text = value ?? string.Empty;
            input.lineType = InputField.LineType.SingleLine;
            return input;
        }

        private RectTransform CreateColumnPanel(Transform parent, string name, float preferredWidth, float flexibleWidth, Color color)
        {
            RectTransform panel = CreatePanel(name, parent, color).GetComponent<RectTransform>();
            LayoutElement layoutElement = panel.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.flexibleWidth = flexibleWidth;

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return panel;
        }

        private RectTransform CreateRow(Transform parent, float height)
        {
            RectTransform row = CreatePanel("Row", parent, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            LayoutElement layoutElement = row.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 300;
            layoutElement.preferredHeight = height;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return row;
        }

        private void Space(Transform parent, float flexibleWidth)
        {
            GameObject space = new GameObject("Flexible Space");
            space.transform.SetParent(parent, false);
            LayoutElement layout = space.AddComponent<LayoutElement>();
            layout.flexibleWidth = flexibleWidth;
        }

        private void CreateDivider(Transform parent, float width)
        {
            GameObject divider = CreatePanel("Divider", parent, new Color(0.24f, 0.26f, 0.3f, 1f));
            LayoutElement layout = divider.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 1f;
        }

        private Button CreatePrimaryButton(Transform parent, string label, float width, UnityEngine.Events.UnityAction action)
        {
            return CreateButton(parent, label, width, new Color(0.78f, 0.24f, 0.64f, 1f), action);
        }

        private Button CreateSecondaryButton(Transform parent, string label, float width, UnityEngine.Events.UnityAction action)
        {
            return CreateButton(parent, label, width, new Color(0.22f, 0.25f, 0.3f, 1f), action);
        }

        private Button CreateButton(Transform parent, string label, float width, Color color, UnityEngine.Events.UnityAction action)
        {
            GameObject root = CreatePanel(label + " Button", parent, color);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 48f;
            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(action);
            Text text = CreateText("Label", root.transform, label, 17, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 8, 8, 4, 4);
            return button;
        }

        private Text CreatePill(Transform parent, string label, float width, Color color)
        {
            GameObject root = CreatePanel(label + " Pill", parent, color);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 34f;
            Text text = CreateText("Label", root.transform, label, 13, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 6, 6, 3, 3);
            return text;
        }

        private Text CreateSymbolPill(Transform parent, string symbolKey, float width, Color color)
        {
            UiGameSymbolResolution symbol = UiGameSymbolRegistry.Resolve(symbolKey, iconPackValidation);
            return CreatePill(parent, symbol.label, width, color);
        }

        private Text CreateLabel(Transform parent, string value, float width, int size, TextAnchor alignment, Color color)
        {
            Text text = CreateText("Label", parent, value, size, alignment, color);
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = Mathf.Max(36f, size + 16f);
            return text;
        }

        private Text CreateSingleLineLabel(Transform parent, string value, float width, int size, TextAnchor alignment, Color color)
        {
            Text text = CreateLabel(parent, value, width, size, alignment, color);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private Text CreateMultilineLabel(Transform parent, string value, float width, float height, int size, Color color)
        {
            Text text = CreateText("Multiline Label", parent, value, size, TextAnchor.UpperLeft, color);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            return text;
        }

        private Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.AddComponent<RectTransform>();
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
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

        private static void AnchorTop(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.offsetMin = new Vector2(0, -height);
            rect.offsetMax = Vector2.zero;
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
