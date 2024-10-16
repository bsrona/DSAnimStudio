﻿namespace DSAnimStudio.TaeEditor
{
    partial class TaeExportUnrealEngineForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("a00_0000.hkx");
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("a00_0001.hkx");
			System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("a00", new System.Windows.Forms.TreeNode[] { treeNode1, treeNode2 });
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("a01_0000.hkx");
			System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("a01", new System.Windows.Forms.TreeNode[] { treeNode4 });
			System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Skeleton.hkx");
			label1 = new System.Windows.Forms.Label();
			textBoxDestinationFolder = new System.Windows.Forms.TextBox();
			buttonBrowse = new System.Windows.Forms.Button();
			label2 = new System.Windows.Forms.Label();
			listBoxExportAsFileType = new System.Windows.Forms.ListBox();
			buttonStartExport = new System.Windows.Forms.Button();
			progressBarExportProgress = new System.Windows.Forms.ProgressBar();
			labelExportStatus = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			buttonSelectAll = new System.Windows.Forms.Button();
			buttonSelectNone = new System.Windows.Forms.Button();
			buttonSelectInvert = new System.Windows.Forms.Button();
			treeViewHkxSelect = new System.Windows.Forms.TreeView();
			buttonCancelExport = new System.Windows.Forms.Button();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.ForeColor = System.Drawing.Color.White;
			label1.Location = new System.Drawing.Point(12, 18);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(117, 17);
			label1.TabIndex = 0;
			label1.Text = "Destination Folder:";
			// 
			// textBoxDestinationFolder
			// 
			textBoxDestinationFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			textBoxDestinationFolder.BackColor = System.Drawing.Color.DimGray;
			textBoxDestinationFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			textBoxDestinationFolder.ForeColor = System.Drawing.Color.White;
			textBoxDestinationFolder.Location = new System.Drawing.Point(124, 14);
			textBoxDestinationFolder.Name = "textBoxDestinationFolder";
			textBoxDestinationFolder.Size = new System.Drawing.Size(494, 23);
			textBoxDestinationFolder.TabIndex = 1;
			textBoxDestinationFolder.TextChanged += textBoxDestinationFolder_TextChanged;
			// 
			// buttonBrowse
			// 
			buttonBrowse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			buttonBrowse.ForeColor = System.Drawing.Color.White;
			buttonBrowse.Location = new System.Drawing.Point(624, 14);
			buttonBrowse.Name = "buttonBrowse";
			buttonBrowse.Size = new System.Drawing.Size(133, 26);
			buttonBrowse.TabIndex = 2;
			buttonBrowse.Text = "Browse...";
			buttonBrowse.UseCompatibleTextRendering = true;
			buttonBrowse.UseVisualStyleBackColor = true;
			buttonBrowse.Click += buttonBrowse_Click;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.ForeColor = System.Drawing.Color.White;
			label2.Location = new System.Drawing.Point(11, 58);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(122, 17);
			label2.TabIndex = 3;
			label2.Text = "Export As File Type:";
			// 
			// listBoxExportAsFileType
			// 
			listBoxExportAsFileType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			listBoxExportAsFileType.BackColor = System.Drawing.Color.DimGray;
			listBoxExportAsFileType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			listBoxExportAsFileType.ForeColor = System.Drawing.Color.White;
			listBoxExportAsFileType.FormattingEnabled = true;
			listBoxExportAsFileType.ItemHeight = 17;
			listBoxExportAsFileType.Items.AddRange(new object[] { "All", "SkeletalMesh(FBX)", "Materials(MAT)", "MTDs(MTD)", "Textures", "AnimationSkeleton(FBX)", "AnimationSequence(FBX)", "AnimationSequences(FBX)", "Taes(TAE)" });
			listBoxExportAsFileType.Location = new System.Drawing.Point(125, 58);
			listBoxExportAsFileType.Name = "listBoxExportAsFileType";
			listBoxExportAsFileType.Size = new System.Drawing.Size(493, 70);
			listBoxExportAsFileType.TabIndex = 5;
			// 
			// buttonStartExport
			// 
			buttonStartExport.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			buttonStartExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			buttonStartExport.ForeColor = System.Drawing.Color.White;
			buttonStartExport.Location = new System.Drawing.Point(11, 352);
			buttonStartExport.Name = "buttonStartExport";
			buttonStartExport.Size = new System.Drawing.Size(107, 27);
			buttonStartExport.TabIndex = 6;
			buttonStartExport.Text = "Start Export";
			buttonStartExport.UseCompatibleTextRendering = true;
			buttonStartExport.UseVisualStyleBackColor = true;
			buttonStartExport.Click += buttonStartExport_Click;
			// 
			// progressBarExportProgress
			// 
			progressBarExportProgress.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			progressBarExportProgress.Location = new System.Drawing.Point(125, 352);
			progressBarExportProgress.Name = "progressBarExportProgress";
			progressBarExportProgress.Size = new System.Drawing.Size(493, 27);
			progressBarExportProgress.TabIndex = 7;
			// 
			// labelExportStatus
			// 
			labelExportStatus.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			labelExportStatus.ForeColor = System.Drawing.Color.White;
			labelExportStatus.Location = new System.Drawing.Point(624, 352);
			labelExportStatus.Name = "labelExportStatus";
			labelExportStatus.Size = new System.Drawing.Size(133, 27);
			labelExportStatus.TabIndex = 8;
			labelExportStatus.Text = "XXXX/XXXX";
			labelExportStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.ForeColor = System.Drawing.Color.White;
			label3.Location = new System.Drawing.Point(35, 135);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(94, 17);
			label3.TabIndex = 10;
			label3.Text = "Files to Export:";
			// 
			// buttonSelectAll
			// 
			buttonSelectAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			buttonSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			buttonSelectAll.ForeColor = System.Drawing.Color.White;
			buttonSelectAll.Location = new System.Drawing.Point(624, 135);
			buttonSelectAll.Name = "buttonSelectAll";
			buttonSelectAll.Size = new System.Drawing.Size(133, 26);
			buttonSelectAll.TabIndex = 11;
			buttonSelectAll.Text = "Select All";
			buttonSelectAll.UseCompatibleTextRendering = true;
			buttonSelectAll.UseVisualStyleBackColor = true;
			buttonSelectAll.Click += buttonSelectAll_Click;
			// 
			// buttonSelectNone
			// 
			buttonSelectNone.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			buttonSelectNone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			buttonSelectNone.ForeColor = System.Drawing.Color.White;
			buttonSelectNone.Location = new System.Drawing.Point(624, 168);
			buttonSelectNone.Name = "buttonSelectNone";
			buttonSelectNone.Size = new System.Drawing.Size(133, 26);
			buttonSelectNone.TabIndex = 12;
			buttonSelectNone.Text = "Select None";
			buttonSelectNone.UseCompatibleTextRendering = true;
			buttonSelectNone.UseVisualStyleBackColor = true;
			buttonSelectNone.Click += buttonSelectNone_Click;
			// 
			// buttonSelectInvert
			// 
			buttonSelectInvert.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			buttonSelectInvert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			buttonSelectInvert.ForeColor = System.Drawing.Color.White;
			buttonSelectInvert.Location = new System.Drawing.Point(624, 201);
			buttonSelectInvert.Name = "buttonSelectInvert";
			buttonSelectInvert.Size = new System.Drawing.Size(133, 26);
			buttonSelectInvert.TabIndex = 13;
			buttonSelectInvert.Text = "Invert Selection";
			buttonSelectInvert.UseCompatibleTextRendering = true;
			buttonSelectInvert.UseVisualStyleBackColor = true;
			buttonSelectInvert.Click += buttonSelectInvert_Click;
			// 
			// treeViewHkxSelect
			// 
			treeViewHkxSelect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			treeViewHkxSelect.CheckBoxes = true;
			treeViewHkxSelect.Location = new System.Drawing.Point(125, 135);
			treeViewHkxSelect.Name = "treeViewHkxSelect";
			treeNode1.Checked = true;
			treeNode1.Name = "Node1";
			treeNode1.Text = "a00_0000.hkx";
			treeNode2.Name = "Node2";
			treeNode2.Text = "a00_0001.hkx";
			treeNode3.Name = "Node0";
			treeNode3.Text = "a00";
			treeNode4.Name = "Node4";
			treeNode4.Text = "a01_0000.hkx";
			treeNode5.Name = "Node3";
			treeNode5.Text = "a01";
			treeNode6.Name = "Node5";
			treeNode6.Text = "Skeleton.hkx";
			treeViewHkxSelect.Nodes.AddRange(new System.Windows.Forms.TreeNode[] { treeNode3, treeNode5, treeNode6 });
			treeViewHkxSelect.Size = new System.Drawing.Size(493, 210);
			treeViewHkxSelect.TabIndex = 14;
			treeViewHkxSelect.AfterCheck += treeViewHkxSelect_AfterCheck;
			// 
			// buttonCancelExport
			// 
			buttonCancelExport.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			buttonCancelExport.Enabled = false;
			buttonCancelExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			buttonCancelExport.ForeColor = System.Drawing.Color.White;
			buttonCancelExport.Location = new System.Drawing.Point(11, 318);
			buttonCancelExport.Name = "buttonCancelExport";
			buttonCancelExport.Size = new System.Drawing.Size(107, 27);
			buttonCancelExport.TabIndex = 15;
			buttonCancelExport.Text = "Cancel Export";
			buttonCancelExport.UseCompatibleTextRendering = true;
			buttonCancelExport.UseVisualStyleBackColor = true;
			buttonCancelExport.Click += buttonCancelExport_Click;
			// 
			// TaeExportUnrealEngineForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
			ClientSize = new System.Drawing.Size(769, 393);
			Controls.Add(buttonCancelExport);
			Controls.Add(treeViewHkxSelect);
			Controls.Add(buttonSelectInvert);
			Controls.Add(buttonSelectNone);
			Controls.Add(buttonSelectAll);
			Controls.Add(label3);
			Controls.Add(labelExportStatus);
			Controls.Add(progressBarExportProgress);
			Controls.Add(buttonStartExport);
			Controls.Add(listBoxExportAsFileType);
			Controls.Add(label2);
			Controls.Add(buttonBrowse);
			Controls.Add(textBoxDestinationFolder);
			Controls.Add(label1);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(625, 357);
			Name = "TaeExportUnrealEngineForm";
			ShowIcon = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Export To Unreal Engine";
			FormClosing += TaeExportAllAnimsForm_FormClosing;
			Load += TaeComboMenu_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDestinationFolder;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxExportAsFileType;
        private System.Windows.Forms.Button buttonStartExport;
        private System.Windows.Forms.ProgressBar progressBarExportProgress;
        private System.Windows.Forms.Label labelExportStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonSelectNone;
        private System.Windows.Forms.Button buttonSelectInvert;
        private System.Windows.Forms.TreeView treeViewHkxSelect;
        private System.Windows.Forms.Button buttonCancelExport;
    }
}