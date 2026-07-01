using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Smoke
{
    [Serializable]
    public sealed class WindowsPerformanceGateReport
    {
        public int schema_version = 1;
        public string milestone = "M27-04";
        public bool accepted;
        public int playtable_target_fps = WindowsPerformanceGate.PlayTableTargetFps;
        public float playtable_frame_budget_ms = WindowsPerformanceGate.PlayTableFrameBudgetMs;
        public int cache_max_thumbnails = WindowsPerformanceGate.CacheMaxThumbnails;
        public int cache_max_full_images = WindowsPerformanceGate.CacheMaxFullImages;
        public int cache_observed_thumbnail_count;
        public int cache_observed_full_image_count;
        public bool cache_clear_memory_passed;
        public long max_repository_load_ms = WindowsPerformanceGate.MaxRepositoryLoadMs;
        public long max_card_query_ms = WindowsPerformanceGate.MaxCardQueryMs;
        public long max_card_detail_ms = WindowsPerformanceGate.MaxCardDetailMs;
        public long max_deck_validation_ms = WindowsPerformanceGate.MaxDeckValidationMs;
        public long max_deck_code_roundtrip_ms = WindowsPerformanceGate.MaxDeckCodeRoundtripMs;
        public float max_headless_average_elapsed_ms = WindowsPerformanceGate.MaxHeadlessAverageElapsedMs;
        public WindowsPerformanceBaselineReport baseline_report;
        public HeadlessPerformanceProfileResult headless_profile;
        public string summary;
        public List<string> blockers = new List<string>();
        public List<string> notes = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static WindowsPerformanceGateReport FromJson(string json)
        {
            WindowsPerformanceGateReport report =
                JsonUtility.FromJson<WindowsPerformanceGateReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Windows performance gate report JSON could not be parsed.",
                    "json");
            }

            report.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (blockers == null)
            {
                blockers = new List<string>();
            }

            if (notes == null)
            {
                notes = new List<string>();
            }
        }
    }

    public static class WindowsPerformanceGate
    {
        public const int PlayTableTargetFps = 30;
        public const float PlayTableFrameBudgetMs = 33.334f;
        public const int CacheMaxThumbnails = 4;
        public const int CacheMaxFullImages = 2;
        public const long MaxRepositoryLoadMs = 2500;
        public const long MaxCardQueryMs = 1000;
        public const long MaxCardDetailMs = 750;
        public const long MaxDeckValidationMs = 2500;
        public const long MaxDeckCodeRoundtripMs = 1000;
        public const float MaxHeadlessAverageElapsedMs = 200f;

        private const int CacheStressImageCount = 10;
        private const int HeadlessProfileRunCount = 3;

        public static WindowsPerformanceGateReport Run()
        {
            WindowsPerformanceGateReport report = new WindowsPerformanceGateReport();
            try
            {
                report.baseline_report = WindowsPerformanceBaseline.Run();
                EvaluateBaseline(report);
                EvaluateImageCache(report);
                EvaluateHeadlessProfile(report);
            }
            catch (Exception exception)
            {
                report.blockers.Add(exception.GetType().Name + ": " + exception.Message);
            }

            Finish(report);
            return report;
        }

        private static void EvaluateBaseline(WindowsPerformanceGateReport report)
        {
            WindowsPerformanceBaselineReport baseline = report.baseline_report;
            if (baseline == null)
            {
                report.blockers.Add("baseline_missing");
                return;
            }

            baseline.EnsureLists();
            if (!baseline.accepted)
            {
                report.blockers.Add("baseline_not_accepted");
                foreach (string blocker in baseline.blockers)
                {
                    report.blockers.Add("baseline:" + blocker);
                }

                return;
            }

            AddThresholdBlocker(
                report.blockers,
                "repository_load_ms",
                baseline.repository_load_ms,
                MaxRepositoryLoadMs);
            AddThresholdBlocker(report.blockers, "card_query_ms", baseline.card_query_ms, MaxCardQueryMs);
            AddThresholdBlocker(report.blockers, "card_detail_ms", baseline.card_detail_ms, MaxCardDetailMs);
            AddThresholdBlocker(
                report.blockers,
                "deck_validation_ms",
                baseline.deck_validation_ms,
                MaxDeckValidationMs);
            AddThresholdBlocker(
                report.blockers,
                "deck_code_roundtrip_ms",
                baseline.deck_code_roundtrip_ms,
                MaxDeckCodeRoundtripMs);
        }

        private static void EvaluateImageCache(WindowsPerformanceGateReport report)
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            CardRepositoryLoadResult loadResult = CardRepositoryFactory.LoadDefault(packDirectory, manifest);
            using (loadResult.Repository as IDisposable)
            using (CardImageCache cache = new CardImageCache(
                       CardPackFileSystem.GetImageRootPath(manifest),
                       CacheMaxThumbnails,
                       CacheMaxFullImages))
            {
                List<string> imagePaths = loadResult.Repository
                    .QueryCards(new CardQueryOptions { Limit = 250 })
                    .Where(card => card.ImageExists && !string.IsNullOrEmpty(card.ImageRelativePath))
                    .Select(card => card.ImageRelativePath)
                    .Distinct()
                    .Take(CacheStressImageCount)
                    .ToList();

                if (imagePaths.Count < CacheMaxFullImages)
                {
                    report.blockers.Add("image_cache_stress_not_enough_images");
                    return;
                }

                foreach (string imagePath in imagePaths)
                {
                    cache.LoadThumbnail(imagePath);
                    cache.LoadFullImage(imagePath);
                }

                report.cache_observed_thumbnail_count = cache.ThumbnailCount;
                report.cache_observed_full_image_count = cache.FullImageCount;
                if (report.cache_observed_thumbnail_count > CacheMaxThumbnails)
                {
                    report.blockers.Add(
                        "thumbnail_cache_exceeded_limit:" +
                        report.cache_observed_thumbnail_count.ToString() +
                        "/" +
                        CacheMaxThumbnails.ToString());
                }

                if (report.cache_observed_full_image_count > CacheMaxFullImages)
                {
                    report.blockers.Add(
                        "full_image_cache_exceeded_limit:" +
                        report.cache_observed_full_image_count.ToString() +
                        "/" +
                        CacheMaxFullImages.ToString());
                }

                cache.ClearMemory();
                report.cache_clear_memory_passed = cache.ThumbnailCount == 0 && cache.FullImageCount == 0;
                if (!report.cache_clear_memory_passed)
                {
                    report.blockers.Add("image_cache_clear_memory_failed");
                }
            }
        }

        private static void EvaluateHeadlessProfile(WindowsPerformanceGateReport report)
        {
            report.headless_profile = HeadlessPerformanceProfiler.Run(new HeadlessPerformanceProfileRequest
            {
                start_seed = 2704,
                run_count = HeadlessProfileRunCount,
                ruleset = "D"
            });

            if (report.headless_profile == null)
            {
                report.blockers.Add("headless_profile_missing");
                return;
            }

            if (!report.headless_profile.accepted)
            {
                report.blockers.Add("headless_profile_not_accepted:" + report.headless_profile.rejection_reason);
                return;
            }

            if (report.headless_profile.average_elapsed_ms > MaxHeadlessAverageElapsedMs)
            {
                report.blockers.Add(
                    "headless_average_elapsed_ms_exceeded:" +
                    report.headless_profile.average_elapsed_ms.ToString("0.###") +
                    "/" +
                    MaxHeadlessAverageElapsedMs.ToString("0.###"));
            }
        }

        private static void AddThresholdBlocker(
            List<string> blockers,
            string metric,
            long observed,
            long maximum)
        {
            if (observed > maximum)
            {
                blockers.Add(metric + "_exceeded:" + observed.ToString() + "/" + maximum.ToString());
            }
        }

        private static void Finish(WindowsPerformanceGateReport report)
        {
            report.EnsureLists();
            report.notes.Add("playtable_target_is_contract_only_not_a_gpu_frame_capture");
            report.notes.Add("image_memory_gate_uses_bounded_cache_slots_not_texture_byte_accounting");
            report.accepted = report.blockers.Count == 0;
            report.summary = report.accepted
                ? "Windows memory/performance gate passed."
                : "Windows memory/performance gate blocked.";
        }
    }
}
