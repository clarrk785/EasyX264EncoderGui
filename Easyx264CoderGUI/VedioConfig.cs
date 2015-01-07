using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easyx264CoderGUI
{
     [Serializable]
    public class VedioConfig
    {
        public EncoderBitrateType BitType = EncoderBitrateType.crf;
        public int bitrate = 3500;
        public float crf = 25f;
        public int depth = 10;
        public string preset = "slow";
        public string tune = "";
        public string UserArgs = "";
        public bool Resize = false;
        public int Width = 1920;
        public int Height = 1080;
        public string csp = "i420";
        public string AvsScript = "";//if inputtype == AvisynthScript

    }

    public enum EncoderBitrateType
    {
        crf,
        twopass,
        qp
    }

}
