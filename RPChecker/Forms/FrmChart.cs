using System;
using RPChecker.Util;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace RPChecker.Forms
{
    public partial class FrmChart : Form
    {
        private readonly ReSulT _info = new ReSulT();
        private readonly int _threshold;
        public FrmChart(ReSulT info, int threshold)
        {
            InitializeComponent();
            _info.FileName = info.FileName;
            _info.Data = info.Data;
            //_info.PropertyChanged += (sender, args) => DrawChart();
            _threshold = threshold;
        }

        private void FrmChart_Load(object sender, EventArgs e)
        {
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            Point saved = ConvertMethod.String2Point(RegistryStorage.Load(@"Software\RPChecker", "ChartLocation"));
            if (saved != new Point(-32000, -32000)) Location = saved;
            DrawChart();
        }

        private void DrawChart()
        {
            chart1.Series.Clear();
            Series series1 = new Series("PSNR")
            {
                Color = Color.Blue,
                ChartType = SeriesChartType.Line,
                IsValueShownAsLabel = false
            };

            Series series2 = new Series("frame")
            {
                Color = Color.Red,
                ChartType = SeriesChartType.Point,
                IsValueShownAsLabel = false
            };
            var task = new Task(() =>
            {
                _info.Data.Sort((a, b) => a.Key.CompareTo(b.Key));

                _info.Data.ForEach(frame =>
                {
                    series1.Points.AddXY(frame.Key, frame.Value);
                    if (frame.Value < _threshold)
                    {
                        series2.Points.AddXY(frame.Key, frame.Value);
                    }
                });
            });
            task.ContinueWith(t =>
            {
                Invoke(new Action(() => chart1.Series.Add(series1)));
                Invoke(new Action(() => chart1.Series.Add(series2)));
            });
            task.Start();
        }

        private void FrmChart_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                RegistryStorage.Save(Location.ToString(), @"Software\RPChecker", "ChartLocation");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void btnSaveAsImage_Click(object sender, EventArgs e)
        {
            var rnd = Path.GetRandomFileName().Substring(0, 8).ToUpper();
            var fileName = Path.GetDirectoryName(_info.FileName) + "\\" + rnd + ".png";
            chart1.SaveImage(fileName, ChartImageFormat.Png);
        }
    }
}
