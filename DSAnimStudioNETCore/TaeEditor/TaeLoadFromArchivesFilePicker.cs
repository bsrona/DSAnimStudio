using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public partial class TaeLoadFromArchivesFilePicker : Form
    {
        public TaeLoadFromArchivesFilePicker()
        {
            InitializeComponent();
        }

        public string SelectedEblFile
        {
            get => listBoxFiles.SelectedItem as string;
        }

        public List<string> SelectedEblFiles()
        {
            List<string> strings = new List<string>();
            foreach(string str in listBoxFiles.SelectedItems)
            {
                strings.Add(str);
            }
            return strings;
        }

        public void InitEblFileList(List<string> eblFiles, string defaultOptionStartMatch, string exactMatchDefault, SelectionMode selectionMode = SelectionMode.One)
        {
            listBoxFiles.Items.Clear();
            listBoxFiles.SelectionMode = selectionMode;
            //listBoxFiles.Items.AddRange(eblFiles.Cast<object>().ToArray());
            bool foundDefaultOption = false;
            for (int i = 0; i < eblFiles.Count; i++)
            {
                if (!listBoxFiles.Items.Contains(eblFiles[i]))
                {
                    listBoxFiles.Items.Add(eblFiles[i]);
                    if (defaultOptionStartMatch != null && !foundDefaultOption && eblFiles[i].ToLowerInvariant().StartsWith(defaultOptionStartMatch.ToLowerInvariant()))
                    {
                        listBoxFiles.SelectedIndex = listBoxFiles.Items.Count - 1;
                        foundDefaultOption = true;
                    }

                    if (exactMatchDefault != null && eblFiles[i].ToLowerInvariant() == exactMatchDefault.ToLowerInvariant())
                    {
                        listBoxFiles.SelectedIndex = listBoxFiles.Items.Count - 1;
                        foundDefaultOption = true;
                    }
                }
            }
        }

        private void TaeLoadFromArchivesFilePicker_Load(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listBoxFiles.SelectedIndex >= 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
