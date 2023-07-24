using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net;
using Newtonsoft.Json;

namespace _1NJ1N3R4
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();
        private string randomCode; // Store the generated random code

        public Form1()
        {
            InitializeComponent();
            pcname.Text = Environment.MachineName;
            textBox1.BackColor = Color.FromArgb(114, 24, 42);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async Task<string> GetIPAddress()
        {
            string ipAddress = string.Empty;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync("https://api.ipify.org");
                    ipAddress = response.Trim();

                    // Replace the remaining part of the IP address with asterisks
                    int dotIndex = ipAddress.IndexOf('.');
                    if (dotIndex >= 0)
                    {
                        ipAddress = ipAddress.Substring(0, dotIndex + 1) + "##.##.##";
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs while fetching the IP address
                Console.WriteLine($"Failed to fetch IP address: {ex.Message}");
            }

            return ipAddress;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            string webhookUrl = "PUT_DISCORD_WEBHOOK_HERE";   // PUT DISCORD WEB HOOK HERE     < -----------------------------

            Random random = new Random();
            randomCode = GenerateRandomCode(); // Generate a random code
            string ipAddress = await GetIPAddress();
            string message = $"👨‍💻 `{randomCode}` 👨‍💻\n💻 (PC: {Environment.MachineName}) 💻\n🌐 (IP: {ipAddress}) 🌐";

            Task.Run(() => SendWebhook(webhookUrl, message));
        }

        private async Task SendWebhook(string url, string message)
        {
            var payload = new
            {
                username = "1NJ1N3R4",
                avatar_url = "https://cdn.discordapp.com/attachments/1094539837735964813/1130166204863086603/guard_png.png",
                embeds = new[]
                {
            new
            {
                title = "1NJ1N3R4",
                description = message
            }
        }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            await client.PostAsync(url, content);

            // Check if textBox1 contains the same code as sent in the HTTP request
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action(() => CheckTextBoxValue(textBox1.Text.Trim())));
            }
            else
            {
                CheckTextBoxValue(textBox1.Text.Trim());
            }
        }



        private void CheckTextBoxValue(string userInput)
        {
            if (userInput == randomCode)
            {
                Form2 form2 = new Form2();
                form2.Show();
                this.Hide();
            }
        }

        private string GenerateRandomCode()
        {
            // Generate a random 6-digit code
            Random random = new Random();
            int code = random.Next(100000, 999999);
            return code.ToString();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                CheckTextBoxValue(textBox1.Text.Trim());
            }
        }

        private bool mouseDown;
        private Point lastLocation;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
    }
}
