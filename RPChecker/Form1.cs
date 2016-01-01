using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RPChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public  readonly List<KeyValuePair<string, string>> FilePathsPair = new List<KeyValuePair<string, string>>();
        private readonly List<ReSulT> _fullData = new List<ReSulT>();

        private int _threshold = 30;

        private void button1_Click(object sender, EventArgs e)
        {
            _fullData.Clear();
            comboBox1.Items.Clear();
            foreach (var item in FilePathsPair)
            {
                string vsFile = $"{item.Value}.vpy";
                try
                {
                    var result = new ReSulT
                    {
                        FileName = item.Value,
                        Data = ConvertMethod.AnalyseFile(item.Key, item.Value, vsFile, comboBox3.SelectedItem.ToString())
                    };
                    _fullData.Add(result);
                    if (checkBox1.Checked) continue;
                    File.Delete(vsFile);
                    File.Delete($"{item.Key}.lwi");
                    File.Delete($"{item.Value}.lwi");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message ,@"PRChecker ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            _fullData.ForEach(item => comboBox1.Items.Add(Path.GetFileName(item.FileName)?? ""));
            if (comboBox1.Items.Count <= 0) return;
            comboBox1.SelectedIndex = 0;
            UpdataGridView(_fullData[comboBox1.SelectedIndex], _frameRate[comboBox2.SelectedIndex]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FrmLoadFiles flf = new FrmLoadFiles(this);
            flf.Show();
        }

        private readonly double[] _frameRate = { 24000 / 1001.0, 24000 / 1000.0,
                                                 25000 / 1000.0, 30000 / 1001.0,
                                                 50000 / 1000.0, 60000 / 1001.0 };
        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0 || comboBox1.SelectedIndex > _fullData.Count) return;
            UpdataGridView(_fullData[comboBox1.SelectedIndex], _frameRate[comboBox2.SelectedIndex]);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //for (int index = 0; index <= 450; index = dataGridView1.Rows.Add());
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            DirectoryInfo current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            current.GetFiles()
                .Where(item => item.Extension.ToLowerInvariant().EndsWith("vpy")).ToList()
                .ForEach(item => comboBox3.Items.Add(item));
        }


        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                UpdataGridView(_fullData[comboBox1.SelectedIndex], _frameRate[comboBox2.SelectedIndex], false, true);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _threshold = Convert.ToInt32(numericUpDown1.Value);
            if (_fullData == null) return;
            UpdataGridView(_fullData[comboBox1.SelectedIndex], _frameRate[comboBox2.SelectedIndex]);
        }

        private void UpdataGridView(ReSulT info, double frameRate, bool clear = true, bool updataTime = false)
        {
            if (clear) { dataGridView1.Rows.Clear(); }
            int index = 0;
            foreach (var item in info.Data)
            {
                if ((dataGridView1.RowCount < 450 || item.Value < _threshold || updataTime) && index < dataGridView1.RowCount)
                {
                    if (clear) { index = dataGridView1.Rows.Add(); }
                    TimeSpan temp = ConvertMethod.Second2Time(item.Key / frameRate);
                    dataGridView1.Rows[index].Cells[0].Value = item.Key;
                    dataGridView1.Rows[index].Cells[1].Value = $"{item.Value:F4}";
                    dataGridView1.Rows[index].Cells[2].Value = ConvertMethod.Time2String(temp);
                    dataGridView1.Rows[index].DefaultCellStyle.BackColor = item.Value < _threshold ? Color.FromArgb(233, 76, 60) : Color.FromArgb(46, 205, 112);
                    if (!clear) { ++index; }
                }
                if (item.Value > _threshold && dataGridView1.RowCount >= 450 && !updataTime) { break; }
            }
        }

        private void comboBox1_MouseEnter(object sender, EventArgs e) => toolTip1.Show(comboBox1.SelectedItem?.ToString(), comboBox1);

        private void comboBox1_MouseLeave(object sender, EventArgs e) => toolTip1.RemoveAll();
    }

    public class ReSulT
    {
        public List<KeyValuePair<int, double>> Data { get; set; }
        public string FileName { get; set; }
    }
}
