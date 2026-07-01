using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;
using UnityEngine;
using UnityEngine.UI;

namespace VanguardThaiSim.Smoke
{
    public sealed class VisualEvidenceScreenshot
    {
        public string screen_name;
        public string file_path;
        public bool exists;
    }

    public sealed class VisualEvidenceReport
    {
        public List<VisualEvidenceScreenshot> screenshots = new List<VisualEvidenceScreenshot>();
        public List<string> issues = new List<string>();

        public bool IsPass
        {
            get { return issues.Count == 0; }
        }
    }

    public static class VisualEvidenceBootstrap
    {
        public const string VisualEvidenceFlag = "-vanguardVisualEvidence";
        public const string OutputArg = "-vanguardVisualEvidenceOutput";
        public const string DirectoryArg = "-vanguardVisualEvidenceDir";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RunFromPlayer()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!IsVisualEvidenceRequested(args))
            {
                return;
            }

            GameObject host = new GameObject("Vanguard Visual Evidence Runner");
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<VisualEvidenceRunner>().Initialize(args);
        }

        public static bool IsVisualEvidenceRequested(string[] args)
        {
            return ContainsArg(args, VisualEvidenceFlag);
        }

        public static string ResolveOutputPath(string[] args, string fallbackPath)
        {
            string explicitPath = GetArgValue(args, OutputArg);
            return Path.GetFullPath(string.IsNullOrWhiteSpace(explicitPath) ? fallbackPath : explicitPath);
        }

        public static string ResolveScreenshotDirectory(string[] args, string fallbackDirectory)
        {
            string explicitPath = GetArgValue(args, DirectoryArg);
            return Path.GetFullPath(string.IsNullOrWhiteSpace(explicitPath) ? fallbackDirectory : explicitPath);
        }

        private static bool ContainsArg(string[] args, string key)
        {
            if (args == null)
            {
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetArgValue(string[] args, string key)
        {
            if (args == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return string.Empty;
        }
    }

    public sealed class VisualEvidenceRunner : MonoBehaviour
    {
        private string[] args = Array.Empty<string>();

        public void Initialize(string[] commandLineArgs)
        {
            args = commandLineArgs ?? Array.Empty<string>();
        }

        private IEnumerator Start()
        {
            VisualEvidenceReport report = new VisualEvidenceReport();
            string screenshotDirectory = VisualEvidenceBootstrap.ResolveScreenshotDirectory(
                args,
                Path.Combine(Application.persistentDataPath, "visual_evidence"));
            string outputPath = VisualEvidenceBootstrap.ResolveOutputPath(
                args,
                Path.Combine(screenshotDirectory, "visual_evidence_report.json"));

            Directory.CreateDirectory(screenshotDirectory);
            Screen.SetResolution(1280, 720, false);
            yield return WaitFrames(3);

            yield return CaptureHome(report, screenshotDirectory);
            yield return CaptureDeckBuilder(report, screenshotDirectory);
            yield return CaptureCardBrowser(report, screenshotDirectory);
            yield return CapturePlayTable(report, screenshotDirectory);
            yield return CaptureReplay(report, screenshotDirectory);

            WriteReport(outputPath, report);
            int exitCode = report.IsPass ? 0 : 2;
            if (report.IsPass)
            {
                Debug.Log("Visual evidence capture passed: " + outputPath);
            }
            else
            {
                Debug.LogError("Visual evidence capture blocked: " + outputPath);
            }

            Application.Quit(exitCode);
        }

        private IEnumerator CaptureHome(VisualEvidenceReport report, string directory)
        {
            DestroyAllRuntimeScreens();
            yield return WaitFrames(2);
            HomeLobbyBootstrap.Show();
            yield return WaitFrames(4);
            yield return Capture(report, directory, "home");
        }

        private IEnumerator CaptureDeckBuilder(VisualEvidenceReport report, string directory)
        {
            DestroyAllRuntimeScreens();
            yield return WaitFrames(2);
            CardBrowserBootstrap.ShowDeckBuilder();
            yield return WaitFrames(5);
            yield return Capture(report, directory, "deck_builder");
        }

        private IEnumerator CaptureCardBrowser(VisualEvidenceReport report, string directory)
        {
            DestroyAllRuntimeScreens();
            yield return WaitFrames(2);
            CardBrowserBootstrap.ShowBrowser();
            yield return WaitFrames(5);
            yield return Capture(report, directory, "card_browser");
        }

        private IEnumerator CapturePlayTable(VisualEvidenceReport report, string directory)
        {
            DestroyAllRuntimeScreens();
            yield return WaitFrames(2);
            VanguardDeck deck = CreateSmokeDeck();
            PlayTableBootstrap.Show(deck, deck, "Visual evidence local table");
            yield return WaitFrames(5);
            yield return Capture(report, directory, "play_table");
        }

        private IEnumerator CaptureReplay(VisualEvidenceReport report, string directory)
        {
            DestroyAllRuntimeScreens();
            yield return WaitFrames(2);
            HomeLobbyBootstrap.Show();
            yield return WaitFrames(4);
            Button replayButton = FindButton("Replay Button");
            if (replayButton == null)
            {
                report.issues.Add("Replay Button not found for visual evidence capture.");
            }
            else
            {
                replayButton.onClick.Invoke();
                yield return WaitFrames(4);
            }

            yield return Capture(report, directory, "replay");
        }

        private IEnumerator Capture(VisualEvidenceReport report, string directory, string screenName)
        {
            string path = Path.Combine(directory, screenName + ".png");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(path);

            float deadline = Time.realtimeSinceStartup + 5f;
            while (!File.Exists(path) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            bool exists = File.Exists(path);
            report.screenshots.Add(new VisualEvidenceScreenshot
            {
                screen_name = screenName,
                file_path = Path.GetFullPath(path),
                exists = exists
            });
            if (!exists)
            {
                report.issues.Add("Screenshot was not written: " + screenName);
            }
        }

        private static VanguardDeck CreateSmokeDeck()
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            CardRepositoryLoadResult loadResult = CardRepositoryFactory.LoadDefault(packDirectory, manifest);
            using (loadResult.Repository as IDisposable)
            {
                IReadOnlyList<CardSummary> cards = loadResult.Repository.QueryCards(new CardQueryOptions { Limit = 60 });
                if (cards.Count < 54)
                {
                    throw new InvalidOperationException("Visual evidence smoke deck needs at least 54 cards.");
                }

                VanguardDeck deck = VanguardDeck.Create("Visual Evidence Deck", "D", manifest.pack_id, manifest.source_version);
                for (int i = 0; i < 50; i++)
                {
                    deck.AddCard(DeckZone.Main, cards[i].CardId, 1);
                }

                for (int i = 0; i < 4; i++)
                {
                    deck.AddCard(DeckZone.Ride, cards[50 + i].CardId, 1);
                }

                return deck;
            }
        }

        private static IEnumerator WaitFrames(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return null;
            }
        }

        private static Button FindButton(string name)
        {
            GameObject root = GameObject.Find(name);
            return root == null ? null : root.GetComponent<Button>();
        }

        private static void DestroyAllRuntimeScreens()
        {
            DestroyAll<HomeLobbyBootstrap>();
            DestroyAll<CardBrowserBootstrap>();
            DestroyAll<PlayTableBootstrap>();
        }

        private static void DestroyAll<T>() where T : MonoBehaviour
        {
            T[] components = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                UnityEngine.Object.Destroy(components[i].gameObject);
            }
        }

        private static void WriteReport(string outputPath, VisualEvidenceReport report)
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, ToJson(report));
        }

        private static string ToJson(VisualEvidenceReport report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"screenshots\": [");
            for (int i = 0; i < report.screenshots.Count; i++)
            {
                VisualEvidenceScreenshot screenshot = report.screenshots[i];
                builder.Append("    { \"screen_name\": \"");
                builder.Append(Escape(screenshot.screen_name));
                builder.Append("\", \"file_path\": \"");
                builder.Append(Escape(screenshot.file_path));
                builder.Append("\", \"exists\": ");
                builder.Append(screenshot.exists ? "true" : "false");
                builder.Append(" }");
                if (i + 1 < report.screenshots.Count)
                {
                    builder.Append(",");
                }

                builder.AppendLine();
            }

            builder.AppendLine("  ],");
            builder.AppendLine("  \"issues\": [");
            for (int i = 0; i < report.issues.Count; i++)
            {
                builder.Append("    \"");
                builder.Append(Escape(report.issues[i]));
                builder.Append("\"");
                if (i + 1 < report.issues.Count)
                {
                    builder.Append(",");
                }

                builder.AppendLine();
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string Escape(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
