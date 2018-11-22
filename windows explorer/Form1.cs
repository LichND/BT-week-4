using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace windows_explorer
{
    public partial class Form1 : Form
    {
        #region My private var
        private enum typePaste
        {
            COPY , CUT , NONE
        }
        private Point delta;
        private List<string> srcFile;
        private typePaste Paste;
        #endregion
        
        public Form1()
        {
            delta.X = 10;
            delta.Y = 81;
            InitializeComponent();
        }

        #region My private function
        private void loadSubFolder(TreeNode root)
        {
            try
            {
                // lay duong dan
                string link = root.FullPath.Remove(0, 12)+"\\";
                DirectoryInfo[] folderList = new DirectoryInfo(link).GetDirectories();
                // xoa ket qua truoc
                if (root != null)
                    root.Nodes.Clear();
                foreach (var folder in folderList)
                {
                    // Nhet node vao Tree
                    TreeNode folderTree = new TreeNode(folder.Name,0,0);
                    if (folder.Name[0] == '.' || folder.Name[0] == '$') 
                        continue;
                    root.Nodes.Add(folderTree);
                }
            }
            catch { };
        }
        private void loadFolderAndFile(TreeNode root)
        {
            try
            {
                if (listView1 != null)
                    listView1.Items.Clear();
                string link = root.FullPath.Remove(0, 12)+"\\";
                DirectoryInfo[] folderList = new DirectoryInfo(link).GetDirectories();
                FileInfo[] fileList = new DirectoryInfo(link).GetFiles();

                toolStripStatusLabel1.Text = "Opening...";
                foreach (var folder in folderList)
                {
                    if (folder.Name[0] == '.' || folder.Name[0] == '$')
                        continue;
                    string[] value = new string[5];
                    value[0] = folder.Name.ToString();
                    value[1] = "Folder";
                    value[2] = "";
                    value[3] = folder.LastAccessTime.ToString();
                    ListViewItem item = new ListViewItem(value, 0);
                    listView1.Items.Add(item);
                }

                foreach (var file in fileList)
                {
                    if (file.Name[0] == '.' || file.Name[0] == '$')
                        continue;
                    string[] value = new string[5];
                    value[0] = file.Name.ToString();
                    if (value[0].Contains('.'))
                        {
                            string[] type = value[0].Split('.');
                            value[1] = type[type.Length - 1] + " file";
                        }
                    else
                        value[1] = "file";
                    if (file.Length > 1024)
                        value[2] = (file.Length >> 8).ToString() + " KB";
                    else
                        value[2] = file.Length.ToString() + " byte";
                    value[3] = file.LastAccessTime.ToString();
                    ListViewItem item = new ListViewItem(value, 3);
                    listView1.Items.Add(item);
                }

                toolStripStatusLabel1.Text = (folderList.Length+fileList.Length).ToString()+" item(s)";
            }
            catch
            {
                if (treeView1.SelectedNode.Text != "My computer")
                    return;
                if (listView1 != null)
                    listView1.Items.Clear();
                foreach(TreeNode disk in treeView1.SelectedNode.Nodes)
                {
                    string[] value = new string[5];
                    value[0] = disk.Text;
                    value[1] = "Disk";
                    value[2] = "N/A";
                    value[3] = "N/A";
                    ListViewItem item = new ListViewItem(value, 2);
                    listView1.Items.Add(item);
                }
            };
        }
        private static void AfterPaste(string src, string des, typePaste e, string sub = "")
        {
            // create selected folder
            string parent = Directory.GetParent(src).FullName;
            string name = src.Substring(parent.Length);
            if (!File.Exists(src))
            {
                if (!Directory.Exists(des + sub + name))
                    Directory.CreateDirectory(des + sub + name);
            }
            else
            {
                if (e == typePaste.COPY)
                    File.Copy(src, des + src.Substring(parent.Length));
                else
                    File.Move(src, des + src.Substring(parent.Length));
                return;
            }
            // copy all file and folder inside this folder
            string[] ListFolder = Directory.GetDirectories(src);
            string[] ListFile = Directory.GetFiles(src);
            if (ListFolder.Length + ListFile.Length != 0)
            {
                for (int i = 0; i < ListFolder.Length; i++)
                    AfterPaste(ListFolder[i], des, e, sub + name);


                for (int i = 0; i < ListFile.Length; i++)
                    File.Copy(ListFile[i], des + sub + name + ListFile[i].Substring(Directory.GetParent(ListFile[i]).FullName.Length));
            }
            if (e == typePaste.CUT)
                Directory.Delete(src, true);
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            // Clear
            if (treeView1 != null)
                treeView1.Nodes.Clear();
            // Node goc
            TreeNode mycoputerNode = new TreeNode("My computer", 1, 1);
            treeView1.Nodes.Add(mycoputerNode);
            // Lay node con
            ManagementObjectSearcher query = new ManagementObjectSearcher("select * from Win32_LogicalDisk");
            ManagementObjectCollection disks = query.Get();

            if (listView1 != null)
                listView1.Items.Clear();

            foreach (var disk in disks)
            {
                TreeNode nodeDisk = new TreeNode(disk.GetPropertyValue("Name").ToString(),2,2);
                treeView1.Nodes[0].Nodes.Add(nodeDisk);
                loadSubFolder(nodeDisk);

                string[] value = new string[5];
                value[0] = nodeDisk.Text;
                value[1] = "Disk";
                value[2] = "N/A";
                value[3] = "N/A";

                ListViewItem item = new ListViewItem(value, 2);
                listView1.Items.Add(item);
            }
            mycoputerNode.Expand();
            toolStripStatusLabel1.Text = disks.Count.ToString() + " item(s)";
            toolStripStatusLabel2.Text = "";
            toolStripStatusLabel3.Text = "";
            srcFile = new List<string>();
            Paste = typePaste.NONE;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selectedNode = e.Node;

            foreach (TreeNode folder in selectedNode.Nodes)
                loadSubFolder(folder);
            loadFolderAndFile(selectedNode);
            selectedNode.Expand();

            try
            { FullLinkPath.Text = treeView1.SelectedNode.FullPath.Remove(0, 12); }
            catch { };
        }

        #region Event button
        private void ButtonUp_Click(object sender, EventArgs e)
        {
            TreeNode now = treeView1.SelectedNode;
            try
            {
                now.Collapse();
                now = now.Parent;
                loadFolderAndFile(now);
                treeView1.SelectedNode = now;
            }
            catch
            {
                MessageBox.Show("Access Denied");
            };
            toolStripStatusLabel1.Text = now.Nodes.Count.ToString() + " item(s)";
        }
        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            TreeNode tmp = treeView1.SelectedNode;
            loadFolderAndFile(tmp);
            loadSubFolder(tmp);
            foreach (TreeNode sub in tmp.Nodes)
                loadSubFolder(sub);
        }
        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(sender, e);
        }
        private void ButtonCut_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem_Click(sender, e);
        }
        private void ButtonPaste_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(sender, e);
        }
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            deleteToolStripMenuItem_Click(sender, e);
        }
        private void ButtonRename_Click(object sender, EventArgs e)
        {
            renameToolStripMenuItem_Click(sender, e);
        }
        #endregion

        #region Event menu
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            else
                if (listView1.SelectedItems.Count>1)
                {
                    MessageBox.Show("Sorry this function isn't support", "Sorry");
                    return;
                }
            string link;
            try
            {
                link = treeView1.SelectedNode.FullPath.Remove(0, 12) + '\\' + listView1.SelectedItems[0].Text;
            }
            catch
            {
                MessageBox.Show("Can't rename disk");
                return;
            }
            
            Rename fr = new Rename();

            string name = link.Substring(Directory.GetParent(link).FullName.Length + 1);
            if (File.Exists(link))
            {
                string[] tmp = name.Split('.');
                string type = '.' + tmp[tmp.Length - 1];
                fr.NewName = name.Remove(name.Length - type.Length - 1, type.Length);

                fr.ShowDialog();
                if (!fr.Change || (fr.NewName == "") || (fr.NewName + type == name)) 
                    return;
                FileSystem.RenameFile(link, fr.NewName + type);
            }
            else
            {
                fr.NewName = name;
                fr.ShowDialog();
                if (!fr.Change || (fr.NewName == "") || (fr.NewName == name)) 
                    return;
                FileSystem.RenameDirectory(link, fr.NewName);
            }
            ButtonRefresh_Click(sender, e);
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete this file?", "Delete file", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
                return;
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                string link = (treeView1.SelectedNode.FullPath + "\\" + item.Text).Remove(0, 12);
                if (File.Exists(link))
                    File.Delete(link);
                if (Directory.Exists(link))
                    Directory.Delete(link, true);
            }
            ButtonRefresh_Click(sender, e);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // menu Edit
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (srcFile != null)
                srcFile.Clear();
            string link = treeView1.SelectedNode.FullPath.Remove(0, 12);
            foreach (ListViewItem item in listView1.SelectedItems)
                srcFile.Add(link + "\\" + item.Text);
            Paste = typePaste.COPY;
            toolStripStatusLabel3.Text = "Copied!";
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (srcFile != null)
                srcFile.Clear();
            string link = treeView1.SelectedNode.FullPath.Remove(0, 12);
            foreach (ListViewItem item in listView1.SelectedItems)
                srcFile.Add(link + "\\" + item.Text);
            Paste = typePaste.CUT;
            toolStripStatusLabel3.Text = "Cut!";
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Paste==typePaste.NONE)
                MessageBox.Show("Nothing to paste", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                foreach(string src in srcFile)
                    AfterPaste(src, treeView1.SelectedNode.FullPath.Remove(0, 12), Paste);
                Paste = typePaste.NONE;
                toolStripStatusLabel3.Text = "";
                ButtonRefresh_Click(sender, e);
            }
        }
        

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) contextMenuStrip1.Show(e.Location.X + Location.X + splitContainer1.SplitterDistance + splitContainer1.SplitterRectangle.Width + delta.X, e.Location.Y + Location.Y + delta.Y);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
                toolStripStatusLabel2.Text = " 1 item selected";
            else
                if (listView1.SelectedItems.Count > 1)
                toolStripStatusLabel2.Text = listView1.SelectedItems.Count + " items selected";
            else
                toolStripStatusLabel2.Text = "";
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = "";
            foreach (TreeNode node in treeView1.SelectedNode.Nodes)
                if (node.Text == listView1.SelectedItems[0].Text)
                {
                    loadSubFolder(node);
                    loadFolderAndFile(node);
                    node.Expand();
                    treeView1.SelectedNode = node;

                    try
                    { FullLinkPath.Text = treeView1.SelectedNode.FullPath.Remove(0, 12); }
                    catch { };

                    if (!node.IsExpanded)
                        return;

                    return;
                }
            MessageBox.Show("Sorry I can't open this file now", "Sory");
        }
        #endregion
        // right mouse
        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(sender, e);
        }
        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem_Click(sender, e);
        }
        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(sender, e);
        }
        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            deleteToolStripMenuItem_Click(sender, e);
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1_DoubleClick(sender, e);
        }
        private void renameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            renameToolStripMenuItem_Click(sender, e);
        }
    }
}
