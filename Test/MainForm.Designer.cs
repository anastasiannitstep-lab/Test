using System.Drawing;
using System.Windows.Forms;

namespace ForbiddenWordsScanner
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblSource;
        private TextBox txtSourcePath;
        private Button btnBrowseFolder;
        private Button btnBrowseFile;

        private Label lblOutput;
        private TextBox txtOutputDir;
        private Button btnBrowseOutput;

        private Label lblWords;
        private TextBox txtForbiddenWords;
        private Button btnLoadWordsFile;

        private Button btnStart;
        private Button btnPauseResume;
        private Button btnStop;
        private Button btnOpenReport;

        private ProgressBar progressBarTotal;
        private Label lblStatus;
        private Label lblCurrentFile;

        private Label lblResults;
        private DataGridView dgvResults;
        private DataGridViewTextBoxColumn colFileName;
        private DataGridViewTextBoxColumn colPath;
        private DataGridViewTextBoxColumn colSize;
        private DataGridViewTextBoxColumn colReplacements;

        private Label lblTopWords;
        private ListBox lstTopWords;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblSource = new Label();
            this.txtSourcePath = new TextBox();
            this.btnBrowseFolder = new Button();
            this.btnBrowseFile = new Button();

            this.lblOutput = new Label();
            this.txtOutputDir = new TextBox();
            this.btnBrowseOutput = new Button();

            this.lblWords = new Label();
            this.txtForbiddenWords = new TextBox();
            this.btnLoadWordsFile = new Button();

            this.btnStart = new Button();
            this.btnPauseResume = new Button();
            this.btnStop = new Button();
            this.btnOpenReport = new Button();

            this.progressBarTotal = new ProgressBar();
            this.lblStatus = new Label();
            this.lblCurrentFile = new Label();

            this.lblResults = new Label();
            this.dgvResults = new DataGridView();
            this.colFileName = new DataGridViewTextBoxColumn();
            this.colPath = new DataGridViewTextBoxColumn();
            this.colSize = new DataGridViewTextBoxColumn();
            this.colReplacements = new DataGridViewTextBoxColumn();

            this.lblTopWords = new Label();
            this.lstTopWords = new ListBox();

            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.SuspendLayout();

            // lblSource
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new Point(15, 12);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new Size(215, 15);
            this.lblSource.Text = "Папка або файл зі списком файлів:";

            // txtSourcePath
            this.txtSourcePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.txtSourcePath.Location = new Point(15, 32);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.Size = new Size(650, 23);

            // btnBrowseFolder
            this.btnBrowseFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnBrowseFolder.Location = new Point(675, 31);
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.Size = new Size(100, 25);
            this.btnBrowseFolder.Text = "Папка...";
            this.btnBrowseFolder.Click += new System.EventHandler(this.BtnBrowseFolder_Click);

            // btnBrowseFile
            this.btnBrowseFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnBrowseFile.Location = new Point(783, 31);
            this.btnBrowseFile.Name = "btnBrowseFile";
            this.btnBrowseFile.Size = new Size(100, 25);
            this.btnBrowseFile.Text = "Файл...";
            this.btnBrowseFile.Click += new System.EventHandler(this.BtnBrowseFile_Click);

            // lblOutput
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new Point(15, 65);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new Size(260, 15);
            this.lblOutput.Text = "Папка для збереження результатів та звіту:";

            // txtOutputDir
            this.txtOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.txtOutputDir.Location = new Point(15, 85);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new Size(758, 23);

            // btnBrowseOutput
            this.btnBrowseOutput.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnBrowseOutput.Location = new Point(783, 84);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new Size(100, 25);
            this.btnBrowseOutput.Text = "Огляд...";
            this.btnBrowseOutput.Click += new System.EventHandler(this.BtnBrowseOutput_Click);

            // lblWords
            this.lblWords.AutoSize = true;
            this.lblWords.Location = new Point(15, 118);
            this.lblWords.Name = "lblWords";
            this.lblWords.Size = new Size(305, 15);
            this.lblWords.Text = "Заборонені слова (через кому або з нового рядка):";

            // txtForbiddenWords
            this.txtForbiddenWords.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.txtForbiddenWords.Location = new Point(15, 138);
            this.txtForbiddenWords.Multiline = true;
            this.txtForbiddenWords.Name = "txtForbiddenWords";
            this.txtForbiddenWords.ScrollBars = ScrollBars.Vertical;
            this.txtForbiddenWords.Size = new Size(650, 68);

            // btnLoadWordsFile
            this.btnLoadWordsFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnLoadWordsFile.Location = new Point(675, 138);
            this.btnLoadWordsFile.Name = "btnLoadWordsFile";
            this.btnLoadWordsFile.Size = new Size(208, 28);
            this.btnLoadWordsFile.Text = "Завантажити слова з файлу...";
            this.btnLoadWordsFile.Click += new System.EventHandler(this.BtnLoadWordsFile_Click);

            // btnStart
            this.btnStart.Location = new Point(15, 218);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new Size(195, 30);
            this.btnStart.Text = "Старт";
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);

            // btnPauseResume
            this.btnPauseResume.Enabled = false;
            this.btnPauseResume.Location = new Point(220, 218);
            this.btnPauseResume.Name = "btnPauseResume";
            this.btnPauseResume.Size = new Size(195, 30);
            this.btnPauseResume.Text = "Пауза";
            this.btnPauseResume.Click += new System.EventHandler(this.BtnPauseResume_Click);

            // btnStop
            this.btnStop.Enabled = false;
            this.btnStop.Location = new Point(425, 218);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new Size(195, 30);
            this.btnStop.Text = "Стоп";
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);

            // btnOpenReport
            this.btnOpenReport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnOpenReport.Enabled = false;
            this.btnOpenReport.Location = new Point(675, 218);
            this.btnOpenReport.Name = "btnOpenReport";
            this.btnOpenReport.Size = new Size(208, 30);
            this.btnOpenReport.Text = "Відкрити файл звіту";
            this.btnOpenReport.Click += new System.EventHandler(this.BtnOpenReport_Click);

            // progressBarTotal
            this.progressBarTotal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.progressBarTotal.Location = new Point(15, 258);
            this.progressBarTotal.Name = "progressBarTotal";
            this.progressBarTotal.Size = new Size(868, 22);

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.lblStatus.Location = new Point(15, 286);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(185, 17);
            this.lblStatus.Text = "Статус: Очікування запуску";

            // lblCurrentFile
            this.lblCurrentFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.lblCurrentFile.AutoEllipsis = true;
            this.lblCurrentFile.Location = new Point(245, 287);
            this.lblCurrentFile.Name = "lblCurrentFile";
            this.lblCurrentFile.Size = new Size(638, 17);

            // lblResults
            this.lblResults.AutoSize = true;
            this.lblResults.Location = new Point(15, 312);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new Size(245, 15);
            this.lblResults.Text = "Знайдені файли із забороненими словами:";

            // dgvResults
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.AllowUserToDeleteRows = false;
            this.dgvResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Columns.AddRange(new DataGridViewColumn[]
{
this.colFileName,
this.colPath,
this.colSize,
this.colReplacements
});
            this.dgvResults.Location = new Point(15, 332);
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.RowHeadersVisible = false;
            this.dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvResults.Size = new Size(575, 230);
            this.colFileName.FillWeight = 28F;
            this.colFileName.HeaderText = "Файл";
            this.colFileName.Name = "colFileName";
            this.colPath.FillWeight = 42F;
            this.colPath.HeaderText = "Шлях";
            this.colPath.Name = "colPath";
            this.colSize.FillWeight = 16F;
            this.colSize.HeaderText = "Розмір (Б)";
            this.colSize.Name = "colSize";
            this.colReplacements.FillWeight = 14F;
            this.colReplacements.HeaderText = "Замін";
            this.colReplacements.Name = "colReplacements";
            // lblTopWords
            this.lblTopWords.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.lblTopWords.AutoSize = true;
            this.lblTopWords.Location = new Point(602, 312);
            this.lblTopWords.Name = "lblTopWords";
            this.lblTopWords.Size = new Size(155, 15);
            this.lblTopWords.Text = "Топ-10 заборонених слів:";
            // lstTopWords
            this.lstTopWords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            this.lstTopWords.FormattingEnabled = true;
            this.lstTopWords.ItemHeight = 15;
            this.lstTopWords.Location = new Point(602, 332);
            this.lstTopWords.Name = "lstTopWords";
            this.lstTopWords.Size = new Size(281, 229);
            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 580);
            this.MinimumSize = new Size(820, 500);
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Сканер заборонених слів";
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.txtSourcePath);
            this.Controls.Add(this.btnBrowseFolder);
            this.Controls.Add(this.btnBrowseFile);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.lblWords);
            this.Controls.Add(this.txtForbiddenWords);
            this.Controls.Add(this.btnLoadWordsFile);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnPauseResume);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnOpenReport);
            this.Controls.Add(this.progressBarTotal);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblCurrentFile);
            this.Controls.Add(this.lblResults);
            this.Controls.Add(this.dgvResults);
            this.Controls.Add(this.lblTopWords);
            this.Controls.Add(this.lstTopWords);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}