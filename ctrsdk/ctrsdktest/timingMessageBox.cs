using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ctrsdktest
{
    public partial class timingMessageBox : Form
    {
        // 自动关闭的时间限制，如3为3秒后自动关闭
        private int second = 0;
        // 计数器，用以判断当前窗口弹出后持续的时间
        private int counter = 0;
        public timingMessageBox(string message, int second)
        {
            InitializeComponent();
            // 显示消息
            this.labelMessage.Text = message;
            if (second >0)
            {
                // 获得时间限制
                this.second = second;
                // 初始化计数器
                this.counter = 0;
                // 初始化按钮的文本
                this.buttonOK.Text = string.Format("确定({0})", this.second - this.counter);
                // 激活并启动timer，设置timer的触发间隔为1000毫秒（1秒）
                this.timer1.Enabled = true;
                this.timer1.Interval = 1000;
                this.timer1.Start();
            }
            else
            {
                this.buttonOK.Text = "确定";
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            // 如果没有到达指定的时间限制
            if (this.counter <= this.second)
            {
                // 刷新按钮的文本
                this.buttonOK.Text = string.Format("确定({0})", this.second - this.counter);
                this.Refresh();
                // 计数器自增
                this.counter++;
            }
            // 如果到达时间限制
            else
            {
                // 关闭timer
                this.timer1.Enabled = false;
                this.timer1.Stop();
                // 关闭对话框
                this.Close();
            }
        }
        private void buttonOK_Click(object sender, EventArgs e)
        {
            // 单击确定按钮，关闭对话框
            this.Close();
        }
    }
}
