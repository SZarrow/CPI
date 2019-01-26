using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CPI.ScheduleMgr
{
    public partial class WinModifyCron : Form
    {
        private XElement _jobEl = null;
        private String JobsConfigFilePath = ConfigurationManager.AppSettings["JobsConfigFilePath"];

        public WinModifyCron()
        {
            InitializeComponent();
            btnOK.Click += this.BtnOK_Click;
        }

        private void BtnOK_Click(Object sender, EventArgs e)
        {
            if (_jobEl != null)
            {
                var cronEl = _jobEl.Element("cron");
                if (cronEl != null)
                {
                    cronEl.Value = txtCron.Text;
                }
                else
                {
                    _jobEl.Add(new XElement("cron", txtCron.Text));
                }

                try
                {
                    _jobEl.Document.Save(JobsConfigFilePath);
                    MessageBox.Show("保存成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败：{ex.Message}");
                }
            }
            else
            {
                MessageBox.Show($"保存失败：未找到指定任务名的节点");
            }
        }

        public Boolean LoadCron(String jobName)
        {
            if (!File.Exists(JobsConfigFilePath))
            {
                MessageBox.Show($"任务配置文件{JobsConfigFilePath}不存在！");
                return false;
            }
            try
            {
                var doc = XDocument.Load(JobsConfigFilePath);
                _jobEl = doc.Root.Descendants("job").Where(x => x.Attribute("name") != null && x.Attribute("name").Value == jobName).FirstOrDefault();
                if (_jobEl != null)
                {
                    var cronEl = _jobEl.Element("cron");
                    if (cronEl != null)
                    {
                        txtCron.Text = cronEl.Value;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取Cron表达式失败：{ex.Message}");
            }

            return false;
        }
    }
}
