using System;
using System.IO;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    public static class PhotonRealtimeConfigLoader
    {
        public const string AppIdEnv = "VANGUARD_PHOTON_APP_ID";
        public const string RegionEnv = "VANGUARD_PHOTON_REGION";
        public const string AppVersionEnv = "VANGUARD_PHOTON_APP_VERSION";
        public const string LocalConfigRelativePath = "ProjectSettings/VanguardPhotonLocal.json";

        public static PhotonRealtimeTransportConfig Load(string projectRoot = null)
        {
            return LoadFrom(
                projectRoot,
                Environment.GetEnvironmentVariable(AppIdEnv),
                Environment.GetEnvironmentVariable(RegionEnv),
                Environment.GetEnvironmentVariable(AppVersionEnv));
        }

        public static PhotonRealtimeTransportConfig LoadFrom(
            string projectRoot,
            string appIdOverride,
            string regionOverride,
            string appVersionOverride)
        {
            LocalPhotonConfigJson fileConfig = LoadFileConfig(projectRoot);
            return new PhotonRealtimeTransportConfig
            {
                app_id = FirstNonEmpty(appIdOverride, fileConfig == null ? null : fileConfig.app_id),
                fixed_region = FirstNonEmpty(regionOverride, fileConfig == null ? null : fileConfig.fixed_region, "asia"),
                app_version = FirstNonEmpty(appVersionOverride, fileConfig == null ? null : fileConfig.app_version, "0.1.0")
            };
        }

        private static LocalPhotonConfigJson LoadFileConfig(string projectRoot)
        {
            string root = string.IsNullOrWhiteSpace(projectRoot)
                ? Path.GetFullPath(Path.Combine(Application.dataPath, ".."))
                : projectRoot;
            string configPath = Path.Combine(root, LocalConfigRelativePath);
            if (!File.Exists(configPath))
            {
                return null;
            }

            string json = File.ReadAllText(configPath);
            return JsonUtility.FromJson<LocalPhotonConfigJson>(json);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        [Serializable]
        private sealed class LocalPhotonConfigJson
        {
            public string app_id;
            public string fixed_region;
            public string app_version;
        }
    }
}
