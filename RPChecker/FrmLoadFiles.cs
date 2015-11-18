using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace RPChecker
{
    public partial class FrmLoadFiles : Form
    {
        readonly Form1 _mainWindow;
        public FrmLoadFiles(Form1 arg)
        {
            _mainWindow = arg;
            InitializeComponent();
        }

        readonly Regex _rpath = new Regex(@".+\\(?<fileName>.+(?:\\.*)?)");

        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            listView1.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        private void listView1_DragOver(object sender, DragEventArgs e)
        {
            Point ptScreen = new Point(e.X, e.Y);
            Point pt = listView1.PointToClient(ptScreen);
            ListViewItem item = listView1.GetItemAt(pt.X, pt.Y);
            if (item != null)
                item.Selected = true;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            ListViewItem draggedItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            Point ptScreen = new Point(e.X, e.Y);
            Point pt = listView1.PointToClient(ptScreen);
            ListViewItem targetItem = listView1.GetItemAt(pt.X, pt.Y);//拖动的项将放置于该项之前
            if (targetItem == null) return;
            listView1.Items.Insert(targetItem.Index, (ListViewItem)draggedItem.Clone());
            listView1.Items.Remove(draggedItem);
        }



        private void listView2_ItemDrag(object sender, ItemDragEventArgs e)
        {
            listView2.DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void listView2_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        private void listView2_DragOver(object sender, DragEventArgs e)
        {
            Point ptScreen = new Point(e.X, e.Y);
            Point pt = listView2.PointToClient(ptScreen);
            ListViewItem item = listView2.GetItemAt(pt.X, pt.Y);
            if (item != null)
                item.Selected = true;
        }
        private void listView2_DragDrop(object sender, DragEventArgs e)
        {

            ListViewItem draggedItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            Point ptScreen = new Point(e.X, e.Y);
            Point pt = listView2.PointToClient(ptScreen);
            ListViewItem targetItem = listView2.GetItemAt(pt.X, pt.Y);//拖动的项将放置于该项之前
            if (targetItem == null) return;
            listView2.Items.Insert(targetItem.Index, (ListViewItem)draggedItem.Clone());
            listView2.Items.Remove(draggedItem);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            foreach (var item in openFileDialog1.FileNames)
            {
                listView1.Items.Add(_rpath.Match(item).Groups["fileName"].Value).Tag = item;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            foreach (var item in openFileDialog1.FileNames)
            {
                listView2.Items.Add(_rpath.Match(item).Groups["fileName"].Value).Tag = item;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _mainWindow.FilePathsPair.Clear();
            if (listView1.Items.Count == listView2.Items.Count)
            {
                int i = 0;
                foreach (ListViewItem item in listView1.Items)
                {
                    _mainWindow.FilePathsPair.Add(new KeyValuePair<string, string>(item.Tag as string, listView2.Items[i].Tag as string));
                    ++i;
                }
                Close();
                return;
            }
            MessageBox.Show(@"个数都不对应，确定个屁啊");
        }

        private void button4_MouseUp(object sender, MouseEventArgs e)
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

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            foreach (var item in (string[]) e.Data.GetData(DataFormats.FileDrop))
            {
                listView1.Items.Add(_rpath.Match(item).Groups["fileName"].Value).Tag = item;
            }
        }
        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            foreach (var item in (string[]) e.Data.GetData(DataFormats.FileDrop))
            {
                listView2.Items.Add(_rpath.Match(item).Groups["fileName"].Value).Tag = item;
            }
        }
        private void textBox2_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.Items.Count != listView2.Items.Count) return;
            foreach (var item in listView1.Items)
            {
                ((ListViewItem) item).ForeColor = Color.Black;
            }
            foreach (var item in listView2.Items)
            {
                ((ListViewItem) item).ForeColor = Color.Black;
            }
            listView2.Items[((ListView) sender).SelectedItems[0].Index].ForeColor = Color.Red;
        }

        private void listView2_ItemActivate(object sender, EventArgs e)
        {
            if (listView1.Items.Count != listView2.Items.Count) return;
            foreach (var item in listView1.Items)
            {
                ((ListViewItem) item).ForeColor = Color.Black;
            }
            foreach (var item in listView2.Items)
            {
                ((ListViewItem) item).ForeColor = Color.Black;
            }
            listView1.Items[((ListView) sender).SelectedItems[0].Index].ForeColor = Color.Red;
        }


    }
}
