using System;
using System.Collections.Generic;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VanguardThaiSim.UI
{
    public sealed class MultiplayerLobbyBootstrap : MonoBehaviour
    {
        private VanguardDeck localDeck;
        private VanguardDeck sessionDeck;
        private List<VanguardDeck> savedLobbyDecks = new List<VanguardDeck>();
        private int quickDeckChoiceIndex;
        private CardPackManifest manifest;
        private MultiplayerLobbyController controller;
        private PhotonRealtimeTransportConfig config;
        private Font font;
        private InputField playerNameInput;
        private InputField roomIdInput;
        private InputField eventCursorInput;
        private InputField revealTargetInput;
        private InputField revealNonceInput;
        private Text statusText;
        private Text deckPackText;
        private Text quickDeckText;
        private Text reconnectText;
        private Text trustText;
        private Text revealText;
        private Text roomText;
        private Text flowText;
        private Text navigationText;
        private GameObject quickEditPanel;
        private Text quickEditStatusText;
        private InputField quickEditDeckCodeInput;
        private Button backHomeButton;
        private Button connectButton;
        private Button hostButton;
        private Button joinButton;
        private Button leaveRoomButton;
        private Button quickDeckPreviousButton;
        private Button quickDeckNextButton;
        private Button quickDeckEditButton;
        private Button quickEditApplyButton;
        private Button quickEditCloseButton;
        private Button reconnectButton;
        private Button startTableButton;
        private Button readyButton;
        private Button notReadyButton;
        private Button rematchButton;
        private Button trustButton;
        private Button revealRequestButton;
        private Button revealSendButton;
        private float nextRefreshTime;
        private bool handoffToPlayTable;
        private string localStatusMessage;

        public static void Show(VanguardDeck localDeck, CardPackManifest manifest)
        {
            Show(localDeck, manifest, null);
        }

        public static void Show(VanguardDeck localDeck, CardPackManifest manifest, IMultiplayerTransport transport)
        {
            Show(localDeck, manifest, transport, null);
        }

        public static void Show(
            VanguardDeck localDeck,
            CardPackManifest manifest,
            IMultiplayerTransport transport,
            IReadOnlyList<VanguardDeck> savedDecks)
        {
            MultiplayerLobbyBootstrap existing = FindAnyObjectByType<MultiplayerLobbyBootstrap>();
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            GameObject host = new GameObject("Vanguard Multiplayer Lobby");
            MultiplayerLobbyBootstrap lobby = host.AddComponent<MultiplayerLobbyBootstrap>();
            lobby.Initialize(localDeck, manifest, transport, savedDecks);
        }

        private void Initialize(
            VanguardDeck deck,
            CardPackManifest cardPackManifest,
            IMultiplayerTransport injectedTransport,
            IReadOnlyList<VanguardDeck> injectedSavedDecks)
        {
            sessionDeck = CloneDeck(deck);
            localDeck = CloneDeck(deck);
            savedLobbyDecks = LoadSavedDecks(injectedSavedDecks);
            manifest = cardPackManifest;
            font = LoadFont();
            EnsureEventSystem();

            config = PhotonRealtimeConfigLoader.Load();
            IMultiplayerTransport transport = injectedTransport ?? new PhotonRealtimeTransportAdapter(config);
            controller = new MultiplayerLobbyController(transport, PackSyncInfo.FromManifest(manifest));

            BuildUi();
            RefreshUi(true);
        }

        private void Update()
        {
            if (controller == null)
            {
                return;
            }

            controller.Tick();
            if (Time.unscaledTime >= nextRefreshTime)
            {
                RefreshUi(false);
            }
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                if (!handoffToPlayTable)
                {
                    controller.Disconnect();
                }
            }
        }

        public MultiplayerLobbyController Controller
        {
            get { return controller; }
        }

        private void BuildUi()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = gameObject.GetComponent<RectTransform>();
            GameObject overlay = CreatePanel("Overlay", root, new Color(0.04f, 0.05f, 0.06f, 0.86f));
            Stretch(overlay.GetComponent<RectTransform>(), 0, 0, 0, 0);

            RectTransform panel = CreatePanel("Lobby Panel", root, new Color(0.13f, 0.15f, 0.18f, 1f)).GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(1160, 680);
            panel.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            RectTransform headerRow = CreateRow(panel, 50);
            CreateLabel(headerRow, "Online Room", 260, 24, TextAnchor.MiddleLeft);
            CreatePill(headerRow, "FRIEND ROOM", 130, new Color(0.72f, 0.18f, 0.58f, 1f));
            CreatePill(headerRow, "TRUSTED CLIENT", 160, new Color(0.18f, 0.42f, 0.58f, 1f));
            Space(headerRow, 1f);
            backHomeButton = CreateButton(headerRow, "Back Home", 120, BackHome);

            Text modeLine = CreateMultilineLabel(panel, MultiplayerLobbyStatusFormatter.TrustedClientModeLine, 1100, 28, 13);
            modeLine.alignment = TextAnchor.MiddleLeft;
            navigationText = CreateMultilineLabel(panel, "", 1100, 28, 13);
            navigationText.alignment = TextAnchor.MiddleLeft;

            RectTransform bodyRow = CreateRow(panel, 504);
            bodyRow.name = "Online Room Body";

            RectTransform connectionPanel = CreateColumnPanel(bodyRow, "Connection Panel", 340, new Color(0.1f, 0.12f, 0.15f, 1f));
            CreateLabel(connectionPanel, "Connection", 300, 20, TextAnchor.MiddleLeft);
            statusText = CreateMultilineLabel(connectionPanel, "", 300, 76, 13);
            deckPackText = CreateMultilineLabel(connectionPanel, "", 300, 56, 13);
            quickDeckText = CreateMultilineLabel(connectionPanel, "", 300, 54, 12);
            quickDeckText.gameObject.name = "Quick Deck Summary Text";
            RectTransform quickDeckRow = CreateRow(connectionPanel, 42);
            quickDeckPreviousButton = CreateButton(quickDeckRow, "Prev Deck", 94, PreviousQuickDeck);
            quickDeckNextButton = CreateButton(quickDeckRow, "Next Deck", 94, NextQuickDeck);
            quickDeckEditButton = CreateButton(quickDeckRow, "Quick Edit", 94, OpenQuickEditDeck);
            CreateDivider(connectionPanel, 300);
            playerNameInput = CreateInput(connectionPanel, "Player name", "Player", 300);
            roomIdInput = CreateInput(connectionPanel, "Room ID", "VGTH-" + DateTime.UtcNow.ToString("HHmm"), 300);
            RectTransform buttonRow = CreateRow(connectionPanel, 48);
            connectButton = CreateButton(buttonRow, "Connect", 96, Connect);
            hostButton = CreateButton(buttonRow, "Host", 82, HostRoom);
            joinButton = CreateButton(buttonRow, "Join", 82, JoinRoom);
            leaveRoomButton = CreateButton(connectionPanel, "Leave Room", 300, LeaveRoom);

            RectTransform roomPanel = CreateColumnPanel(bodyRow, "Room Panel", 370, new Color(0.09f, 0.11f, 0.13f, 1f));
            CreateLabel(roomPanel, "Room", 330, 20, TextAnchor.MiddleLeft);
            flowText = CreateMultilineLabel(roomPanel, "", 330, 76, 13);
            roomText = CreateMultilineLabel(roomPanel, "", 330, 92, 13);
            reconnectText = CreateMultilineLabel(roomPanel, "", 330, 86, 12);
            reconnectText.gameObject.name = "Reconnect Summary Text";
            CreateDivider(roomPanel, 330);
            eventCursorInput = CreateInput(roomPanel, "Reconnect cursor", "0", 330);
            RectTransform readyRow = CreateRow(roomPanel, 48);
            readyButton = CreateButton(readyRow, "Ready", 96, ReadyRoom);
            notReadyButton = CreateButton(readyRow, "Not Ready", 112, NotReadyRoom);
            rematchButton = CreateButton(readyRow, "Rematch", 102, RematchRoom);
            RectTransform reconnectRow = CreateRow(roomPanel, 48);
            reconnectButton = CreateButton(reconnectRow, "Reconnect", 150, ReconnectRoom);
            startTableButton = CreateButton(reconnectRow, "Start Table", 150, StartOnlineTable);

            RectTransform safetyPanel = CreateColumnPanel(bodyRow, "Safety Reveal Panel", 360, new Color(0.1f, 0.12f, 0.15f, 1f));
            CreateLabel(safetyPanel, "Safety / Reveal", 320, 20, TextAnchor.MiddleLeft);
            trustText = CreateMultilineLabel(safetyPanel, "", 320, 118, 12);
            trustButton = CreateButton(safetyPanel, "Acknowledge Trust", 320, AcknowledgeClientTrustWarning);
            CreateDivider(safetyPanel, 320);
            revealText = CreateMultilineLabel(safetyPanel, "", 320, 86, 13);
            RectTransform revealInputRow = CreateRow(safetyPanel, 48);
            revealTargetInput = CreateInput(revealInputRow, "Reveal target", "", 154);
            revealNonceInput = CreateInput(revealInputRow, "Reveal nonce", "", 154);
            RectTransform revealButtonRow = CreateRow(safetyPanel, 48);
            revealRequestButton = CreateButton(revealButtonRow, "Request Reveal", 154, RequestDeckReveal);
            revealSendButton = CreateButton(revealButtonRow, "Send Reveal", 154, SendDeckReveal);

            BuildQuickEditModal(root);
        }

        private void Connect()
        {
            localStatusMessage = null;
            controller.Connect();
            RefreshUi(true);
        }

        private void HostRoom()
        {
            localStatusMessage = null;
            if (!RequireOnlineDeckReady("Host"))
            {
                return;
            }

            if (!RequireLobbyReady("Host"))
            {
                return;
            }

            controller.CreateRoom(roomIdInput.text, playerNameInput.text, localDeck);
            RefreshUi(true);
        }

        private void JoinRoom()
        {
            localStatusMessage = null;
            if (!RequireOnlineDeckReady("Join"))
            {
                return;
            }

            if (!RequireLobbyReady("Join"))
            {
                return;
            }

            controller.JoinRoom(roomIdInput.text, playerNameInput.text, localDeck);
            RefreshUi(true);
        }

        private void ReconnectRoom()
        {
            localStatusMessage = null;
            if (!RequireOnlineDeckReady("Reconnect"))
            {
                return;
            }

            if (!RequireLobbyReady("Reconnect"))
            {
                return;
            }

            int cursor;
            if (!int.TryParse(eventCursorInput.text, out cursor))
            {
                cursor = 0;
            }

            controller.ReconnectRoom(roomIdInput.text, playerNameInput.text, localDeck, cursor);
            RefreshUi(true);
        }

        private void ReadyRoom()
        {
            localStatusMessage = null;
            controller.SetLocalReady(true);
            RefreshUi(true);
        }

        private void NotReadyRoom()
        {
            localStatusMessage = null;
            controller.SetLocalReady(false);
            RefreshUi(true);
        }

        private void RematchRoom()
        {
            localStatusMessage = null;
            controller.TryRematchRoom();
            RefreshUi(true);
        }

        private void LeaveRoom()
        {
            localStatusMessage = null;
            controller.Disconnect();
            RefreshUi(true);
        }

        private void PreviousQuickDeck()
        {
            CycleQuickDeck(-1);
        }

        private void NextQuickDeck()
        {
            CycleQuickDeck(1);
        }

        private void CycleQuickDeck(int direction)
        {
            if (controller != null && controller.CurrentRoom != null)
            {
                localStatusMessage = "Leave Room before changing the online deck.";
                RefreshUi(true);
                return;
            }

            if (savedLobbyDecks == null || savedLobbyDecks.Count <= 0)
            {
                localStatusMessage = "No saved decks found. Use Quick Edit to build or save a deck first.";
                RefreshUi(true);
                return;
            }

            int maxIndex = savedLobbyDecks.Count;
            quickDeckChoiceIndex += direction < 0 ? -1 : 1;
            if (quickDeckChoiceIndex < 0)
            {
                quickDeckChoiceIndex = maxIndex;
            }
            else if (quickDeckChoiceIndex > maxIndex)
            {
                quickDeckChoiceIndex = 0;
            }

            ApplyQuickDeckChoice();
            localStatusMessage = "Selected online deck: " +
                (localDeck == null || string.IsNullOrWhiteSpace(localDeck.name) ? "Untitled Deck" : localDeck.name.Trim()) + ".";
            RefreshUi(true);
        }

        private void ApplyQuickDeckChoice()
        {
            if (quickDeckChoiceIndex <= 0)
            {
                localDeck = CloneDeck(sessionDeck);
                return;
            }

            int savedIndex = quickDeckChoiceIndex - 1;
            if (savedLobbyDecks == null || savedIndex < 0 || savedIndex >= savedLobbyDecks.Count)
            {
                quickDeckChoiceIndex = 0;
                localDeck = CloneDeck(sessionDeck);
                return;
            }

            localDeck = CloneDeck(savedLobbyDecks[savedIndex]);
        }

        private void OpenQuickEditDeck()
        {
            if (controller != null && controller.CurrentRoom != null)
            {
                localStatusMessage = "Leave Room before opening Quick Edit.";
                RefreshUi(true);
                return;
            }

            if (quickEditPanel != null)
            {
                quickEditPanel.SetActive(true);
            }

            RefreshQuickEditStatus(null);
            RefreshUi(true);
        }

        private void CloseQuickEditDeck()
        {
            if (quickEditPanel != null)
            {
                quickEditPanel.SetActive(false);
            }

            RefreshUi(true);
        }

        private void ApplyQuickEditDeckCode()
        {
            if (controller != null && controller.CurrentRoom != null)
            {
                RefreshQuickEditStatus("Leave Room before editing the online deck.");
                RefreshUi(true);
                return;
            }

            string deckCode = quickEditDeckCodeInput == null ? "" : quickEditDeckCodeInput.text;
            if (string.IsNullOrWhiteSpace(deckCode))
            {
                RefreshQuickEditStatus("Paste a deck code before applying.");
                RefreshUi(true);
                return;
            }

            try
            {
                VanguardDeck imported = DeckCodeCodec.Import(deckCode.Trim());
                sessionDeck = CloneDeck(imported);
                localDeck = CloneDeck(imported);
                quickDeckChoiceIndex = 0;
                localStatusMessage = "Quick Edit applied deck: " +
                    (localDeck == null || string.IsNullOrWhiteSpace(localDeck.name)
                        ? "Untitled Deck"
                        : localDeck.name.Trim()) + ".";
                if (quickEditDeckCodeInput != null)
                {
                    quickEditDeckCodeInput.text = "";
                }

                if (quickEditPanel != null)
                {
                    quickEditPanel.SetActive(false);
                }
            }
            catch (Exception exception)
            {
                RefreshQuickEditStatus("Deck code import failed: " + exception.Message);
            }

            RefreshUi(true);
        }

        private void RequestDeckReveal()
        {
            localStatusMessage = null;
            string targetPlayerId = revealTargetInput == null ? "" : revealTargetInput.text;
            if (string.IsNullOrWhiteSpace(targetPlayerId))
            {
                localStatusMessage = "Reveal request needs a target player id.";
                RefreshUi(true);
                return;
            }

            controller.RequestDeckReveal(targetPlayerId.Trim());
            RefreshUi(true);
        }

        private void SendDeckReveal()
        {
            localStatusMessage = null;
            string nonce = revealNonceInput == null ? "" : revealNonceInput.text;
            controller.SendDeckRevealResponse(localDeck, nonce);
            RefreshUi(true);
        }

        private void AcknowledgeClientTrustWarning()
        {
            localStatusMessage = null;
            controller.AcknowledgeClientTrustWarning();
            RefreshUi(true);
        }

        private void StartOnlineTable()
        {
            localStatusMessage = null;
            MultiplayerTransportResult startResult = controller.TryStartRoom();
            if (startResult == null || !startResult.ok)
            {
                localStatusMessage = "Cannot start table: " + (startResult == null ? "ROOM_START_FAILED" : startResult.message) + ".";
                RefreshUi(true);
                return;
            }

            MultiplayerGameSessionController session;
            string rejectionReason;
            if (!controller.TryCreateGameSession(out session, out rejectionReason))
            {
                localStatusMessage = MultiplayerLobbyStatusFormatter.FormatStartTableRejection(rejectionReason);
                RefreshUi(true);
                return;
            }

            handoffToPlayTable = true;
            PlayTableBootstrap.ShowOnline(session);
            DestroyRuntimeObject(gameObject);
        }

        private void BackHome()
        {
            if (controller != null && controller.CurrentRoom != null)
            {
                localStatusMessage = "Leave Room before returning Home. Back navigation is locked while a Photon room is active.";
                RefreshUi(true);
                return;
            }

            DestroyRuntimeObject(gameObject);
            HomeLobbyBootstrap.Show();
        }

        private bool RequireLobbyReady(string action)
        {
            if (controller.Status == MultiplayerTransportStatus.ConnectedToLobby)
            {
                return true;
            }

            if (controller.Status == MultiplayerTransportStatus.Disconnected || controller.Status == MultiplayerTransportStatus.Failed)
            {
                controller.Connect();
                localStatusMessage = action + " needs lobby connection. Connecting now; retry when status is ConnectedToLobby.";
                RefreshUi(true);
                return false;
            }

            localStatusMessage = action + " needs status ConnectedToLobby. Current status: " + controller.Status + ".";
            RefreshUi(true);
            return false;
        }

        private bool RequireOnlineDeckReady(string action)
        {
            MultiplayerLobbyDeckReadinessResult readiness =
                MultiplayerLobbyDeckReadiness.Evaluate(localDeck);
            if (readiness.accepted)
            {
                return true;
            }

            localStatusMessage = action + " needs a ready deck: " + readiness.rejection_reason;
            RefreshUi(true);
            return false;
        }

        private void RefreshUi(bool force)
        {
            nextRefreshTime = Time.unscaledTime + 0.25f;
            if (controller == null)
            {
                return;
            }

            DeckPrivacyGameplayDecision privacyDecision = controller.EvaluateDeckPrivacyForGameplay();
            string message = string.IsNullOrWhiteSpace(localStatusMessage)
                ? controller.LastMessage
                : localStatusMessage;

            statusText.text = MultiplayerLobbyStatusFormatter.FormatConnectionStatus(
                controller.TransportName,
                controller.Status,
                config != null && config.IsConfigured,
                message);
            if (deckPackText != null)
            {
                deckPackText.text = MultiplayerLobbyStatusFormatter.FormatDeckPackStatus(localDeck, manifest);
            }
            if (quickDeckText != null)
            {
                quickDeckText.text = MultiplayerLobbyStatusFormatter.FormatQuickDeckSelector(
                    localDeck,
                    savedLobbyDecks == null ? 0 : savedLobbyDecks.Count,
                    quickDeckChoiceIndex,
                    controller.CurrentRoom != null);
            }

            trustText.text = MultiplayerLobbyStatusFormatter.FormatTrustSummary(controller.CurrentRoom, privacyDecision);
            revealText.text = BuildRevealSummary();
            if (flowText != null)
            {
                flowText.text = MultiplayerLobbyStatusFormatter.FormatFlowSummary(
                    controller.Status,
                    controller.CurrentRoom,
                    controller.LocalPlayer,
                    privacyDecision);
            }
            if (navigationText != null)
            {
                navigationText.text = MultiplayerLobbyStatusFormatter.FormatNavigationLockout(controller.CurrentRoom);
            }
            roomText.text = MultiplayerLobbyStatusFormatter.FormatRoomStatus(
                controller.Status,
                controller.CurrentRoom,
                PackSyncInfo.FromManifest(manifest));
            if (reconnectText != null)
            {
                reconnectText.text = MultiplayerLobbyStatusFormatter.FormatReconnectSummary(
                    controller.LastReconnectRequest,
                    controller.LastReconnectBatch,
                    controller.CurrentRoom,
                    controller.LocalPlayer == null ? 0 : controller.LocalPlayer.event_cursor,
                    controller.LastMessage);
            }

            RefreshButtonState(privacyDecision);
        }

        private void RefreshButtonState(DeckPrivacyGameplayDecision privacyDecision)
        {
            bool canUseLobbyActions = controller.Status == MultiplayerTransportStatus.ConnectedToLobby;
            bool hasRoom = controller.CurrentRoom != null;
            bool canConnect = controller.Status == MultiplayerTransportStatus.Disconnected ||
                              controller.Status == MultiplayerTransportStatus.Failed;
            bool roomEnded = hasRoom &&
                string.Equals(controller.CurrentRoom.state ?? "", RoomLifecycleStates.Ended, StringComparison.Ordinal);
            bool roomPlaying = hasRoom &&
                string.Equals(controller.CurrentRoom.state ?? "", RoomLifecycleStates.Playing, StringComparison.Ordinal);

            SetButtonInteractable(backHomeButton, !hasRoom);
            SetButtonInteractable(connectButton, canConnect);
            SetButtonInteractable(hostButton, canUseLobbyActions);
            SetButtonInteractable(joinButton, canUseLobbyActions);
            SetButtonInteractable(leaveRoomButton, hasRoom);
            SetButtonInteractable(quickDeckPreviousButton, !hasRoom && savedLobbyDecks != null && savedLobbyDecks.Count > 0);
            SetButtonInteractable(quickDeckNextButton, !hasRoom && savedLobbyDecks != null && savedLobbyDecks.Count > 0);
            SetButtonInteractable(quickDeckEditButton, !hasRoom);
            SetButtonInteractable(quickEditApplyButton, !hasRoom);
            SetButtonInteractable(quickEditCloseButton, true);
            SetButtonInteractable(reconnectButton, canUseLobbyActions);
            SetButtonInteractable(readyButton, hasRoom && !roomPlaying);
            SetButtonInteractable(notReadyButton, hasRoom && !roomPlaying);
            SetButtonInteractable(rematchButton, roomEnded);
            SetButtonInteractable(startTableButton, hasRoom && !roomEnded);
            SetButtonInteractable(
                trustButton,
                hasRoom && privacyDecision != null && privacyDecision.requires_client_trust_warning);
            SetButtonInteractable(revealRequestButton, hasRoom);
            SetButtonInteractable(revealSendButton, hasRoom);
        }

        private string BuildRevealSummary()
        {
            if (controller == null)
            {
                return "";
            }

            return MultiplayerLobbyStatusFormatter.FormatRevealSummary(
                controller.LastDeckRevealRequest,
                controller.LastDeckRevealResponse,
                controller.LastDeckRevealAccepted,
                controller.LastDeckRevealMessage);
        }

        private void RefreshQuickEditStatus(string message)
        {
            if (quickEditStatusText == null)
            {
                return;
            }

            quickEditStatusText.text = MultiplayerLobbyStatusFormatter.FormatQuickEditStatus(
                localDeck,
                message,
                controller != null && controller.CurrentRoom != null);
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private RectTransform CreateRow(Transform parent, float height)
        {
            RectTransform row = CreatePanel("Row", parent, new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            HorizontalLayoutGroup horizontal = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.spacing = 8;
            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.childControlHeight = true;
            horizontal.childControlWidth = false;
            return row;
        }

        private RectTransform CreateColumnPanel(Transform parent, string name, float width, Color color)
        {
            RectTransform column = CreatePanel(name, parent, color).GetComponent<RectTransform>();
            LayoutElement layoutElement = column.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = width;
            layoutElement.flexibleWidth = 0f;

            VerticalLayoutGroup vertical = column.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(14, 14, 14, 14);
            vertical.spacing = 7;
            vertical.childAlignment = TextAnchor.UpperLeft;
            vertical.childControlWidth = false;
            vertical.childControlHeight = false;
            return column;
        }

        private void CreateDivider(Transform parent, float width)
        {
            GameObject divider = CreatePanel("Divider", parent, new Color(0.25f, 0.27f, 0.31f, 1f));
            LayoutElement layout = divider.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 1f;
        }

        private Text CreatePill(Transform parent, string label, float width, Color color)
        {
            GameObject root = CreatePanel(label + " Pill", parent, color);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 32f;
            Text text = CreateText("Label", root.transform, label, 12, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 6, 6, 3, 3);
            return text;
        }

        private void Space(Transform parent, float flexibleWidth)
        {
            GameObject space = new GameObject("Flexible Space");
            space.transform.SetParent(parent, false);
            LayoutElement layout = space.AddComponent<LayoutElement>();
            layout.flexibleWidth = flexibleWidth;
        }

        private InputField CreateInput(Transform parent, string placeholder, string value, float width)
        {
            GameObject root = CreatePanel(placeholder + " Input", parent, Color.white);
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 42;

            InputField input = root.AddComponent<InputField>();
            Text text = CreateText("Text", root.transform, value, 16, TextAnchor.MiddleLeft, Color.black);
            Stretch(text.rectTransform, 10, 10, 6, 6);
            Text placeholderText = CreateText("Placeholder", root.transform, placeholder, 16, TextAnchor.MiddleLeft, new Color(0.45f, 0.45f, 0.45f, 1f));
            Stretch(placeholderText.rectTransform, 10, 10, 6, 6);
            input.textComponent = text;
            input.placeholder = placeholderText;
            input.text = value;
            return input;
        }

        private Button CreateButton(Transform parent, string label, float width, UnityEngine.Events.UnityAction action)
        {
            GameObject root = CreatePanel(label + " Button", parent, new Color(0.78f, 0.24f, 0.64f, 1f));
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 42;
            Button button = root.AddComponent<Button>();
            button.onClick.AddListener(action);
            Text text = CreateText("Label", root.transform, label, 15, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform, 0, 0, 0, 0);
            return button;
        }

        private Text CreateLabel(Transform parent, string value, float width, int size, TextAnchor alignment)
        {
            Text text = CreateText("Label", parent, value, size, alignment, Color.white);
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 34;
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

        private void BuildQuickEditModal(Transform parent)
        {
            quickEditPanel = CreatePanel("Quick Edit Modal", parent, new Color(0.06f, 0.07f, 0.09f, 0.98f));
            RectTransform panelRect = quickEditPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(820, 300);
            panelRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = quickEditPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 10;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            CreateLabel(quickEditPanel.transform, "Quick Edit Online Deck", 760, 22, TextAnchor.MiddleLeft);
            quickEditStatusText = CreateMultilineLabel(quickEditPanel.transform, "", 760, 76, 13);
            quickEditStatusText.gameObject.name = "Quick Edit Status Text";
            quickEditDeckCodeInput = CreateInput(quickEditPanel.transform, "Paste deck code", "", 760);
            quickEditDeckCodeInput.gameObject.name = "Quick Edit Deck Code Input";
            RectTransform buttonRow = CreateRow(quickEditPanel.transform, 48);
            quickEditApplyButton = CreateButton(buttonRow, "Apply Deck Code", 180, ApplyQuickEditDeckCode);
            quickEditCloseButton = CreateButton(buttonRow, "Close Edit", 130, CloseQuickEditDeck);
            Space(buttonRow, 1f);
            quickEditPanel.SetActive(false);
            RefreshQuickEditStatus(null);
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

        private static List<VanguardDeck> LoadSavedDecks(IReadOnlyList<VanguardDeck> injectedSavedDecks)
        {
            List<VanguardDeck> decks = new List<VanguardDeck>();
            if (injectedSavedDecks != null)
            {
                for (int i = 0; i < injectedSavedDecks.Count; i++)
                {
                    VanguardDeck deck = CloneDeck(injectedSavedDecks[i]);
                    if (deck != null)
                    {
                        decks.Add(deck);
                    }
                }

                return decks;
            }

            DeckStorage storage = new DeckStorage();
            foreach (string deckId in storage.ListDeckIds())
            {
                try
                {
                    VanguardDeck deck = CloneDeck(storage.Load(deckId));
                    if (deck != null)
                    {
                        decks.Add(deck);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Failed to load saved online deck " + deckId + ": " + exception.Message);
                }
            }

            return decks;
        }

        private static VanguardDeck CloneDeck(VanguardDeck deck)
        {
            return deck == null ? null : VanguardDeck.FromJson(deck.ToJson(false));
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

        private static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
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
