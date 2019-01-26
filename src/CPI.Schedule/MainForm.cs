using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CPI.ScheduleMgr
{
    public partial class MainForm : Form
    {
        private SynchronizationContext _sync;
        private NotifyIcon _notifyIcon;
        private ContextMenu _notifyContextMenu;
        private String _jobConfigFile;
        private Process _jobSchedulerProcess;

        public MainForm()
        {
            InitializeComponent();

            this.components = new Container();
            _sync = SynchronizationContext.Current;

            var cms = new ContextMenuStrip(this.components);
            cms.Items.Add("修改Cron表达式", null, GV_ModifyCron);
            gv.ContextMenuStrip = cms;

            _notifyContextMenu = new ContextMenu();
            _notifyContextMenu.MenuItems.Add(new MenuItem("显示主界面", NotifyIcon_ShowMainUI));
            _notifyContextMenu.MenuItems.Add(new MenuItem("-"));
            _notifyContextMenu.MenuItems.Add(new MenuItem("退出", NotifyIcon_Exit));

            _notifyIcon = new NotifyIcon(this.components)
            {
                Text = "CPI定时调度器 v1.0",
                BalloonTipText = "程序已最小化",
                ContextMenu = _notifyContextMenu,
                Icon = global::CPI.ScheduleMgr.Properties.Resources.notify_icon,
                Visible = true
            };

            _notifyIcon.DoubleClick += this._notifyIcon_DoubleClick;
            this.SizeChanged += this.MainForm_SizeChanged;
            this.Load += this.MainForm_Load;
            this.FormClosing += this.MainForm_FormClosing;
        }

        private void _notifyIcon_DoubleClick(Object sender, EventArgs e)
        {
            NotifyIcon_ShowMainUI(sender, e);
        }

        private void MainForm_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (!AllTaskStoped())
            {
                MessageBox.Show("当前还有正在运行的任务，请停止所有正在运行的任务，然后再退出！", "系统警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
                return;
            }

            if (MessageBox.Show("确定退出吗？", "系统提示", MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                e.Cancel = true;
                return;
            }

            CloseJobScheduler();
            _notifyIcon.Visible = false;
        }

        private void MainForm_SizeChanged(Object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                _notifyIcon.ShowBalloonTip(2000);
            }
            else
            {
                this.ShowInTaskbar = true;
            }
        }

        private void MainForm_Load(Object sender, EventArgs e)
        {
            LoadJobs();
            StartupJobScheduler();
        }

        private void BtnStart_Click(Object sender, EventArgs e)
        {
            var names = GetSelectedJobNames();
            if (names.Count() > 0)
            {
                btnStart.Enabled = false;
                ChangeSwitch(names, "on");
                btnStart.Enabled = true;
            }
        }

        private void BtnStop_Click(Object sender, EventArgs e)
        {
            var names = GetSelectedJobNames();
            if (names.Count() > 0)
            {
                btnStop.Enabled = false;
                ChangeSwitch(names, "off");
                btnStop.Enabled = true;
            }
        }

        private void GV_ModifyCron(Object sender, EventArgs e)
        {
            if (gv.SelectedRows.Count > 0)
            {
                var row = gv.SelectedRows[0];
                var jobName = row.Cells[0].Value.ToString();

                var win = new WinModifyCron
                {
                    Owner = this
                };

                if (win.LoadCron(jobName))
                {
                    win.ShowDialog();
                }
            }
        }

        private void NotifyIcon_ShowMainUI(Object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void NotifyIcon_Exit(Object sender, EventArgs e)
        {
            this.Close();
        }

        private Boolean AllTaskStoped()
        {
            if (gv.Rows.Count > 0)
            {
                Int32 stopedCount = 0;
                foreach (DataGridViewRow row in gv.Rows)
                {
                    if (row.Cells.Count < 3) { continue; }
                    if (String.Compare((row.Cells[2].Tag ?? String.Empty).ToString(), "off", true) == 0)
                    {
                        stopedCount++;
                    }
                }

                return gv.Rows.Count == stopedCount;
            }

            return true;
        }

        private IEnumerable<String> GetSelectedJobNames()
        {
            var selectedRows = gv.SelectedRows;
            List<String> names = new List<String>(selectedRows.Count);
            foreach (DataGridViewRow row in selectedRows)
            {
                names.Add(row.Cells[0].Value.ToString());
            }
            return names;
        }

        private void ChangeSwitch(IEnumerable<String> jobNames, String value)
        {
            String op = value == "off" ? "停止" : "启动";
            String confirmInfo = $"确定{op}【{String.Join("，", jobNames)}】这些任务吗？";

            if (MessageBox.Show(confirmInfo, "系统提示", MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                return;
            }

            XDocument doc = null;
            try
            {
                doc = XDocument.Load(_jobConfigFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载任务配置文件失败：{ex.Message}");
                return;
            }

            if (doc == null)
            {
                MessageBox.Show($"操作失败：解析配置文件返回空");
                return;
            }

            var findJobEls = doc.Root.Descendants("job").Where(x => jobNames.Contains(x.Attribute("name").Value));
            if (findJobEls == null || findJobEls.Count() == 0)
            {
                MessageBox.Show($"未找到要修改的任务");
            }

            foreach (var el in findJobEls)
            {
                el.Element("switch").Value = value;
            }

            try
            {
                doc.Save(_jobConfigFile);
                Thread.Sleep(1000);
                LoadJobs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存修改失败：{ex.Message}");
                return;
            }
        }

        private void LoadJobs()
        {
            String jobConfigFile = ConfigurationManager.AppSettings["JobsConfigFilePath"];
            if (!File.Exists(jobConfigFile))
            {
                MessageBox.Show($"任务配置文件{jobConfigFile}不存在");
                this.Close();
                return;
            }

            XDocument doc = null;
            try
            {
                doc = XDocument.Load(jobConfigFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析配置文件{jobConfigFile}失败：{ex.Message}");
                this.Close();
                return;
            }

            if (doc == null) { return; }

            _jobConfigFile = jobConfigFile;

            gv.Rows.Clear();

            var jobEls = doc.Root.Elements();
            foreach (var jobEl in jobEls)
            {
                var off = String.Compare(jobEl.Element("switch").Value, "off", true) == 0;

                var row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell()
                {
                    Value = jobEl.Attribute("name").Value
                });
                row.Cells.Add(new DataGridViewTextBoxCell()
                {
                    Value = jobEl.Attribute("desc").Value
                });
                row.Cells.Add(new DataGridViewTextBoxCell()
                {
                    Value = off ? "已停止" : "已启动",
                    Style = new DataGridViewCellStyle()
                    {
                        ForeColor = off ? Color.Red : Color.Green,
                        Font = new Font("文泉驿微米黑", 10.5f)
                    },
                    Tag = off ? "off" : "on"
                });

                gv.Rows.Add(row);
            }
        }

        private void StartupJobScheduler()
        {
            String jobsExecutorFilePath = ConfigurationManager.AppSettings["JobsExecutorFilePath"];
            if (!File.Exists(jobsExecutorFilePath))
            {
                MessageBox.Show("任务执行文件不存在！");
                return;
            }

            _jobSchedulerProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = jobsExecutorFilePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });

            Task.Run(() =>
            {
                var outputStream = _jobSchedulerProcess.StandardOutput;
                String readLine = null;
                Int32 lineCount = 0;
                while ((readLine = outputStream.ReadLine()) != null)
                {
                    _sync.Post(o =>
                    {
                        if (lineCount > 100)
                        {
                            txtOut.Clear();
                            lineCount = 0;
                        }

                        txtOut.Text += $"{readLine}{Environment.NewLine}";
                        lineCount++;
                        txtOut.SelectionStart = txtOut.Text.Length;
                        txtOut.ScrollToCaret();
                    }, null);
                }
            });
        }

        private void CloseJobScheduler()
        {
            if (_jobSchedulerProcess != null)
            {
                try
                {
                    _jobSchedulerProcess.Kill();
                    _jobSchedulerProcess.WaitForExit(10);
                }
                catch { }
            }
        }
    }
}
