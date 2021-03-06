﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HashKingsMaster
{
    public partial class frmMain : Form
    {
        HttpListener HttpServ;
        string PhpCompilerPath = @"php\php.exe";
        public frmMain()
        {
            InitializeComponent();
            HttpServ = new HttpListener();
        }

        

        public string ProcessPhpPage(string phpCompilerPath, string pageFileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = phpCompilerPath;
            proc.StartInfo.Arguments = "-d \"display_errors=1\" -d \"error_reporting=E_PARSE\" \"" + pageFileName + "\"";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            string res = proc.StandardOutput.ReadToEnd();
            if (string.IsNullOrEmpty(res))
            {
                res = proc.StandardError.ReadToEnd();
                res = "<h2 style=\"color:red;\">Error!</h2><hr/> <h4>Error Details :</h4> <pre>" + res + "</pre>";
                proc.StandardError.Close();
            }
            if (res.StartsWith("\nParse error: syntax error"))
                res = "<h2 style=\"color:red;\">Error!</h2><hr/> <h4>Error Details :</h4> <pre>" + res+"</pre>";


            proc.StandardOutput.Close();
           
            proc.Close();
            return res;
        }

        private async void btnStartHttpServ_Click(object sender, EventArgs e)
        {
            if (HttpServ.IsListening)
            {
                try
                {
                    HttpServ.Stop();
                    // HttpServ.Abort();
                }
                catch
                {
                    txtHttpLog.Text += "Server has been Shutdown.\r\n";
                }
                btnStartHttpServ.Text = "Start Server";
            }
            else
            {
                await StartServer();
            }
            
                
            
        }

        private async Task StartServer()
        {
            HttpServ = new HttpListener();
            btnStartHttpServ.Text = "Stop Server";
            HttpServ.Prefixes.Add("http://+:" + txtHttpPort.Value.ToString() + "/");
            HttpServ.Start();
            txtHttpLog.Text += "Start Listining on Port :" + txtHttpPort.Value + "\r\n";
            txtHttpLog.Text += "Server is Running...\r\n\r\n";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                try
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        txtHttpLog.Text += "Local master IP is " + ip.ToString() + "\r\n";
                    }
                    
                }
                catch
                {
                    throw new Exception("No network adapters with an IPv4 address in the system!");
                }
            }
            txtHttpLog.Text += "\r\n"; txtHttpLog.Text += "\r\n";
            while (true)
            {
                try
                {
                    var ctx = await HttpServ.GetContextAsync();


                    txtHttpLog.Text += "Request: " + ctx.Request.Url.AbsoluteUri + "\r\n";
                    var page = Application.StartupPath + "/" + ctx.Request.Url.LocalPath;
                    //txtHttpLog.Text += ctx.Request.Url.LocalPath + "\r\n";
                    if (ctx.Request.Url.LocalPath is null)
                        page = "index.php";
                    if (ctx.Request.Url.LocalPath is "/")
                        page = "index.php";

                    if (File.Exists(page))
                    {
                        string file;
                        var ext = new FileInfo(page);
                        if (ext.Extension == ".php")
                        {
                            file = ProcessPhpPage(PhpCompilerPath, page);
                            txtHttpLog.Text += "Processing php page...\r\n";
                        }
                        else
                        {
                            file = File.ReadAllText(page);
                        }

                        await ctx.Response.OutputStream.WriteAsync(ASCIIEncoding.UTF8.GetBytes(file), 0, file.Length);
                        ctx.Response.OutputStream.Close();
                        ctx.Response.Close();
                        txtHttpLog.Text += "Status 200 Fetch OK\r\n\r\n";

                    }

                   

                    else
                    {
                        ctx.Response.StatusCode = 404;
                        var file = "404 File Not Found !!!";
                        await ctx.Response.OutputStream.WriteAsync(ASCIIEncoding.UTF8.GetBytes(file), 0, file.Length);
                        ctx.Response.OutputStream.Close();

                        ctx.Response.Close();
                        txtHttpLog.Text += "API Not Found AKA 404 Did the URL include index.php?\r\n\r\n";
                        

                    }
                }
                catch (Exception ex)
                {
                    txtHttpLog.Text += "\r\nException: Server Stopped!!!\r\n\r\n";
                   // txtHttpLog.Text += "\r\nException: " + ex.Message + "\r\n\r\n";
                    break;
                }
                //txtHttpLog.Select(0, 0);
            }


        }

        private void lnkSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://hash-kings.com");
        }

    }
}
