using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BEBanChecker
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            fillListBox();

            listSteamIDs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listSteamIDs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        public void fillListBox()
        {
            string path;
            if (tryFindSteamConfig(out path))
            {
                Dictionary<string, string> users = parseSteamConfig(path);
                foreach (KeyValuePair<string, string> kvp in users)
                {
                    listSteamIDs.Items.Add(new ListViewItem(new string[] { kvp.Value, Program.CheckKey(kvp.Key), kvp.Key }));
                }
            }
        }

        private Dictionary<string, string> parseSteamConfig(string configPath)
        {
            Dictionary<string, string> user = new Dictionary<string, string>();
            StreamReader file = new StreamReader(configPath);
            String text = null;
            String line;

            int i = 0;
            int k = 0;

            while ((line = file.ReadLine().Trim()) != null)
            {
                if (line.Contains("Accounts"))
                {
                    text = line + "\n";
                }
                else if (text != null)
                {
                    text += line + "\n";
                    if (line.Contains("{")) i++;
                    if (line.Contains("}")) k++;
                    if (i == k)
                    {
                        break;
                    }
                }
            }

            file.Close();

            var regex = new Regex(@"[^\S\r\n]+|.*Accounts.*|.*}.*|.*{.*", RegexOptions.Multiline);
            text = regex.Replace(text, "");
            text = text.Replace("\"SteamID\"", "");
            text = text.Replace("\"", "");


            string[] userIDArray = text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (userIDArray.Length < 2 || userIDArray.Length % 2 != 0)
                return user;

            for (i = 1; i < userIDArray.Length; i = i + 2)
            {
                string name = userIDArray[i - 1];
                string id = userIDArray[i];
                user.Add(id, name);
            }

            return user;
        }

        private bool tryFindSteamConfig(out string path)
        {
            string configPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", string.Empty);
            configPath += @"\config\config.vdf";
            if (File.Exists(configPath))
            {
                path = configPath;
                return true;
            }
            path = "";
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            About aboutForm = new About();
            aboutForm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.TextLength == 17)
                MessageBox.Show("Steam ID: "
                                + textBox1.Text
                                + "\nGUID: "
                                + Program.CreateBEGuid(Convert.ToInt64(textBox1.Text))
                                + "\nStatus: "
                                + Program.CheckKey(textBox1.Text), "Status");
            //else
            //    textBox1.Select();
        }
    }
}
