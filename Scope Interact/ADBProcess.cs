using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ScopeInteract
{
    public static class ADBProcess
    {
        public static List<int> pro = new List<int>();

        public static async void closeAllCon()
        {
            await ADBProcess.newCommand("adb kill-server");
            foreach (int p in pro)
            {
                KillProcessAndChildren(p);
            }
            pro.Clear();
            Infos._main.loaded = false;
        }

        private static void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (Exception)
            {
                // Process already exited.
            }
        }

        public static async Task<string> newCommand(string scmd)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            pro.Add(cmd.Id);
            cmd.StandardInput.WriteLine(scmd);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            await Task.Run(() => cmd.WaitForExit());
            string outp = await cmd.StandardOutput.ReadToEndAsync();
            string[] str = outp.Split(new string[] { Directory.GetCurrentDirectory() + ">" }, StringSplitOptions.RemoveEmptyEntries);
            return str[1].Replace(scmd + "\r\n", "");
        }
    }
}
