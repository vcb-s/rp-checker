using System.ComponentModel;
using System.Windows.Forms;

namespace RPChecker.Forms
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.frams = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PSNR = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnLoad = new System.Windows.Forms.Button();
            this.cbFileList = new System.Windows.Forms.ComboBox();
            this.cbFPS = new System.Windows.Forms.ComboBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.cbVpyFile = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnLog = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.btnChart = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusStdError = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnalyze.Location = new System.Drawing.Point(408, 391);
            this.btnAnalyze.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(34, 63);
            this.btnAnalyze.TabIndex = 1;
            this.btnAnalyze.Text = "分析";
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.frams,
            this.PSNR,
            this.Time});
            this.dataGridView1.GridColor = System.Drawing.SystemColors.Highlight;
            this.dataGridView1.Location = new System.Drawing.Point(12, 13);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(388, 440);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.TabStop = false;
            // 
            // frams
            // 
            this.frams.HeaderText = "帧数";
            this.frams.Name = "frams";
            this.frams.ReadOnly = true;
            // 
            // PSNR
            // 
            this.PSNR.HeaderText = "PSNR";
            this.PSNR.Name = "PSNR";
            this.PSNR.ReadOnly = true;
            // 
            // Time
            // 
            this.Time.HeaderText = "时间";
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.Location = new System.Drawing.Point(408, 349);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(152, 35);
            this.btnLoad.TabIndex = 0;
            this.btnLoad.Text = "载入";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // cbFileList
            // 
            this.cbFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFileList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFileList.FormattingEnabled = true;
            this.cbFileList.Location = new System.Drawing.Point(409, 13);
            this.cbFileList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbFileList.Name = "cbFileList";
            this.cbFileList.Size = new System.Drawing.Size(150, 25);
            this.cbFileList.TabIndex = 4;
            this.cbFileList.SelectionChangeCommitted += new System.EventHandler(this.cbFileList_SelectionChangeCommitted);
            this.cbFileList.MouseEnter += new System.EventHandler(this.cbFileList_MouseEnter);
            this.cbFileList.MouseLeave += new System.EventHandler(this.cbFileList_MouseLeave);
            // 
            // cbFPS
            // 
            this.cbFPS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFPS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFPS.FormattingEnabled = true;
            this.cbFPS.Items.AddRange(new object[] {
            "24000 / 1001",
            "24000 / 1000",
            "25000 / 1000",
            "30000 / 1001",
            "50000 / 1000",
            "60000 / 1001"});
            this.cbFPS.Location = new System.Drawing.Point(409, 46);
            this.cbFPS.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbFPS.Name = "cbFPS";
            this.cbFPS.Size = new System.Drawing.Size(150, 25);
            this.cbFPS.TabIndex = 5;
            this.cbFPS.SelectedIndexChanged += new System.EventHandler(this.cbFPS_SelectedIndexChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDown1.Location = new System.Drawing.Point(506, 79);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(53, 23);
            this.numericUpDown1.TabIndex = 6;
            this.numericUpDown1.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(409, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "峰值信噪比阈值";
            // 
            // cbVpyFile
            // 
            this.cbVpyFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbVpyFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbVpyFile.FormattingEnabled = true;
            this.cbVpyFile.Items.AddRange(new object[] {
            "Default"});
            this.cbVpyFile.Location = new System.Drawing.Point(409, 110);
            this.cbVpyFile.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbVpyFile.Name = "cbVpyFile";
            this.cbVpyFile.Size = new System.Drawing.Size(150, 25);
            this.cbVpyFile.TabIndex = 7;
            // 
            // btnLog
            // 
            this.btnLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLog.Enabled = false;
            this.btnLog.Location = new System.Drawing.Point(490, 424);
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(70, 30);
            this.btnLog.TabIndex = 10;
            this.btnLog.Text = "LOG";
            this.btnLog.UseVisualStyleBackColor = true;
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbort.Enabled = false;
            this.btnAbort.Location = new System.Drawing.Point(449, 391);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(34, 63);
            this.btnAbort.TabIndex = 11;
            this.btnAbort.Text = "中断";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // btnChart
            // 
            this.btnChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChart.Enabled = false;
            this.btnChart.Location = new System.Drawing.Point(490, 391);
            this.btnChart.Name = "btnChart";
            this.btnChart.Size = new System.Drawing.Size(70, 30);
            this.btnChart.TabIndex = 12;
            this.btnChart.Text = "图表";
            this.btnChart.UseVisualStyleBackColor = true;
            this.btnChart.Click += new System.EventHandler(this.btnChart_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Consolas", 9F);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusStdError,
            this.toolStripProgressBar1,
            this.toolStripSplitButton1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 463);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(569, 22);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusStdError
            // 
            this.toolStripStatusStdError.Name = "toolStripStatusStdError";
            this.toolStripStatusStdError.Size = new System.Drawing.Size(420, 17);
            this.toolStripStatusStdError.Spring = true;
            this.toolStripStatusStdError.Text = "   ";
            this.toolStripStatusStdError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar1.Step = 1;
            this.toolStripProgressBar1.Click += new System.EventHandler(this.toolStripProgressBar1_Click);
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(32, 20);
            this.toolStripSplitButton1.Text = "toolStripSplitButton1";
            this.toolStripSplitButton1.ButtonClick += new System.EventHandler(this.toolStripSplitButton1_ButtonClick);
            this.toolStripSplitButton1.MouseEnter += new System.EventHandler(this.toolStripSplitButton1_MouseEnter);
            this.toolStripSplitButton1.MouseLeave += new System.EventHandler(this.toolStripSplitButton1_MouseLeave);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(569, 485);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnChart);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnLog);
            this.Controls.Add(this.cbVpyFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.cbFPS);
            this.Controls.Add(this.cbFileList);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnAnalyze);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(585, 524);
            this.Name = "Form1";
            this.Text = "RP Checker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnAnalyze;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn frams;
        private DataGridViewTextBoxColumn PSNR;
        private DataGridViewTextBoxColumn Time;
        private Button btnLoad;
        private ComboBox cbFileList;
        private ComboBox cbFPS;
        private NumericUpDown numericUpDown1;
        private Label label1;
        private ComboBox cbVpyFile;
        private ToolTip toolTip1;
        private Button btnLog;
        private Button btnAbort;
        private Button btnChart;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusStdError;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripSplitButton toolStripSplitButton1;
    }
}

