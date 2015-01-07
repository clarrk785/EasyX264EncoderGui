using Easyx264CoderGUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Easyx264CoderGUI
{
    public static class CommandHelper
    {
        public static string RunAvsx264mod(FileConfig fileConfig)
        {
            VedioConfig vedioConfig = fileConfig.VedioConfig;
            ProcessStartInfo processinfo = new ProcessStartInfo();
            processinfo.UseShellExecute = false;    //输出信息重定向
            processinfo.CreateNoWindow = true;
            processinfo.RedirectStandardInput = true;
            processinfo.RedirectStandardOutput = true;
            processinfo.RedirectStandardError = true;
            processinfo.WindowStyle = ProcessWindowStyle.Hidden;

            processinfo.FileName = Path.Combine(Application.StartupPath, "tools\\avs4x264mod.exe");
            if (!File.Exists(processinfo.FileName))
            {
                throw new EncoderException("找不到指定程序：" + processinfo.FileName);
            }

            string x264exe = GetX264exeFullName(vedioConfig);
            if (!File.Exists(x264exe))
            {
                throw new EncoderException("找不到指定程序：" + x264exe);
            }
            string x264Line;
            string outputpath = "";
            Process avsx264mod = new Process();
            if (vedioConfig.BitType == EncoderBitrateType.crf)
            {
                Getx264Line(fileConfig, 0, out x264Line, out outputpath);
                string avsx264modarg = string.Format("--x264-binary \"{0}\" ", Path.GetFileName(x264exe));
                processinfo.Arguments = avsx264modarg + x264Line;
                avsx264mod.StartInfo = processinfo;
                OutputToText(fileConfig, avsx264mod);
                avsx264mod.Start();
                avsx264mod.BeginOutputReadLine();
                avsx264mod.BeginErrorReadLine();
                avsx264mod.WaitForExit();
            }
            else if (vedioConfig.BitType == EncoderBitrateType.twopass)
            {
                Getx264Line(fileConfig, 1, out x264Line, out outputpath);
                string avsx264modarg = string.Format("--x264-binary \"{0}\" ", x264exe);
                processinfo.Arguments = avsx264modarg + x264Line;
                avsx264mod.StartInfo = processinfo;
                OutputToText(fileConfig, avsx264mod);
                avsx264mod.Start();
                avsx264mod.BeginOutputReadLine();
                avsx264mod.BeginErrorReadLine();
                avsx264mod.WaitForExit();

                Getx264Line(fileConfig, 2, out x264Line, out outputpath);
                avsx264modarg = string.Format("--x264-binary \"{0}\" ", x264exe);
                processinfo.Arguments = avsx264modarg + x264Line;
                avsx264mod.StartInfo = processinfo;
                OutputToText(fileConfig, avsx264mod);
                avsx264mod.Start();
                avsx264mod.BeginOutputReadLine();
                avsx264mod.BeginErrorReadLine();
                avsx264mod.WaitForExit();
            }

            avsx264mod.Dispose();
            return outputpath;

        }

        private static void OutputToText(FileConfig fileConfig, Process avsx264mod)
        {
            avsx264mod.OutputDataReceived += new DataReceivedEventHandler(delegate(object sender, DataReceivedEventArgs e)
            {
                fileConfig.EncoderTaskInfo.AppendOutput(e.Data);
            });
            avsx264mod.ErrorDataReceived += new DataReceivedEventHandler(delegate(object sender, DataReceivedEventArgs e)
            {
                //fileConfig.state = -10;
                //fileConfig.EncoderTaskInfo.AppendOutput("[简单批量x264编码]avs转码错误");
                fileConfig.EncoderTaskInfo.AppendOutput(e.Data);
            });
        }

        public static string RunX264Command(FileConfig fileConfig)
        {
            VedioConfig vedioConfig = fileConfig.VedioConfig;
            ProcessStartInfo processinfo = new ProcessStartInfo();

            string x264exe = GetX264exeFullName(vedioConfig);
            if (!File.Exists(x264exe))
            {
                throw new EncoderException("找不到指定程序：" + x264exe);
            }

            processinfo.FileName = x264exe;
            //processinfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
            string x264Line;
            string outputpath = ""; ;

            //processinfo.Arguments = "/c \"" + Path.Combine(Application.StartupPath, "tools\\avs4x264mod.exe") + "\" " + x264Line;
            //processinfo.UseShellExecute = false;    //输出信息重定向
            //processinfo.CreateNoWindow = true;
            //processinfo.RedirectStandardInput = true;
            //processinfo.RedirectStandardOutput = true;
            //processinfo.RedirectStandardError = false;
            //processinfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process avsx264mod = new Process();

            if (vedioConfig.BitType == EncoderBitrateType.crf)
            {
                Getx264Line(fileConfig, 0, out x264Line, out outputpath);
                processinfo.Arguments = x264Line;
                avsx264mod.StartInfo = processinfo;
                avsx264mod.Start();
                avsx264mod.WaitForExit();
            }
            else if (vedioConfig.BitType == EncoderBitrateType.twopass)
            {
                Getx264Line(fileConfig, 1, out x264Line, out outputpath);
                processinfo.Arguments = x264Line;
                avsx264mod.StartInfo = processinfo;
                avsx264mod.Start();
                avsx264mod.WaitForExit();

                Getx264Line(fileConfig, 2, out x264Line, out outputpath);
                processinfo.Arguments = x264Line;
                avsx264mod.StartInfo = processinfo;
                avsx264mod.Start();
                avsx264mod.WaitForExit();
            }

            avsx264mod.Dispose();
            return outputpath;

        }

        private static string GetX264exeFullName(VedioConfig vedioConfig)
        {
            string x264exe = "";
            if (vedioConfig.depth == 10)
            {
                x264exe = Path.Combine(Application.StartupPath, "tools\\x264_64_tMod-10bit-all.exe");
            }
            else
            {
                x264exe = Path.Combine(Application.StartupPath, "tools\\x264_64_tMod-8bit-all.exe");
            }
            if (!Environment.Is64BitOperatingSystem || !File.Exists(x264exe))
            {
                if (vedioConfig.depth == 10)
                {
                    x264exe = Path.Combine(Application.StartupPath, "tools\\x264_32_tMod-10bit-all.exe");
                }
                else
                {
                    x264exe = Path.Combine(Application.StartupPath, "tools\\x264_32_tMod-8bit-all.exe");
                }
            }
            return x264exe;
        }

        private static void Getx264Line(FileConfig fileConfig, int pass, out string x264Line, out string outputpath)
        {
            VedioConfig vedioConfig = fileConfig.VedioConfig;
            x264Line = Resource1.x264Line;
            x264Line = x264Line.Replace("$preset$", vedioConfig.preset);
            if (string.IsNullOrEmpty(vedioConfig.tune))
            {
                x264Line = x264Line.Replace("$tune$", "");
            }
            else
            {
                x264Line = x264Line.Replace("$tune$", "--tune " + vedioConfig.tune);
            }
            if (vedioConfig.BitType == EncoderBitrateType.crf)
            {
                x264Line = x264Line.Replace("$crf$", "--crf " + vedioConfig.crf.ToString());
            }
            else
            {
                string twopassstr = "--pass " + pass + " --bitrate " + vedioConfig.bitrate.ToString();
                x264ArgsManager manager = new x264ArgsManager(x264Line);
                //提供索引
                if (!string.IsNullOrEmpty(fileConfig.VedioFileFullName))
                    if (File.Exists(fileConfig.VedioFileFullName + ".lwi") && manager.GetArgValue("demuxer") == "lavf")
                    {
                        twopassstr += " --index \"" + fileConfig.VedioFileFullName + ".lwi\" ";
                    }
                    else if (File.Exists(fileConfig.VedioFileFullName + ".ffindex") && manager.GetArgValue("demuxer") == "ffms")
                    {
                        twopassstr += " --index \"" + fileConfig.VedioFileFullName + ".ffindex\" ";
                    }

                x264Line = x264Line.Replace("$crf$", twopassstr);
            }
            if (fileConfig.AudioConfig.Enabled && fileConfig.AudioConfig.CopyStream)
            {
                x264Line = x264Line.Replace("$acodec$", "copy");
            }
            else
            {
                x264Line = x264Line.Replace("$acodec$", "none");
            }
            x264Line = x264Line.Replace("$csp$", vedioConfig.csp);

            x264Line = x264Line.Replace("$resize$", vedioConfig.Resize ? string.Format("--vf resize:width={0},height={1},method=lanczos", vedioConfig.Width, vedioConfig.Height) : "");
            outputpath = string.Empty;

            string fileExtension = "." + fileConfig.Muxer;

            if (fileConfig.AudioConfig.CopyStream || !fileConfig.AudioConfig.Enabled)
            {
                outputpath = fileConfig.OutputFile + fileExtension;
            }
            else
            {//临时目录
                outputpath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".mp4"));

            }
            if (fileConfig.InputType == InputType.AvisynthScriptFile)
            {
                x264Line = x264Line.Replace("$input$", fileConfig.AvsFileFullName);
            }
            else
            {
                x264Line = x264Line.Replace("$input$", fileConfig.VedioFileFullName);
            }
            x264Line = x264Line.Replace("$outputfile$", outputpath);

            string log = "--log-file \"" + Path.GetFileNameWithoutExtension(fileConfig.FullName) + "_x264.log\" --log-file-level info ";
            if (fileConfig.InputType == InputType.AvisynthScriptFile || fileConfig.InputType == InputType.AvisynthScript ||
               fileConfig.VedioConfig.BitType == EncoderBitrateType.twopass)
            {
                vedioConfig.UserArgs = vedioConfig.UserArgs.Replace("--demuxer lavf", "");
            }
            x264Line = x264Line.Replace("$userargs$", log + vedioConfig.UserArgs);
        }



        public static string RunFFmpegToAAC(FileConfig fileConfig)
        {
            AudioConfig audioConfig = fileConfig.AudioConfig;
            ProcessStartInfo processinfo = new ProcessStartInfo();
            string tmp = Path.GetTempFileName();
            string audiofile = Path.Combine(Path.GetDirectoryName(tmp), Path.GetFileNameWithoutExtension(tmp) + ".aac");
            processinfo.FileName = Environment.GetEnvironmentVariable("ComSpec");

            string bat = string.Empty;
            if (fileConfig.InputType == InputType.AvisynthScriptFile)
            {
                bat = getAudiobat(fileConfig.AudioInputFile, audiofile, audioConfig);
            }
            else
            {
                bat = getAudiobat(fileConfig.AudioInputFile, audiofile, audioConfig);
            }

            processinfo.Arguments = "/c " + bat;
            processinfo.UseShellExecute = false;    //输出信息重定向
            processinfo.CreateNoWindow = true;
            processinfo.RedirectStandardInput = true;
            processinfo.RedirectStandardOutput = true;
            processinfo.RedirectStandardError = false;
            processinfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process ffmpeg = new Process();
            ffmpeg.StartInfo = processinfo;
            ffmpeg.Start();

            var result = ffmpeg.StandardOutput.ReadToEnd();
            ffmpeg.WaitForExit();
            //ffmpeg.Kill();//等待进程结束
            ffmpeg.Dispose();

            return audiofile;
        }

        private static string getAudiobat(string input, string output, AudioConfig audioconfig)
        {
            string ffmpegfile = Path.Combine(Application.StartupPath, "tools\\ffmpeg.exe");
            string neroAacEncfile = Path.Combine(Application.StartupPath, "tools\\neroAacEnc.exe");
            return string.Format("tools\\ffmpeg.exe -vn -i \"{0}\" -f  wav pipe:| tools\\neroAacEnc -ignorelength -q {2} -lc -if - -of \"{1}\"",
                input, output, audioconfig.Quality);
        }



        public static string MKVmergin(FileConfig fileConfig, string vedio, string audio, int delay = 0)
        {
            string outfile = FileUtility.GetNoSameNameFile(fileConfig.OutputFile + ".mkv");
            ProcessStartInfo processinfo = new ProcessStartInfo();
            processinfo.FileName = Path.Combine(Application.StartupPath, "tools\\mkvmerge.exe");
            processinfo.Arguments = string.Format(" -o \"{0}\" \"{1}\" {3} \"{2}\"",
                outfile, vedio, audio, delay == 0 ? "" : ("--sync 0:" + delay)
                );
            processinfo.UseShellExecute = false;    //输出信息重定向
            processinfo.CreateNoWindow = true;
            processinfo.RedirectStandardInput = false;
            processinfo.RedirectStandardOutput = false;
            processinfo.RedirectStandardError = false;
            processinfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process avsx264mod = new Process();
            avsx264mod.StartInfo = processinfo;
            avsx264mod.Start();

            avsx264mod.WaitForExit();
            avsx264mod.Dispose();
            return outfile;
        }



        public static string ffmpegmux(FileConfig fileConfig, string vedio, string audio, string extension)
        {
            string outfile = FileUtility.GetNoSameNameFile(fileConfig.OutputFile + "." + extension);
            ProcessStartInfo processinfo = new ProcessStartInfo();
            processinfo.FileName = Path.Combine(Application.StartupPath, "tools\\ffmpeg.exe");
            processinfo.Arguments = string.Format("-y -i \"{1}\" -i \"{2}\" -vcodec copy -acodec copy \"{0}\"",
                outfile, vedio, audio
                );
            processinfo.UseShellExecute = false;    //输出信息重定向
            processinfo.CreateNoWindow = true;
            processinfo.RedirectStandardInput = true;
            processinfo.RedirectStandardOutput = true;
            processinfo.RedirectStandardError = false;
            processinfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process ffmpeg = new Process();
            ffmpeg.StartInfo = processinfo;
            ffmpeg.Start();

            ffmpeg.WaitForExit();
            ffmpeg.Dispose();
            return outfile;
        }

        /// <summary>
        /// 无损提取音频
        /// </summary>
        internal static string DemuxAudio(FileConfig fileConfig)
        {
            string tmp = Path.GetRandomFileName();
            string audiofile = Path.Combine(Path.GetDirectoryName(tmp), Path.ChangeExtension(tmp, ".mp4"));
            ProcessStartInfo processinfo = new ProcessStartInfo();
            processinfo.FileName = Path.Combine(Application.StartupPath, "tools\\ffmpeg.exe");
            processinfo.Arguments = string.Format(" -i \"{0}\" -vn -acodec copy \"{1}\"",
                fileConfig.AudioInputFile, audiofile
                );
            processinfo.UseShellExecute = false;    //输出信息重定向
            processinfo.CreateNoWindow = true;
            processinfo.RedirectStandardInput = true;
            processinfo.RedirectStandardOutput = true;
            processinfo.RedirectStandardError = false;
            processinfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process ffmpeg = new Process();
            ffmpeg.StartInfo = processinfo;
            ffmpeg.Start();
            ffmpeg.WaitForExit();
            ffmpeg.Dispose();
            return audiofile;
        }

        #region Mp4Box
        public static string mp4box(FileConfig fileConfig, string vedio, string audio, int audiodelay = 0)
        {
            string outfile = FileUtility.GetNoSameNameFile(fileConfig.OutputFile + ".mp4");
            ProcessStartInfo processinfo = new ProcessStartInfo();
            processinfo.FileName = Path.Combine(Application.StartupPath, "tools\\mp4box.exe");
            processinfo.Arguments = string.Format("-add \"{1}\" -add \"{2}\" {3} \"{0}\"",
                outfile, vedio, audio, audiodelay == 0 ? "" : ("-delay 2=" + audiodelay)
                );
            processinfo.UseShellExecute = false;    //输出信息重定向
            processinfo.CreateNoWindow = true;
            processinfo.RedirectStandardInput = false;
            processinfo.RedirectStandardOutput = false;
            processinfo.RedirectStandardError = false;
            processinfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process ffmpeg = new Process();
            ffmpeg.StartInfo = processinfo;
            ffmpeg.Start();

            ffmpeg.WaitForExit();
            ffmpeg.Dispose();
            return outfile;
        }
        #endregion
    }
}
