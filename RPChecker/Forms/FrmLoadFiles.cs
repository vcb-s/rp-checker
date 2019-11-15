using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RPChecker.Util;
using System.Drawing;
using System.Windows.Forms;


namespace RPChecker.Forms
{
    public partial class FrmLoadFiles : Form
    {
        private readonly Form1 _mainWindow;
        public FrmLoadFiles(Form1 arg)
        {
            _mainWindow = arg;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            InitializeComponent();
            if (NativeMethods.IsUserAnAdmin())
            {
                textBox1.Text = textBox2.Text = "管理员模式下并不支持拖拽";
                textBox1.Enabled = textBox2.Enabled = false;
            }
        }

        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var listView = sender as ListView;
            listView?.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void listView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void listView_DragOver(object sender, DragEventArgs e)
        {
            if (!(sender is ListView listView)) return;
            var ptScreen = new Point(e.X, e.Y);
            var pt       = listView.PointToClient(ptScreen);
            var item     = listView.GetItemAt(pt.X, pt.Y);
            if (item != null)
                item.Selected = true;
        }

        private void listView_DragDrop(object sender, DragEventArgs e)
        {
            if (!(sender is ListView listView)) return;
            var draggedItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            var ptScreen    = new Point(e.X, e.Y);
            var pt          = listView.PointToClient(ptScreen);
            var targetItem  = listView.GetItemAt(pt.X, pt.Y);//拖动的项将放置于该项之前
            if (targetItem == null) return;
            listView.Items.Insert(targetItem.Index, (ListViewItem)draggedItem.Clone());
            listView.Items.Remove(draggedItem);
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.Items.Count != listView2.Items.Count) return;
            foreach (ListViewItem item in listView1.Items) item.ForeColor = Color.Black;
            foreach (ListViewItem item in listView2.Items) item.ForeColor = Color.Black;
            listView2.Items[listView1.SelectedItems[0].Index].ForeColor = Color.Red;
        }

        private void listView2_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.Items.Count != listView2.Items.Count) return;
            foreach (ListViewItem item in listView1.Items) item.ForeColor = Color.Black;
            foreach (ListViewItem item in listView2.Items) item.ForeColor = Color.Black;
            listView1.Items[listView2.SelectedItems[0].Index].ForeColor = Color.Red;
        }


        private void btnLoad1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            foreach (var item in openFileDialog1.FileNames)
            {
                listView1.Items.Add(Path.GetFileName(item)).Tag = item;
            }
        }

        private void btnLoad2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            foreach (var item in openFileDialog1.FileNames)
            {
                listView2.Items.Add(Path.GetFileName(item)).Tag = item;
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == listView2.Items.Count)
            {
                _mainWindow.FilePathsPair.Clear();
                _mainWindow.FilePathsPair.AddRange(listView1.Items.Cast<ListViewItem>()
                    .Zip(listView2.Items.Cast<ListViewItem>(),
                        (item1, item2) => (item1.Tag as string, item2.Tag as string)));
                Close();
                return;
            }
            MessageBox.Show(@"个数都不对应，确定个屁啊", @"RPChecker Warning");
        }

        private void btnClear_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    listView1.Items.Clear();
                    break;
                case MouseButtons.Right:
                    listView2.Items.Clear();
                    break;
            }
        }

        private void textBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            var list = (string[]) e.Data.GetData(DataFormats.FileDrop);
            Array.Sort(list);
            foreach (var item in list)
            {
                listView1.Items.Add(Path.GetFileName(item)).Tag = item;
            }
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            var list = (string[])e.Data.GetData(DataFormats.FileDrop);
            Array.Sort(list);
            foreach (var item in list)
            {
                listView2.Items.Add(Path.GetFileName(item)).Tag = item;
            }
        }

        private void FrmLoadFiles_Resize(object sender, EventArgs e)
        {
            var midLine         = (textBox1.Location.X - textBox1.Margin.Left - listView1.Margin.Left) / 2;
            listView2.Width     = listView1.Width = midLine - 12;
            listView2.Location  = new Point(btnLoad2.Location.X  +  btnLoad2.Width - listView2.Width,    listView2.Location.Y);
            btnLoad1.Location   = new Point(listView1.Location.X + listView1.Width -  btnLoad1.Width + 1, btnLoad1.Location.Y);
            label2.Location     = new Point(listView2.Location.X, label2.Location.Y);
            columnHeader1.Width = listView1.Width - 4;
            columnHeader2.Width = listView1.Width - 4;
        }

        private void FrmLoadFiles_Load(object sender, EventArgs e)
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            var saved = ToolKits.String2Point(RegistryStorage.Load(@"Software\RPChecker", "LoadLocation"));
            if (saved != new Point(-32000, -32000)) Location = saved;
            this.NormalizePosition();
        }

        private void FrmLoadFiles_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryStorage.Save(Location.ToString(), @"Software\RPChecker", "LoadLocation");
        }
    }
}
