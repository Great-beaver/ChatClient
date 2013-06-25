using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AxAXISMEDIACONTROLLib;
using DevExpress.XtraEditors;

namespace Server
{
    public partial class VideoWindow : DevExpress.XtraEditors.XtraForm
    {
        public VideoWindow(VideoInfo videoInfo)
        {
            InitializeComponent();
            _videoInfo = videoInfo;
            AMC = new AxAxisMediaControl();
            AMC.Tag = videoInfo.Number;
            AMC.OnDoubleClick += (s, ea) =>
            {
                if (s is AxAxisMediaControl)
                {
                    var amc = s as AxAxisMediaControl;
                    amc.FullScreen = !amc.FullScreen;
                }
            };

            this.Text = "Video window № " + videoInfo.Number;

        }

        private AxAxisMediaControl AMC;
        private VideoInfo _videoInfo;

        private void VideoWindow_Shown(object sender, EventArgs e)
        {
            this.Controls.Add(AMC);
            AMC.MediaURL = _videoInfo.MediaUrl;
            AMC.MediaType = _videoInfo.MediaType;
            AMC.MediaUsername = _videoInfo.MediaUsername;
            AMC.MediaPassword = _videoInfo.MediaPassword;
            AMC.StretchToFit = true;
            AMC.MaintainAspectRatio = true;
            AMC.BackgroundColor = 2960685;
            AMC.EnableReconnect = true;
            AMC.SetReconnectionStrategy(300000, 20000, 1800000, 60000, 10800000, 300000, true);
            AMC.Dock = DockStyle.Fill;
            AMC.Play();
        }
    }
}