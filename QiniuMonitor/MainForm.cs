/*
 * Created by SharpDevelop.
 * User: 中希
 * Date: 2016/4/12
 * Time: 8:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json.Converters;
using Qiniu.Auth;
using Qiniu.IO;
using Qiniu.IO.Resumable;
using Qiniu.RS;
using Qiniu.Conf;

namespace QiniuMonitor
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		private FileSystemWatcher fsw ;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			// 读取配置文件
			Configuration conf = Configuration.Read();
			if(conf != null)
			{
				this.textBox1.Text = conf.GetString("AK");
				this.textBox2.Text = conf.GetString("SK");
				this.textBox3.Text = conf.GetString("space");
				this.textBox4.Text = conf.GetString("dict");
				this.textBox5.Text = conf.GetString("domain");
			}
			
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			dialog.Description = "请选择监控文件路径";
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				String foldPath = dialog.SelectedPath;
				this.textBox4.Text = foldPath;
			}
		}
		
		void B1Click(object sender, EventArgs e)
		{
			//检查输入
			if(this.textBox1.Text == "" ||
			   this.textBox2.Text == "" ||
			   this.textBox3.Text == "" ||
			   this.textBox4.Text == "" ||
			   this.textBox5.Text == "" )
			{
				MessageBox.Show("请填写所有设置项");
			}
			else{
				//首先保存当前的设置
				Configuration conf = new Configuration();
				conf.Add("AK",this.textBox1.Text);
				conf.Add("SK",this.textBox2.Text);
				conf.Add("space",this.textBox3.Text);
				conf.Add("dict",this.textBox4.Text);
				conf.Add("domain",this.textBox5.Text);
				Configuration.Save(conf);
				
				//七牛上传组件初始化
				Qiniu.Conf.Config.ACCESS_KEY= this.textBox1.Text;
				Qiniu.Conf.Config.SECRET_KEY = this.textBox2.Text;

				this.b1.Enabled = false ;
				this.timer1.Start() ;
				createMonitor(this.textBox4.Text);
				this.Visible = false ;
				this.notifyIcon1.Visible = true ;
			}
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			this.timer1.Stop();
			this.b1.Enabled = true ;
		}
		
		private void createMonitor(String path) {
			fsw = new FileSystemWatcher(path);
			fsw.IncludeSubdirectories = true;
			fsw.Created += fileCreate_EventHandle;
			fsw.EnableRaisingEvents = true;
			
		}
		
		private void fileCreate_EventHandle(Object sender, FileSystemEventArgs e)
		{
			//创建上传处理
			bool flag = false ;
			var policy = new PutPolicy(this.textBox3.Text, 3600);
			String upToken = policy.Token();
			PutExtra extra = new PutExtra();
			IOClient client = new IOClient();
			showToolTip("上传中...") ;
			try{
				PutRet ret = client.PutFile(upToken, e.Name, e.FullPath, extra);　　				
				showToolTip("上传完成，图片链接已复制到剪切板") ;
				flag = true ;
			}
			catch(Exception ex){
				MessageBox.Show(ex.ToString());
				showToolTip("上传失败，请检查网络和设置项") ;
			}
			if(flag){
				//Clipboard.SetText(this.textBox5 + "/" + e.Name);
				Thread thread = new Thread(() => Clipboard.SetText(this.textBox5.Text + "/" + e.Name));
				thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
				thread.Start(); 
				thread.Join();
			}
		}
		
		
		void NotifyIcon1Click(object sender, EventArgs e)
		{
			this.Visible = true ;
			this.WindowState = FormWindowState.Normal ;
			this.notifyIcon1.Visible = false ;
		}
		
		void MainFormSizeChanged(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				this.Visible = false ;
				this.notifyIcon1.Visible = true ;
			}
		}
		
		void showToolTip(String msg)
		{
			this.notifyIcon1.Visible = true ;
			this.notifyIcon1.ShowBalloonTip(2000, "温馨提示："
			                                , msg, ToolTipIcon.Info);
		}
		
	}
}
