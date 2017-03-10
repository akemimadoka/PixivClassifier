using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using PixivClassifier.Properties;

namespace PixivClassifier
{
	public partial class Form1 : Form
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

		public Form1()
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
					responseMessage = await httpClient.GetAsync($"http://www.pixiv.net/member_illust.php?mode=medium&illust_id={pixivId}");
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
					var match = matchObj as Match;
					if (match == null || !match.Success)
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
			}

			var allPixivIds = _pixivIds.Keys;

			tvPictures.Nodes.Clear();
			txtPicturePath.Enabled = txtThreadCount.Enabled = txtTimeout.Enabled = btnSelectFolder.Enabled = btnStartSearch.Enabled = chkSearchRecursively.Enabled = false;

			progressBar1.Value = 0;
			progressBar1.Minimum = 0;
			progressBar1.Maximum = allPixivIds.Count;

			threadCount = Math.Min(allPixivIds.Count, threadCount);

			var filesPerThread = allPixivIds.Count / threadCount;
			var remainedFiles = allPixivIds.Count % threadCount;

			var allTask = new List<Task>(threadCount);
			for (var i = 0; i < threadCount; ++i)
			{
				var currentThread = i;
				var cachedThreadCount = threadCount;
				allTask.Add(SearchTag(
					allPixivIds.Skip(currentThread * filesPerThread)
						.Take((currentThread == cachedThreadCount - 1 ? remainedFiles : 0) + filesPerThread),
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

		private void Form1_Load(object sender, EventArgs e)
		{
			var toolTip = new ToolTip
			{
				ReshowDelay = 100,
				InitialDelay = 100
			};
			toolTip.SetToolTip(txtThreadCount, Resources.ThreadCountToolTip);
			toolTip.SetToolTip(txtTimeout, Resources.TimeoutToolTip);
		}
	}
}
