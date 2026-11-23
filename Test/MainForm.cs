using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ForbiddenWordsScanner
{
    public partial class MainForm : Form
    {
        private readonly ScanEngine _engine = new ScanEngine();
        private string _lastReportPath;

        public MainForm()
        {
            InitializeComponent();
            SubscribeEngineEvents();
        }

        private void SubscribeEngineEvents()
        {
            _engine.ProgressChanged += (processed, total, currentFile) =>
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new Action(() => UpdateProgress(processed, total, currentFile)));
                else
                    UpdateProgress(processed, total, currentFile);
            };

            _engine.StatusChanged += status =>
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new Action(() => lblStatus.Text = $"Статус: {status}"));
                else
                    lblStatus.Text = $"Статус: {status}";
            };
        }

        private void UpdateProgress(int processed, int total, string currentFile)
        {
            progressBarTotal.Maximum = total > 0 ? total : 1;
            progressBarTotal.Value = Math.Min(processed, progressBarTotal.Maximum);
            lblCurrentFile.Text = $"({processed}/{total}) {currentFile}";
        }

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtSourcePath.Text = dlg.SelectedPath;
            }
        }

        private void BtnBrowseFile_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "Усі файли (*.*)|*.*|ZIP архіви (*.zip)|*.zip" })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtSourcePath.Text = dlg.FileName;
            }
        }

        private void BtnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtOutputDir.Text = dlg.SelectedPath;
            }
        }

        private void BtnLoadWordsFile_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*" })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtForbiddenWords.Text = File.ReadAllText(dlg.FileName);
            }
        }

        private void BtnOpenReport_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastReportPath) && File.Exists(_lastReportPath))
            {
                Process.Start("notepad.exe", _lastReportPath);
            }
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSourcePath.Text) || string.IsNullOrWhiteSpace(txtOutputDir.Text))
            {
                MessageBox.Show("Вкажіть джерело пошуку та папку призначення.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var words = txtForbiddenWords.Text
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .Where(w => !string.IsNullOrEmpty(w))
                .ToHashSet();

            if (words.Count == 0)
            {
                MessageBox.Show("Введіть або завантажте хоча б одне заборонене слово.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvResults.Rows.Clear();
            lstTopWords.Items.Clear();
            progressBarTotal.Value = 0;
            btnOpenReport.Enabled = false;

            SetControlsState(isRunning: true);

            var options = new ScanOptions
            {
                SourcePath = txtSourcePath.Text.Trim(),
                OutputDirectory = txtOutputDir.Text.Trim(),
                ForbiddenWords = words
            };

            try
            {
                var summary = await _engine.StartScanAsync(options);
                DisplayResults(summary);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час виконання: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsState(isRunning: false);
            }
        }

        private void BtnPauseResume_Click(object sender, EventArgs e)
        {
            if (_engine.IsPaused)
            {
                _engine.Resume();
                btnPauseResume.Text = "Пауза";
            }
            else
            {
                _engine.Pause();
                btnPauseResume.Text = "Відновити";
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            _engine.Stop();
            btnPauseResume.Enabled = false;
            btnStop.Enabled = false;
        }

        private void SetControlsState(bool isRunning)
        {
            btnStart.Enabled = !isRunning;
            btnBrowseFolder.Enabled = !isRunning;
            btnBrowseFile.Enabled = !isRunning;
            btnBrowseOutput.Enabled = !isRunning;
            btnLoadWordsFile.Enabled = !isRunning;
            txtForbiddenWords.ReadOnly = isRunning;

            btnPauseResume.Enabled = isRunning;
            btnPauseResume.Text = "Пауза";
            btnStop.Enabled = isRunning;
        }

        private void DisplayResults(ScanSummary summary)
        {
            foreach (var file in summary.MatchedFiles)
            {
                dgvResults.Rows.Add(
                    Path.GetFileName(file.OriginalPath),
                    file.OriginalPath,
                    file.FileSizeBytes,
                    file.TotalReplacements
                );
            }

            var top10 = summary.GlobalWordStats
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();

            int rank = 1;
            foreach (var item in top10)
            {
                lstTopWords.Items.Add($"{rank++}. {item.Key} ({item.Value})");
            }

            _lastReportPath = summary.ReportFilePath;
            btnOpenReport.Enabled = File.Exists(_lastReportPath);

            MessageBox.Show(
                $"Перевірено файлів: {summary.TotalFilesScanned}\nЗнайдено із забороненими словами: {summary.FilesWithForbiddenWords}\nЗвіт: {_lastReportPath}",
                "Сканування завершено",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}