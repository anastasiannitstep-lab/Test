using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ForbiddenWordsScanner
{
    public class ScanEngine
    {
        private CancellationTokenSource _cts;
        private readonly ManualResetEventSlim _pauseEvent =
            new ManualResetEventSlim(true);

        private static readonly HashSet<string> BinaryExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".exe", ".dll", ".bin", ".obj", ".pdb",
                ".jpg", ".jpeg", ".png", ".gif", ".bmp",
                ".mp3", ".mp4", ".avi", ".mkv", ".pdf",
                ".doc", ".docx", ".xls", ".xlsx",
                ".zip", ".rar", ".7z"
            };

        public event Action<int, int, string> ProgressChanged;
        public event Action<string> StatusChanged;

        public bool IsPaused => !_pauseEvent.IsSet;

        public void Pause()
        {
            _pauseEvent.Reset();
            StatusChanged?.Invoke("Призупинено");
        }

        public void Resume()
        {
            _pauseEvent.Set();
            StatusChanged?.Invoke("Виконується...");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _pauseEvent.Set();
            StatusChanged?.Invoke("Зупинка...");
        }

        public async Task<ScanSummary> StartScanAsync(ScanOptions options)
        {
            _cts = new CancellationTokenSource();
            _pauseEvent.Set();
            var token = _cts.Token;

            return await Task.Run(() =>
            {
                string tempDir = null;

                try
                {
                    StatusChanged?.Invoke("Підготовка файлів...");
                    var files = PrepareFiles(options.SourcePath, out tempDir);

                    int total = files.Count;
                    int processed = 0;

                    var matched = new ConcurrentBag<FileScanResult>();
                    var stats = new ConcurrentDictionary<string, int>(
                        StringComparer.OrdinalIgnoreCase);

                    string outDir = options.OutputDirectory;
                    string origDir = Path.Combine(outDir, "Originals");
                    string cleanDir = Path.Combine(outDir, "Cleaned");

                    Directory.CreateDirectory(origDir);
                    Directory.CreateDirectory(cleanDir);

                    var patterns = BuildPatterns(options.ForbiddenWords);
                    StatusChanged?.Invoke("Сканування файлів...");

                    var pOptions = new ParallelOptions
                    {
                        CancellationToken = token,
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    };

                    try
                    {
                        Parallel.ForEach(files, pOptions, file =>
                        {
                            _pauseEvent.Wait(token);
                            token.ThrowIfCancellationRequested();

                            ProcessFile(
                                file, tempDir, patterns,
                                origDir, cleanDir, matched, stats);

                            int current = Interlocked.Increment(ref processed);
                            string name = Path.GetFileName(file);
                            ProgressChanged?.Invoke(current, total, name);
                        });
                    }
                    catch (OperationCanceledException) { }

                    var summary = new ScanSummary
                    {
                        TotalFilesScanned = processed,
                        FilesWithForbiddenWords = matched.Count,
                        MatchedFiles = matched
                            .OrderByDescending(f => f.TotalReplacements)
                            .ToList(),
                        GlobalWordStats = stats.ToDictionary(
                            k => k.Key, v => v.Value)
                    };

                    summary.ReportFilePath = GenerateReport(summary, outDir);

                    string endStatus = token.IsCancellationRequested
                        ? "Зупинено користувачем"
                        : "Завершено";
                    StatusChanged?.Invoke(endStatus);

                    return summary;
                }
                finally
                {
                    CleanupTempDir(tempDir);
                }
            }, token);
        }

        private Dictionary<string, Regex> BuildPatterns(
            IEnumerable<string> words)
        {
            return words
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    w => w,
                    w => new Regex(
                        $@"\b{Regex.Escape(w)}\b",
                        RegexOptions.IgnoreCase | RegexOptions.Compiled)
                );
        }

        private List<string> PrepareFiles(
            string sourcePath, out string tempDir)
        {
            tempDir = null;
            var files = new List<string>();

            if (Directory.Exists(sourcePath))
            {
                EnumerateSafe(sourcePath, files);
                return files;
            }

            if (!File.Exists(sourcePath))
                return files;

            string ext = Path.GetExtension(sourcePath);

            // 1. ZIP-архів (файл, усередині якого є інші файли)
            if (ext.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                string folderName = "ScanZip_" + Guid.NewGuid().ToString("N");
                tempDir = Path.Combine(Path.GetTempPath(), folderName);
                Directory.CreateDirectory(tempDir);

                ZipFile.ExtractToDirectory(sourcePath, tempDir);
                EnumerateSafe(tempDir, files);
                return files;
            }

            // 2. Текстовий файл зі списком шляхів до інших файлів
            bool isListFile = false;
            foreach (var line in File.ReadAllLines(sourcePath))
            {
                string path = line.Trim().Trim('"');
                if (string.IsNullOrEmpty(path)) continue;

                if (File.Exists(path))
                {
                    files.Add(path);
                    isListFile = true;
                }
                else if (Directory.Exists(path))
                {
                    EnumerateSafe(path, files);
                    isListFile = true;
                }
            }

            // 3. Звичайний файл із текстом
            if (!isListFile)
                files.Add(sourcePath);

            return files;
        }

        private void EnumerateSafe(string rootDir, List<string> files)
        {
            try
            {
                foreach (var file in Directory.GetFiles(rootDir))
                {
                    string ext = Path.GetExtension(file);
                    if (!BinaryExtensions.Contains(ext))
                        files.Add(file);
                }

                foreach (var dir in Directory.GetDirectories(rootDir))
                {
                    EnumerateSafe(dir, files);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
        }

        private void ProcessFile(
            string filePath,
            string tempDir,
            Dictionary<string, Regex> patterns,
            string origDir,
            string cleanDir,
            ConcurrentBag<FileScanResult> matched,
            ConcurrentDictionary<string, int> stats)
        {
            try
            {
                string content = File.ReadAllText(filePath, Encoding.UTF8);
                string modified = content;

                int totalReplaced = 0;
                var fileStats = new Dictionary<string, int>();

                foreach (var kvp in patterns)
                {
                    int count = kvp.Value.Matches(modified).Count;
                    if (count > 0)
                    {
                        modified = kvp.Value.Replace(modified, "*******");
                        totalReplaced += count;
                        fileStats[kvp.Key] = count;

                        stats.AddOrUpdate(
                            kvp.Key, count, (_, old) => old + count);
                    }
                }

                if (totalReplaced == 0)
                    return;

                var info = new FileInfo(filePath);
                string prefix = Guid.NewGuid().ToString("N").Substring(0, 6);
                string fileName = Path.GetFileName(filePath);

                string destOrig = Path.Combine(
                    origDir, $"{prefix}_{fileName}");
                string destClean = Path.Combine(
                    cleanDir, $"{prefix}_cleaned_{fileName}");

                File.Copy(filePath, destOrig, true);
                File.WriteAllText(destClean, modified, Encoding.UTF8);

                string displayPath = GetDisplayPath(filePath, tempDir);

                matched.Add(new FileScanResult
                {
                    OriginalPath = displayPath,
                    CopiedPath = destOrig,
                    CleanedPath = destClean,
                    FileSizeBytes = info.Length,
                    TotalReplacements = totalReplaced,
                    WordCounts = fileStats
                });
            }
            catch
            {
                // Пропуск заблокованих або системних файлів
            }
        }

        private string GetDisplayPath(string filePath, string tempDir)
        {
            bool isFromZip = !string.IsNullOrEmpty(tempDir)
            && filePath.StartsWith(tempDir);
            if (!isFromZip)
                return filePath;
            string rel = filePath.Substring(tempDir.Length).TrimStart('\\', '/');
            return "[ZIP] " + rel;
        }
        private string GenerateReport(ScanSummary summary, string outDir)
        {
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string reportPath = Path.Combine(outDir, $"Report_{stamp}.txt");
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine($"ЗВІТ ({DateTime.Now:dd.MM.yyyy HH:mm:ss})");
            sb.AppendLine("========================================");
            sb.AppendLine($"Перевірено файлів: {summary.TotalFilesScanned}");
            sb.AppendLine($"Із порушеннями:   {summary.FilesWithForbiddenWords}");
            sb.AppendLine();
            sb.AppendLine("--- ТОП-10 ЗАБОРОНЕНИХ СЛІВ ---");
            var top10 = summary.GlobalWordStats
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToList();
            if (top10.Count == 0)
            {
                sb.AppendLine("Збігів не знайдено.");
            }
            else
            {
                for (int i = 0; i < top10.Count; i++)
                {
                    sb.AppendLine(
                    $"{i + 1}. {top10[i].Key} — {top10[i].Value} разів");
                }
            }
            sb.AppendLine();
            sb.AppendLine("--- ДЕТАЛІ ПО ФАЙЛАХ ---");
            foreach (var item in summary.MatchedFiles)
            {
                string wordsList = string.Join(
                ", ",
                item.WordCounts.Select(w => $"{w.Key} ({w.Value})"));
                sb.AppendLine($"Файл:            {item.OriginalPath}");
                sb.AppendLine($"Копія оригіналу: {item.CopiedPath}");
                sb.AppendLine($"Очищена копія:   {item.CleanedPath}");
                sb.AppendLine($"Розмір:          {item.FileSizeBytes} байт");
                sb.AppendLine($"Замін у файлі:   {item.TotalReplacements}");
                sb.AppendLine($"Слова:           {wordsList}");
                sb.AppendLine("----------------------------------------");
            }
            File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
            return reportPath;
        }
        private void CleanupTempDir(string tempDir)
        {
            if (string.IsNullOrEmpty(tempDir) || !Directory.Exists(tempDir))
                return;
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}