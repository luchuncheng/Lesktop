using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace Update
{
    public partial class UpdateProgress : Form
    {
        string _url;

        public string Url
        {
            get { return _url; }
        }

        string _target;

        public string Target
        {
            get { return _target; }
        }

        public UpdateProgress(string url, string target)
        {
            _url = url;
            _target = target;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted+=new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            client.DownloadFileAsync(new Uri(Url), Target);
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = (int)(e.BytesReceived * 100 / e.TotalBytesToReceive);
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}