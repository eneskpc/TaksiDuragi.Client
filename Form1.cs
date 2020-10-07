using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TakipsiClient.Models;

namespace TakipsiClient
{
    public partial class Form1 : Form
    {
        HubConnection connection;

        [DllImport("cid.dll", EntryPoint = "CidData", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public static extern string CidData();

        [DllImport("cid.dll", EntryPoint = "CidStart")]
        public static extern string CidStart();

        [DllImport("cid.dll", EntryPoint = "CidStop")]
        public static extern string CidStop();

        public Form1()
        {
            InitializeComponent();

            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:44346/Caller-Hub")
                .Build();

            connection.Closed += ConnectionListener;

            connection.Reconnected += async (selam) =>
            {
                MessageBox.Show(selam);
            };

            connection.StartAsync();

            ConnectionListener();
        }

        private async Task ConnectionListener(Exception exp = null)
        {
            while (connection.State != HubConnectionState.Connected)
            {
                if (connection.State == HubConnectionState.Disconnected)
                {
                    connection.StartAsync();
                }
                await Task.Delay(3000);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;

            CidStart();
            timer1.Start();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.BalloonTipTitle = "Takipsi Aktif";
                notifyIcon1.BalloonTipText = "Çağrı hareketlerinizi takipsi.com üzerinden takip edebilirsiniz.";
                notifyIcon1.ShowBalloonTip(1000);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string tempData = CidData();
            if (!string.IsNullOrEmpty(tempData))
            {
                string[] tempDataArray = tempData.Split(',');

                CallerInfo callerInfo = new CallerInfo();
                callerInfo.DeviceSerialNumber = tempDataArray[0];
                callerInfo.LineNumber = tempDataArray[1];
                callerInfo.CallerNumber = tempDataArray[2];

                string[] callDateTime = tempDataArray[3].Split(' ');
                string[] callDate = callDateTime[0].Split('-');
                string[] callTime = callDateTime[1].Split(':');

                if (int.TryParse(callDate[1], out int month) &&
                    int.TryParse(callDate[0], out int day) &&
                    int.TryParse(callTime[0], out int hour) &&
                    int.TryParse(callTime[1], out int minute))
                {
                    callerInfo.CallDateTime = new DateTime(
                        year: DateTime.Now.Year,
                        month: month > 0 ? month : 1,
                        day: day > 0 ? day : 1,
                        hour: hour > 0 ? hour : 1,
                        minute: minute > 0 ? minute : 1,
                        second: 0);
                }
                else
                {
                    callerInfo.CallDateTime = DateTime.Now;
                }

                try
                {
                    connection.InvokeAsync("SendCallerInfo", callerInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Arayan bilgisi gönderilirken bir hata oluştu", MessageBoxButtons.OK);
                }
            }
        }
    }
}
