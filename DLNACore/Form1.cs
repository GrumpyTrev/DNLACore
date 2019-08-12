using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace DLNACore
{
    //Test program for SSDP and DLNADevices from RNCDesktop@Safe-mail.net or see  http://www.codeproject.com/Articles/893791/DLNA-made-easy-with-Play-To-from-any-device
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void CmdSSDP_Click(object sender, EventArgs e)
        {
            DLNA.SSDP.Start();//Start a service as this will take a long time
            Thread.Sleep(14000);//Wait for each TV/Device to reply to the broadcast
            DLNA.SSDP.Stop();//Stop the service if it has not stopped already
            this.textBox1.Text = DLNA.SSDP.Servers;//Best to save this string to a file or windows registry as we don't want to keep looking for devices on the network
            if (this.textBox1.Text.Length < 10)
                this.textBox1.Text = "Are you sure that your smart TV and devices are turned on !";
            else
                CmdPlay.Enabled = true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void CmdPlay_Click(object sender, EventArgs e)
        { //IMPORTANT: This will not work because i don't know where you host your music or movie files but this is how you would use the code to play the same track to all your devices in the house
            foreach (string DeviceUrl in DLNA.SSDP.Servers.Split(' '))
            {
                DLNA.DLNADevice Device = new DLNA.DLNADevice(DeviceUrl);//You will need to Keep a referance to each device so that you can stop it playing or what ever and don't need to keep calling "IsConnected();"
                if (Device.IsConnected())//Will make sure that the device is switched on and runs a avtransport:1 service protocol
                {//Best to use an IP-Address and not a machine name because the TV/Device might not be able to resolve a machine/domain name
                    string Reply = Device.TryToPlayFile("http://192.168.1.9:80/RemoteMusic/Devo/Freedom%20of%20Choice/1.%20Girl%20U%20Want.mp3");
                    if (Reply == "OK")//The above mps is hosted on a IIS-7 web-site on port 8080 but some devices might also work with a file on a mapped drive
                        this.textBox1.Text += Environment.NewLine + "Playing to " + Device.FriendlyName;
                    else
                        this.textBox1.Text += Environment.NewLine + "#ERROR# Playing to " + Device.FriendlyName + " Edit the code to fix";

                }
                //Shown below is sample code for adding music tracks to a playlist and to then poll the TV/Device to play the queued play-list tracks.
                //bool NewTrackPlaying=false;
                //D.AddToQueue("http://192.168.0.10/Music/Somefile1.mp3", ref NewTrackPlaying);//NewTrackPlaying returns as true if first track started OK
                //D.AddToQueue("http://192.168.0.10/Music/Somefile2.mp3", ref NewTrackPlaying);//Queue up our second track
                //D.AddToQueue("http://192.168.0.10/Music/Somefile3.mp3", ref NewTrackPlaying);//Queue up our last track
                //while (true)
                //{//Now we keep polling the TV/Device to start the next track just as the previous track ends
                //    Thread.Sleep(5000);
                //    D.PlayNextQueue(false);//Set the flag to true if called from a "Next Track" button event
                //}
            }
        }
    }
}
