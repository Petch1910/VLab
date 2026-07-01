using System;
using System.Collections.Generic;
using System.IO;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VanguardThaiSim.UI
{
    public sealed class PlayTableBootstrap : MonoBehaviour
    {
        private Font font;
        private GameState state;
        private GameState replayInitialState;
        private MultiplayerGameSessionController multiplayerSession;
        private int playerIndex;
        private string localModeDetail;
        private VanguardThaiSim.Cards.ICardRepository repository;
        private CardImageCache imageCache;
        private Text cardPreviewText;
        private string selectedCardInstanceId;
        private string selectedCardId;
        private GameZone selectedZone;
        private string selectedTargetCardInstanceId;
        private string selectedTargetCardId;
        private GameZone selectedTargetZone;
        private int pendingAutoAbilitySelectedIndex = -1;
        private PendingAutoAbilitySelectionState pendingAutoAbilitySelectionState =
            PendingAutoAbilitySelection.Clear();
        private string pendingAutoAbilityManualResolutionDecisionType =
            PendingAutoAbilityManualResolutionDecisionTypes.Resolve;
        private TriggerType manualDraftTriggerType = TriggerType.Unknown;
        private TriggerCheckSource manualDraftCheckSource = TriggerCheckSource.Manual;
        private int manualDraftCheckIndex;
        private readonly List<PlayTableManualNote> manualNotes = new List<PlayTableManualNote>();
        private CanvasScaler canvasScaler;
        private RectTransform toolbarRect;
        private RectTransform mainRect;
        private HorizontalLayoutGroup toolbarLayout;
        private HorizontalLayoutGroup mainLayout;
        private LayoutElement fieldRowLayout;
        private LayoutElement resourceRowLayout;
        private LayoutElement handPanelLayout;
        private LayoutElement sidePanelLayout;
        private LayoutElement commandDockLayout;
        private RectTransform sidePanelRect;
        private ResponsiveLayoutProfile activeLayoutProfile;
        private ResponsiveDeviceClass activeDeviceClass;
        private bool hasResponsiveProfile;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;
        private int lastRenderedEventCount = -1;
        private bool pendingMultiplayerRefresh;
        private GameObject advancedDrawer;
        private GameObject manualScreen;
        private CanvasGroup advancedDrawerCanvasGroup;
        private LayoutElement advancedDrawerLayout;

        private Text statusText;
        private Text opponentText;
        private Text logText;
        private Text fullLogText;
        private Text selectionText;
        private Text setupStatusText;
        private Text guidedNextActionText;
        private Text actionGroupLegendText;
        private Text battleFlowText;
        private Text botExplanationText;
        private Text manualNoteText;
        private Text handStripHeaderText;
        private Text zoneStatusText;
        private Text giftMarkerText;
        private Text onlineDebugText;
        private Text triggerCheckText;
        private Text pendingAutoAbilityText;
        private Text pendingAutoAbilityItemListText;
        private Text pendingAutoAbilitySelectionStatusText;
        private Text pendingAutoAbilityResolutionRequestPreviewText;
        private Text pendingAutoAbilityResolutionRequestSummaryText;
        private Text pendingAutoAbilityResolutionRequestListText;
        private Text pendingAutoAbilityManualResolutionDecisionSummaryText;
        private Text pendingAutoAbilityManualResolutionDecisionListText;
        private Text pendingAutoAbilityManualResolutionDecisionValidationText;
        private Text pendingAutoAbilityManualResolutionApplyPreviewLogLatestText;
        private Text pendingAutoAbilityManualResolutionApplyPreviewLogListText;
        private Text triggerCheckDraftSummaryText;
        private Button triggerCheckPublishButton;
        private Button drawButton;
        private Button standAndDrawPhaseButton;
        private Button ridePhaseButton;
        private Button mainPhaseButton;
        private Button battlePhaseButton;
        private Button endPhaseButton;
        private Button undoButton;
        private Button forceMarkerButton;
        private Button accelMarkerButton;
        private Button protectMarkerButton;
        private Button moveToVanguardButton;
        private Button moveToRearGuardButton;
        private Button moveToDropButton;
        private Button moveToDamageButton;
        private Button driveCheckButton;
        private Button damageCheckButton;
        private Button guardSelectedButton;
        private Button mulliganSelectedButton;
        private Button attackVanguardButton;
        private Button attackTargetButton;
        private Button manualNoteButton;
        private Button localSeatButton;
        private Button pendingAutoAbilityPublishButton;
        private Button pendingAutoAbilitySelectionButton;
        private Button pendingAutoAbilityResolutionRequestPublishButton;
        private Text pendingAutoAbilityManualResolutionDecisionTypeText;
        private Button pendingAutoAbilityManualResolutionDecisionDraftButton;
        private Button pendingAutoAbilityManualResolutionDecisionPublishButton;
        private Button pendingAutoAbilityManualResolutionApplyPreviewButton;
        private Button pendingAutoAbilityClearSelectionButton;
        private Button triggerCheckDraftButton;
        private Button triggerCheckDraftClearButton;
        private Text triggerCheckDraftTypeText;
        private Text triggerCheckDraftSourceText;
        private Text triggerCheckDraftIndexText;
        private RectTransform opponentVanguardContent;
        private RectTransform opponentRearGuardContent;
        private readonly Dictionary<GameZone, RectTransform> zoneContents = new Dictionary<GameZone, RectTransform>();
        private readonly List<ResponsiveLayoutBinding> responsiveBindings = new List<ResponsiveLayoutBinding>();

        public static void Show(VanguardDeck playerDeck, VanguardDeck opponentDeck)
        {
            Show(playerDeck, opponentDeck, string.Empty);
        }

        public static void Show(VanguardDeck playerDeck, VanguardDeck opponentDeck, string modeDetail)
        {
            PlayTableBootstrap existing = FindAnyObjectByType<PlayTableBootstrap>();
            if (existing != null)
            {
                DestroyRuntimeObject(existing.gameObject);
            }

            GameObject host = new GameObject("Vanguard Play Table");
            PlayTableBootstrap table = host.AddComponent<PlayTableBootstrap>();
            table.Initialize(playerDeck, opponentDeck, modeDetail);
        }

        public static void ShowOnline(MultiplayerGameSessionController session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            PlayTableBootstrap existing = FindAnyObjectByType<PlayTableBootstrap>();
            if (existing != null)
            {
                DestroyRuntimeObject(existing.gameObject);
            }

            GameObject host = new GameObject("Vanguard Online Play Table");
            PlayTableBootstrap table = host.AddComponent<PlayTableBootstrap>();
            table.Initialize(session.State, session, session.LocalPlayerIndex);
        }

        public int CurrentPlayerIndex => playerIndex;

        private void Initialize(VanguardDeck playerDeck, VanguardDeck opponentDeck, string modeDetail)
        {
            localModeDetail = string.IsNullOrWhiteSpace(modeDetail) ? string.Empty : modeDetail.Trim();
            Initialize(GameStateFactory.CreateTwoPlayerGame(playerDeck, opponentDeck, Environment.TickCount), null, 0);
        }

        private void Initialize(GameState initialState, MultiplayerGameSessionController session, int localPlayerIndex)
        {
            font = LoadFont();
            EnsureEventSystem();
            multiplayerSession = session;
            playerIndex = Mathf.Clamp(localPlayerIndex, 0, 1);
            state = multiplayerSession == null ? initialState : multiplayerSession.State;
            replayInitialState = multiplayerSession == null && initialState != null
                ? GameState.FromJson(initialState.ToJson(false))
                : null;
            if (replayInitialState != null)
            {
                replayInitialState.event_log.Clear();
            }

            if (multiplayerSession != null)
            {
                multiplayerSession.StateChanged += HandleMultiplayerStateChanged;
            }

            try
            {
                string packDirectory = CardPackFileSystem.DefaultPackDirectory;
                CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
                repository = CardRepositoryFactory.LoadDefault(packDirectory, manifest).Repository;
                try
                {
                    imageCache = new CardImageCache(CardPackFileSystem.GetImageRootPath(manifest), 40, 2);
                }
                catch (Exception imageException)
                {
                    Debug.LogWarning("Failed to load card image cache in PlayTable: " + imageException.Message);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to load repository in PlayTable: " + e.Message);
            }

            BuildUi();
            RefreshUi();
        }

        private void Update()
        {
            TickMultiplayerSession();
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
            if (changed && previousClass != activeDeviceClass && state != null)
            {
                RefreshUi();
            }
        }

        private void OnDestroy()
        {
            if (multiplayerSession != null)
            {
                multiplayerSession.StateChanged -= HandleMultiplayerStateChanged;
            }

            if (imageCache != null)
            {
                imageCache.Dispose();
                imageCache = null;
            }
        }

        private void BuildUi()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<GraphicRaycaster>();
            canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280, 720);
            canvasScaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = gameObject.GetComponent<RectTransform>();
            GameObject background = CreatePanel("Table Background", canvasRect, new Color(0.07f, 0.1f, 0.09f, 0.98f));
            Stretch(background.GetComponent<RectTransform>(), 0, 0, 0, 0);

            RectTransform toolbar = CreatePanel("Toolbar", canvasRect, new Color(0.12f, 0.14f, 0.13f, 1f)).GetComponent<RectTransform>();
            toolbarRect = toolbar;
            AnchorTop(toolbar, 70);
            toolbarLayout = toolbar.gameObject.AddComponent<HorizontalLayoutGroup>();
            toolbarLayout.padding = new RectOffset(12, 12, 10, 10);
            toolbarLayout.spacing = 8;
            toolbarLayout.childAlignment = TextAnchor.MiddleLeft;
            toolbarLayout.childControlHeight = true;
            toolbarLayout.childControlWidth = false;
            toolbarLayout.childForceExpandWidth = false;

            CreateLabel(toolbar, "Play", 58, 18, TextAnchor.MiddleCenter, true);
            CreateButton(toolbar, "Manual", 78, OpenManual);
            Button advancedButton = CreateButton(toolbar, PlayTableAdvancedDrawerFormatter.DrawerTitle, 96, ToggleAdvancedDrawer);
            advancedButton.gameObject.name = "Advanced Button";
            localSeatButton = CreateButton(toolbar, "Seat", 66, ToggleLocalSeat);
            localSeatButton.gameObject.name = "Seat P2 Button";
            CreateButton(toolbar, "Close", 64, delegate { DestroyRuntimeObject(gameObject); });
            statusText = CreateLabel(toolbar, "", 360, 15, TextAnchor.MiddleLeft, true);

            RectTransform main = CreatePanel("Main", canvasRect, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            mainRect = main;
            Stretch(main, 222, 0, 0, 70);
            mainLayout = main.gameObject.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = new RectOffset(12, 12, 12, 12);
            mainLayout.spacing = 12;
            mainLayout.childControlHeight = true;
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandWidth = false;

            RectTransform tablePanel = CreatePanel(
                PlayTableZoneFirstLayoutFormatter.BoardPanelName,
                main,
                new Color(0.09f, 0.12f, 0.11f, 1f)).GetComponent<RectTransform>();
            LayoutElement tablePanelLayout = tablePanel.gameObject.AddComponent<LayoutElement>();
            tablePanelLayout.minWidth = 0f;
            tablePanelLayout.flexibleWidth = 1f;
            VerticalLayoutGroup tableLayout = tablePanel.gameObject.AddComponent<VerticalLayoutGroup>();
            tableLayout.padding = new RectOffset(8, 8, 6, 6);
            tableLayout.spacing = 4;
            tableLayout.childControlHeight = true;
            tableLayout.childControlWidth = true;
            tableLayout.childForceExpandHeight = false;

            opponentText = CreateMultilineLabel(tablePanel, "", 900, 22, 12);

            RectTransform fieldMat = CreatePanel(
                "Area Field Mat",
                tablePanel,
                new Color(0.11f, 0.12f, 0.1f, 1f)).GetComponent<RectTransform>();
            fieldRowLayout = fieldMat.gameObject.AddComponent<LayoutElement>();
            fieldRowLayout.minHeight = 330f;
            fieldRowLayout.flexibleHeight = 1f;

            AddPlaymatSlotSkeleton(fieldMat);

            AddAreaZonePanel(fieldMat, GameZone.Damage, "Damage", 0.09f, 0.07f, 0.20f, 0.31f, false);
            AddAreaZonePanel(fieldMat, GameZone.Order, "Order", 0.09f, 0.36f, 0.22f, 0.51f, false);
            AddAreaZonePanel(fieldMat, GameZone.Deck, "Deck", 0.76f, 0.23f, PlayTableZoneFirstLayoutFormatter.MaximumDefaultRightFieldZoneEdge, 0.43f, false);
            AddAreaZonePanel(fieldMat, GameZone.Drop, "Drop", 0.76f, 0.04f, PlayTableZoneFirstLayoutFormatter.MaximumDefaultRightFieldZoneEdge, 0.21f, false);
            AddAreaZonePanel(fieldMat, GameZone.Bind, "Bind", 0.76f, 0.45f, PlayTableZoneFirstLayoutFormatter.MaximumDefaultRightFieldZoneEdge, 0.56f, false);
            AddAreaZonePanel(fieldMat, GameZone.RideDeck, "Ride Deck", 0.70f, 0.56f, PlayTableZoneFirstLayoutFormatter.MaximumDefaultRightFieldZoneEdge, 0.70f, false);
            AddAreaZonePanel(fieldMat, GameZone.Trigger, "Trigger Zone", 0.70f, 0.72f, PlayTableZoneFirstLayoutFormatter.MaximumDefaultRightFieldZoneEdge, 0.84f, false);
            AddAreaGiftPanel(fieldMat, 0.70f, 0.86f, PlayTableZoneFirstLayoutFormatter.MaximumDefaultRightFieldZoneEdge, 0.97f);

            opponentRearGuardContent = AddAreaPlaymatCardContentOverlay(
                fieldMat,
                "Opponent Rear-guard Card Content",
                0.28f,
                0.62f,
                0.72f,
                0.97f,
                true);
            opponentVanguardContent = AddAreaPlaymatZonePanel(
                fieldMat,
                GameZone.Vanguard,
                "Opponent Vanguard Slot",
                "VG",
                0.44f,
                0.55f,
                0.56f,
                0.73f,
                false,
                new Color(0.16f, 0.10f, 0.11f, 0.72f),
                false);

            AddAreaZonePanel(fieldMat, GameZone.Soul, "Soul", 0.58f, 0.47f, 0.70f, 0.58f, true);
            AddAreaPlaymatZonePanel(
                fieldMat,
                GameZone.Vanguard,
                "Local Vanguard Slot",
                "VG",
                0.44f,
                0.29f,
                0.56f,
                0.47f,
                false,
                new Color(0.13f, 0.20f, 0.17f, 0.78f),
                true);
            zoneContents[GameZone.RearGuard] = AddAreaPlaymatCardContentOverlay(
                fieldMat,
                "Local Rear-guard Card Content",
                0.28f,
                0.08f,
                0.72f,
                0.45f,
                true);

            RectTransform phaseRail = AddAreaPhaseRail(fieldMat);
            standAndDrawPhaseButton = CreatePhaseRailButton(phaseRail, "Stand", delegate { SetPhase(GamePhase.StandAndDraw); });
            drawButton = CreatePhaseRailButton(phaseRail, "Draw", Draw);
            ridePhaseButton = CreatePhaseRailButton(phaseRail, "Ride", delegate { SetPhase(GamePhase.Ride); });
            mainPhaseButton = CreatePhaseRailButton(phaseRail, "Main", delegate { SetPhase(GamePhase.Main); });
            battlePhaseButton = CreatePhaseRailButton(phaseRail, "Battle", delegate { SetPhase(GamePhase.Battle); });
            endPhaseButton = CreatePhaseRailButton(phaseRail, "End", delegate { SetPhase(GamePhase.End); });

            RectTransform commandDock = CreatePanel(
                "Area Command Dock",
                tablePanel,
                new Color(0.06f, 0.075f, 0.07f, 0.96f)).GetComponent<RectTransform>();
            commandDockLayout = commandDock.gameObject.AddComponent<LayoutElement>();
            commandDockLayout.minHeight = 68f;
            commandDockLayout.preferredHeight = 68f;
            commandDockLayout.flexibleHeight = 0f;
            VerticalLayoutGroup commandLayout = commandDock.gameObject.AddComponent<VerticalLayoutGroup>();
            commandLayout.padding = new RectOffset(6, 6, 3, 3);
            commandLayout.spacing = 3;
            commandLayout.childAlignment = TextAnchor.MiddleCenter;
            commandLayout.childControlHeight = true;
            commandLayout.childControlWidth = true;
            commandLayout.childForceExpandHeight = false;

            RectTransform primaryActionRow = CreateActionRow(commandDock, "Primary Action Row");
            moveToVanguardButton = CreateCommandButton(primaryActionRow, "VG", 44, delegate { MoveSelected(GameZone.Vanguard); });
            moveToRearGuardButton = CreateCommandButton(primaryActionRow, "RG", 44, delegate { MoveSelected(GameZone.RearGuard); });
            moveToDropButton = CreateCommandButton(primaryActionRow, "Drop", 54, delegate { MoveSelected(GameZone.Drop); });
            moveToDamageButton = CreateCommandButton(primaryActionRow, "Dmg", 48, delegate { MoveSelected(GameZone.Damage); });
            guardSelectedButton = CreateCommandButton(primaryActionRow, "Guard", 56, GuardSelected);
            mulliganSelectedButton = CreateCommandButton(primaryActionRow, "Mull", 52, MulliganSelected);
            attackVanguardButton = CreateCommandButton(primaryActionRow, "AtkVG", 60, AttackSelectedVanguard);
            attackTargetButton = CreateCommandButton(primaryActionRow, "AtkRG", 60, AttackSelectedTarget);
            manualNoteButton = CreateCommandButton(primaryActionRow, "Note", 50, AddManualNote);

            RectTransform moveActionRow = CreateActionRow(commandDock, "Move Action Row");
            driveCheckButton = CreateCommandButton(moveActionRow, "Drive", 56, delegate { TriggerCheck(TriggerCheckSource.Drive); });
            damageCheckButton = CreateCommandButton(moveActionRow, "DmgChk", 70, delegate { TriggerCheck(TriggerCheckSource.Damage); });
            undoButton = CreateCommandButton(moveActionRow, "Undo", 54, Undo);
            forceMarkerButton = CreateCommandButton(moveActionRow, "Force", 56, delegate { AddGiftMarker(GiftMarkerType.Force); });
            accelMarkerButton = CreateCommandButton(moveActionRow, "Accel", 56, delegate { AddGiftMarker(GiftMarkerType.Accel); });
            protectMarkerButton = CreateCommandButton(moveActionRow, "Protect", 64, delegate { AddGiftMarker(GiftMarkerType.Protect); });
            moveToRearGuardButton.gameObject.name = "Rear Button";
            moveToDamageButton.gameObject.name = "Damage Button";
            damageCheckButton.gameObject.name = "Damage Check Button";
            mulliganSelectedButton.gameObject.name = "Mulligan Button";
            attackVanguardButton.gameObject.name = "Atk VG Button";
            attackTargetButton.gameObject.name = "Atk Target Button";

            handPanelLayout = AddHandStripPanel(tablePanel, GameZone.Hand, "Hand");

            RectTransform sidePanel = CreatePanel("Side Panel", canvasRect, new Color(0.05f, 0.055f, 0.06f, 0.70f)).GetComponent<RectTransform>();
            sidePanelRect = sidePanel;
            AnchorRightHud(sidePanelRect, 168f, PlayTableZoneFirstLayoutFormatter.MaximumDefaultInspectHudHeight, 76f, 10f);
            sidePanelLayout = sidePanel.gameObject.AddComponent<LayoutElement>();
            sidePanelLayout.preferredWidth = 168;
            sidePanelLayout.minWidth = 168;
            sidePanelLayout.flexibleWidth = 0;
            VerticalLayoutGroup sideVertical = sidePanel.gameObject.AddComponent<VerticalLayoutGroup>();
            sideVertical.padding = new RectOffset(6, 6, 6, 6);
            sideVertical.spacing = 4;
            sideVertical.childControlHeight = false;
            sideVertical.childControlWidth = true;
            sideVertical.childForceExpandWidth = false;

            CreateLabel(sidePanel, "Inspect", 150, 16, TextAnchor.MiddleCenter, true);
            selectionText = CreateMultilineLabel(sidePanel, "", 150, 30, 11);
            CreateLabel(sidePanel, "Selected", 150, 15, TextAnchor.MiddleCenter, true);
            cardPreviewText = CreateMultilineLabel(sidePanel, "", 150, 78, 10);
            cardPreviewText.alignment = TextAnchor.UpperLeft;
            CreateLabel(sidePanel, "Next", 150, 15, TextAnchor.MiddleCenter, true);
            guidedNextActionText = CreateMultilineLabel(sidePanel, "", 150, 46, 10);
            guidedNextActionText.gameObject.name = "PlayTable Guided Next Action";
            BuildAdvancedDrawer(sidePanel);
            Transform advancedParent = advancedDrawer == null ? sidePanel : advancedDrawer.transform;

            CreateLabel(advancedParent, "Action Groups", 260, 18, TextAnchor.MiddleCenter, true);
            actionGroupLegendText = CreateMultilineLabel(advancedParent, "", 260, 58, 11);
            actionGroupLegendText.gameObject.name = "PlayTable Action Group Legend";
            CreateLabel(advancedParent, "Setup", 260, 18, TextAnchor.MiddleCenter, true);
            setupStatusText = CreateMultilineLabel(advancedParent, "", 260, 46, 12);
            setupStatusText.gameObject.name = "PlayTable Setup Status";
            CreateLabel(advancedParent, "Battle Flow", 260, 18, TextAnchor.MiddleCenter, true);
            battleFlowText = CreateMultilineLabel(advancedParent, "", 260, 42, 12);
            battleFlowText.gameObject.name = "PlayTable Battle Flow Status";
            CreateLabel(advancedParent, "Manual Notes", 260, 18, TextAnchor.MiddleCenter, true);
            manualNoteText = CreateMultilineLabel(advancedParent, "", 260, 58, 11);
            manualNoteText.gameObject.name = "PlayTable Manual Notes";
            CreateLabel(advancedParent, "Zone Status", 260, 20, TextAnchor.MiddleCenter, true);
            zoneStatusText = CreateMultilineLabel(advancedParent, "", 260, 72, 11);
            zoneStatusText.gameObject.name = "PlayTable Zone Status";

            CreateLabel(advancedParent, "Bot Plan", 280, 18, TextAnchor.MiddleCenter, true);
            botExplanationText = CreateMultilineLabel(advancedParent, BotExplanationPanelFormatter.EmptyMessage, 280, 64, 11);
            botExplanationText.gameObject.name = "PlayTable Bot Explanation";

            CreateLabel(advancedParent, "Online Debug", 280, 20, TextAnchor.MiddleCenter, true);
            onlineDebugText = CreateMultilineLabel(advancedParent, "", 280, 54, 11);
            onlineDebugText.gameObject.name = "Online Debug Status";

            CreateLabel(advancedParent, "Trigger Checks", 280, 20, TextAnchor.MiddleCenter, true);
            triggerCheckDraftSummaryText = CreateMultilineLabel(advancedParent, "", 280, 40, 12);
            triggerCheckDraftSummaryText.gameObject.name = "Trigger Draft Summary";
            triggerCheckText = CreateMultilineLabel(advancedParent, "", 280, 74, 12);
            triggerCheckText.gameObject.name = "Trigger Check Summary";
            CreateLabel(advancedParent, "Pending Abilities", 280, 20, TextAnchor.MiddleCenter, true);
            pendingAutoAbilityText = CreateMultilineLabel(advancedParent, "", 280, 74, 12);
            pendingAutoAbilityText.gameObject.name = "Pending Ability Summary";
            pendingAutoAbilityItemListText = CreateMultilineLabel(advancedParent, "", 280, 92, 11);
            pendingAutoAbilityItemListText.gameObject.name = "Pending Ability Items";
            pendingAutoAbilitySelectionStatusText = CreateMultilineLabel(advancedParent, "", 280, 38, 11);
            pendingAutoAbilitySelectionStatusText.gameObject.name = "Pending Ability Selection Status";
            pendingAutoAbilityResolutionRequestPreviewText = CreateMultilineLabel(advancedParent, "", 280, 44, 11);
            pendingAutoAbilityResolutionRequestPreviewText.gameObject.name = "Pending Ability Resolution Request Preview";
            pendingAutoAbilityResolutionRequestSummaryText = CreateMultilineLabel(advancedParent, "", 280, 54, 11);
            pendingAutoAbilityResolutionRequestSummaryText.gameObject.name = "Pending Ability Resolution Request Summary";
            pendingAutoAbilityResolutionRequestListText = CreateMultilineLabel(advancedParent, "", 280, 78, 10);
            pendingAutoAbilityResolutionRequestListText.gameObject.name = "Pending Ability Resolution Request List";
            pendingAutoAbilityManualResolutionDecisionSummaryText = CreateMultilineLabel(advancedParent, "", 280, 54, 10);
            pendingAutoAbilityManualResolutionDecisionSummaryText.gameObject.name =
                "Pending Ability Manual Resolution Decision Summary";
            pendingAutoAbilityManualResolutionDecisionListText = CreateMultilineLabel(advancedParent, "", 280, 74, 10);
            pendingAutoAbilityManualResolutionDecisionListText.gameObject.name =
                "Pending Ability Manual Resolution Decision List";
            pendingAutoAbilityManualResolutionDecisionValidationText = CreateMultilineLabel(advancedParent, "", 280, 38, 10);
            pendingAutoAbilityManualResolutionDecisionValidationText.gameObject.name =
                "Pending Ability Manual Resolution Decision Validation";
            pendingAutoAbilityManualResolutionApplyPreviewLogLatestText =
                CreateMultilineLabel(advancedParent, "", 280, 38, 10);
            pendingAutoAbilityManualResolutionApplyPreviewLogLatestText.gameObject.name =
                "Pending Ability Manual Resolution Apply Preview Log Latest";
            pendingAutoAbilityManualResolutionApplyPreviewLogListText =
                CreateMultilineLabel(advancedParent, "", 280, 58, 10);
            pendingAutoAbilityManualResolutionApplyPreviewLogListText.gameObject.name =
                "Pending Ability Manual Resolution Apply Preview Log List";
            CreateLabel(advancedParent, "Full Match Log", 280, 18, TextAnchor.MiddleCenter, true);
            fullLogText = CreateMultilineLabel(advancedParent, "", 280, 210, 11);
            fullLogText.gameObject.name = "PlayTable Full Match Log";

            CreateLabel(advancedParent, "Recent Log", 280, 18, TextAnchor.MiddleCenter, true);
            logText = CreateMultilineLabel(advancedParent, "", 280, 96, 11);
            logText.gameObject.name = "PlayTable Compact Match Log";
            ApplyResponsiveLayout();
        }

        private void BuildAdvancedDrawer(Transform parent)
        {
            advancedDrawer = CreatePanel("Advanced Drawer", parent, new Color(0.1f, 0.12f, 0.14f, 1f));
            advancedDrawerCanvasGroup = advancedDrawer.AddComponent<CanvasGroup>();
            advancedDrawerLayout = advancedDrawer.AddComponent<LayoutElement>();
            advancedDrawerLayout.preferredWidth = 280;
            advancedDrawerLayout.preferredHeight = 980;
            VerticalLayoutGroup drawerVertical = advancedDrawer.AddComponent<VerticalLayoutGroup>();
            drawerVertical.padding = new RectOffset(8, 8, 8, 8);
            drawerVertical.spacing = 6;
            drawerVertical.childControlHeight = false;
            drawerVertical.childControlWidth = true;

            CreateLabel(advancedDrawer.transform, PlayTableAdvancedDrawerFormatter.DrawerTitle, 260, 18, TextAnchor.MiddleCenter, true);

            RectTransform rowOne = CreateAdvancedRow(advancedDrawer.transform);
            triggerCheckPublishButton = CreateButton(rowOne, "TrigLog", 78, PublishLatestTriggerCheckReplayLog);
            pendingAutoAbilityPublishButton = CreateButton(rowOne, "AutoQ", 74, PublishLatestPendingAutoAbilityQueue);
            pendingAutoAbilitySelectionButton = CreateButton(rowOne, "SelAuto", 82, CyclePendingAutoAbilitySelection);

            RectTransform rowTwo = CreateAdvancedRow(advancedDrawer.transform);
            pendingAutoAbilityResolutionRequestPublishButton =
                CreateButton(rowTwo, "ReqAuto", 84, PublishPendingAutoAbilityResolutionRequest);
            Button decisionTypeButton =
                CreateButton(rowTwo, "DecType", 96, CyclePendingAutoAbilityManualResolutionDecisionType);
            pendingAutoAbilityManualResolutionDecisionTypeText = decisionTypeButton.GetComponentInChildren<Text>();

            RectTransform rowThree = CreateAdvancedRow(advancedDrawer.transform);
            pendingAutoAbilityManualResolutionDecisionDraftButton =
                CreateButton(rowThree, "DraftDec", 88, CreatePendingAutoAbilityManualResolutionDecisionDraft);
            pendingAutoAbilityManualResolutionDecisionPublishButton =
                CreateButton(rowThree, "DecAuto", 84, PublishLatestPendingAutoAbilityManualResolutionDecision);
            pendingAutoAbilityManualResolutionApplyPreviewButton =
                CreateButton(rowThree, "ApplyDec", 92, PreviewApplyLatestPendingAutoAbilityManualResolutionDecision);

            RectTransform rowFour = CreateAdvancedRow(advancedDrawer.transform);
            pendingAutoAbilityClearSelectionButton = CreateButton(rowFour, "ClrAuto", 82, ClearPendingAutoAbilitySelection);
            triggerCheckDraftButton = CreateButton(rowFour, "DraftTrig", 90, PublishManualTriggerCheckDraft);
            triggerCheckDraftClearButton = CreateButton(rowFour, "ClrDraft", 86, ClearManualTriggerDraftSelection);

            RectTransform rowFive = CreateAdvancedRow(advancedDrawer.transform);
            Button draftTypeButton = CreateButton(rowFive, "TrigType", 92, CycleManualDraftTriggerType);
            triggerCheckDraftTypeText = draftTypeButton.GetComponentInChildren<Text>();
            Button draftSourceButton = CreateButton(rowFive, "ChkSrc", 84, CycleManualDraftCheckSource);
            triggerCheckDraftSourceText = draftSourceButton.GetComponentInChildren<Text>();
            Button draftIndexButton = CreateButton(rowFive, "ChkIdx", 74, CycleManualDraftCheckIndex);
            triggerCheckDraftIndexText = draftIndexButton.GetComponentInChildren<Text>();

            RectTransform rowSix = CreateAdvancedRow(advancedDrawer.transform);
            CreateButton(rowSix, "Export Replay", 132, ExportLocalReplay);

            SetAdvancedDrawerVisible(false);
        }

        private RectTransform CreateAdvancedRow(Transform parent)
        {
            RectTransform row = CreatePanel("Advanced Row", parent, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 40;
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 5;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = false;
            return row;
        }

        private void ToggleAdvancedDrawer()
        {
            if (advancedDrawer == null)
            {
                return;
            }

            bool visible = advancedDrawerCanvasGroup == null || advancedDrawerCanvasGroup.alpha <= 0.01f;
            SetAdvancedDrawerVisible(visible);
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

            SetSelectionMessage("Manual opened.");
        }

        private void CloseManual()
        {
            if (manualScreen != null)
            {
                manualScreen.SetActive(false);
            }

            SetSelectionMessage("Manual closed.");
        }

        private void SetAdvancedDrawerVisible(bool visible)
        {
            if (advancedDrawer == null)
            {
                return;
            }

            if (advancedDrawerCanvasGroup != null)
            {
                advancedDrawerCanvasGroup.alpha = visible ? 1f : 0f;
                advancedDrawerCanvasGroup.interactable = visible;
                advancedDrawerCanvasGroup.blocksRaycasts = visible;
            }

            if (advancedDrawerLayout != null)
            {
                advancedDrawerLayout.ignoreLayout = !visible;
            }
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
            AnchorTop(toolbarRect, profile.PlayToolbarHeight);
            Stretch(mainRect, 0f, 0f, 0, profile.PlayToolbarHeight);
            if (sidePanelRect != null)
            {
                AnchorRightHud(
                    sidePanelRect,
                    profile.PlaySidePanelWidth,
                    PlayTableZoneFirstLayoutFormatter.MaximumDefaultInspectHudHeight,
                    profile.PlayToolbarHeight + 6f,
                    10f);
            }

            toolbarLayout.padding = profile.IsCompact ? new RectOffset(8, 8, 8, 8) : new RectOffset(12, 12, 8, 8);
            toolbarLayout.spacing = profile.IsCompact ? 4f : 8f;
            mainLayout.padding = profile.IsCompact ? new RectOffset(8, 8, 8, 8) : new RectOffset(12, 12, 12, 12);
            mainLayout.spacing = profile.IsCompact ? 8f : 12f;

            fieldRowLayout.minHeight = Mathf.Max(300f, profile.PlayFieldRowHeight);
            fieldRowLayout.preferredHeight = 0f;
            fieldRowLayout.flexibleHeight = 1f;
            if (resourceRowLayout != null)
            {
                resourceRowLayout.minHeight = Mathf.Max(64f, profile.PlayResourceRowHeight);
                resourceRowLayout.preferredHeight = Mathf.Max(64f, profile.PlayResourceRowHeight);
                resourceRowLayout.flexibleHeight = 0f;
            }

            if (commandDockLayout != null)
            {
                commandDockLayout.minHeight = Mathf.Max(64f, profile.PlayResourceRowHeight);
                commandDockLayout.preferredHeight = Mathf.Max(64f, profile.PlayResourceRowHeight);
                commandDockLayout.flexibleHeight = 0f;
            }

            handPanelLayout.minHeight = Mathf.Max(164f, profile.PlayHandHeight);
            handPanelLayout.preferredHeight = Mathf.Max(164f, profile.PlayHandHeight);
            handPanelLayout.flexibleHeight = 0f;
            sidePanelLayout.preferredWidth = profile.PlaySidePanelWidth;
            sidePanelLayout.minWidth = profile.PlaySidePanelWidth;

            responsiveBindings.RemoveAll(binding => !binding.IsAlive);
            foreach (ResponsiveLayoutBinding binding in responsiveBindings)
            {
                binding.Apply(profile);
            }

            return deviceClassChanged;
        }

        private RectTransform AddAreaZonePanel(
            Transform parent,
            GameZone zone,
            string title,
            float minX,
            float minY,
            float maxX,
            float maxY,
            bool horizontalContent)
        {
            RectTransform panel = CreatePanel(
                title + " Area Panel",
                parent,
                new Color(0.12f, 0.15f, 0.14f, 0.82f)).GetComponent<RectTransform>();
            AnchorBox(panel, minX, minY, maxX, maxY, 4f);
            RectTransform content = ConfigureAreaZonePanel(panel, title, horizontalContent);
            zoneContents[zone] = content;
            return content;
        }

        private RectTransform AddAreaOpponentZonePanel(
            Transform parent,
            string title,
            float minX,
            float minY,
            float maxX,
            float maxY,
            bool horizontalContent)
        {
            RectTransform panel = CreatePanel(
                title + " Area Panel",
                parent,
                new Color(0.15f, 0.10f, 0.11f, 0.82f)).GetComponent<RectTransform>();
            AnchorBox(panel, minX, minY, maxX, maxY, 4f);
            return ConfigureAreaZonePanel(panel, title, horizontalContent);
        }

        private void AddPlaymatSlotSkeleton(Transform parent)
        {
            Color opponentSlot = new Color(0.23f, 0.17f, 0.11f, 0.36f);
            Color localSlot = new Color(0.17f, 0.23f, 0.12f, 0.42f);
            Color centerSlot = new Color(0.08f, 0.20f, 0.20f, 0.46f);

            AddAreaPlaymatVisualSlot(parent, "Opponent Back Left RG Slot", "RG", 0.28f, 0.80f, 0.39f, 0.97f, opponentSlot);
            AddAreaPlaymatVisualSlot(parent, "Opponent Back Center RG Slot", "RG", 0.445f, 0.80f, 0.555f, 0.97f, opponentSlot);
            AddAreaPlaymatVisualSlot(parent, "Opponent Back Right RG Slot", "RG", 0.61f, 0.80f, 0.72f, 0.97f, opponentSlot);
            AddAreaPlaymatVisualSlot(parent, "Opponent Front Left RG Slot", "RG", 0.335f, 0.61f, 0.445f, 0.78f, opponentSlot);
            AddAreaPlaymatVisualSlot(parent, "Opponent Front Right RG Slot", "RG", 0.555f, 0.61f, 0.665f, 0.78f, opponentSlot);
            AddAreaPlaymatVisualSlot(parent, "Opponent Vanguard Reference Slot", "VG", 0.445f, 0.56f, 0.555f, 0.74f, centerSlot);

            AddAreaPlaymatVisualSlot(parent, "Local Front Left RG Slot", "RG", 0.335f, 0.28f, 0.445f, 0.45f, localSlot);
            AddAreaPlaymatVisualSlot(parent, "Local Front Right RG Slot", "RG", 0.555f, 0.28f, 0.665f, 0.45f, localSlot);
            AddAreaPlaymatVisualSlot(parent, "Local Vanguard Reference Slot", "VG", 0.445f, 0.27f, 0.555f, 0.47f, centerSlot);
            AddAreaPlaymatVisualSlot(parent, "Local Back Left RG Slot", "RG", 0.28f, 0.08f, 0.39f, 0.25f, localSlot);
            AddAreaPlaymatVisualSlot(parent, "Local Back Center RG Slot", "RG", 0.445f, 0.06f, 0.555f, 0.23f, localSlot);
            AddAreaPlaymatVisualSlot(parent, "Local Back Right RG Slot", "RG", 0.61f, 0.08f, 0.72f, 0.25f, localSlot);
        }

        private RectTransform AddAreaPlaymatVisualSlot(
            Transform parent,
            string name,
            string label,
            float minX,
            float minY,
            float maxX,
            float maxY,
            Color color)
        {
            RectTransform panel = CreatePanel(name, parent, color).GetComponent<RectTransform>();
            AnchorBox(panel, minX, minY, maxX, maxY, 2f);

            Text text = CreateText(name + " Label", panel, label, 18, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.58f, 0.72f));
            Stretch(text.rectTransform, 2f, 2f, 2f, 2f);
            return panel;
        }

        private RectTransform AddAreaPlaymatZonePanel(
            Transform parent,
            GameZone zone,
            string name,
            string title,
            float minX,
            float minY,
            float maxX,
            float maxY,
            bool horizontalContent,
            Color color,
            bool registerLocalZone)
        {
            RectTransform panel = CreatePanel(name, parent, color).GetComponent<RectTransform>();
            AnchorBox(panel, minX, minY, maxX, maxY, 2f);
            RectTransform content = ConfigureAreaZonePanel(panel, title, horizontalContent);
            if (registerLocalZone)
            {
                zoneContents[zone] = content;
            }

            return content;
        }

        private RectTransform AddAreaPlaymatCardContentOverlay(
            Transform parent,
            string name,
            float minX,
            float minY,
            float maxX,
            float maxY,
            bool horizontalContent)
        {
            RectTransform content = new GameObject(name).AddComponent<RectTransform>();
            content.transform.SetParent(parent, false);
            AnchorBox(content, minX, minY, maxX, maxY, 2f);
            if (horizontalContent)
            {
                HorizontalLayoutGroup horizontal = content.gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontal.spacing = 5f;
                horizontal.childAlignment = TextAnchor.MiddleCenter;
                horizontal.childControlHeight = false;
                horizontal.childControlWidth = false;
                horizontal.childForceExpandHeight = false;
                horizontal.childForceExpandWidth = false;
            }
            else
            {
                VerticalLayoutGroup vertical = content.gameObject.AddComponent<VerticalLayoutGroup>();
                vertical.spacing = 4f;
                vertical.childAlignment = TextAnchor.MiddleCenter;
                vertical.childControlHeight = false;
                vertical.childControlWidth = false;
            }

            return content;
        }

        private void AddAreaGiftPanel(
            Transform parent,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            RectTransform panel = CreatePanel(
                "Gift Marker Area Panel",
                parent,
                new Color(0.13f, 0.12f, 0.17f, 0.82f)).GetComponent<RectTransform>();
            AnchorBox(panel, minX, minY, maxX, maxY, 4f);

            VerticalLayoutGroup vertical = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(6, 6, 5, 5);
            vertical.spacing = 4;
            vertical.childControlHeight = false;
            vertical.childControlWidth = true;

            CreateLabel(panel, "Gift Marker", 120, 14, TextAnchor.MiddleCenter);
            giftMarkerText = CreateMultilineLabel(panel, "", 120, 28, 11);
        }

        private RectTransform AddAreaPhaseRail(Transform parent)
        {
            RectTransform rail = CreatePanel(
                "Phase Rail",
                parent,
                new Color(0.05f, 0.06f, 0.07f, 0.74f)).GetComponent<RectTransform>();
            AnchorBox(rail, 0.01f, 0.08f, 0.075f, 0.94f, 3f);

            VerticalLayoutGroup vertical = rail.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(4, 4, 6, 6);
            vertical.spacing = 5;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlHeight = true;
            vertical.childControlWidth = true;
            vertical.childForceExpandHeight = false;
            vertical.childForceExpandWidth = true;

            CreateLabel(rail, "TURN", 58, 13, TextAnchor.MiddleCenter);
            return rail;
        }

        private RectTransform ConfigureAreaZonePanel(RectTransform panel, string title, bool horizontalContent)
        {
            VerticalLayoutGroup vertical = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(5, 5, 4, 4);
            vertical.spacing = 3;
            vertical.childControlHeight = false;
            vertical.childControlWidth = true;
            vertical.childForceExpandHeight = false;

            Text titleText = CreateLabel(panel, title, 150, 12, TextAnchor.MiddleCenter);
            LayoutElement titleLayout = titleText.GetComponent<LayoutElement>();
            if (titleLayout != null)
            {
                titleLayout.preferredHeight = 16f;
            }
            RectTransform content = new GameObject(title + " Content").AddComponent<RectTransform>();
            content.transform.SetParent(panel, false);
            content.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            if (horizontalContent)
            {
                HorizontalLayoutGroup horizontal = content.gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontal.spacing = 4;
                horizontal.childControlHeight = true;
                horizontal.childControlWidth = false;
            }
            else
            {
                VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
                contentLayout.spacing = 2;
                contentLayout.childControlHeight = false;
                contentLayout.childControlWidth = true;
            }

            return content;
        }

        private LayoutElement AddZonePanel(Transform parent, GameZone zone, string title, float widthOrFlex)
        {
            RectTransform panel = CreatePanel(title + " Panel", parent, new Color(0.15f, 0.18f, 0.17f, 1f)).GetComponent<RectTransform>();
            LayoutElement layout = panel.gameObject.AddComponent<LayoutElement>();
            if (widthOrFlex > 10)
            {
                layout.preferredWidth = widthOrFlex;
            }
            else
            {
                layout.flexibleWidth = widthOrFlex;
            }

            VerticalLayoutGroup vertical = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(8, 8, 8, 8);
            vertical.spacing = 5;
            vertical.childControlHeight = false;
            vertical.childControlWidth = true;
            vertical.childForceExpandHeight = false;

            CreateLabel(panel, title, 120, 15, TextAnchor.MiddleCenter);
            RectTransform content = new GameObject(title + " Content").AddComponent<RectTransform>();
            content.transform.SetParent(panel, false);
            content.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 4;
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            zoneContents[zone] = content;
            return layout;
        }

        private RectTransform AddOpponentZonePanel(Transform parent, string title, float widthOrFlex)
        {
            RectTransform panel = CreatePanel(title + " Panel", parent, new Color(0.16f, 0.13f, 0.13f, 1f)).GetComponent<RectTransform>();
            LayoutElement layout = panel.gameObject.AddComponent<LayoutElement>();
            if (widthOrFlex > 10)
            {
                layout.preferredWidth = widthOrFlex;
            }
            else
            {
                layout.flexibleWidth = widthOrFlex;
            }

            VerticalLayoutGroup vertical = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(8, 8, 8, 8);
            vertical.spacing = 5;
            vertical.childControlHeight = false;
            vertical.childControlWidth = true;

            CreateLabel(panel, title, 160, 15, TextAnchor.MiddleCenter);
            RectTransform content = new GameObject(title + " Content").AddComponent<RectTransform>();
            content.transform.SetParent(panel, false);
            content.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            HorizontalLayoutGroup contentLayout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            contentLayout.spacing = 6;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = false;
            return content;
        }

        private LayoutElement AddHandStripPanel(Transform parent, GameZone zone, string title)
        {
            RectTransform panel = CreatePanel(title + " Panel", parent, new Color(0.14f, 0.17f, 0.16f, 1f)).GetComponent<RectTransform>();
            LayoutElement layout = panel.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 164;
            layout.preferredHeight = 164;
            layout.flexibleHeight = 0;

            VerticalLayoutGroup vertical = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(8, 8, 8, 8);
            vertical.spacing = 5;
            vertical.childControlHeight = false;
            vertical.childControlWidth = true;
            vertical.childForceExpandHeight = false;

            Text header = CreateLabel(panel, PlayTableHandStripFormatter.FormatHeader(0), 160, 15, TextAnchor.MiddleLeft);
            if (zone == GameZone.Hand)
            {
                handStripHeaderText = header;
            }

            RectTransform content = new GameObject(title + " Content").AddComponent<RectTransform>();
            content.transform.SetParent(panel, false);
            LayoutElement contentLayoutElement = content.gameObject.AddComponent<LayoutElement>();
            contentLayoutElement.minHeight = PlayTableHandStripFormatter.CardButtonHeight + 8f;
            contentLayoutElement.preferredHeight = PlayTableHandStripFormatter.CardButtonHeight + 8f;
            contentLayoutElement.flexibleHeight = 0f;
            HorizontalLayoutGroup contentLayout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            contentLayout.spacing = 6;
            contentLayout.childAlignment = TextAnchor.MiddleLeft;
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = false;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = false;
            zoneContents[zone] = content;
            return layout;
        }

        private void AddGiftPanel(Transform parent)
        {
            RectTransform panel = CreatePanel("Gift Marker Panel", parent, new Color(0.18f, 0.16f, 0.21f, 1f)).GetComponent<RectTransform>();
            LayoutElement layout = panel.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 170;
            VerticalLayoutGroup vertical = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(8, 8, 8, 8);
            vertical.spacing = 5;
            vertical.childControlHeight = false;
            vertical.childControlWidth = true;

            CreateLabel(panel, "Gift Marker", 140, 15, TextAnchor.MiddleCenter);
            giftMarkerText = CreateMultilineLabel(panel, "", 140, 100, 13);
        }

        private void Draw()
        {
            ExecuteFirstLegalAction(
                action => action.action_type == GameActionType.Draw,
                PlayTableActionStatusFormatter.FormatCannotDraw());
        }

        private void MoveSelected(GameZone targetZone)
        {
            if (string.IsNullOrEmpty(selectedCardInstanceId))
            {
                SetSelectionMessage(PlayTableCardSelectionStatusFormatter.FormatSelectCardFirst());
                return;
            }

            try
            {
                RulesCommandResult result = ExecuteAction(new LegalGameAction
                {
                    action_type = GameActionType.MoveCard,
                    actor_index = playerIndex,
                    card_instance_id = selectedCardInstanceId,
                    from_zone = selectedZone,
                    to_zone = targetZone
                });
                if (!result.accepted)
                {
                    SetSelectionMessage(result.rejection_reason);
                    return;
                }

                selectedCardInstanceId = null;
                selectedCardId = null;
                RefreshUi();
            }
            catch (Exception exception)
            {
                SetSelectionMessage(exception.Message);
            }
        }

        private void SetPhase(GamePhase phase)
        {
            ExecuteFirstLegalAction(
                action => action.action_type == GameActionType.SetPhase && action.phase == phase,
                PlayTableActionStatusFormatter.FormatCannotSetPhase(phase));
        }

        private void AddGiftMarker(GiftMarkerType markerType)
        {
            ExecuteFirstLegalAction(
                action => action.action_type == GameActionType.AddGiftMarker && action.gift_marker_type == markerType,
                PlayTableActionStatusFormatter.FormatCannotAddGiftMarker(markerType));
        }

        private void TriggerCheck(TriggerCheckSource checkSource)
        {
            ExecuteFirstLegalAction(
                action => action.action_type == GameActionType.TriggerCheck &&
                    action.trigger_check_source == checkSource,
                PlayTableActionStatusFormatter.FormatCannotTriggerCheck());
        }

        private void GuardSelected()
        {
            if (string.IsNullOrEmpty(selectedCardInstanceId))
            {
                SetSelectionMessage(PlayTableActionStatusFormatter.FormatCannotGuard());
                return;
            }

            RulesCommandResult result = ExecuteAction(new LegalGameAction
            {
                action_type = GameActionType.Guard,
                actor_index = playerIndex,
                card_instance_id = selectedCardInstanceId
            });
            if (!result.accepted)
            {
                SetSelectionMessage(result.rejection_reason);
                return;
            }

            selectedCardInstanceId = null;
            selectedCardId = null;
            RefreshUi();
        }

        private void MulliganSelected()
        {
            if (string.IsNullOrEmpty(selectedCardInstanceId) || selectedZone != GameZone.Hand)
            {
                SetSelectionMessage("Select a hand card to mulligan.");
                return;
            }

            RulesCommandResult result = ExecuteAction(new LegalGameAction
            {
                action_type = GameActionType.MulliganCards,
                actor_index = playerIndex,
                card_instance_ids = new List<string> { selectedCardInstanceId }
            });
            if (!result.accepted)
            {
                SetSelectionMessage(result.rejection_reason);
                return;
            }

            selectedCardInstanceId = null;
            selectedCardId = null;
            RefreshUi();
        }

        private void AttackSelectedVanguard()
        {
            if (string.IsNullOrEmpty(selectedCardInstanceId))
            {
                SetSelectionMessage(PlayTableActionStatusFormatter.FormatCannotAttack());
                return;
            }

            ExecuteFirstLegalAction(
                action => action.action_type == GameActionType.DeclareAttack &&
                    action.card_instance_id == selectedCardInstanceId &&
                    IsOpponentVanguardTarget(action.target_card_instance_id),
                PlayTableActionStatusFormatter.FormatCannotAttack());
        }

        private void AttackSelectedTarget()
        {
            if (string.IsNullOrEmpty(selectedCardInstanceId) ||
                string.IsNullOrEmpty(selectedTargetCardInstanceId))
            {
                SetSelectionMessage(PlayTableActionStatusFormatter.FormatCannotAttackTarget());
                return;
            }

            ExecuteFirstLegalAction(
                action => action.action_type == GameActionType.DeclareAttack &&
                    action.card_instance_id == selectedCardInstanceId &&
                    action.target_card_instance_id == selectedTargetCardInstanceId,
                PlayTableActionStatusFormatter.FormatCannotAttackTarget());
        }

        private void AddManualNote()
        {
            GamePhase phase = state == null ? GamePhase.StandAndDraw : state.phase;
            PlayTableManualNote note = PlayTableManualNoteFactory.Create(
                manualNotes.Count + 1,
                phase,
                selectedCardId,
                selectedZone,
                selectedTargetCardId,
                selectedTargetZone);
            manualNotes.Add(note);
            SetSelectionMessage("Manual note added.");
            if (manualNoteText != null)
            {
                manualNoteText.text = PlayTableManualNoteFormatter.FormatList(manualNotes, 3);
            }
        }

        private void Undo()
        {
            if (multiplayerSession != null)
            {
                SetSelectionMessage(PlayTableActionStatusFormatter.FormatUndoDisabledOnline());
                return;
            }

            GameActionService.UndoLast(state);
            selectedCardInstanceId = null;
            selectedCardId = null;
            RefreshUi();
        }

        public GameState CreateDisplayStateView()
        {
            if (state == null)
            {
                return null;
            }

            return GameStateViewFactory.CreatePlayerView(state, playerIndex);
        }

        private void SelectCard(GameCardInstance card, GameZone zone)
        {
            selectedCardInstanceId = card.instance_id;
            selectedCardId = card.card_id;
            selectedZone = zone;
            SetSelectionMessage(
                PlayTableCardSelectionStatusFormatter.FormatSelectedCard(card.card_id, zone));

            CardDetail detail = GetVisibleCardDetail(card);
            string preview = PlayTableSelectedCardPreviewFormatter.Format(
                card,
                zone,
                detail,
                PlayTableSelectedCardPreviewFormatter.FormatActionHint(
                    RulesCore.GetLegalActions(state, playerIndex),
                    card.instance_id));
            if (cardPreviewText != null)
            {
                cardPreviewText.text = preview;
            }
            RefreshTriggerCheckButtonStates();
            RefreshCommonActionButtonStates();
        }

        private void SelectTargetCard(GameCardInstance card, GameZone zone)
        {
            if (card == null)
            {
                return;
            }

            selectedTargetCardInstanceId = card.instance_id;
            selectedTargetCardId = card.card_id;
            selectedTargetZone = zone;
            SetSelectionMessage(
                PlayTableCardSelectionStatusFormatter.FormatSelectedTarget(card.card_id, zone));
            RefreshCommonActionButtonStates();
        }

        private void ToggleLocalSeat()
        {
            if (multiplayerSession != null)
            {
                SetSelectionMessage("Seat switch is locked in online mode.");
                return;
            }

            if (state == null || state.players == null || state.players.Count < 2)
            {
                SetSelectionMessage("Seat switch requires a two-player local table.");
                return;
            }

            playerIndex = playerIndex == 0 ? 1 : 0;
            ClearSelectionState();
            RefreshUi();
            SetSelectionMessage("Manual seat: P" + (playerIndex + 1) + ".");
        }

        private void ClearSelectionState()
        {
            selectedCardInstanceId = null;
            selectedCardId = null;
            selectedZone = GameZone.Hand;
            selectedTargetCardInstanceId = null;
            selectedTargetCardId = null;
            selectedTargetZone = GameZone.Vanguard;
            pendingAutoAbilitySelectedIndex = -1;
            pendingAutoAbilitySelectionState = PendingAutoAbilitySelection.Clear();
        }

        private void ExecuteFirstLegalAction(Func<LegalGameAction, bool> predicate, string fallbackMessage)
        {
            foreach (LegalGameAction action in RulesCore.GetLegalActions(state, playerIndex))
            {
                if (!predicate(action))
                {
                    continue;
                }

                RulesCommandResult result = ExecuteAction(action);
                if (!result.accepted)
                {
                    SetSelectionMessage(result.rejection_reason);
                    return;
                }

                RefreshUi();
                return;
            }

            SetSelectionMessage(fallbackMessage);
        }

        private RulesCommandResult ExecuteAction(LegalGameAction action)
        {
            if (multiplayerSession != null)
            {
                return multiplayerSession.ExecuteLocalAction(action);
            }

            return RulesCore.TryExecute(state, action);
        }

        private void TickMultiplayerSession()
        {
            if (multiplayerSession == null)
            {
                return;
            }

            multiplayerSession.Tick();
            if (pendingMultiplayerRefresh || lastRenderedEventCount != state.event_log.Count)
            {
                RefreshUi();
            }
        }

        private void HandleMultiplayerStateChanged()
        {
            pendingMultiplayerRefresh = true;
        }

        private void RefreshUi()
        {
            GameState displayState = CreateDisplayStateView();
            PlayerGameState player = displayState.GetPlayer(playerIndex);
            PlayerGameState opponent = displayState.GetPlayer(playerIndex == 0 ? 1 : 0);
            string mode = CreateModeSummary();
            string sessionMessage = multiplayerSession == null || string.IsNullOrWhiteSpace(multiplayerSession.LastMessage)
                ? ""
                : " | " + multiplayerSession.LastMessage;
            statusText.text = mode + " | Turn " + state.turn_number + " | Phase " + state.phase + " | P" + (playerIndex + 1) + " deck " + player.deck.Count + " hand " + player.hand.Count + sessionMessage;
            opponentText.text = "Opponent: deck " + opponent.deck.Count + " | hand " + opponent.hand.Count + " | damage " + opponent.damage.Count + " | drop " + opponent.drop.Count;
            RefreshOpponentZone(opponentVanguardContent, opponent.vanguard, GameZone.Vanguard);
            RefreshOpponentZone(opponentRearGuardContent, opponent.rear_guard, GameZone.RearGuard);
            if (battleFlowText != null)
            {
                battleFlowText.text = PlayTableBattleFlowStatusFormatter.Format(displayState);
            }
            if (botExplanationText != null)
            {
                botExplanationText.text = CreateBotExplanationPanel(null);
            }
            if (setupStatusText != null)
            {
                setupStatusText.text = PlayTableSetupStatusFormatter.Format(displayState, playerIndex);
            }
            if (guidedNextActionText != null)
            {
                guidedNextActionText.text = CreateGuidedNextAction();
            }
            if (actionGroupLegendText != null)
            {
                actionGroupLegendText.text = PlayTableActionGroupLegendFormatter.Format();
            }
            if (manualNoteText != null)
            {
                manualNoteText.text = PlayTableManualNoteFormatter.FormatList(manualNotes, 3);
            }
            if (zoneStatusText != null)
            {
                zoneStatusText.text = PlayTableZoneStatusFormatter.Format(player);
            }

            if (handStripHeaderText != null)
            {
                handStripHeaderText.text = PlayTableHandStripFormatter.FormatHeader(player.hand.Count);
            }

            if (giftMarkerText != null)
            {
                giftMarkerText.text =
                    "F:" + player.GetGiftMarkerCount(GiftMarkerType.Force) + "  " +
                    "A:" + player.GetGiftMarkerCount(GiftMarkerType.Accel) + "  " +
                    "P:" + player.GetGiftMarkerCount(GiftMarkerType.Protect);
            }

            if (onlineDebugText != null)
            {
                onlineDebugText.text = CreateOnlineDebugSummary();
            }

            RefreshLocalSeatButton();

            foreach (KeyValuePair<GameZone, RectTransform> pair in zoneContents)
            {
                RefreshZone(pair.Key, pair.Value, player.GetZone(pair.Key));
            }

            logText.text = BuildCompactLogText(displayState);
            if (fullLogText != null)
            {
                fullLogText.text = BuildFullLogText(displayState);
            }

            if (triggerCheckText != null)
            {
                triggerCheckText.text = BuildTriggerCheckLogText();
            }

            if (triggerCheckDraftSummaryText != null)
            {
                triggerCheckDraftSummaryText.text = BuildTriggerDraftSummaryText();
            }

            if (pendingAutoAbilityText != null)
            {
                pendingAutoAbilityText.text = BuildPendingAutoAbilitySummaryText();
            }

            if (pendingAutoAbilityItemListText != null)
            {
                pendingAutoAbilityItemListText.text = BuildPendingAutoAbilityItemListText();
            }

            if (pendingAutoAbilitySelectionStatusText != null)
            {
                pendingAutoAbilitySelectionStatusText.text = BuildPendingAutoAbilitySelectionStatusText();
            }

            if (pendingAutoAbilityResolutionRequestPreviewText != null)
            {
                pendingAutoAbilityResolutionRequestPreviewText.text =
                    BuildPendingAutoAbilityResolutionRequestPreviewText();
            }

            if (pendingAutoAbilityResolutionRequestSummaryText != null)
            {
                pendingAutoAbilityResolutionRequestSummaryText.text =
                    BuildPendingAutoAbilityResolutionRequestSummaryText();
            }

            if (pendingAutoAbilityResolutionRequestListText != null)
            {
                pendingAutoAbilityResolutionRequestListText.text =
                    BuildPendingAutoAbilityResolutionRequestListText();
            }

            if (pendingAutoAbilityManualResolutionDecisionSummaryText != null)
            {
                pendingAutoAbilityManualResolutionDecisionSummaryText.text =
                    BuildPendingAutoAbilityManualResolutionDecisionSummaryText();
            }

            if (pendingAutoAbilityManualResolutionDecisionListText != null)
            {
                pendingAutoAbilityManualResolutionDecisionListText.text =
                    BuildPendingAutoAbilityManualResolutionDecisionListText();
            }

            if (pendingAutoAbilityManualResolutionDecisionValidationText != null)
            {
                pendingAutoAbilityManualResolutionDecisionValidationText.text =
                    BuildPendingAutoAbilityManualResolutionDecisionValidationText();
            }

            if (pendingAutoAbilityManualResolutionApplyPreviewLogLatestText != null)
            {
                pendingAutoAbilityManualResolutionApplyPreviewLogLatestText.text =
                    BuildPendingAutoAbilityManualResolutionApplyPreviewLogLatestText();
            }

            if (pendingAutoAbilityManualResolutionApplyPreviewLogListText != null)
            {
                pendingAutoAbilityManualResolutionApplyPreviewLogListText.text =
                    BuildPendingAutoAbilityManualResolutionApplyPreviewLogListText();
            }

            RefreshTriggerCheckButtonStates();
            RefreshCommonActionButtonStates();

            if (string.IsNullOrEmpty(selectedCardInstanceId))
            {
                SetSelectionMessage(PlayTableCardSelectionStatusFormatter.FormatNoCardSelected());
                if (cardPreviewText != null)
                {
                    cardPreviewText.text = PlayTableSelectedCardPreviewFormatter.NoSelectionText;
                }
            }

            lastRenderedEventCount = state.event_log.Count;
            pendingMultiplayerRefresh = false;
        }

        private void RefreshLocalSeatButton()
        {
            if (localSeatButton == null)
            {
                return;
            }

            bool canSwitch = multiplayerSession == null &&
                state != null &&
                state.players != null &&
                state.players.Count > 1;
            localSeatButton.interactable = canSwitch;

            Text label = localSeatButton.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = canSwitch
                    ? "Seat P" + (playerIndex == 0 ? "2" : "1")
                    : "Seat Lock";
            }
        }

        private string CreateModeSummary()
        {
            if (multiplayerSession == null)
            {
                return PlayTableModeSummaryFormatter.FormatLocal(localModeDetail);
            }

            return PlayTableModeSummaryFormatter.Format(
                true,
                multiplayerSession.Status,
                multiplayerSession.TransportName,
                multiplayerSession.EventCursor,
                multiplayerSession.TriggerCheckReplayLogPayloads.Count,
                multiplayerSession.LastReconnectAppliedCount,
                multiplayerSession.LastReconnectFromEventIndex);
        }

        private string CreateOnlineDebugSummary()
        {
            if (multiplayerSession == null)
            {
                return PlayTableModeSummaryFormatter.FormatAdvancedDetails(false, 0, 0, 0, -1);
            }

            return PlayTableModeSummaryFormatter.FormatAdvancedDetails(
                true,
                multiplayerSession.EventCursor,
                multiplayerSession.TriggerCheckReplayLogPayloads.Count,
                multiplayerSession.LastReconnectAppliedCount,
                multiplayerSession.LastReconnectFromEventIndex);
        }

        private void RefreshZone(GameZone zone, RectTransform content, IReadOnlyList<GameCardInstance> cards)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                DestroyRuntimeObject(content.GetChild(i).gameObject);
            }

            if (IsCompactPileZone(zone))
            {
                RefreshCompactPileZone(zone, content, cards);
                return;
            }

            int max = zone == GameZone.Deck ? Math.Min(cards.Count, 1) : Math.Min(cards.Count, 18);
            if (cards.Count == 0)
            {
                CreateLabel(
                    content,
                    zone == GameZone.Hand ? PlayTableHandStripFormatter.EmptyHandText : UiStateMessageFormatter.ZoneEmpty,
                    120,
                    12,
                    TextAnchor.MiddleCenter);
                return;
            }

            if (zone == GameZone.Deck)
            {
                return;
            }

            for (int i = 0; i < max; i++)
            {
                GameCardInstance card = cards[i];
                Button button;
                if (IsBoardFaceZone(zone))
                {
                    button = CreateBoardCardFaceButton(content, card, GetVisibleCardDetail(card), zone);
                }
                else if (zone == GameZone.Hand)
                {
                    button = CreateHandCardButton(content, card, GetVisibleCardDetail(card), i + 1);
                }
                else
                {
                    string label = ShortCardLabel(card.card_id);
                    button = CreateButton(content, label, 120f, delegate { SelectCard(card, zone); }, false, 38f);
                }

                if (zone == GameZone.Hand && card != null && !string.IsNullOrEmpty(card.card_id))
                {
                    button.gameObject.name = card.card_id + " Button";
                }
            }

            if (cards.Count > max)
            {
                CreateLabel(content, "+" + (cards.Count - max) + " more", 120, 12, TextAnchor.MiddleCenter);
            }
        }

        private void RefreshCompactPileZone(GameZone zone, RectTransform content, IReadOnlyList<GameCardInstance> cards)
        {
            if (zone == GameZone.RideDeck)
            {
                return;
            }

            if (cards == null || cards.Count == 0)
            {
                CreateCompactZoneLabel(content, UiStateMessageFormatter.ZoneEmpty, 86f, 13f, 10);
                return;
            }

            string countText = cards.Count == 1 ? "1 card" : cards.Count + " cards";
            CreateCompactZoneLabel(content, countText, 86f, 12f, 10);
        }

        private void RefreshOpponentZone(
            RectTransform content,
            IReadOnlyList<GameCardInstance> cards,
            GameZone zone)
        {
            if (content == null)
            {
                return;
            }

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                DestroyRuntimeObject(content.GetChild(i).gameObject);
            }

            if (cards == null || cards.Count == 0)
            {
                CreateLabel(content, UiStateMessageFormatter.ZoneEmpty, 120, 12, TextAnchor.MiddleCenter);
                return;
            }

            int max = Math.Min(cards.Count, zone == GameZone.Vanguard ? 2 : 6);
            for (int i = 0; i < max; i++)
            {
                GameCardInstance card = cards[i];
                CreateOpponentTargetCardFaceButton(content, card, GetVisibleCardDetail(card), zone);
            }

            if (cards.Count > max)
            {
                CreateLabel(content, "+" + (cards.Count - max) + " more", 120, 12, TextAnchor.MiddleCenter);
            }
        }

        private string BuildCompactLogText(GameState displayState)
        {
            return BuildLogText(displayState, true);
        }

        private string BuildFullLogText(GameState displayState)
        {
            return BuildLogText(displayState, false);
        }

        private string BuildLogText(GameState displayState, bool compact)
        {
            IReadOnlyList<GameEvent> eventLog = displayState == null ? null : displayState.event_log;
            string log = compact
                ? PlayTableEventReplayPanelFormatter.FormatCompact(eventLog)
                : PlayTableEventReplayPanelFormatter.Format(eventLog);
            if (multiplayerSession == null)
            {
                return log;
            }

            string sync = PlayTableReplaySyncStatusFormatter.FormatOnlineStatus(
                multiplayerSession.EventCursor,
                multiplayerSession.PublicEventLog == null ? 0 : multiplayerSession.PublicEventLog.Count,
                multiplayerSession.LastReconnectAppliedCount,
                multiplayerSession.LastReconnectFromEventIndex);
            return sync + "\n\n" + log;
        }

        private CardDetail GetVisibleCardDetail(GameCardInstance card)
        {
            if (repository == null ||
                card == null ||
                !card.face_up ||
                string.IsNullOrEmpty(card.card_id) ||
                card.card_id == GameStateViewFactory.HiddenCardId)
            {
                return null;
            }

            return repository.GetCard(card.card_id);
        }

        private Button CreateHandCardButton(
            Transform parent,
            GameCardInstance card,
            CardDetail detail,
            int displayIndex)
        {
            GameObject root = CreatePanel("Hand Card Button", parent, new Color(0.22f, 0.30f, 0.27f, 1f));
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = PlayTableHandStripFormatter.CardButtonWidth;
            layout.preferredHeight = PlayTableHandStripFormatter.CardButtonHeight;
            layout.minHeight = PlayTableHandStripFormatter.CardButtonHeight;
            if (hasResponsiveProfile)
            {
                layout.preferredWidth = activeLayoutProfile.ScaleControlWidth(PlayTableHandStripFormatter.CardButtonWidth);
                layout.preferredHeight = Mathf.Max(PlayTableHandStripFormatter.CardButtonHeight, activeLayoutProfile.TouchTargetHeight);
                layout.minHeight = layout.preferredHeight;
            }

            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(delegate { SelectCard(card, GameZone.Hand); });

            VerticalLayoutGroup vertical = root.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(4, 4, 4, 4);
            vertical.spacing = 2f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = false;
            vertical.childForceExpandWidth = false;
            vertical.childForceExpandHeight = false;

            bool hasThumbnail = PlayTableBoardCardFaceFormatter.ShouldShowThumbnail(card, detail) && imageCache != null;
            if (hasThumbnail)
            {
                RawImage thumbnail = CreateRawImage(
                    root.transform,
                    PlayTableHandStripFormatter.CardThumbnailWidth,
                    PlayTableHandStripFormatter.CardThumbnailHeight);
                thumbnail.texture = imageCache.LoadThumbnail(detail.ImageRelativePath);
            }
            else
            {
                Text label = CreateText(
                    "Hand Card Label",
                    root.transform,
                    PlayTableHandStripFormatter.FormatCardButtonLabel(card, detail, displayIndex, 8),
                    8,
                    TextAnchor.MiddleCenter,
                    Color.white);
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;
                label.gameObject.AddComponent<LayoutElement>().preferredHeight = 44f;
            }

            return button;
        }

        private Button CreateBoardCardFaceButton(
            Transform parent,
            GameCardInstance card,
            CardDetail detail,
            GameZone zone)
        {
            const float width = 88f;
            float height = zone == GameZone.Vanguard ? 82f : 72f;
            GameObject root = CreatePanel("Board Card Face Button", parent, new Color(0.2f, 0.29f, 0.25f, 1f));
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            if (hasResponsiveProfile)
            {
                layout.preferredWidth = activeLayoutProfile.ScaleControlWidth(width);
                layout.preferredHeight = Mathf.Max(height, activeLayoutProfile.TouchTargetHeight);
            }

            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(delegate { SelectCard(card, zone); });

            VerticalLayoutGroup vertical = root.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(4, 4, 4, 4);
            vertical.spacing = 2f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = false;

            if (PlayTableBoardCardFaceFormatter.ShouldShowThumbnail(card, detail) && imageCache != null)
            {
                RawImage thumbnail = CreateRawImage(root.transform, 38f, zone == GameZone.Vanguard ? 42f : 34f);
                thumbnail.texture = imageCache.LoadThumbnail(detail.ImageRelativePath);
            }

            Text title = CreateText(
                "Card Face Title",
                root.transform,
                PlayTableBoardCardFaceFormatter.FormatTitle(card, detail),
                10,
                TextAnchor.MiddleCenter,
                Color.white);
            title.horizontalOverflow = HorizontalWrapMode.Wrap;
            title.verticalOverflow = VerticalWrapMode.Truncate;
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 18f;

            string stats = PlayTableBoardCardFaceFormatter.FormatStats(detail);
            if (!string.IsNullOrEmpty(stats))
            {
                Text statsText = CreateText("Card Face Stats", root.transform, stats, 9, TextAnchor.MiddleCenter, new Color(0.82f, 0.9f, 0.86f, 1f));
                statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
                statsText.verticalOverflow = VerticalWrapMode.Truncate;
                statsText.gameObject.AddComponent<LayoutElement>().preferredHeight = 12f;
            }

            return button;
        }

        private Button CreateOpponentTargetCardFaceButton(
            Transform parent,
            GameCardInstance card,
            CardDetail detail,
            GameZone zone)
        {
            GameObject root = CreatePanel("Opponent Target Card Button", parent, new Color(0.25f, 0.18f, 0.18f, 1f));
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = zone == GameZone.Vanguard ? 88f : 80f;
            layout.preferredHeight = zone == GameZone.Vanguard ? 74f : 64f;
            if (hasResponsiveProfile)
            {
                layout.preferredWidth = activeLayoutProfile.ScaleControlWidth(layout.preferredWidth);
                layout.preferredHeight = Mathf.Max(layout.preferredHeight, activeLayoutProfile.TouchTargetHeight);
            }

            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(delegate { SelectTargetCard(card, zone); });

            VerticalLayoutGroup vertical = root.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(4, 4, 4, 4);
            vertical.spacing = 2f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = false;

            Text title = CreateText(
                "Opponent Target Title",
                root.transform,
                PlayTableBoardCardFaceFormatter.FormatTitle(card, detail),
                10,
                TextAnchor.MiddleCenter,
                Color.white);
            title.horizontalOverflow = HorizontalWrapMode.Wrap;
            title.verticalOverflow = VerticalWrapMode.Truncate;
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            string stats = PlayTableBoardCardFaceFormatter.FormatStats(detail);
            if (!string.IsNullOrEmpty(stats))
            {
                Text statsText = CreateText("Opponent Target Stats", root.transform, stats, 9, TextAnchor.MiddleCenter, new Color(0.9f, 0.84f, 0.84f, 1f));
                statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
                statsText.verticalOverflow = VerticalWrapMode.Truncate;
                statsText.gameObject.AddComponent<LayoutElement>().preferredHeight = 14f;
            }

            return button;
        }

        private RawImage CreateRawImage(Transform parent, float width, float height)
        {
            GameObject root = new GameObject("Thumbnail");
            root.transform.SetParent(parent, false);
            RawImage image = root.AddComponent<RawImage>();
            image.color = Color.white;
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            return image;
        }

        private static bool IsBoardFaceZone(GameZone zone)
        {
            return zone == GameZone.Vanguard || zone == GameZone.RearGuard;
        }

        private static bool IsCompactPileZone(GameZone zone)
        {
            return zone == GameZone.RideDeck ||
                   zone == GameZone.Damage ||
                   zone == GameZone.Drop ||
                   zone == GameZone.Bind ||
                   zone == GameZone.Order ||
                   zone == GameZone.Trigger ||
                   zone == GameZone.Soul;
        }

        public string CreateEventReplayPanel()
        {
            return BuildFullLogText(CreateDisplayStateView());
        }

        public string CreateCompactEventReplayPanel()
        {
            return BuildCompactLogText(CreateDisplayStateView());
        }

        public string CreateTriggerCheckLogSummary()
        {
            return BuildTriggerCheckLogText();
        }

        public string CreateBotExplanationPanel(BotDebugTrace trace)
        {
            return BotExplanationPanelFormatter.Format(trace);
        }

        private string BuildTriggerCheckLogText()
        {
            return TriggerCheckPanelFormatter.Format(
                multiplayerSession == null ? null : multiplayerSession.TriggerCheckReplayLogPayloads,
                multiplayerSession != null,
                manualDraftTriggerType,
                manualDraftCheckSource,
                manualDraftCheckIndex,
                selectedCardId,
                selectedCardInstanceId,
                selectedZone);
        }

        public string CreatePendingAutoAbilitySummary()
        {
            return BuildPendingAutoAbilitySummaryText();
        }

        public string CreatePendingAutoAbilityItemList()
        {
            return BuildPendingAutoAbilityItemListText();
        }

        public string CreatePendingAutoAbilitySelectionStatus()
        {
            return BuildPendingAutoAbilitySelectionStatusText();
        }

        public string CreatePendingAutoAbilityResolutionRequestPreview()
        {
            return BuildPendingAutoAbilityResolutionRequestPreviewText();
        }

        public string CreatePendingAutoAbilityResolutionRequestSummary()
        {
            return BuildPendingAutoAbilityResolutionRequestSummaryText();
        }

        public string CreatePendingAutoAbilityResolutionRequestList()
        {
            return BuildPendingAutoAbilityResolutionRequestListText();
        }

        public string CreatePendingAutoAbilityManualResolutionDecisionSummary()
        {
            return BuildPendingAutoAbilityManualResolutionDecisionSummaryText();
        }

        public string CreatePendingAutoAbilityManualResolutionDecisionList()
        {
            return BuildPendingAutoAbilityManualResolutionDecisionListText();
        }

        public string CreatePendingAutoAbilityManualResolutionDecisionValidationPreview()
        {
            return BuildPendingAutoAbilityManualResolutionDecisionValidationText();
        }

        public string CreatePendingAutoAbilityManualResolutionApplyPreviewLogLatest()
        {
            return BuildPendingAutoAbilityManualResolutionApplyPreviewLogLatestText();
        }

        public string CreatePendingAutoAbilityManualResolutionApplyPreviewLogList()
        {
            return BuildPendingAutoAbilityManualResolutionApplyPreviewLogListText();
        }

        public string CreateSelectionMessage()
        {
            return selectionText == null ? string.Empty : selectionText.text;
        }

        public string CreateGuidedNextAction()
        {
            GameState displayState = CreateDisplayStateView();
            bool canSwitchLocalSeat = multiplayerSession == null &&
                displayState != null &&
                displayState.players != null &&
                displayState.players.Count > 1;
            return PlayTableGuidedNextActionFormatter.Format(displayState, playerIndex, canSwitchLocalSeat);
        }

        private string BuildPendingAutoAbilitySummaryText()
        {
            return PendingAutoAbilityPanelFormatter.Format(
                multiplayerSession == null ? null : multiplayerSession.PendingAutoAbilityQueuePayloads,
                pendingAutoAbilitySelectionState);
        }

        private string BuildPendingAutoAbilityItemListText()
        {
            return PendingAutoAbilityItemListFormatter.FormatLatest(
                multiplayerSession == null ? null : multiplayerSession.PendingAutoAbilityQueuePayloads,
                3);
        }

        private string BuildPendingAutoAbilitySelectionStatusText()
        {
            return PendingAutoAbilitySelectionStatusFormatter.Format(pendingAutoAbilitySelectionState);
        }

        private string BuildPendingAutoAbilityResolutionRequestPreviewText()
        {
            if (multiplayerSession == null)
            {
                return PendingAutoAbilityResolutionRequestFormatter.NullRequestMessage;
            }

            PendingAutoAbilityResolutionRequestResult result =
                PendingAutoAbilityResolutionRequestFactory.Create(pendingAutoAbilitySelectionState);
            return PendingAutoAbilityResolutionRequestFormatter.Format(result);
        }

        private string BuildPendingAutoAbilityResolutionRequestSummaryText()
        {
            return PendingAutoAbilityResolutionRequestSummaryFormatter.FormatLatest(
                multiplayerSession == null ? null : multiplayerSession.PendingAutoAbilityResolutionRequestPayloads);
        }

        private string BuildPendingAutoAbilityResolutionRequestListText()
        {
            return PendingAutoAbilityResolutionRequestListFormatter.Format(
                multiplayerSession == null ? null : multiplayerSession.PendingAutoAbilityResolutionRequestPayloads,
                3);
        }

        private string BuildPendingAutoAbilityManualResolutionDecisionSummaryText()
        {
            return PendingAutoAbilityManualResolutionPanelFormatter.Format(
                multiplayerSession == null
                    ? null
                    : multiplayerSession.PendingAutoAbilityManualResolutionDecisionPayloads,
                multiplayerSession == null
                    ? null
                    : multiplayerSession.LatestPendingAutoAbilityManualResolutionApplyPreviewLog);
        }

        private string BuildPendingAutoAbilityManualResolutionDecisionListText()
        {
            return PendingAutoAbilityManualResolutionDecisionListFormatter.Format(
                multiplayerSession == null
                    ? null
                    : multiplayerSession.PendingAutoAbilityManualResolutionDecisionPayloads,
                3);
        }

        private string BuildPendingAutoAbilityManualResolutionDecisionValidationText()
        {
            if (multiplayerSession == null ||
                multiplayerSession.PendingAutoAbilityManualResolutionDecisionPayloads == null ||
                multiplayerSession.PendingAutoAbilityManualResolutionDecisionPayloads.Count == 0)
            {
                return PendingAutoAbilityManualResolutionDecisionValidationResultFormatter.Format(null);
            }

            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                multiplayerSession.PendingAutoAbilityManualResolutionDecisionPayloads[
                    multiplayerSession.PendingAutoAbilityManualResolutionDecisionPayloads.Count - 1];
            return PendingAutoAbilityManualResolutionDecisionValidationResultFormatter.Format(
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(payload));
        }

        private string BuildPendingAutoAbilityManualResolutionApplyPreviewLogLatestText()
        {
            return PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.Format(
                multiplayerSession == null
                    ? null
                    : multiplayerSession.LatestPendingAutoAbilityManualResolutionApplyPreviewLog);
        }

        private string BuildPendingAutoAbilityManualResolutionApplyPreviewLogListText()
        {
            return PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.FormatList(
                multiplayerSession == null
                    ? null
                    : multiplayerSession.PendingAutoAbilityManualResolutionApplyPreviewLogs,
                3);
        }

        public string CreateTriggerDraftSummary()
        {
            return BuildTriggerDraftSummaryText();
        }

        private string BuildTriggerDraftSummaryText()
        {
            return TriggerCheckDraftSummaryFormatter.FormatSummary(
                multiplayerSession != null,
                manualDraftTriggerType,
                manualDraftCheckSource,
                manualDraftCheckIndex,
                selectedCardId,
                selectedCardInstanceId,
                selectedZone);
        }

        private void PublishLatestTriggerCheckReplayLog()
        {
            TriggerCheckLogPublishValidation validation =
                TriggerCheckLogPublishValidator.Validate(multiplayerSession != null);
            if (!validation.accepted)
            {
                SetSelectionMessage(validation.rejection_reason);
                return;
            }

            MultiplayerTransportResult result = multiplayerSession.PublishLatestTriggerCheckReplayLog();
            SetSelectionMessage(TriggerCheckLogPublishResultFormatter.Format(result));
            RefreshUi();
        }

        private void PublishLatestPendingAutoAbilityQueue()
        {
            TriggerCheckLogPublishValidation validation =
                TriggerCheckLogPublishValidator.Validate(multiplayerSession != null);
            if (!validation.accepted)
            {
                SetSelectionMessage(validation.rejection_reason);
                return;
            }

            MultiplayerTransportResult result = multiplayerSession.PublishLatestPendingAutoAbilityQueue();
            SetSelectionMessage(PendingAutoAbilityPublishResultFormatter.Format(result));
            RefreshUi();
        }

        private void CyclePendingAutoAbilitySelection()
        {
            if (multiplayerSession == null)
            {
                SetSelectionMessage("Pending auto ability selection is only available online.");
                return;
            }

            PendingAutoAbilityQueue queue;
            string rejectionReason;
            if (!TryDecodeLatestPendingAutoAbilityQueue(out queue, out rejectionReason))
            {
                pendingAutoAbilitySelectedIndex = -1;
                pendingAutoAbilitySelectionState = PendingAutoAbilitySelection.Select(null, 0);
                if (!string.IsNullOrEmpty(rejectionReason))
                {
                    pendingAutoAbilitySelectionState.rejection_reason = rejectionReason;
                }

                SetSelectionMessage(
                    PendingAutoAbilitySelectionStatusFormatter.Format(pendingAutoAbilitySelectionState));
                RefreshUi();
                return;
            }

            queue.EnsureLists();
            int nextIndex = queue.pending.Count == 0 ? 0 : (pendingAutoAbilitySelectedIndex + 1) % queue.pending.Count;
            PendingAutoAbilitySelectionState selection = PendingAutoAbilitySelection.Select(queue, nextIndex);
            if (selection.accepted)
            {
                pendingAutoAbilitySelectedIndex = nextIndex;
            }
            else
            {
                pendingAutoAbilitySelectedIndex = -1;
            }

            pendingAutoAbilitySelectionState = selection;
            SetSelectionMessage(PendingAutoAbilitySelectionStatusFormatter.Format(selection));
            RefreshUi();
        }

        private void ClearPendingAutoAbilitySelection()
        {
            pendingAutoAbilitySelectedIndex = -1;
            pendingAutoAbilitySelectionState = PendingAutoAbilitySelection.Clear();
            SetSelectionMessage(PendingAutoAbilitySelectionStatusFormatter.Format(pendingAutoAbilitySelectionState));
            RefreshUi();
        }

        private void PublishPendingAutoAbilityResolutionRequest()
        {
            if (multiplayerSession == null)
            {
                SetSelectionMessage("Pending auto ability resolution request publish is only available online.");
                return;
            }

            PendingAutoAbilityResolutionRequestResult requestResult =
                PendingAutoAbilityResolutionRequestFactory.Create(pendingAutoAbilitySelectionState);
            if (!requestResult.accepted)
            {
                SetSelectionMessage(PendingAutoAbilityResolutionRequestFormatter.Format(requestResult));
                RefreshTriggerCheckButtonStates();
                return;
            }

            MultiplayerTransportResult result =
                multiplayerSession.PublishPendingAutoAbilityResolutionRequest(requestResult.request);
            SetSelectionMessage(result.ok
                ? PendingAutoAbilityResolutionRequestFormatter.Format(requestResult.request)
                : result.error_code + ": " + result.message);
            RefreshUi();
        }

        private void PublishLatestPendingAutoAbilityManualResolutionDecision()
        {
            if (multiplayerSession == null)
            {
                SetSelectionMessage("Pending auto ability manual resolution decision publish is only available online.");
                return;
            }

            MultiplayerTransportResult result =
                multiplayerSession.PublishLatestPendingAutoAbilityManualResolutionDecision();
            SetSelectionMessage(PendingAutoAbilityManualResolutionDecisionPublishResultFormatter.Format(result));
            RefreshUi();
        }

        private void CreatePendingAutoAbilityManualResolutionDecisionDraft()
        {
            if (multiplayerSession == null)
            {
                SetSelectionMessage("Pending auto ability manual resolution decision draft is only available online.");
                return;
            }

            PendingAutoAbilityManualResolutionDecisionDraftResult result =
                multiplayerSession.CreatePendingAutoAbilityManualResolutionDecisionDraft(
                    pendingAutoAbilityManualResolutionDecisionType,
                    "PlayTable " +
                    PendingAutoAbilityManualResolutionDecisionTypeSelector.Normalize(
                        pendingAutoAbilityManualResolutionDecisionType) +
                    " draft");
            RefreshUi();
            SetSelectionMessage(PendingAutoAbilityManualResolutionDecisionDraftResultFormatter.Format(result));
        }

        private void PreviewApplyLatestPendingAutoAbilityManualResolutionDecision()
        {
            if (multiplayerSession == null)
            {
                SetSelectionMessage("Pending auto ability manual resolution apply preview is only available online.");
                return;
            }

            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                multiplayerSession.PreviewApplyLatestPendingAutoAbilityManualResolutionDecision();
            RefreshUi();
            SetSelectionMessage(
                PendingAutoAbilityManualResolutionApplyResultFormatter.Format(
                    result == null ? null : result.apply_result));
        }

        private void CyclePendingAutoAbilityManualResolutionDecisionType()
        {
            pendingAutoAbilityManualResolutionDecisionType =
                PendingAutoAbilityManualResolutionDecisionTypeSelector.Next(
                    pendingAutoAbilityManualResolutionDecisionType);
            SetSelectionMessage(
                PendingAutoAbilityManualResolutionDecisionTypeSelector.FormatStatusMessage(
                    pendingAutoAbilityManualResolutionDecisionType));
            RefreshTriggerCheckButtonStates();
        }

        private bool TryDecodeLatestPendingAutoAbilityQueue(
            out PendingAutoAbilityQueue queue,
            out string rejectionReason)
        {
            queue = null;
            rejectionReason = string.Empty;
            if (multiplayerSession == null ||
                multiplayerSession.PendingAutoAbilityQueuePayloads == null ||
                multiplayerSession.PendingAutoAbilityQueuePayloads.Count == 0)
            {
                rejectionReason = PendingAutoAbilitySelection.EmptyQueueReason;
                return false;
            }

            NetworkPendingAutoAbilityQueuePayload payload =
                multiplayerSession.PendingAutoAbilityQueuePayloads[
                    multiplayerSession.PendingAutoAbilityQueuePayloads.Count - 1];
            return PendingAutoAbilityQueuePayloadCodec.TryDecode(payload, out queue, out rejectionReason);
        }

        private void PublishManualTriggerCheckDraft()
        {
            TriggerCheckDraftSelectionValidation validation = TriggerCheckDraftSelectionValidator.Validate(
                multiplayerSession != null,
                selectedCardInstanceId,
                selectedCardId);
            if (!validation.accepted)
            {
                SetSelectionMessage(validation.rejection_reason);
                RefreshTriggerCheckButtonStates();
                return;
            }

            ManualTriggerCheckDraftResult result = multiplayerSession.PublishManualTriggerCheckDraft(
                TriggerCheckDraftRequestFactory.Create(
                    playerIndex,
                    manualDraftCheckSource,
                    manualDraftCheckIndex,
                    selectedCardInstanceId,
                    selectedCardId,
                    manualDraftTriggerType));
            SetSelectionMessage(TriggerCheckDraftPublishResultFormatter.Format(result));
            RefreshUi();
        }

        private void CycleManualDraftTriggerType()
        {
            manualDraftTriggerType =
                TriggerCheckDraftSelectorCycleHelper.NextTriggerType(manualDraftTriggerType);
            SetSelectionMessage(
                TriggerCheckDraftStatusMessageFormatter.FormatTriggerTypeChanged(manualDraftTriggerType));
            RefreshTriggerCheckButtonStates();
        }

        private void CycleManualDraftCheckSource()
        {
            manualDraftCheckSource =
                TriggerCheckDraftSelectorCycleHelper.NextCheckSource(manualDraftCheckSource);
            SetSelectionMessage(
                TriggerCheckDraftStatusMessageFormatter.FormatCheckSourceChanged(manualDraftCheckSource));
            RefreshTriggerCheckButtonStates();
        }

        private void CycleManualDraftCheckIndex()
        {
            manualDraftCheckIndex =
                TriggerCheckDraftSelectorCycleHelper.NextCheckIndex(manualDraftCheckIndex);
            SetSelectionMessage(
                TriggerCheckDraftStatusMessageFormatter.FormatCheckIndexChanged(manualDraftCheckIndex));
            RefreshTriggerCheckButtonStates();
        }

        private void ClearManualTriggerDraftSelection()
        {
            selectedCardInstanceId = null;
            selectedCardId = null;
            SetSelectionMessage(TriggerCheckDraftStatusMessageFormatter.FormatSelectionCleared());
            RefreshTriggerCheckButtonStates();
        }

        private void ExportLocalReplay()
        {
            if (multiplayerSession != null)
            {
                SetSelectionMessage("Replay export is local table only for now.");
                return;
            }

            PlayTableReplayExportResult result =
                PlayTableReplayExporter.Export(replayInitialState, state, ResolveLocalReplayExportPath());
            if (!result.accepted)
            {
                SetSelectionMessage("Replay export failed: " + result.rejection_reason);
                return;
            }

            SetSelectionMessage("Replay exported: " + result.output_path);
        }

        private static string ResolveLocalReplayExportPath()
        {
            string baseDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "work"));
            return Path.Combine(baseDirectory, "vanguard_latest_replay.json");
        }

        private void RefreshTriggerCheckButtonStates()
        {
            TriggerCheckDraftControlState controlState = TriggerCheckDraftControlStateHelper.Evaluate(
                multiplayerSession != null,
                multiplayerSession != null && multiplayerSession.CanPublishTriggerCheckReplayLog,
                multiplayerSession != null && multiplayerSession.CanPublishPendingAutoAbilityQueue,
                pendingAutoAbilitySelectionState != null && pendingAutoAbilitySelectionState.has_selection,
                selectedCardInstanceId,
                selectedCardId);

            if (triggerCheckPublishButton != null)
            {
                triggerCheckPublishButton.interactable = controlState.can_publish_trigger_log;
            }

            if (pendingAutoAbilityPublishButton != null)
            {
                pendingAutoAbilityPublishButton.interactable =
                    controlState.can_publish_pending_auto_ability_queue;
            }

            if (pendingAutoAbilitySelectionButton != null)
            {
                pendingAutoAbilitySelectionButton.interactable =
                    controlState.can_cycle_pending_auto_ability_selection;
            }

            if (pendingAutoAbilityResolutionRequestPublishButton != null)
            {
                pendingAutoAbilityResolutionRequestPublishButton.interactable =
                    controlState.can_publish_pending_auto_ability_resolution_request;
            }

            if (pendingAutoAbilityManualResolutionDecisionPublishButton != null)
            {
                pendingAutoAbilityManualResolutionDecisionPublishButton.interactable =
                    multiplayerSession != null &&
                    multiplayerSession.CanPublishPendingAutoAbilityManualResolutionDecision;
            }

            if (pendingAutoAbilityManualResolutionDecisionDraftButton != null)
            {
                pendingAutoAbilityManualResolutionDecisionDraftButton.interactable =
                    multiplayerSession != null &&
                    multiplayerSession.CanCreatePendingAutoAbilityManualResolutionDecisionDraft;
            }

            if (pendingAutoAbilityManualResolutionApplyPreviewButton != null)
            {
                pendingAutoAbilityManualResolutionApplyPreviewButton.interactable =
                    multiplayerSession != null;
            }

            if (pendingAutoAbilityManualResolutionDecisionTypeText != null)
            {
                pendingAutoAbilityManualResolutionDecisionTypeText.text =
                    PendingAutoAbilityManualResolutionDecisionTypeSelector.FormatButtonLabel(
                        pendingAutoAbilityManualResolutionDecisionType);
            }

            if (pendingAutoAbilityClearSelectionButton != null)
            {
                pendingAutoAbilityClearSelectionButton.interactable =
                    controlState.can_clear_pending_auto_ability_selection;
            }

            if (triggerCheckDraftButton != null)
            {
                triggerCheckDraftButton.interactable = controlState.can_publish_manual_draft;
            }

            if (triggerCheckDraftClearButton != null)
            {
                triggerCheckDraftClearButton.interactable = controlState.can_clear_selection;
            }

            if (triggerCheckDraftTypeText != null)
            {
                triggerCheckDraftTypeText.text =
                    TriggerCheckDraftMetadataFormatter.FormatTypeButtonLabel(manualDraftTriggerType);
            }

            if (triggerCheckDraftSourceText != null)
            {
                triggerCheckDraftSourceText.text =
                    TriggerCheckDraftMetadataFormatter.FormatSourceButtonLabel(manualDraftCheckSource);
            }

            if (triggerCheckDraftIndexText != null)
            {
                triggerCheckDraftIndexText.text =
                    TriggerCheckDraftMetadataFormatter.FormatIndexButtonLabel(manualDraftCheckIndex);
            }

            if (triggerCheckDraftSummaryText != null)
            {
                triggerCheckDraftSummaryText.text = BuildTriggerDraftSummaryText();
            }
        }

        private void RefreshCommonActionButtonStates()
        {
            PlayTableCommonActionAvailability availability =
                PlayTableCommonActionAvailability.FromState(
                    state,
                    playerIndex,
                    selectedCardInstanceId,
                    selectedZone,
                    selectedTargetCardInstanceId,
                    multiplayerSession != null);

            SetButtonInteractable(drawButton, availability.can_draw);
            SetButtonInteractable(standAndDrawPhaseButton, availability.can_set_stand_and_draw);
            SetButtonInteractable(ridePhaseButton, availability.can_set_ride);
            SetButtonInteractable(mainPhaseButton, availability.can_set_main);
            SetButtonInteractable(battlePhaseButton, availability.can_set_battle);
            SetButtonInteractable(endPhaseButton, availability.can_set_end);
            SetButtonInteractable(undoButton, availability.can_undo);
            SetButtonInteractable(forceMarkerButton, availability.can_add_force);
            SetButtonInteractable(accelMarkerButton, availability.can_add_accel);
            SetButtonInteractable(protectMarkerButton, availability.can_add_protect);
            SetButtonInteractable(moveToVanguardButton, availability.can_move_to_vanguard);
            SetButtonInteractable(moveToRearGuardButton, availability.can_move_to_rear_guard);
            SetButtonInteractable(moveToDropButton, availability.can_move_to_drop);
            SetButtonInteractable(moveToDamageButton, availability.can_move_to_damage);
            SetButtonInteractable(driveCheckButton, availability.can_drive_check);
            SetButtonInteractable(damageCheckButton, availability.can_damage_check);
            SetButtonInteractable(guardSelectedButton, availability.can_guard_selected);
            SetButtonInteractable(mulliganSelectedButton, availability.can_mulligan_selected);
            SetButtonInteractable(attackVanguardButton, availability.can_attack_vanguard_selected);
            SetButtonInteractable(attackTargetButton, availability.can_attack_selected_target);
            SetButtonInteractable(manualNoteButton, state != null);
        }

        private bool IsOpponentVanguardTarget(string targetCardInstanceId)
        {
            if (state == null || string.IsNullOrEmpty(targetCardInstanceId))
            {
                return false;
            }

            int opponentIndex = 1 - playerIndex;
            if (opponentIndex < 0 || opponentIndex >= state.players.Count)
            {
                return false;
            }

            PlayerGameState opponent = state.GetPlayer(opponentIndex);
            foreach (GameCardInstance card in opponent.vanguard)
            {
                if (card != null && card.instance_id == targetCardInstanceId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void SetSelectionMessage(string message)
        {
            if (selectionText != null)
            {
                selectionText.text = message;
            }
        }

        private static string ShortCardLabel(string cardId)
        {
            if (string.IsNullOrEmpty(cardId) || cardId.Length <= 18)
            {
                return cardId;
            }

            return cardId.Substring(0, 18);
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

        private GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.AddComponent<RectTransform>();
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

        private RectTransform CreateActionRow(Transform parent, string name)
        {
            RectTransform row = CreatePanel(name, parent, new Color(0.08f, 0.11f, 0.1f, 1f)).GetComponent<RectTransform>();
            LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 34f;
            layout.preferredHeight = 34f;
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(4, 4, 2, 2);
            rowLayout.spacing = 4;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = false;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childForceExpandWidth = false;
            return row;
        }

        private Button CreateCommandButton(
            Transform parent,
            string label,
            float width,
            UnityEngine.Events.UnityAction action)
        {
            Button button = CreateButton(parent, label, width, action, false, 30f);
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.16f, 0.30f, 0.27f, 0.95f);
            }

            return button;
        }

        private Button CreatePhaseRailButton(
            Transform parent,
            string label,
            UnityEngine.Events.UnityAction action)
        {
            Button button = CreateButton(parent, label, 54f, action, false, 28f);
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.18f, 0.38f, 0.33f, 0.95f);
            }

            return button;
        }

        private Button CreateButton(
            Transform parent,
            string label,
            float width,
            UnityEngine.Events.UnityAction action,
            bool responsive = true,
            float height = 38f)
        {
            GameObject root = CreatePanel(label + " Button", parent, new Color(0.25f, 0.48f, 0.42f, 1f));
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.minHeight = height;
            layout.preferredHeight = height;
            if (responsive)
            {
                RegisterResponsiveBinding(layout, true, true);
            }
            else if (hasResponsiveProfile)
            {
                layout.preferredWidth = activeLayoutProfile.ScaleControlWidth(width);
                layout.preferredHeight = Mathf.Max(height, activeLayoutProfile.TouchTargetHeight);
            }

            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(action);
            Text text = CreateText("Label", root.transform, label, 13, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 2, 2, 0, 0);
            return button;
        }

        private Text CreateLabel(Transform parent, string value, float width, int size, TextAnchor alignment, bool responsive = false)
        {
            Text text = CreateText("Label", parent, value, size, alignment, Color.white);
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 30;
            if (responsive)
            {
                RegisterResponsiveBinding(layout, true, false);
            }

            return text;
        }

        private Text CreateCompactZoneLabel(Transform parent, string value, float width, float height, int size)
        {
            Text text = CreateText("Compact Zone Label", parent, value, size, TextAnchor.MiddleCenter, Color.white);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            return text;
        }

        private Button CreateCompactZoneButton(
            Transform parent,
            string label,
            float width,
            float height,
            UnityEngine.Events.UnityAction action)
        {
            GameObject root = CreatePanel(label + " Compact Zone Button", parent, new Color(0.18f, 0.34f, 0.30f, 0.92f));
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.minHeight = height;
            layout.preferredHeight = height;

            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(action);
            Text text = CreateText("Label", root.transform, label, 10, TextAnchor.MiddleCenter, Color.white);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            Stretch(text.rectTransform, 2f, 2f, 0f, 0f);
            return button;
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

        private static void AnchorBox(
            RectTransform rect,
            float minX,
            float minY,
            float maxX,
            float maxY,
            float inset)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private static void AnchorRightPanel(
            RectTransform rect,
            float width,
            float top,
            float bottom,
            float right)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.offsetMin = new Vector2(-width - right, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void AnchorRightHud(
            RectTransform rect,
            float width,
            float height,
            float top,
            float right)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(-width - right, -height - top);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
