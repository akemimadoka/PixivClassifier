using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading;
using PixivClassifier.Properties;

namespace PixivClassifier
{
	public partial class frmClassifier : Form
	{
		// Key: PixivID, Value: 图片文件名
		private readonly Dictionary<string, HashSet<string>> _pixivIds = new Dictionary<string, HashSet<string>>();
		// Key: Tag, Value: PixivID
		private readonly Dictionary<string, HashSet<string>> _tagMap = new Dictionary<string, HashSet<string>>();
		private readonly object _lockId = new object();
		private readonly object _lockTagMap = new object();
		private readonly Regex _pixivFilenamePattern = new Regex(@"(\d+)_p\d+(?:_master\d*)??\.\w+", RegexOptions.Compiled);
		private readonly Regex _pixivTagPattern = new Regex("<li class=\"tag\">.*?<a href=.*?class=\"text\">(.*?)</a>.*?</li>", RegexOptions.Compiled);
		private int _timeout;
		private bool _isSearching;

		public frmClassifier()
		{
			InitializeComponent();
		}

		private async Task<IEnumerable<string>> GetTags(string pixivId)
		{
			using (var httpClient = new HttpClient())
			{
				if (_timeout > 0)
				{
					httpClient.Timeout = new TimeSpan(_timeout);
				}
				HttpResponseMessage responseMessage;
				try
				{
					responseMessage = await httpClient.GetAsync($"https://www.pixiv.net/member_illust.php?mode=medium&illust_id={pixivId}");
					if (!responseMessage.IsSuccessStatusCode)
					{
						return null;
					}
				}
				catch (Exception)
				{
					return null;
				}

				var contentString = await responseMessage.Content.ReadAsStringAsync();
				var tags = new HashSet<string>();

				foreach (var matchObj in _pixivTagPattern.Matches(contentString))
				{
					if (!(matchObj is Match match) || !match.Success)
					{
						continue;
					}

					var tagName = match.Groups[1].Value;
					if (!string.IsNullOrWhiteSpace(tagName))
					{
						tags.Add(tagName);
					}
				}

				return tags;
			}
		}

		private async Task SearchTag(IEnumerable<string> allPixivIds, Action completeOneCallback)
		{
			if (completeOneCallback == null)
			{
				throw new ArgumentNullException(nameof(completeOneCallback));
			}

			Contract.EndContractBlock();

			foreach (var pixivId in allPixivIds)
			{
				try
				{
					var tags = await GetTags(pixivId);
					if (tags == null)
					{
						continue;
					}

					foreach (var tag in tags)
					{
						lock (_lockTagMap)
						{
							if (!_tagMap.TryGetValue(tag, out var pixivIds))
							{
								pixivIds = new HashSet<string>();
								_tagMap.Add(tag, pixivIds);
							}

							pixivIds.Add(pixivId);
						}
					}
				}
				finally
				{
					completeOneCallback();
				}
			}
		}

		private void btnSelectFolder_Click(object sender, EventArgs e)
		{
			using (var folderBrowserDialog = new FolderBrowserDialog())
			{
				var result = folderBrowserDialog.ShowDialog();
				if (result == DialogResult.OK && Directory.Exists(folderBrowserDialog.SelectedPath))
				{
					txtPicturePath.Text = folderBrowserDialog.SelectedPath;
				}
			}
		}

		private void tvPictures_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
			{
				return;
			}

			mnuForAllFileInTag.Enabled = tvPictures.SelectedNode != null && tvPictures.SelectedNode.Level == 0;
			contextMenuStrip1.Show(MousePosition);
		}

		private void mnuCopySelectedItem_Click(object sender, EventArgs e)
		{
			if (tvPictures.SelectedNode == null)
			{
				return;
			}

			try
			{
				Clipboard.SetText(tvPictures.SelectedNode.Text);
			}
			catch (Exception exception)
			{
				MessageBox.Show(string.Format(Resources.CopyToClipboardFailedDueTo, exception), Resources.MessageError, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void btnStartSearch_Click(object sender, EventArgs e)
		{
			if (_isSearching)
			{
				return;
			}
			
			if (!int.TryParse(txtThreadCount.Text, out var threadCount))
			{
				MessageBox.Show(Resources.ThreadCountIsNotValidNumber, Resources.MessageError, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (threadCount <= 0 || threadCount > 100)
			{
				MessageBox.Show(Resources.ThreadCountOutOfAcceptableRange, Resources.MessageError, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (!int.TryParse(txtTimeout.Text, out _timeout))
			{
				MessageBox.Show(Resources.TimeoutIsNotValidNumber, Resources.MessageError, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (_timeout < 0)
			{
				MessageBox.Show(Resources.TimeoutOutOfAcceptableRange, Resources.MessageError, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (!Directory.Exists(txtPicturePath.Text))
			{
				MessageBox.Show(Resources.PicturePathNotValidOrNotExist, Resources.MessageError, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			_isSearching = true;

			lock (_lockId)
			lock (_lockTagMap)
			{
					_pixivIds.Clear();
					_tagMap.Clear();
			}
			
			var filenames = new HashSet<string>();
			foreach (var filename in
						Directory.EnumerateFiles(txtPicturePath.Text, "*",
						chkSearchRecursively.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
			{
				filenames.Add(filename);
			}

			Dictionary<string, HashSet<string>>.KeyCollection allPixivIds;

			lock (_lockId)
			{
				foreach (var filename in filenames)
				{
					var match = _pixivFilenamePattern.Match(filename);
					if (!match.Success)
					{
						continue;
					}

					var pixivId = match.Groups[1].Value;
					if (!_pixivIds.TryGetValue(pixivId, out var pixivPictures))
					{
						pixivPictures = new HashSet<string>();
						_pixivIds.Add(pixivId, pixivPictures);
					}
					pixivPictures.Add(filename);
				}

				allPixivIds = _pixivIds.Keys;
			}

			if (allPixivIds.Count == 0)
			{
				return;
			}

			tvPictures.Nodes.Clear();
			txtPicturePath.Enabled = txtThreadCount.Enabled = txtTimeout.Enabled = btnSelectFolder.Enabled = btnStartSearch.Enabled = chkSearchRecursively.Enabled = false;

			progressBar1.Value = 0;
			progressBar1.Minimum = 0;
			progressBar1.Maximum = allPixivIds.Count;

			threadCount = Math.Min(allPixivIds.Count, threadCount);

			var filesPerThread = allPixivIds.Count / threadCount;
			var remainedFiles = allPixivIds.Count % threadCount;

			var allTask = new List<Task>(threadCount);
			for (var currentThread = 0; currentThread < threadCount; ++currentThread)
			{
				allTask.Add(SearchTag(
					allPixivIds.Skip(currentThread * filesPerThread)
						.Take((currentThread == threadCount - 1 ? remainedFiles : 0) + filesPerThread),
					() => { progressBar1.Invoke((Action)(() => { ++progressBar1.Value; })); }));
			}

			await Task.Run(() =>
			{
				foreach (var task in allTask)
				{
					task.Wait();
				}

				Invoke((Action)(() =>
				{
					lock (_lockTagMap)
					{
						foreach (var tagPair in _tagMap)
						{
							var node = tvPictures.Nodes.Add(tagPair.Key);
							foreach (var pixivIds in tagPair.Value)
							{
								node.Nodes.Add(pixivIds);
							}
						}

						tvPictures.Sort();
						txtPicturePath.Enabled = txtThreadCount.Enabled = txtTimeout.Enabled = btnSelectFolder.Enabled = btnStartSearch.Enabled = chkSearchRecursively.Enabled = true;
					}
				}));
				_isSearching = false;
			});
		}

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			mnuCopySelectedItem.Enabled = tvPictures.SelectedNode != null;
		}

		private void frmClassifier_Load(object sender, EventArgs e)
		{
			var toolTip = new ToolTip
			{
				ReshowDelay = 100,
				InitialDelay = 100
			};
			toolTip.SetToolTip(txtThreadCount, Resources.ThreadCountToolTip);
			toolTip.SetToolTip(txtTimeout, Resources.TimeoutToolTip);
		}

		// 对于以下函数
		// 假设当当前菜单可选时总是已检查当前选定节点的有效性，并且无任何异步任务正在执行
		// TODO: 大量重复代码
		private void mnuCopyTo_Click(object sender, EventArgs e)
		{
			Contract.Assert(!string.IsNullOrWhiteSpace(tvPictures.SelectedNode?.Text) && tvPictures.SelectedNode.Level == 0, "当前选定的节点无效");
			// 仅检查了锁，并不严谨
			Contract.Assert(!Monitor.IsEntered(_lockId) && !Monitor.IsEntered(_lockTagMap), "当前正在运行一个或多个异步任务");

			var tagName = tvPictures.SelectedNode.Text;

			string path;
			using (var folderBrowserDlg = new FolderBrowserDialog())
			{
				var result = folderBrowserDlg.ShowDialog();
				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDlg.SelectedPath))
				{
					path = folderBrowserDlg.SelectedPath;
				}
				else
				{
					return;
				}
			}

			var idSet = _tagMap[tagName];

			progressBar1.Value = 0;
			progressBar1.Minimum = 0;
			progressBar1.Maximum = idSet.Count;

			var alwaysIgnore = false;

			foreach (var id in idSet)
			{
				foreach (var filePath in _pixivIds[id])
				{
					var fileName = Path.GetFileName(filePath);
					if (fileName == null)
					{
						continue;
					}

					var targetPath = path + '\\' + fileName;

					while (true)
					{
						try
						{
							File.Copy(filePath, targetPath);
							break;
						}
						catch (Exception exception)
						{
							if (alwaysIgnore)
							{
								break;
							}

							var result = MessageBox.Show(string.Format(Resources.CopyErrorDescription, filePath, targetPath, exception), Resources.MessageError, MessageBoxButtons.AbortRetryIgnore);
							switch (result)
							{
								case DialogResult.Abort:
									MessageBox.Show(Resources.OperationStoppedByUser, Resources.Notification);
									progressBar1.Value = 0;
									return;
								case DialogResult.Retry:
									continue;
								case DialogResult.Ignore:
									alwaysIgnore = MessageBox.Show(Resources.ApplyToAllOperationConfirmation, Resources.Notification, MessageBoxButtons.YesNo) == DialogResult.Yes;
									goto NextFile;
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
					}
					NextFile:;
				}

				++progressBar1.Value;
			}

			MessageBox.Show(Resources.OperationCompleted, Resources.Notification);
		}

		private void mnuMoveTo_Click(object sender, EventArgs e)
		{
			Contract.Assert(!string.IsNullOrWhiteSpace(tvPictures.SelectedNode?.Text) && tvPictures.SelectedNode.Level == 0, "当前选定的节点无效");
			// 仅检查了锁，并不严谨
			Contract.Assert(!Monitor.IsEntered(_lockId) && !Monitor.IsEntered(_lockTagMap), "当前正在运行一个或多个异步任务");

			var node = tvPictures.SelectedNode;
			var tagName = node.Text;

			string path;
			using (var folderBrowserDlg = new FolderBrowserDialog())
			{
				var result = folderBrowserDlg.ShowDialog();
				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDlg.SelectedPath))
				{
					path = folderBrowserDlg.SelectedPath;
				}
				else
				{
					return;
				}
			}

			var idSet = _tagMap[tagName];

			progressBar1.Value = 0;
			progressBar1.Minimum = 0;
			progressBar1.Maximum = idSet.Count;

			var alwaysIgnore = false;

			foreach (var id in idSet)
			{
				foreach (var filePath in _pixivIds[id])
				{
					var fileName = Path.GetFileName(filePath);
					if (fileName == null)
					{
						continue;
					}

					var targetPath = path + '\\' + fileName;

					while (true)
					{
						try
						{
							File.Move(filePath, targetPath);
							break;
						}
						catch (Exception exception)
						{
							if (alwaysIgnore)
							{
								break;
							}

							var result = MessageBox.Show(string.Format(Resources.MoveErrorDescription, filePath, targetPath, exception), Resources.MessageError, MessageBoxButtons.AbortRetryIgnore);
							switch (result)
							{
								case DialogResult.Abort:
									MessageBox.Show(Resources.OperationStoppedByUser, Resources.Notification);
									progressBar1.Value = 0;
									return;
								case DialogResult.Retry:
									continue;
								case DialogResult.Ignore:
									alwaysIgnore = MessageBox.Show(Resources.ApplyToAllOperationConfirmation, Resources.Notification, MessageBoxButtons.YesNo) == DialogResult.Yes;
									goto NextFile;
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
					}
					NextFile:;
				}

				_pixivIds.Remove(id);
				++progressBar1.Value;
			}

			_tagMap.Remove(tagName);
			tvPictures.Nodes.Remove(node);

			MessageBox.Show(Resources.OperationCompleted, Resources.Notification);
		}

		private void mnuDelete_Click(object sender, EventArgs e)
		{
			Contract.Assert(!string.IsNullOrWhiteSpace(tvPictures.SelectedNode?.Text) && tvPictures.SelectedNode.Level == 0, "当前选定的节点无效");
			// 仅检查了锁，并不严谨
			Contract.Assert(!Monitor.IsEntered(_lockId) && !Monitor.IsEntered(_lockTagMap), "当前正在运行一个或多个异步任务");

			var node = tvPictures.SelectedNode;
			var tagName = node.Text;

			if (MessageBox.Show(string.Format(Resources.DeleteConfirmation, tagName), Resources.Notification, MessageBoxButtons.OKCancel) != DialogResult.OK)
			{
				return;
			}

			var idSet = _tagMap[tagName];

			progressBar1.Value = 0;
			progressBar1.Minimum = 0;
			progressBar1.Maximum = idSet.Count;

			var alwaysIgnore = false;

			foreach (var id in idSet)
			{
				foreach (var filePath in _pixivIds[id])
				{
					var fileName = Path.GetFileName(filePath);
					if (fileName == null)
					{
						continue;
					}

					while (true)
					{
						try
						{
							File.Delete(filePath);
							break;
						}
						catch (Exception exception)
						{
							if (alwaysIgnore)
							{
								break;
							}

							var result = MessageBox.Show(string.Format(Resources.DeleteErrorDescription, filePath, exception), Resources.MessageError, MessageBoxButtons.AbortRetryIgnore);
							switch (result)
							{
								case DialogResult.Abort:
									MessageBox.Show(Resources.OperationStoppedByUser, Resources.Notification);
									progressBar1.Value = 0;
									return;
								case DialogResult.Retry:
									continue;
								case DialogResult.Ignore:
									alwaysIgnore = MessageBox.Show(Resources.ApplyToAllOperationConfirmation, Resources.Notification, MessageBoxButtons.YesNo) == DialogResult.Yes;
									goto NextFile;
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
					}
					NextFile:;
				}

				_pixivIds.Remove(id);
				++progressBar1.Value;
			}

			_tagMap.Remove(tagName);
			tvPictures.Nodes.Remove(node);

			MessageBox.Show(Resources.OperationCompleted, Resources.Notification);
		}
	}
}
