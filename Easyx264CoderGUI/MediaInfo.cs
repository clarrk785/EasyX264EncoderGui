using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Easyx264CoderGUI
{
    public class MediaInfo
    {
        public string MediaInfoText { set; get; }
        public MediaInfo(string filename)
        {
            ProcessStartInfo processinfo = new ProcessStartInfo();
            string mediainfofile = Path.Combine(Application.StartupPath, "tools\\mediainfo.exe");
            processinfo.FileName = mediainfofile;
            processinfo.Arguments = "\"" + filename + "\"";
            processinfo.UseShellExecute = false;    //输出信息重定向
            processinfo.CreateNoWindow = true;
            processinfo.RedirectStandardInput = true;
            processinfo.RedirectStandardOutput = true;
            processinfo.RedirectStandardError = false;
            processinfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process mediainfo = new Process();
            mediainfo.StartInfo = processinfo;
            mediainfo.Start();
            MediaInfoText = mediainfo.StandardOutput.ReadToEnd();
            mediainfo.Dispose();
        }

        public int DelayRelativeToVideo
        {
            get
            {
                string delayStr = GetValueByText("Delay relative to video").Replace("ms", "");
                int delay = 0;
                int.TryParse(delayStr, out delay);
                return delay;
            }
        }

        public string GetValueByText(string key)
        {
            return Regex.Match(MediaInfoText, string.Format(@"{0}\s*:\s(?<Value>.*?)\r", key)).Groups["Value"].Value;
        }
    }
}
