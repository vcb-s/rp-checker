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
        private readonly int _threshold;
        private readonly double _fps;
        private readonly string _type;
        public FrmChart(ReSulT info, int threshold, double fps, string type)
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
            var series1 = new Series(_type)
            {
                Color = Color.FromArgb(078, 079, 251),
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
            var task = new Task(() =>
            {
                foreach(var frame in _info.Data.OrderBy(item => item.index))
                {
                    series1.Points.AddXY(frame.index, frame.value);
                    if ((frame.index + 1) % interval == 0)
                    {
                        Invoke(new Action(() => chart1.ChartAreas[0].AxisX.CustomLabels.Add(frame.index - 20, frame.index + 20,
                            $"{TimeSpan.FromSeconds(Math.Round(frame.index / _fps)):mm\\:ss}")));
                    }
                    if (frame.value < _threshold)
                    {
                        series2.Points.AddXY(frame.index, frame.value);
                    }
                }
            });
            task.ContinueWith(t =>
            {
                Invoke(new Action(() =>
                {
                    chart1.Series.Add(series1);
                    chart1.Series.Add(series2);
                }));
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
            var fileName = Path.Combine(Path.GetDirectoryName(_info.FileNamePair.opt) ?? "", $"{Guid.NewGuid()}.png");
            chart1.SaveImage(fileName, ChartImageFormat.Png);
        }
    }
}
