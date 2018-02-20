namespace PixivClassifier
{
	partial class frmClassifier
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this.txtPicturePath = new System.Windows.Forms.TextBox();
			this.btnSelectFolder = new System.Windows.Forms.Button();
			this.tvPictures = new System.Windows.Forms.TreeView();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.label2 = new System.Windows.Forms.Label();
			this.txtThreadCount = new System.Windows.Forms.TextBox();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuCopySelectedItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuForAllFileInTag = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuCopyTo = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuMoveTo = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.chkSearchRecursively = new System.Windows.Forms.CheckBox();
			this.btnStartSearch = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.txtTimeout = new System.Windows.Forms.TextBox();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(89, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "图片文件路径：";
			// 
			// txtPicturePath
			// 
			this.txtPicturePath.Location = new System.Drawing.Point(97, 9);
			this.txtPicturePath.Name = "txtPicturePath";
			this.txtPicturePath.Size = new System.Drawing.Size(227, 21);
			this.txtPicturePath.TabIndex = 1;
			// 
			// btnSelectFolder
			// 
			this.btnSelectFolder.Location = new System.Drawing.Point(330, 8);
			this.btnSelectFolder.Name = "btnSelectFolder";
			this.btnSelectFolder.Size = new System.Drawing.Size(40, 22);
			this.btnSelectFolder.TabIndex = 2;
			this.btnSelectFolder.Text = "...";
			this.btnSelectFolder.UseVisualStyleBackColor = true;
			this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
			// 
			// tvPictures
			// 
			this.tvPictures.Location = new System.Drawing.Point(9, 89);
			this.tvPictures.Name = "tvPictures";
			this.tvPictures.Size = new System.Drawing.Size(360, 204);
			this.tvPictures.TabIndex = 3;
			this.tvPictures.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tvPictures_MouseClick);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(10, 62);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(360, 21);
			this.progressBar1.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 12);
			this.label2.TabIndex = 5;
			this.label2.Text = "线程数：";
			// 
			// txtThreadCount
			// 
			this.txtThreadCount.Location = new System.Drawing.Point(60, 35);
			this.txtThreadCount.Name = "txtThreadCount";
			this.txtThreadCount.Size = new System.Drawing.Size(42, 21);
			this.txtThreadCount.TabIndex = 6;
			this.txtThreadCount.Text = "15";
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopySelectedItem,
            this.mnuForAllFileInTag});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(330, 70);
			this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
			// 
			// mnuCopySelectedItem
			// 
			this.mnuCopySelectedItem.Name = "mnuCopySelectedItem";
			this.mnuCopySelectedItem.Size = new System.Drawing.Size(329, 22);
			this.mnuCopySelectedItem.Text = "复制选中内容（&C）";
			this.mnuCopySelectedItem.Click += new System.EventHandler(this.mnuCopySelectedItem_Click);
			// 
			// mnuForAllFileInTag
			// 
			this.mnuForAllFileInTag.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopyTo,
            this.mnuMoveTo,
            this.mnuDelete});
			this.mnuForAllFileInTag.Enabled = false;
			this.mnuForAllFileInTag.Name = "mnuForAllFileInTag";
			this.mnuForAllFileInTag.Size = new System.Drawing.Size(329, 22);
			this.mnuForAllFileInTag.Text = "对选中的 Tag 下的所有文件执行同一操作（&E）";
			// 
			// mnuCopyTo
			// 
			this.mnuCopyTo.Name = "mnuCopyTo";
			this.mnuCopyTo.Size = new System.Drawing.Size(157, 22);
			this.mnuCopyTo.Text = "复制到...（&O）";
			this.mnuCopyTo.Click += new System.EventHandler(this.mnuCopyTo_Click);
			// 
			// mnuMoveTo
			// 
			this.mnuMoveTo.Name = "mnuMoveTo";
			this.mnuMoveTo.Size = new System.Drawing.Size(157, 22);
			this.mnuMoveTo.Text = "移动到...（&M）";
			this.mnuMoveTo.Click += new System.EventHandler(this.mnuMoveTo_Click);
			// 
			// mnuDelete
			// 
			this.mnuDelete.Name = "mnuDelete";
			this.mnuDelete.Size = new System.Drawing.Size(157, 22);
			this.mnuDelete.Text = "删除（&D）";
			this.mnuDelete.Click += new System.EventHandler(this.mnuDelete_Click);
			// 
			// chkSearchRecursively
			// 
			this.chkSearchRecursively.AutoSize = true;
			this.chkSearchRecursively.Location = new System.Drawing.Point(194, 37);
			this.chkSearchRecursively.Name = "chkSearchRecursively";
			this.chkSearchRecursively.Size = new System.Drawing.Size(108, 16);
			this.chkSearchRecursively.TabIndex = 8;
			this.chkSearchRecursively.Text = "递归地搜索图片";
			this.chkSearchRecursively.UseVisualStyleBackColor = true;
			// 
			// btnStartSearch
			// 
			this.btnStartSearch.Location = new System.Drawing.Point(308, 34);
			this.btnStartSearch.Name = "btnStartSearch";
			this.btnStartSearch.Size = new System.Drawing.Size(62, 20);
			this.btnStartSearch.TabIndex = 9;
			this.btnStartSearch.Text = "开始";
			this.btnStartSearch.UseVisualStyleBackColor = true;
			this.btnStartSearch.Click += new System.EventHandler(this.btnStartSearch_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(105, 39);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 12);
			this.label3.TabIndex = 10;
			this.label3.Text = "超时：";
			// 
			// txtTimeout
			// 
			this.txtTimeout.Location = new System.Drawing.Point(144, 35);
			this.txtTimeout.Name = "txtTimeout";
			this.txtTimeout.Size = new System.Drawing.Size(42, 21);
			this.txtTimeout.TabIndex = 11;
			this.txtTimeout.Text = "0";
			// 
			// frmClassifier
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(381, 305);
			this.Controls.Add(this.txtTimeout);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnStartSearch);
			this.Controls.Add(this.chkSearchRecursively);
			this.Controls.Add(this.txtThreadCount);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.tvPictures);
			this.Controls.Add(this.btnSelectFolder);
			this.Controls.Add(this.txtPicturePath);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmClassifier";
			this.Text = "Pixiv快速分类器";
			this.Load += new System.EventHandler(this.frmClassifier_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtPicturePath;
		private System.Windows.Forms.Button btnSelectFolder;
		private System.Windows.Forms.TreeView tvPictures;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtThreadCount;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem mnuCopySelectedItem;
		private System.Windows.Forms.CheckBox chkSearchRecursively;
		private System.Windows.Forms.Button btnStartSearch;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtTimeout;
		private System.Windows.Forms.ToolStripMenuItem mnuForAllFileInTag;
		private System.Windows.Forms.ToolStripMenuItem mnuCopyTo;
		private System.Windows.Forms.ToolStripMenuItem mnuMoveTo;
		private System.Windows.Forms.ToolStripMenuItem mnuDelete;
	}
}

