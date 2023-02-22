using System;
using RPChecker.Util;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;

namespace RPChecker.Forms
{
    public partial class FrmChart : Form
    {
        private readonly ReSulT _info;
        private readonly double _threshold;
        private readonly double _fps;
        private readonly string _type;
        public FrmChart(ReSulT info, double threshold, double fps, string type)
        {
            InitializeComponent();
            _info = info;
            _threshold = threshold;
            _fps = fps;
            _type = type;
        }

        private void FrmChart_Load(object sender, EventArgs e)
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = $"{_type} Chart";
            var saved = ToolKits.String2Point(RegistryStorage.Load(@"Software\RPChecker", "ChartLocation"));
            if (saved != new Point(-32000, -32000)) Location = saved;
            DrawChart();
        }

        private void DrawChart()
        {
            chart1.Series.Clear();
            var series_y = new Series(_type)
            {
                Color = Color.FromArgb(35, 25, 85),
                ChartType = SeriesChartType.Line,
                IsValueShownAsLabel = false
            };
            var series_u = new Series("U Series")
            {
                Color = Color.FromArgb(31, 70, 144),
                ChartType = SeriesChartType.Line,
                IsValueShownAsLabel = false
            };
            var series_v = new Series("V Series")
            {
                Color = Color.FromArgb(232, 170, 66),
                ChartType = SeriesChartType.Line,
                IsValueShownAsLabel = false
            };

            var series2 = new Series("frame")
            {
                Color = Color.FromArgb(255, 010, 050),
                ChartType = SeriesChartType.Point,
                IsValueShownAsLabel = false
            };

            var interval = (int) Math.Round(_fps) * 30;
            var totalFrame = _info.Data.Count;
            var labelCount = totalFrame / interval;

            void AddLabel(int position, TimeSpan time) => chart1.ChartAreas[0].AxisX.CustomLabels
                .Add(position - 20, position + 20, $"{time:mm\\:ss}");
            for (var i = 1; i <= labelCount; i++)
            {
                AddLabel(i * interval, TimeSpan.FromSeconds(i * 30));
            }
            AddLabel(totalFrame, TimeSpan.FromSeconds(totalFrame / _fps));

            new Task(() =>
            {
                bool valid_u = false, valid_v = false;
                foreach(var (index, value_y, value_u, value_v) in _info.Data.OrderBy(item => item.index))
                {
                    series_y.Points.AddXY(index, value_y);
                    if (value_y < _threshold)
                    {
                        series2.Points.AddXY(index, value_y);
                    }

                    if (value_u > 1e-5)
                    {
                        valid_u = true;
                        series_u.Points.AddXY(index, value_u);
                        if (value_u < _threshold)
                        {
                            series2.Points.AddXY(index, value_u);
                        }
                    }
                    
                    if (value_v > 1e-5)
                    {
                        valid_v = true;
                        series_v.Points.AddXY(index, value_v);
                        if (value_v < _threshold)
                        {
                            series2.Points.AddXY(index, value_v);
                        }
                    }
                }
                Invoke(new Action(() =>
                {
                    chart1.Series.Add(series_y);
                    if (valid_u) chart1.Series.Add(series_u);
                    if (valid_v) chart1.Series.Add(series_v);
                    chart1.Series.Add(series2);
                }));
            }).Start();
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
            var fileName = Path.Combine(Path.GetDirectoryName(_info.FileNamePair.opt) ?? "", $"{Guid.NewGuid()}.png");
            chart1.SaveImage(fileName, ChartImageFormat.Png);
        }
    }
}
