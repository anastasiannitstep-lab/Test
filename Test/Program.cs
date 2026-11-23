using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ForbiddenWordsScanner
{
    static class Program
    {
        // Унікальний ідентифікатор Mutex для блокування запуску другої копії програми
        private const string AppMutexName = "Global\\ForbiddenWordsScanner_SingleInstance_Mutex_98421";

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        [STAThread]
        static void Main(string[] args)
        {
            using (var mutex = new Mutex(true, AppMutexName, out bool isNewInstance))
            {
                if (!isNewInstance)
                {
                    if (args.Length == 0)
                    {
                        MessageBox.Show("Додаток уже запущено!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }

                // Якщо передано аргументи командного рядка — запускаємо у консольному режимі без UI
                if (args.Length > 0)
                {
                    AttachConsole(-1); // Підключення до батьківської консолі (якщо запущено з cmd/powershell)
                    RunHeadlessMode(args);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }

        private static void RunHeadlessMode(string[] args)
        {
            // Очікуваний формат:
            // ForbiddenWordsScanner.exe -s "C:\Source" -o "C:\Output" -w "C:\words.txt"
            string source = GetArgValue(args, "-s");
            string output = GetArgValue(args, "-o");
            string wordsParam = GetArgValue(args, "-w");

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(output) || string.IsNullOrEmpty(wordsParam))
            {
                Console.WriteLine("Використання: ForbiddenWordsScanner.exe -s <шлях_до_папки_або_файлу> -o <папка_звіту> -w <файл_зі_словами_або_список_через_кому>");
                return;
            }

            string rawWords = File.Exists(wordsParam) ? File.ReadAllText(wordsParam) : wordsParam;
            var words = rawWords
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .Where(w => !string.IsNullOrEmpty(w))
                .ToHashSet();

            var options = new ScanOptions
            {
                SourcePath = source,
                OutputDirectory = output,
                ForbiddenWords = words
            };

            var engine = new ScanEngine();
            engine.ProgressChanged += (processed, total, file) =>
            {
                Console.Write($"\rОброблено: {processed}/{total}");
            };

            Console.WriteLine("Запуск сканування у фоновому режимі...");
            var summary = engine.StartScanAsync(options).GetAwaiter().GetResult();
            Console.WriteLine($"\nЗавершено! Знайдено файлів: {summary.FilesWithForbiddenWords}. Звіт: {summary.ReportFilePath}");
        }

        private static string GetArgValue(string[] args, string key)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }
    }
}
