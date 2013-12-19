using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Doge
{
    public partial class DogeForm : Form
    {

        private readonly BackgroundWorker _bg;
        private readonly List<Casino> _casinos;

        public DogeForm()
        {
            
            InitializeComponent();
            _casinos = new List<Casino>();
            _bg = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = true };
            _bg.DoWork += GetGambleAddress;
            _bg.ProgressChanged += ReportProgress;
            _bg.RunWorkerCompleted += BgOnRunWorkerCompleted;
            var max = ConfigurationManager.AppSettings.AllKeys.Count(d => d.Contains("casino"));
            if (max == 0) return;
            for (var i = 0; i < max; i++)
            {
                var c = new Casino(ConfigurationManager.AppSettings[i]);
                _casinos.Add(c);
                comboBox1.Items.Add(c.Name);
            }
        }

        private void BgOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            button1.Text = "Many gamble";
            if (gambleAddress.Text != "")
                Clipboard.SetText(String.IsNullOrEmpty(gambleAddress.Text) ? "" : gambleAddress.Text);
        }

        private void GetAddressClick(object sender, EventArgs e)
        {
            button1.Text = "Much getting";
            if (_bg.IsBusy)
                _bg.CancelAsync();
            else
                _bg.RunWorkerAsync(new Tuple<string, string, string>((string)comboBox1.SelectedItem, (string)comboBox2.SelectedItem, addressTextBox.Text));
        }

        private void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState == null) return;
                gambleAddress.Text = e.UserState.ToString().Trim();
        }


        private void GetGambleAddress(object sender, DoWorkEventArgs e)
        {
            var tuple = e.Argument as Tuple<string,string,string>;
            if (tuple == null) return;
            var address = tuple.Item3;
            var option = tuple.Item2;
            var casinoName = tuple.Item1;
            
            
            var selected = _casinos.Find(d => d.Name == casinoName);
            if (selected == null)
            {
                MessageBox.Show("Very selection. Much invalid.");
                return;
            }
            var worker = sender as BackgroundWorker;
            if (worker == null) return;
            var request = (HttpWebRequest)WebRequest.Create(selected.RequestAddress);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            var formatted = String.Format(selected.RequestText, address, option);
            var byteVersion = Encoding.ASCII.GetBytes(formatted);
                
            request.ContentLength = byteVersion.Length;

            var stream = request.GetRequestStream();
            stream.Write(byteVersion, 0, byteVersion.Length);
            stream.Close();

            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null) return;
            string html;
            using (var reader = new StreamReader(responseStream))
                html = reader.ReadToEnd();
            if (html == "") return;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(selected.ParseText);
            if (nodes == null) return;
            worker.ReportProgress(0, nodes[0].InnerText.Trim());
        }

        private void DogeFormOnLoad(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            Casino c = null;
            foreach (var x in _casinos.Where(x => x.Name == comboBox1.SelectedItem.ToString()))
                c = x;
            if (c == null) return;
            foreach (var o in c.Options.Where(o => !String.IsNullOrEmpty(o)))
                comboBox2.Items.Add(o);

        }
    }
}
