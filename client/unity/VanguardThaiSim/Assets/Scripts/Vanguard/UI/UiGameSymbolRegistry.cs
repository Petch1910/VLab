using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VanguardThaiSim.UI
{
    public static class UiGameSymbolKeys
    {
        public const string TriggerCritical = "trigger_critical";
        public const string TriggerDraw = "trigger_draw";
        public const string TriggerFront = "trigger_front";
        public const string TriggerHeal = "trigger_heal";
        public const string TriggerOver = "trigger_over";
        public const string TriggerStandLegacy = "trigger_stand_legacy";

        public const string MarkerForce = "marker_force";
        public const string MarkerAccel = "marker_accel";
        public const string MarkerProtect = "marker_protect";
        public const string MarkerQuickShield = "marker_quick_shield";
        public const string MarkerPersona = "marker_persona";
        public const string MarkerCrest = "marker_crest";
        public const string MarkerEnergy = "marker_energy";
        public const string MarkerGauge = "marker_gauge";

        public const string CardUnit = "card_unit";
        public const string CardOrder = "card_order";
        public const string CardTrigger = "card_trigger";

        public const string ZoneDeck = "zone_deck";
        public const string ZoneHand = "zone_hand";
        public const string ZoneDrop = "zone_drop";
        public const string ZoneDamage = "zone_damage";
        public const string ZoneSoul = "zone_soul";
        public const string ZoneBind = "zone_bind";
        public const string ZoneReplay = "zone_replay";
    }

    [Serializable]
    public sealed class UiGameSymbolDefinition
    {
        public string key;
        public string default_label;
        public string fallback_icon_asset;
        public string color_token;
        public string tooltip;

        public UiGameSymbolDefinition Clone()
        {
            return new UiGameSymbolDefinition
            {
                key = key ?? string.Empty,
                default_label = default_label ?? string.Empty,
                fallback_icon_asset = fallback_icon_asset ?? string.Empty,
                color_token = color_token ?? string.Empty,
                tooltip = tooltip ?? string.Empty
            };
        }
    }

    [Serializable]
    public sealed class UiGameSymbolResolution
    {
        public string key;
        public string label;
        public string icon_path;
        public string color_token;
        public string tooltip;
        public string source;
        public string fallback_reason;
    }

    public static class UiGameSymbolRegistry
    {
        private static readonly Dictionary<string, UiGameSymbolDefinition> Definitions =
            new Dictionary<string, UiGameSymbolDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { UiGameSymbolKeys.TriggerCritical, Def(UiGameSymbolKeys.TriggerCritical, "CRITICAL", "Lucide/swords.svg", "critical", "Critical trigger") },
                { UiGameSymbolKeys.TriggerDraw, Def(UiGameSymbolKeys.TriggerDraw, "DRAW", "Lucide/download.svg", "draw", "Draw trigger") },
                { UiGameSymbolKeys.TriggerFront, Def(UiGameSymbolKeys.TriggerFront, "FRONT", "Lucide/arrow-up.svg", "front", "Front trigger") },
                { UiGameSymbolKeys.TriggerHeal, Def(UiGameSymbolKeys.TriggerHeal, "HEAL", "Lucide/heart-pulse.svg", "heal", "Heal trigger") },
                { UiGameSymbolKeys.TriggerOver, Def(UiGameSymbolKeys.TriggerOver, "OVER", "Lucide/sparkles.svg", "over", "Over trigger") },
                { UiGameSymbolKeys.TriggerStandLegacy, Def(UiGameSymbolKeys.TriggerStandLegacy, "STAND", "Lucide/rotate-cw.svg", "stand", "Legacy stand trigger") },

                { UiGameSymbolKeys.MarkerForce, Def(UiGameSymbolKeys.MarkerForce, "FORCE", "Lucide/swords.svg", "force", "Force marker") },
                { UiGameSymbolKeys.MarkerAccel, Def(UiGameSymbolKeys.MarkerAccel, "ACCEL", "Lucide/zap.svg", "accel", "Accel marker") },
                { UiGameSymbolKeys.MarkerProtect, Def(UiGameSymbolKeys.MarkerProtect, "PROTECT", "Lucide/shield.svg", "protect", "Protect marker") },
                { UiGameSymbolKeys.MarkerQuickShield, Def(UiGameSymbolKeys.MarkerQuickShield, "Q-SHIELD", "Lucide/shield-plus.svg", "protect", "Quick Shield") },
                { UiGameSymbolKeys.MarkerPersona, Def(UiGameSymbolKeys.MarkerPersona, "PERSONA", "Lucide/star-check.svg", "persona", "Persona Ride") },
                { UiGameSymbolKeys.MarkerCrest, Def(UiGameSymbolKeys.MarkerCrest, "CREST", "Lucide/badge-check.svg", "crest", "Crest") },
                { UiGameSymbolKeys.MarkerEnergy, Def(UiGameSymbolKeys.MarkerEnergy, "ENERGY", "Lucide/zap.svg", "energy", "Energy") },
                { UiGameSymbolKeys.MarkerGauge, Def(UiGameSymbolKeys.MarkerGauge, "GAUGE", "Lucide/gauge.svg", "gauge", "Gauge") },

                { UiGameSymbolKeys.CardUnit, Def(UiGameSymbolKeys.CardUnit, "UNIT", "Lucide/card-sim.svg", "card", "Unit card") },
                { UiGameSymbolKeys.CardOrder, Def(UiGameSymbolKeys.CardOrder, "ORDER", "Lucide/scroll-text.svg", "card", "Order card") },
                { UiGameSymbolKeys.CardTrigger, Def(UiGameSymbolKeys.CardTrigger, "TRIGGER", "Lucide/sparkles.svg", "trigger", "Trigger card") },

                { UiGameSymbolKeys.ZoneDeck, Def(UiGameSymbolKeys.ZoneDeck, "DECK", "Lucide/wallet-cards.svg", "zone", "Deck zone") },
                { UiGameSymbolKeys.ZoneHand, Def(UiGameSymbolKeys.ZoneHand, "HAND", "Lucide/hand.svg", "zone", "Hand zone") },
                { UiGameSymbolKeys.ZoneDrop, Def(UiGameSymbolKeys.ZoneDrop, "DROP", "Lucide/trash-2.svg", "zone", "Drop zone") },
                { UiGameSymbolKeys.ZoneDamage, Def(UiGameSymbolKeys.ZoneDamage, "DAMAGE", "Lucide/shield-alert.svg", "zone", "Damage zone") },
                { UiGameSymbolKeys.ZoneSoul, Def(UiGameSymbolKeys.ZoneSoul, "SOUL", "Lucide/orbit.svg", "zone", "Soul zone") },
                { UiGameSymbolKeys.ZoneBind, Def(UiGameSymbolKeys.ZoneBind, "BIND", "Lucide/target.svg", "zone", "Bind zone") },
                { UiGameSymbolKeys.ZoneReplay, Def(UiGameSymbolKeys.ZoneReplay, "REPLAY", "Lucide/scroll-text.svg", "zone", "Replay") }
            };

        public static bool IsKnownKey(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && Definitions.ContainsKey(key.Trim());
        }

        public static IReadOnlyList<UiGameSymbolDefinition> DefaultDefinitions()
        {
            List<UiGameSymbolDefinition> definitions = new List<UiGameSymbolDefinition>(Definitions.Count);
            foreach (UiGameSymbolDefinition definition in Definitions.Values)
            {
                definitions.Add(definition.Clone());
            }

            return definitions;
        }

        public static UiGameSymbolDefinition GetDefault(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Def(string.Empty, "UNKNOWN", string.Empty, "neutral", "Unknown symbol");
            }

            UiGameSymbolDefinition definition;
            if (Definitions.TryGetValue(key.Trim(), out definition))
            {
                return definition.Clone();
            }

            return Def(key.Trim(), key.Trim().ToUpperInvariant(), string.Empty, "neutral", "Unknown symbol");
        }

        public static UiGameSymbolResolution Resolve(string key, UserIconPackValidationResult iconPack)
        {
            UiGameSymbolDefinition definition = GetDefault(key);
            UserIconPackResolvedIcon overrideIcon = iconPack == null ? null : iconPack.FindResolvedIcon(definition.key);
            if (overrideIcon != null)
            {
                return new UiGameSymbolResolution
                {
                    key = definition.key,
                    label = definition.default_label,
                    icon_path = overrideIcon.full_path,
                    color_token = definition.color_token,
                    tooltip = definition.tooltip,
                    source = "user",
                    fallback_reason = string.Empty
                };
            }

            return new UiGameSymbolResolution
            {
                key = definition.key,
                label = definition.default_label,
                icon_path = definition.fallback_icon_asset,
                color_token = definition.color_token,
                tooltip = definition.tooltip,
                source = "default",
                fallback_reason = "user icon missing or not loaded"
            };
        }

        private static UiGameSymbolDefinition Def(string key, string label, string icon, string color, string tooltip)
        {
            return new UiGameSymbolDefinition
            {
                key = key,
                default_label = label,
                fallback_icon_asset = icon,
                color_token = color,
                tooltip = tooltip
            };
        }
    }

    public static class UserIconPackRuntime
    {
        public const string ManifestFileName = "icon-pack-manifest.json";

        private static readonly string[] UserProvidedSegments =
        {
            "UI",
            "Icons",
            "UserProvided"
        };

        private static readonly string[] PersistentSegments =
        {
            "Icons",
            "UserProvided"
        };

        private static readonly string[] AssetsSegments =
        {
            "Assets",
            "UI",
            "Icons",
            "UserProvided"
        };

        private static readonly string[] ProjectSegments =
        {
            "client",
            "unity",
            "VanguardThaiSim",
            "Assets",
            "UI",
            "Icons",
            "UserProvided"
        };

        public static UserIconPackValidationResult LoadDefault()
        {
            try
            {
                return UserIconPackValidator.LoadAndValidate(ResolveDefaultManifestPath());
            }
            catch (Exception exception)
            {
                return CreateFallbackWarning("MANIFEST_READ_FAILED", "User icon manifest could not be read; defaults will be used: " + exception.GetType().Name);
            }
        }

        public static string ResolveDefaultManifestPath()
        {
            List<string> roots = new List<string>();
            AddIfNotEmpty(roots, Application.persistentDataPath);
            AddIfNotEmpty(roots, Application.dataPath);
            AddIfNotEmpty(roots, Directory.GetCurrentDirectory());
            return ResolveManifestPath(roots);
        }

        public static string ResolveManifestPath(IReadOnlyList<string> candidateRoots)
        {
            if (candidateRoots == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < candidateRoots.Count; i++)
            {
                string root = candidateRoots[i];
                string direct = TryResolveDirect(root);
                if (!string.IsNullOrEmpty(direct))
                {
                    return direct;
                }

                string near = TryResolveNear(root);
                if (!string.IsNullOrEmpty(near))
                {
                    return near;
                }
            }

            return string.Empty;
        }

        private static string TryResolveDirect(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return string.Empty;
            }

            string fullRoot = Path.GetFullPath(root);
            if (File.Exists(fullRoot) &&
                string.Equals(Path.GetFileName(fullRoot), ManifestFileName, StringComparison.OrdinalIgnoreCase))
            {
                return fullRoot;
            }

            if (!Directory.Exists(fullRoot))
            {
                return string.Empty;
            }

            string inRoot = Path.Combine(fullRoot, ManifestFileName);
            if (File.Exists(inRoot))
            {
                return Path.GetFullPath(inRoot);
            }

            string inPersistentRoot = Combine(fullRoot, PersistentSegments, ManifestFileName);
            if (File.Exists(inPersistentRoot))
            {
                return Path.GetFullPath(inPersistentRoot);
            }

            string inAssetRoot = Combine(fullRoot, UserProvidedSegments, ManifestFileName);
            if (File.Exists(inAssetRoot))
            {
                return Path.GetFullPath(inAssetRoot);
            }

            string inAssetsProjectRoot = Combine(fullRoot, AssetsSegments, ManifestFileName);
            if (File.Exists(inAssetsProjectRoot))
            {
                return Path.GetFullPath(inAssetsProjectRoot);
            }

            string inProjectRoot = Combine(fullRoot, ProjectSegments, ManifestFileName);
            if (File.Exists(inProjectRoot))
            {
                return Path.GetFullPath(inProjectRoot);
            }

            return string.Empty;
        }

        private static string TryResolveNear(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return string.Empty;
            }

            DirectoryInfo current = Directory.Exists(root)
                ? new DirectoryInfo(Path.GetFullPath(root))
                : Directory.GetParent(Path.GetFullPath(root));

            while (current != null)
            {
                string direct = TryResolveDirect(current.FullName);
                if (!string.IsNullOrEmpty(direct))
                {
                    return direct;
                }

                current = current.Parent;
            }

            return string.Empty;
        }

        private static string Combine(string root, string[] segments, string fileName)
        {
            string path = root;
            for (int i = 0; i < segments.Length; i++)
            {
                path = Path.Combine(path, segments[i]);
            }

            return Path.Combine(path, fileName);
        }

        private static void AddIfNotEmpty(List<string> roots, string root)
        {
            if (!string.IsNullOrWhiteSpace(root))
            {
                roots.Add(root);
            }
        }

        private static UserIconPackValidationResult CreateFallbackWarning(string code, string message)
        {
            UserIconPackValidationResult result = new UserIconPackValidationResult
            {
                accepted = true
            };
            result.issues.Add(new UserIconPackValidationIssue
            {
                severity = "warning",
                code = code ?? string.Empty,
                key = string.Empty,
                message = message ?? string.Empty
            });
            return result;
        }
    }

    [Serializable]
    public sealed class UserIconPackManifest
    {
        public const string SupportedSchema = "vanguard-icon-pack-v1";

        public string schema;
        public string pack_id;
        public string display_name;
        public List<UserIconPackManifestEntry> icons = new List<UserIconPackManifestEntry>();

        public string ToJson(bool prettyPrint)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static UserIconPackManifest FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new UserIconPackManifest();
            }

            UserIconPackManifest manifest = JsonUtility.FromJson<UserIconPackManifest>(json) ?? new UserIconPackManifest();
            manifest.icons = ParseIconObjectEntries(json);
            manifest.EnsureLists();
            return manifest;
        }

        private void EnsureLists()
        {
            if (icons == null)
            {
                icons = new List<UserIconPackManifestEntry>();
            }
        }

        private static List<UserIconPackManifestEntry> ParseIconObjectEntries(string json)
        {
            List<UserIconPackManifestEntry> entries = new List<UserIconPackManifestEntry>();
            int propertyIndex = json.IndexOf("\"icons\"", StringComparison.Ordinal);
            if (propertyIndex < 0)
            {
                return entries;
            }

            int objectStart = json.IndexOf('{', propertyIndex);
            if (objectStart < 0)
            {
                return entries;
            }

            int objectEnd = FindMatchingBrace(json, objectStart);
            if (objectEnd <= objectStart)
            {
                return entries;
            }

            string body = json.Substring(objectStart + 1, objectEnd - objectStart - 1);
            MatchCollection matches = Regex.Matches(
                body,
                "\"(?<key>(?:\\\\.|[^\"])*)\"\\s*:\\s*\"(?<file>(?:\\\\.|[^\"])*)\"");
            for (int i = 0; i < matches.Count; i++)
            {
                entries.Add(new UserIconPackManifestEntry
                {
                    key = Regex.Unescape(matches[i].Groups["key"].Value),
                    file = Regex.Unescape(matches[i].Groups["file"].Value)
                });
            }

            return entries;
        }

        private static int FindMatchingBrace(string value, int start)
        {
            bool inString = false;
            bool escaped = false;
            int depth = 0;
            for (int i = start; i < value.Length; i++)
            {
                char ch = value[i];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (ch == '\\')
                    {
                        escaped = true;
                    }
                    else if (ch == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                }
                else if (ch == '{')
                {
                    depth++;
                }
                else if (ch == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }

    [Serializable]
    public sealed class UserIconPackManifestEntry
    {
        public string key;
        public string file;
    }

    [Serializable]
    public sealed class UserIconPackValidationIssue
    {
        public string severity;
        public string code;
        public string key;
        public string message;
    }

    [Serializable]
    public sealed class UserIconPackResolvedIcon
    {
        public string key;
        public string file;
        public string full_path;
    }

    [Serializable]
    public sealed class UserIconPackValidationResult
    {
        public bool accepted;
        public string pack_id;
        public string display_name;
        public int declared_icon_count;
        public int accepted_icon_count;
        public int fallback_icon_count;
        public List<UserIconPackResolvedIcon> resolved_icons = new List<UserIconPackResolvedIcon>();
        public List<UserIconPackValidationIssue> issues = new List<UserIconPackValidationIssue>();

        public string ToJson(bool prettyPrint)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static UserIconPackValidationResult FromJson(string json)
        {
            UserIconPackValidationResult result = string.IsNullOrWhiteSpace(json)
                ? new UserIconPackValidationResult()
                : JsonUtility.FromJson<UserIconPackValidationResult>(json);
            if (result == null)
            {
                result = new UserIconPackValidationResult();
            }

            result.EnsureLists();
            return result;
        }

        public UserIconPackResolvedIcon FindResolvedIcon(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || resolved_icons == null)
            {
                return null;
            }

            for (int i = 0; i < resolved_icons.Count; i++)
            {
                UserIconPackResolvedIcon icon = resolved_icons[i];
                if (icon != null && string.Equals(icon.key, key.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return icon;
                }
            }

            return null;
        }

        private void EnsureLists()
        {
            if (resolved_icons == null)
            {
                resolved_icons = new List<UserIconPackResolvedIcon>();
            }

            if (issues == null)
            {
                issues = new List<UserIconPackValidationIssue>();
            }
        }
    }

    public static class UserIconPackValidator
    {
        private const string Error = "error";
        private const string Warning = "warning";

        public static UserIconPackValidationResult Validate(UserIconPackManifest manifest, string rootDirectory)
        {
            UserIconPackValidationResult result = new UserIconPackValidationResult();
            if (manifest == null)
            {
                AddIssue(result, Error, "MANIFEST_MISSING", string.Empty, "Icon pack manifest is missing.");
                Finish(result);
                return result;
            }

            if (manifest.icons == null)
            {
                manifest.icons = new List<UserIconPackManifestEntry>();
            }

            result.pack_id = manifest.pack_id ?? string.Empty;
            result.display_name = manifest.display_name ?? string.Empty;
            result.declared_icon_count = manifest.icons.Count;

            if (!string.Equals(manifest.schema, UserIconPackManifest.SupportedSchema, StringComparison.Ordinal))
            {
                AddIssue(result, Error, "SCHEMA_UNSUPPORTED", string.Empty, "Icon pack schema must be " + UserIconPackManifest.SupportedSchema + ".");
            }

            if (string.IsNullOrWhiteSpace(manifest.pack_id))
            {
                AddIssue(result, Warning, "PACK_ID_MISSING", string.Empty, "Icon pack id is missing.");
            }

            string fullRoot = NormalizeRoot(rootDirectory);
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < manifest.icons.Count; i++)
            {
                UserIconPackManifestEntry entry = manifest.icons[i];
                string key = entry == null ? string.Empty : (entry.key ?? string.Empty).Trim();
                string file = entry == null ? string.Empty : (entry.file ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(key))
                {
                    AddIssue(result, Warning, "ICON_KEY_MISSING", string.Empty, "Icon entry key is missing.");
                    continue;
                }

                if (!seen.Add(key))
                {
                    AddIssue(result, Warning, "ICON_KEY_DUPLICATE", key, "Duplicate icon key is ignored.");
                    continue;
                }

                if (!UiGameSymbolRegistry.IsKnownKey(key))
                {
                    AddIssue(result, Warning, "ICON_KEY_UNKNOWN", key, "Icon key is not a supported UI semantic key.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(file))
                {
                    AddIssue(result, Warning, "ICON_FILE_MISSING_NAME", key, "Icon file name is missing; default symbol will be used.");
                    continue;
                }

                string extension = Path.GetExtension(file);
                if (!string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase))
                {
                    AddIssue(result, Warning, "ICON_FILE_UNSUPPORTED_EXTENSION", key, "Only png/svg icon overrides are supported.");
                    continue;
                }

                string fullPath = ResolveInsideRoot(fullRoot, file);
                if (string.IsNullOrEmpty(fullPath))
                {
                    AddIssue(result, Error, "ICON_FILE_OUTSIDE_ROOT", key, "Icon file path must stay inside the user-provided icon folder.");
                    continue;
                }

                if (!File.Exists(fullPath))
                {
                    AddIssue(result, Warning, "ICON_FILE_NOT_FOUND", key, "Icon file was not found; default symbol will be used.");
                    continue;
                }

                result.resolved_icons.Add(new UserIconPackResolvedIcon
                {
                    key = key,
                    file = file,
                    full_path = fullPath
                });
            }

            Finish(result);
            return result;
        }

        public static UserIconPackValidationResult LoadAndValidate(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
            {
                UserIconPackValidationResult result = new UserIconPackValidationResult();
                AddIssue(result, Warning, "MANIFEST_FILE_NOT_FOUND", string.Empty, "User icon manifest was not found; defaults will be used.");
                Finish(result);
                return result;
            }

            string json = File.ReadAllText(manifestPath);
            UserIconPackManifest manifest = UserIconPackManifest.FromJson(json);
            return Validate(manifest, Path.GetDirectoryName(Path.GetFullPath(manifestPath)));
        }

        private static void Finish(UserIconPackValidationResult result)
        {
            int errors = 0;
            for (int i = 0; i < result.issues.Count; i++)
            {
                if (result.issues[i] != null && result.issues[i].severity == Error)
                {
                    errors++;
                }
            }

            result.accepted_icon_count = result.resolved_icons.Count;
            result.fallback_icon_count = Math.Max(0, result.declared_icon_count - result.accepted_icon_count);
            result.accepted = errors == 0;
        }

        private static void AddIssue(UserIconPackValidationResult result, string severity, string code, string key, string message)
        {
            result.issues.Add(new UserIconPackValidationIssue
            {
                severity = severity,
                code = code,
                key = key ?? string.Empty,
                message = message ?? string.Empty
            });
        }

        private static string NormalizeRoot(string rootDirectory)
        {
            string root = string.IsNullOrWhiteSpace(rootDirectory) ? Directory.GetCurrentDirectory() : rootDirectory;
            return Path.GetFullPath(root);
        }

        private static string ResolveInsideRoot(string fullRoot, string relativeFile)
        {
            if (Path.IsPathRooted(relativeFile))
            {
                return string.Empty;
            }

            string fullPath = Path.GetFullPath(Path.Combine(fullRoot, relativeFile.Replace('/', Path.DirectorySeparatorChar)));
            string rootWithSeparator = fullRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? fullRoot
                : fullRoot + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(fullPath, fullRoot, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return fullPath;
        }
    }
}
