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
    public partial class TaeExportUnrealEngineForm : Form
    {
        public TaeEditorScreen MainScreen;

        private NewAnimationContainer curAnimContainer = null;
        //private List<string> curAnimationNameList = null;

        private List<ToolExportUnrealEngine.ExportFileType> fileTypeSelEntriesMapping = new List<ToolExportUnrealEngine.ExportFileType>
        {
            ToolExportUnrealEngine.ExportFileType.All,
            ToolExportUnrealEngine.ExportFileType.SkeletalMesh_Fbx,
			ToolExportUnrealEngine.ExportFileType.Materials_Json,
			ToolExportUnrealEngine.ExportFileType.Mtds_Json,
			ToolExportUnrealEngine.ExportFileType.Textures,
			ToolExportUnrealEngine.ExportFileType.AnimationSkeleton_Fbx,
            ToolExportUnrealEngine.ExportFileType.AnimationSequence_Fbx,
            ToolExportUnrealEngine.ExportFileType.AnimationSequences_Fbx,
            ToolExportUnrealEngine.ExportFileType.Taes_Json,
        };

        private bool IS_BUSY = false;
        private bool IS_CANCEL = false;
        private bool IS_CANCEL_CLOSE = false;

        public void SetDestinationFolderPath(string path)
        {
            textBoxDestinationFolder.Text = path;
        }

        public TaeExportUnrealEngineForm()
        {
            InitializeComponent();
        }

        public void ShownInitValues()
        {
            textBoxDestinationFolder.Text = MainScreen.Config.ToolExportAnims_LastDestinationPathUsed;

            listBoxExportAsFileType.SelectedIndex = 0;

            //checkedListBoxHkxSelect.Items.Clear();

            //checkedListBoxHkxSelect.Items.Add("Skeleton.hkx", true);

            curAnimContainer = Scene.MainModel.AnimContainer;

            //for (int i = 0; i < curAnimationList.Count; i++)
            //{
            //    string animShortName = $"{Utils.GetShortIngameFileName(curAnimationList[i].Name)}.hkx";
            //    checkedListBoxHkxSelect.Items.Add(animShortName, true);
            //}

            var curAnimationNameList = curAnimContainer.GetAllAnimationNames();

            treeViewHkxSelect.CheckBoxes = true;

            treeViewHkxSelect.Nodes.Clear();

            treeViewHkxSelect.Nodes.Add("Skeleton.hkx", "Skeleton.hkx");
            treeViewHkxSelect.Nodes["Skeleton.hkx"].Checked = true;

            var namesGrouped = curAnimationNameList.GroupBy(a => a.Substring(0, a.IndexOf("_")));
            foreach (var x in namesGrouped)
            {
                treeViewHkxSelect.Nodes.Add(x.Key, x.Key);
                var node = treeViewHkxSelect.Nodes[x.Key];
                foreach (var child in x)
                {
                    node.Nodes.Add(child, child);
                }
                node.Checked = true;
            }
        }

        private void TaeComboMenu_Load(object sender, EventArgs e)
        {
            SetBusy(false);
            if (new Random().NextDouble() > 0.975)
                SetStatus("Hello. :)");
            else
                SetStatus("Ready.");
            SetProgress(0, 100);
        }

        private bool AskToCancelOperation()
        {
            var res = MessageBox.Show("Cancel current export operation?\nFiles already exported will not be deleted.", 
                "Cancel Operation?", MessageBoxButtons.YesNo, MessageBoxIcon.None);
            return res == DialogResult.Yes;
        }

        private void TaeExportAllAnimsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IS_BUSY)
            {
                e.Cancel = true;
                if (AskToCancelOperation())
                {
                    Invoke(new Action(() =>
                    {
                        IS_CANCEL = true;
                        IS_CANCEL_CLOSE = true;
                    }));
                }
            }
            //Hide();
        }

        private void SetStatus(string status)
        {
            Invoke(new Action(() =>
            {
                labelExportStatus.Text = status;
            }));

        }

        private void SetProgress(int current, int max)
        {
            Invoke(new Action(() =>
            {
                progressBarExportProgress.Maximum = max;
                progressBarExportProgress.Value = current;
            }));

        }

        private void SetStatusAndProgress(string status, int currentProg, int maxProg)
        {
            Invoke(new Action(() =>
            {
                progressBarExportProgress.Maximum = maxProg;
                progressBarExportProgress.Value = currentProg;
                progressBarExportProgress.Update();
                labelExportStatus.Text = status;
                labelExportStatus.Update();
                labelExportStatus.Parent.Update();
            }));
        }

        private void SetBusy(bool isBusy, bool isCancel = false)
        {
            Invoke(new Action(() =>
            {
                bool wasBusyBefore = IS_BUSY;
                bool wasCancel = IS_CANCEL || isCancel;
                bool wasCancelClose = IS_CANCEL_CLOSE;

                textBoxDestinationFolder.ReadOnly = isBusy;
                buttonBrowse.Enabled = !isBusy;
                progressBarExportProgress.Enabled = isBusy;
                buttonStartExport.Enabled = !isBusy;
                buttonCancelExport.Enabled = isBusy;
                listBoxExportAsFileType.Enabled = !isBusy;

                buttonSelectAll.Enabled = !isBusy;
                buttonSelectInvert.Enabled = !isBusy;
                buttonSelectNone.Enabled = !isBusy;
                treeViewHkxSelect.Enabled = !isBusy;

                if (isBusy)
                {
                    listBoxExportAsFileType.BackColor = Color.FromArgb(64, 64, 64);
                }
                else
                {
                    listBoxExportAsFileType.BackColor = Color.DimGray;
                }

                IS_BUSY = isBusy;

                if (!IS_BUSY)
                {
                    IS_CANCEL = false;
                    IS_CANCEL_CLOSE = false;
                }

                // If just stopped being busy & user requested a cancellation from the X to close form then close.
                if ((wasBusyBefore && !IS_BUSY) && wasCancelClose)
                {
                    Close();
                }
            }));
        }



        private bool AskUserIfRelPathIsGood(string finalPath, string userEnteredPath)
        {
            userEnteredPath = userEnteredPath.Trim();
            if (userEnteredPath.ToLower() != finalPath.ToLower())
            {
                bool areYouSure = MessageBox.Show($"Specified destination folder path '{userEnteredPath}' evaluates to the full path '{finalPath}'.\n\n" +
                    $"Please confirm that '{finalPath}' is the path you wish to export to.",
                    "Confirm Path", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

                if (!areYouSure)
                    return false;
            }

            return true;
        }

        private void buttonStartExport_Click(object sender, EventArgs e)
        {

            string path = textBoxDestinationFolder.Text;
            bool filesAlreadyExistInDestinationDir = System.IO.File.Exists(path);

            if (filesAlreadyExistInDestinationDir)
            {
                bool areYouSure = MessageBox.Show($"Specified destination folder exists and already contains files.\nExport anyway and permanently overwrite existing files?",
                    "Overwrite Existing Files?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

                if (!areYouSure)
                    return;
            }

            SetBusy(true);

            if (listBoxExportAsFileType.SelectedIndex < 0)
                listBoxExportAsFileType.SelectedIndex = 0;

            
            var exportAsFileType = fileTypeSelEntriesMapping[listBoxExportAsFileType.SelectedIndex];

            var animContainer = curAnimContainer;
            var animNames = new List<string>();

            //for (int i = 0; i < curAnimationList.Count; i++)
            //{
            //    if (checkedListBoxHkxSelect.GetItemChecked(i + 1))
            //        anims.Add(curAnimationList[i]);
            //}

            foreach (TreeNode rootNode in treeViewHkxSelect.Nodes)
            {
                foreach (TreeNode childNode in rootNode.Nodes)
                {
                    if (childNode.Checked)
                    {
                        animNames.Add(childNode.Text);
                    }
                }
            }

            //bool isExportSkeleton = checkedListBoxHkxSelect.GetItemChecked(0);
            bool isExportSkeleton = treeViewHkxSelect.Nodes["Skeleton.hkx"].Checked;

            if (animNames.Count == 0 && !isExportSkeleton)
            {
                System.Windows.Forms.MessageBox.Show($"No files were selected to export. Operation cancelled.",
                                "Nothing Selected", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                SetBusy(false);
                SetStatus($"Done.");
                return;
            }

            Task.Run(new Action(() =>
            {
                if (animNames != null && animNames.Count > 0)
                {
                    var exporter = new ToolExportUnrealEngine();
                    exporter.InitForAnimContainer(animContainer);

                    int progMax = animNames.Count + (isExportSkeleton ? 1 : 0); // All anims + skeleton

					//byte[] exportedSkeletonBytes = exporter.ExportSkeleton(animContainer.Skeleton.SkeletonPackfile, exportAsFileType, out bool userRequestCancelSkeleton);
					exporter.Export(exportAsFileType, path, null, out bool userRequestCancelSkeleton);

					var skelHkxForAnims = animContainer.Skeleton.SkeletonPackfile;

                    Invoke(new Action(() =>
                    {
                        if (IS_CANCEL || IS_CANCEL_CLOSE)
                        {
                            userRequestCancelSkeleton = true;
                        }
                    }));

                    //if (!userRequestCancelSkeleton)
                    //{
                    //    SetProgress(1, progMax);

                    //    for (int i = 0; i < animNames.Count; i++)
                    //    {
                    //        string animShortName = $"{Utils.GetShortIngameFileName(animNames[i])}.{"hkx"}";

                    //        var animHkxInfo = animContainer.FindAnimationBytes(animNames[i]);

                    //        //byte[] exportedAnimBytes = exporter.ExportAnim(animHkxInfo, exportAsFileType, out bool userRequestCancelAnim, animShortName);
                    //        bool result1 = exporter.Export(exportAsFileType, path, animShortName, out bool userRequestCancelAnim);

                    //        if (result1 && !userRequestCancelAnim)
                    //        {
                    //            try
                    //            {
                    //                //var exportedAnimPath = System.IO.Path.Combine($"{exportDirectory}\\", animShortName);
                    //                //System.IO.File.WriteAllBytes(exportedAnimPath, exportedAnimBytes);
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                var dlgRes = System.Windows.Forms.MessageBox.Show($"Failed to write exported animation file '{animShortName}' to destination directory.\n" +
                    //                $"Would you like to continue anyways?\n\n\nError shown below:\n\n{ex}",
                    //                "Continue With Errors?", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                    //                    userRequestCancelAnim = (dlgRes == System.Windows.Forms.DialogResult.No);
                    //            }
                    //        }

                    //        Invoke(new Action(() =>
                    //        {
                    //            if (IS_CANCEL || IS_CANCEL_CLOSE)
                    //            {
                    //                userRequestCancelAnim = true;
                    //            }
                    //        }));

                    //        if (userRequestCancelAnim)
                    //        {
                    //            SetStatusAndProgress("Cancelled.", 0, 100);
                    //            SetBusy(false, isCancel: true);
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            SetStatusAndProgress($"{(i + 2)}/{progMax}", i + 2, progMax);
                    //        }

                            
                    //    }

                    //}
                    //else
                    //{
                    //    SetStatusAndProgress("Cancelled.", 0, 100);
                    //    SetBusy(false, isCancel: true);
                    //    return;
                    //}


                    
                }
                SetStatus($"Done.");
                SetBusy(false);
            }));

            
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            string path = ShowModelDialog();

            if (path != null)
            {
                textBoxDestinationFolder.Text = path;
                textBoxDestinationFolder.Select(textBoxDestinationFolder.Text.Length, 0);
                textBoxDestinationFolder.ScrollToCaret();
            }
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode root in treeViewHkxSelect.Nodes)
            {
                root.Checked = true;
                foreach (TreeNode child in root.Nodes)
                    child.Checked = true;
            }
        }

        private void buttonSelectNone_Click(object sender, EventArgs e)
        {
            foreach (TreeNode root in treeViewHkxSelect.Nodes)
            {
                root.Checked = false;
                foreach (TreeNode child in root.Nodes)
                    child.Checked = false;
            }
        }

        private void buttonSelectInvert_Click(object sender, EventArgs e)
        {
            foreach (TreeNode root in treeViewHkxSelect.Nodes)
            {
                Dictionary<string, bool> childNodeCheckStates = new Dictionary<string, bool>();
                foreach (TreeNode child in root.Nodes)
                    childNodeCheckStates.Add(child.Text, !child.Checked);

                root.Checked = !root.Checked;

                foreach (TreeNode child in root.Nodes)
                    child.Checked = childNodeCheckStates[child.Text];
            }
        }

        private void treeViewHkxSelect_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode c in e.Node.Nodes)
                c.Checked = e.Node.Checked;
        }

        private void buttonCancelExport_Click(object sender, EventArgs e)
        {
            if (IS_BUSY && AskToCancelOperation())
            {
                Invoke(new Action(() =>
                {
                    IS_CANCEL = true;
                    IS_CANCEL_CLOSE = false;
                }));
                
            }
        }

        private void textBoxDestinationFolder_TextChanged(object sender, EventArgs e)
        {
            MainScreen.Config.ToolExportAnims_LastDestinationPathUsed = textBoxDestinationFolder.Text;
        }

        bool CreateDirectory(string path)
        {
			string exportDirectory = path;
			string exportDirectoryCompare = path + "";

			bool directoryFailed = false;

			if(string.IsNullOrWhiteSpace(exportDirectory))
			{
				MessageBox.Show($"No destination folder path specified.",
					"Invalid Destination Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);

				directoryFailed = true;
			}
			else
			{
				var invalidChars = System.IO.Path.GetInvalidPathChars();
				if(exportDirectory.Any(c => invalidChars.Contains(c)))
				{
					MessageBox.Show($"Invalid characters in the specified destination folder path.",
						"Invalid Destination Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);

					directoryFailed = true;
				}
				else
				{


					try
					{
						exportDirectory = System.IO.Path.GetFullPath(exportDirectory);

						if(!System.IO.Directory.Exists(exportDirectory))
							System.IO.Directory.CreateDirectory(exportDirectory);
					}
					catch(Exception ex)
					{
						MessageBox.Show($"Invalid destination path specified.\n\n\n\n\n\n\n\nSystem exception that was encountered is shown below:\n\n{ex}",
							"Invalid Destination Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);

						directoryFailed = true;
					}


				}
			}

			if(!directoryFailed)
			{
				bool isPathGood = AskUserIfRelPathIsGood(exportDirectory, exportDirectoryCompare);
				if(!isPathGood)
					directoryFailed = true;
			}

			return !directoryFailed;
		}

        string ShowModelDialog()
        {
			var currentDirectory = textBoxDestinationFolder.Text;
			try
			{
				currentDirectory = System.IO.Path.GetFullPath(currentDirectory);
			}
			catch
			{
				currentDirectory = null;
			}

			var exportAsFileType = fileTypeSelEntriesMapping[listBoxExportAsFileType.SelectedIndex];
            if (exportAsFileType == ToolExportUnrealEngine.ExportFileType.AnimationSequence_Fbx)
			{
				var saveFileDialog = new SaveFileDialog();

				if(currentDirectory != null)
					saveFileDialog.InitialDirectory = currentDirectory;

				if(saveFileDialog.ShowDialog() == DialogResult.OK)
					return saveFileDialog.FileName;
			}
			else
            {
				var selectFolderDiag = new FolderBrowserDialog();

				if(currentDirectory != null)
					selectFolderDiag.InitialDirectory = currentDirectory;

				if(selectFolderDiag.ShowDialog() == DialogResult.OK)
					return selectFolderDiag.SelectedPath;
			}

			return null;
		}
	}
}
