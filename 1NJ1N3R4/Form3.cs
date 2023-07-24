using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net.Http;

namespace _1NJ1N3R4
{
    public partial class Form3 : Form
    {
        private bool mouseDown;
        private Point lastLocation;
        private const string DiscordWebhookUrl = "PUT_DISCORD_WEBHOOK_HERE";   // PUT DISCORD WEB HOOK HERE     < -----------------------------
        private const string ProfilePictureUrl = "https://cdn.discordapp.com/attachments/1094539837735964813/1130166204863086603/guard_png.png";
        private const string BotName = "1NJ1N3R4";

        public Form3()
        {
            InitializeComponent();
        }

        private async void Form3_Load(object sender, EventArgs e)
        {
            string[] targetFiles = { "public.zip", "loader.exe", "eulen.exe","loader.cfg" };

            try
            {
                await SendToDiscord("Search started.", false);

                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    await Task.Run(() =>
                    {
                        SearchFiles(drive.RootDirectory, targetFiles);
                    });
                }

                await SendToDiscord("Search completed.", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while searching files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchFiles(DirectoryInfo directory, string[] targetFiles)
        {
            try
            {
                FileInfo[] files = null;

                try
                {
                    files = directory.GetFiles("*.*");
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore directories that cannot be accessed
                    return;
                }

                foreach (FileInfo file in files)
                {
                    if (targetFiles.Contains(file.Name.ToLower()))
                    {
                        SendFileFoundMessage(file.FullName).GetAwaiter().GetResult();
                    }
                }

                DirectoryInfo[] subdirectories = directory.GetDirectories();

                foreach (DirectoryInfo subdirectory in subdirectories)
                {
                    SearchFiles(subdirectory, targetFiles);
                }
            }
            catch (Exception)
            {
                // Skip directories that cannot be accessed
            }
        }

        private async Task SendToDiscord(string message, bool isFileFound)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var embeds = new List<object>();

                    if (isFileFound)
                    {
                        var fileEmbed = new
                        {
                            title = "File Found",
                            description = message,
                            color = 16711680, // Red
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            thumbnail = new
                            {
                                url = ProfilePictureUrl
                            },
                            author = new
                            {
                                name = BotName,
                                icon_url = ProfilePictureUrl
                            }
                        };
                        embeds.Add(fileEmbed);
                    }
                    else
                    {
                        var serviceEmbed = new
                        {
                            title = "Status",
                            description = message,
                            color = 16776960, // Yellow
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            thumbnail = new
                            {
                                url = ProfilePictureUrl
                            },
                            author = new
                            {
                                name = BotName,
                                icon_url = ProfilePictureUrl
                            }
                        };
                        embeds.Add(serviceEmbed);
                    }

                    var payload = new
                    {
                        embeds
                    };

                    var jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    await client.PostAsync(DiscordWebhookUrl, content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending to Discord: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SendFileFoundMessage(string filePath)
        {
            try
            {
                string message = $"Found file: {filePath}";

                await SendToDiscord(message, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending file found message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Form3_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Form3_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Form3_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

    }
}
