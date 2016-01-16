using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;

using System.Windows.Forms.DataVisualization.Charting;

namespace RPChecker
{
    public partial class FrmChart : Form
    {
        private readonly ReSulT _info = new ReSulT();
        private readonly int _threshold;
        public FrmChart(ReSulT info, int threshold)
        {
            InitializeComponent();
            _info.FileName = info.FileName;
            _info.Data = new List<KeyValuePair<int, double>>(info.Data.ToArray());
            _threshold = threshold;
        }

        private void FrmChart_Load(object sender, EventArgs e)
        {
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            Point saved = ConvertMethod.String2Point(RegistryStorage.Load(@"Software\RPChecker", "ChartLocation"));
            if (saved != new Point(-32000, -32000)) Location = saved;
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

            _info.Data.Sort((a,b) => a.Key.CompareTo(b.Key));

            _info.Data.ForEach(frame =>
            {
                series1.Points.AddXY(frame.Key, frame.Value);
                if (frame.Value < _threshold)
                {
                    series2.Points.AddXY(frame.Key, frame.Value);
                }
            });
            chart1.Series.Add(series1);
            chart1.Series.Add(series2);
        }

        private void FrmChart_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryStorage.Save(Location.ToString(), @"Software\RPChecker", "ChartLocation");
        }
    }
}
