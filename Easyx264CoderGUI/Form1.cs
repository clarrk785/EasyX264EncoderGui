﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tsanie.FlvBugger;

namespace Easyx264CoderGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnClearList_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cblCompeteAction.SelectedIndex = 0;
            cbVedioConfigTemplete.SelectedIndex = 0;
            txtAvsScript.Text = Resource1.AvsAlmostFilterTemplete.Replace("$avisynth_plugin$", Path.Combine(Application.StartupPath, "tools\\avsplugin"));
            cbMuxer.SelectedIndex = 0;
        }

        public static string FileExtension = ".avi|.mp4|.mkv|.wmv|.avs|.ts|.tp|.m2ts";
        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.Text = Path.GetDirectoryName(s[0]);
            }
            foreach (string path in s)
            {
                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    foreach (string p in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    //foreach (string p in FileUtility.GetFiles(path))
                    {
                        AddFileToList(p, path);
                    }
                }
                else
                {
                    AddFileToList(path, Path.GetDirectoryName(path));
                }
            }
        }

        private void AddFileToList(string path, string dir)
        {
            if (FileExtension.Split('|').Any(f => f.Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase)))
            {
                AddFileToListSkipExtgensionCheck(path, dir);
            }
        }

        private void AddFileToListSkipExtgensionCheck(string path, string dir)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = (listView1.Items.Count + 1).ToString();
            lvi.SubItems.Add(Path.GetFileName(path));//.Name = "FileName";
            lvi.SubItems.Add(Path.GetDirectoryName(path));//.Name = "Path";
            FileConfig fileConfig = new FileConfig();
            fileConfig.FullName = path;
            fileConfig.DirPath = dir;
            if (Path.GetExtension(path).Equals(".avs", StringComparison.OrdinalIgnoreCase))
            {
                fileConfig.InputType = InputType.AvisynthScriptFile;
                fileConfig.AvsFileFullName = path;
            }
            else
            {
                fileConfig.VedioFileFullName = path;
                fileConfig.AudioInputFile = path;
            }
            lvi.Tag = fileConfig;
            listView1.Items.Add(lvi);
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (File.GetAttributes(s[0]).HasFlag(FileAttributes.Directory))
            {
                ((TextBox)sender).Text = s[0];
            }
            else
            {
                ((TextBox)sender).Text = Path.GetDirectoryName(s[0]);
            }
        }

        private void textBoxFile_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            ((TextBox)sender).Text = s[0];

        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            FileDragEnter(e);
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            FileDragEnter(e);
        }

        private static void FileDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        //添加任务
        private void btnAddtoTasklist_Click(object sender, EventArgs e)
        {
            添加到任务列表();
        }

        private void 添加到任务列表()
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                FileConfig fileConfig = lvi.Tag as FileConfig;

                FillFileConfig(fileConfig);

            }
        }

        private void FillFileConfig(FileConfig fileConfigSrc)
        {
            FileConfig fileConfig = fileConfigSrc.Clone();
            fileConfig.OutputPath = textBox1.Text;
            fileConfig.CompleteDo = cbCompleteDo.Checked;
            fileConfig.CompleteAction = cblCompeteAction.Text;
            fileConfig.CompleteActionDir = textBox2.Text;
            fileConfig.KeepDirection = cbKeepFileTree.Checked;
            if (cbMuxer.Text == TextManager.zhalangflvmuxer)
            {
                fileConfig.Muxer = "flv";
                fileConfig.sinablack = true;
            }
            else if (cbMuxer.Text == TextManager.zhalangflvpreblack)
            {
                fileConfig.Muxer = "flv";
                fileConfig.sinaPreblack = true;
            }
            else
            {
                fileConfig.Muxer = cbMuxer.Text;
            }
            SetVedioConfigByControl(fileConfig);
            SetAudioConfigByControl(fileConfig);
            AddToTaskList(fileConfig);
        }

        private void SetAudioConfigByControl(FileConfig fileConfig)
        {

            AudioConfig audioConfig = fileConfig.AudioConfig;
            audioConfig.Enabled = cbUseAudio.Checked;
            audioConfig.Quality = float.Parse(txtQuality.Text);
            audioConfig.CopyStream = cbcopuaudio.Checked;
            if (fileConfig.InputType == InputType.AvisynthScriptFile && fileConfig.AudioInputFile == string.Empty)
            {
                fileConfig.AudioConfig.Enabled = false;
            }
        }

        private void SetVedioConfigByControl(FileConfig fileConfig)
        {
            VedioConfig vedioConfig = fileConfig.VedioConfig;
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                vedioConfig.crf = float.Parse(textBox3.Text);
                vedioConfig.BitType = EncoderBitrateType.crf;
            }
            else
            {
                vedioConfig.bitrate = int.Parse(txtbitrate.Text);
                vedioConfig.BitType = EncoderBitrateType.twopass;
            }
            vedioConfig.depth = int.Parse(cbColorDepth.Text);
            vedioConfig.preset = cbpreset.Text;
            vedioConfig.tune = comboBox1.Text;
            vedioConfig.UserArgs = txtUserArgs.Text;
            vedioConfig.Resize = checkBox1.Checked;
            vedioConfig.csp = cbcsp.Text;
            vedioConfig.Width = textBox4.Text == "" ? 0 : int.Parse(textBox4.Text);
            vedioConfig.Height = textBox5.Text == "" ? 0 : int.Parse(textBox5.Text);
            if (cbUseAvsTemplete.Checked)
            {//处理自定义avs模板
                fileConfig.InputType = InputType.AvisynthScript;
                string avsscript = txtAvsScript.Text;
                avsscript = avsscript.Replace("$InputVedio$", fileConfig.VedioFileFullName);
                vedioConfig.AvsScript = avsscript;
            }
        }

        private void AddToTaskList(FileConfig fileConfig)
        {
            ListViewItem lvi2 = new ListViewItem();
            lvi2.Text = (listView2.Items.Count + 1).ToString();
            lvi2.SubItems.Add(Path.GetFileName(fileConfig.FullName));//.Name = "FileName";
            lvi2.SubItems.Add("待转码").Name = "States";
            lvi2.SubItems.Add(fileConfig.DirPath);//.Name = "Path";
            lvi2.Tag = fileConfig;
            listView2.Items.Add(lvi2);
        }

        private void btnClearList_Click_1(object sender, EventArgs e)
        {
            listView1.Items.Clear();
        }

        private void 删除此任务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView2.SelectedItems)
            {
                if (hasHandle > -1)
                {
                    var fileconfig = (item.Tag as FileConfig);
                    if (fileconfig.state != -1)
                    {
                        hasHandle--;
                    }
                }
                listView2.Items.Remove(item);
            }
        }



        int hasHandle = -1;
        object handledlock = new object();
        private void button3_Click(object sender, EventArgs e)
        {
            开始转码();
        }

        private void 开始转码()
        {
            int threadcount = Convert.ToInt32(txtTaskCount.Text);
            Task.Factory.StartNew(delegate()
            {
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < threadcount; i++)
                {
                    var t = Task.Factory.StartNew(delegate()
                    {
                        StartOneThread();
                    });
                    tasks.Add(t);
                }
            });
        }
        private void StartOneThread()
        {
            int isHandling = -1;
            while (true)
            {
                int thisstate = -1;
                ListViewItem item = null;
                FileConfig fileConfig = null;
                try
                {

                    lock (handledlock)
                    {
                        if (isHandling >= listView2.Items.Count - 1)
                        {
                            return;
                        }
                        hasHandle++;
                        isHandling = hasHandle;

                        this.Invoke((Action)delegate()
                        {
                            item = listView2.Items[isHandling];
                            fileConfig = item.Tag as FileConfig;
                            if (fileConfig.state != -1)
                            {
                                thisstate = -10;
                            }
                            listView2.Items[isHandling].SubItems["States"].Text = "视频转码中";

                            fileConfig.state++;

                        });
                        if (thisstate == -10)
                        {
                            continue;
                        }

                        this.Invoke((Action)delegate()
                         {
                             if (fileConfig.InputType == InputType.AvisynthScriptFile || fileConfig.InputType == InputType.AvisynthScript)
                             {
                                 EncoderTaskInfoForm form = new EncoderTaskInfoForm();
                                 form.fileConfig = fileConfig;
                                 form.lbFile.Text = fileConfig.FullName;
                                 form.Text = fileConfig.FullName;
                                 fileConfig.EncoderTaskInfo.infoForm = form;
                                 form.Show();
                             }
                         });


                    }


                    string outputfile = "";
                    string copyto = string.Empty;
                    string ralative = string.Empty;
                    //仅输出视频部分
                    if (fileConfig.KeepDirection)
                    {//保持目录树结构
                        ralative = FileUtility.MakeRelativePath(fileConfig.DirPath + "/", Path.GetDirectoryName(fileConfig.FullName));
                        string outpath = Path.Combine(fileConfig.OutputPath, ralative);
                        outputfile = Path.Combine(outpath, Path.GetFileNameWithoutExtension(fileConfig.FullName));
                    }
                    else if (fileConfig.OutputPath != "")
                    {//有输出目录
                        outputfile = Path.Combine(fileConfig.OutputPath, Path.GetFileNameWithoutExtension(fileConfig.FullName));
                    }
                    else
                    {//输出原路径
                        outputfile = Path.Combine(Path.GetDirectoryName(fileConfig.FullName), Path.GetFileNameWithoutExtension(fileConfig.FullName));

                    }
                    if (fileConfig.CompleteDo && !string.IsNullOrEmpty(fileConfig.CompleteActionDir))
                    {
                        if (fileConfig.KeepDirection)
                        {//保持目录树结构 
                            copyto = Path.Combine(fileConfig.CompleteActionDir, ralative, Path.GetFileNameWithoutExtension(fileConfig.FullName));
                        }
                        else
                        {
                            copyto = Path.Combine(fileConfig.CompleteActionDir, Path.GetFileNameWithoutExtension(fileConfig.FullName));
                        }
                    }
                    else
                    {
                        fileConfig.CompleteDo = false;
                    }


                    fileConfig.OutputFile = outputfile;


                    VedioConfig vedioconfig = fileConfig.VedioConfig;
                    string vedioOutputFile = string.Empty;
                    try
                    {
                        if (fileConfig.InputType == InputType.Vedio)
                        {
                            vedioOutputFile = CommandHelper.RunX264Command(fileConfig);
                        }
                        else if (fileConfig.InputType == InputType.AvisynthScriptFile)
                        {
                            vedioOutputFile = CommandHelper.RunAvsx264mod(fileConfig);
                        }
                        else if (fileConfig.InputType == InputType.AvisynthScript)
                        {
                            string avsfilename = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".avs"));
                            File.WriteAllText(avsfilename, vedioconfig.AvsScript, System.Text.Encoding.Default);
                            fileConfig.AvsFileFullName = avsfilename;
                            fileConfig.InputType = InputType.AvisynthScriptFile;
                            vedioOutputFile = CommandHelper.RunAvsx264mod(fileConfig);
                        }
                    }
                    catch (EncoderException e)
                    {
                        this.Invoke((Action)delegate()
                        {
                            listView2.Items[isHandling].SubItems["States"].Text = e.Message;
                        });
                        fileConfig.state = -10;
                        continue;
                    }

                    if (!File.Exists(vedioOutputFile))
                    {
                        this.Invoke((Action)delegate()
                        {
                            listView2.Items[isHandling].SubItems["States"].Text = "视频编码失败";
                        });
                        fileConfig.state = -10;
                        continue;
                    }

                    if (fileConfig.AudioConfig.Enabled && fileConfig.state != 10)
                    {
                        if (fileConfig.InputType == InputType.Vedio && fileConfig.AudioConfig.CopyStream)
                        {
                            //直接由x264处理掉了
                        }
                        else
                        {
                            this.Invoke((Action)delegate()
                            {
                                item = listView2.Items[isHandling];
                                item.SubItems["States"].Text = "音频转码中";
                            });

                            string audiofile = string.Empty;
                            if (fileConfig.AudioConfig.CopyStream)
                            {
                                audiofile = CommandHelper.DemuxAudio(fileConfig);
                            }
                            else
                            {
                                audiofile = CommandHelper.RunFFmpegToAAC(fileConfig);
                            }


                            this.Invoke((Action)delegate()
                            {
                                item = listView2.Items[isHandling];
                                item.SubItems["States"].Text = "封装中";
                            });
                            int delay = 0;
                            MediaInfo mediainfo = new MediaInfo(fileConfig.VedioFileFullName);
                            delay = mediainfo.DelayRelativeToVideo;
                            delay = delay + delay / 3;
                            if (fileConfig.Muxer == "mkv")
                            {
                                vedioOutputFile = CommandHelper.MKVmergin(fileConfig, vedioOutputFile, audiofile, delay);
                            }
                            else if (fileConfig.Muxer == "mp4")
                            {
                                vedioOutputFile = CommandHelper.mp4box(fileConfig, vedioOutputFile, audiofile, delay);
                            }
                            else if (fileConfig.Muxer == "flv")
                            {
                                vedioOutputFile = CommandHelper.ffmpegmux(fileConfig, vedioOutputFile, audiofile, fileConfig.Muxer);
                                if (fileConfig.sinablack)
                                {
                                    FlvMain flvbugger = new FlvMain();
                                    flvbugger.addFile(vedioOutputFile);
                                    flvbugger.ExecuteBlack(1000d, -1, Path.ChangeExtension(vedioOutputFile, ".black.flv"));
                                }
                                else if (fileConfig.sinaPreblack)
                                {
                                    FlvMain flvbugger = new FlvMain();
                                    flvbugger.addFile(vedioOutputFile);
                                    flvbugger.ExecuteTime(1000d, -1, Path.ChangeExtension(vedioOutputFile, ".speed.flv"));
                                }


                            }
                        }

                    }

                    if (fileConfig.CompleteDo && fileConfig.state != 10)
                    {
                        try
                        {
                            copyto = copyto + Path.GetExtension(vedioOutputFile);
                            copyto = FileUtility.GetNoSameNameFile(copyto);
                            if (fileConfig.CompleteAction == "拷贝到")
                            {
                                this.Invoke((Action)delegate()
                                {
                                    listView2.Items[isHandling].SubItems["States"].Text = "拷贝中";
                                });

                                File.Copy(vedioOutputFile, copyto, true);

                            }
                            else if (fileConfig.CompleteAction == "剪切到")
                            {
                                this.Invoke((Action)delegate()
                                {
                                    listView2.Items[isHandling].SubItems["States"].Text = "剪切中";
                                });

                                File.Move(vedioOutputFile, copyto);
                            }
                        }
                        catch { }
                    }

                    this.Invoke((Action)delegate()
                    {
                        if (fileConfig.state == -10)
                        {
                            listView2.Items[isHandling].SubItems["States"].Text = "失败";
                        }
                        else
                        {
                            listView2.Items[isHandling].SubItems["States"].Text = "完成";
                        }

                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((Action)delegate()
                    {
                        listView2.Items[isHandling].SubItems["States"].Text = "失败：" + ex.Message;
                    });
                }

            }

        }

        private void 退出RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnOneclickStart_Click(object sender, EventArgs e)
        {
            添加到任务列表();
            tabControl1.SelectedIndex = 2;
            开始转码();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "视频文件|*.mp4;*.mkv;*.avi;*.ts;*.tp;*.ts;*.tp;*.m2ts|所有文件|*";
            var result = ofd.ShowDialog();


            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fullname in ofd.FileNames)
                {
                    AddFileToListSkipExtgensionCheck(fullname, Path.GetDirectoryName(fullname));
                }
            }

        }

        private void cbVedioConfigTemplete_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbVedioConfigTemplete.Text == "网络视频")
            {
                cbColorDepth.Text = "8";
                cbcsp.SelectedIndex = 0;
                txtUserArgs.Text = Resource1.TempleteOnline;
                txtbitrate.Text = "3510";
                textBox3.Text = "";
                cbpreset.Text = "fast";
            }
            else if (cbVedioConfigTemplete.Text == "高清视频")
            {
                cbColorDepth.Text = "8";
                cbcsp.SelectedIndex = 0;
                txtUserArgs.Text = Resource1.TempleteHDi420;
                textBox3.Text = "26";
                cbpreset.Text = "slow";
            }
            else if (cbVedioConfigTemplete.Text == "高保真游戏视频")
            {
                cbColorDepth.Text = "10";
                cbcsp.SelectedIndex = 2;
                txtUserArgs.Text = Resource1.TempleteGamei444;
                textBox3.Text = "25";
                cbpreset.Text = "slow";
            }
            else if (cbVedioConfigTemplete.Text == "战渣浪")
            {
                cbColorDepth.Text = "8";
                cbcsp.SelectedIndex = 0;
                txtUserArgs.Text = Resource1.TempleteGamei444;
                txtbitrate.Text = "2000";
                textBox3.Text = "";
                cbpreset.Text = "slow";
                cbMuxer.Text = TextManager.zhalangflvmuxer;
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            FileConfig fileConfig = new FileConfig();
            fileConfig.FullName = txtAvsFile.Text;
            fileConfig.DirPath = Path.GetDirectoryName(txtAvsFile.Text);
            if (Path.GetExtension(txtAvsFile.Text).Equals(".avs", StringComparison.OrdinalIgnoreCase))
            {
                fileConfig.InputType = InputType.AvisynthScriptFile;
            }
            fileConfig.AudioInputFile = txtAudioInput.Text;
            FillFileConfig(fileConfig);
        }


    }
}
