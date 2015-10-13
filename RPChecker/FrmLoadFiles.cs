using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace RPChecker
{
    public partial class FrmLoadFiles : Form
    {
        Form1 mainWindow;
        public FrmLoadFiles(Form1 arg)
        {
            mainWindow = arg;
            InitializeComponent();
        }

        Regex Rpath = new Regex(@".+\\(?<fileName>.+\\.*)");


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
            ListViewItem TargetItem = listView1.GetItemAt(pt.X, pt.Y);//拖动的项将放置于该项之前  
            if (TargetItem!= null)
            {
                listView1.Items.Insert(TargetItem.Index, (ListViewItem)draggedItem.Clone());
                listView1.Items.Remove(draggedItem);
            }  
        }


        /////////////////////

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
            ListViewItem TargetItem = listView2.GetItemAt(pt.X, pt.Y);//拖动的项将放置于该项之前  
            if (TargetItem != null)
            {
                listView2.Items.Insert(TargetItem.Index, (ListViewItem)draggedItem.Clone());
                listView2.Items.Remove(draggedItem);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (var item in openFileDialog1.FileNames)
                {
                    listView1.Items.Add(Rpath.Match(item).Groups["fileName"].Value).Tag = item;
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (var item in openFileDialog1.FileNames)
                {
                    listView2.Items.Add(Rpath.Match(item).Groups["fileName"].Value).Tag = item;
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            mainWindow.FilePathsPair.Clear();
            if (listView1.Items.Count == listView2.Items.Count)
            {
                int i = 0;
                foreach (ListViewItem item in listView1.Items)
                {
                    mainWindow.FilePathsPair.Add(new KeyValuePair<string, string>(item.Tag as string, listView2.Items[i].Tag as string));
                    ++i;
                }
            }
            else
            {
                MessageBox.Show("个数都不对应，确定个屁啊");
            }
            
            this.Close();
        }
        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)    
            {
                listView1.Items.Clear();
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    listView2.Items.Clear();
                }
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            foreach (var item in e.Data.GetData(DataFormats.FileDrop) as string[])
            {
                listView1.Items.Add(Rpath.Match(item).Groups["fileName"].Value).Tag = item; 
            }
        }
        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effect = DragDropEffects.Copy; }
            else { e.Effect = DragDropEffects.None; }
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            foreach (var item in e.Data.GetData(DataFormats.FileDrop) as string[])
            {
                listView2.Items.Add(Rpath.Match(item).Groups["fileName"].Value).Tag = item;
            }
        }
        private void textBox2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effect = DragDropEffects.Copy; }
            else { e.Effect = DragDropEffects.None; }
        }
    }
}
