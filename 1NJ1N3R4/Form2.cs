using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace _1NJ1N3R4
{
    public partial class Form2 : Form
    {
        private readonly long[] TargetFileSizes = { 7072768, 16529408, 5794816, 1776128, 3143864, 9692176, 5707776, 12406784, 5764096, 16643584 };
        private const string DiscordWebhookUrl = "PUT_DISCORD_WEBHOOK_HERE";   // PUT DISCORD WEB HOOK HERE     < -----------------------------
        private const string ProfilePictureUrl = "https://cdn.discordapp.com/attachments/1094539837735964813/1130166204863086603/guard_png.png";
        private const string BotName = "1NJ1N3R4";

        private readonly string[] TargetServices = {
            "SysMain",
            "PcaSvc",
            "DPS",
            "DusmSvc",
            "Eventlog",
            "Appinfo",
            "Bam",
            "CdpUserService_"
        };

        public Form2()
        {
            InitializeComponent();
        }

        private DateTime GetRecycleBinLastModifiedDate()
        {
            try
            {
                Guid recycleBinGuid = new Guid("B7534046-3ECB-4C18-BE4E-64CD4CB7D6AC"); // Recycle Bin folder GUID
                IntPtr pPath = IntPtr.Zero;

                int result = SHGetKnownFolderPath(ref recycleBinGuid, 0, IntPtr.Zero, out pPath);
                if (result == 0)
                {
                    string recycleBinPath = Marshal.PtrToStringUni(pPath);
                    Marshal.FreeCoTaskMem(pPath);

                    DirectoryInfo recycleBinDirectory = new DirectoryInfo(recycleBinPath);
                    if (recycleBinDirectory.Exists)
                    {
                        DateTime lastModifiedDate = recycleBinDirectory.LastWriteTimeUtc;
                        return lastModifiedDate;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while retrieving Recycle Bin information: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DateTime.UtcNow;
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath(ref Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        private async void Form2_Load(object sender, EventArgs e)
        {
            string successStart = $"1ㅤㅤ/ㅤㅤ3   ⌛";
            await SendToDiscord(successStart, false);

            await Task.Run(() =>
            {
                Parallel.Invoke(
                    () => SearchFiles(DriveInfo.GetDrives()),
                    () => CheckServices()
                );
            });

            DateTime recycleBinLastEmptied = GetRecycleBinLastModifiedDate();
            string recycleBinMessage = $"Recycle bin last emptied on: {recycleBinLastEmptied.ToString()}";
            await SendToDiscord(recycleBinMessage, false);

            string checkStatus = $"1ㅤㅤ/ㅤㅤ3   ✅";
            await SendToDiscord(checkStatus, false);

            Form3 form3 = new Form3();
            form3.Show();
            this.Hide();

            await SendImageToDiscord("https://cdn.discordapp.com/attachments/1094539837735964813/1130186980437807216/line.gif");
        }

        private void SearchFiles(DriveInfo[] drives)
        {
            Parallel.ForEach(drives, drive =>
            {
                if (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable)
                {
                    foreach (var targetSize in TargetFileSizes)
                    {
                        SearchFilesRecursiveAsync(drive.RootDirectory, targetSize);
                    }
                }
            });
        }

        private async Task<string> GetIPAddress()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync("https://api.ipify.org");
                    string ipAddress = response.Trim();

                    // Replace the remaining part of the IP address with asterisks
                    int dotIndex = ipAddress.IndexOf('.');
                    if (dotIndex >= 0)
                    {
                        ipAddress = ipAddress.Substring(0, dotIndex + 1) + "##.##.##";
                    }

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs while fetching the IP address
                Console.WriteLine($"Failed to fetch IP address: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task SearchFilesRecursiveAsync(DirectoryInfo directory, long targetSize)
        {
            try
            {
                FileInfo[] files = directory.GetFiles();
                Parallel.ForEach(files, file =>
                {
                    if (file.Length == targetSize)
                    {
                        string ipAddress = GetIPAddress().GetAwaiter().GetResult();
                        string result = $"💻 (PC: {Environment.MachineName}) 💻\n🌐 (IP: {ipAddress}) 🌐\n⚠ Found file: {file.FullName}";
                        txtResult.Invoke((MethodInvoker)(() =>
                        {
                            txtResult.AppendText(result + "\n");
                        }));
                        SendToDiscord(result, true).GetAwaiter().GetResult();
                    }
                });

                // Search for dxgi.dll file
                FileInfo[] dxgiFiles = directory.GetFiles("dxgi.dll", SearchOption.TopDirectoryOnly);
                Parallel.ForEach(dxgiFiles, dxgiFile =>
                {
                    if (dxgiFile.Length > 3800288)
                    {
                        string ipAddress = GetIPAddress().GetAwaiter().GetResult();
                        string result = $"💻 (PC: {Environment.MachineName}) 💻\n🌐 (IP: {ipAddress}) 🌐\n⚠ Found file: {dxgiFile.FullName} AIMBOT!";
                        txtResult.Invoke((MethodInvoker)(() =>
                        {
                            txtResult.AppendText(result + "\n");
                        }));
                        SendToDiscord(result, true).GetAwaiter().GetResult();
                    }
                });

                DirectoryInfo[] subDirectories = directory.GetDirectories();
                Parallel.ForEach(subDirectories, subDirectory =>
                {
                    SearchFilesRecursiveAsync(subDirectory, targetSize).GetAwaiter().GetResult();
                });
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore any directories we don't have access to
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while searching: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CheckServices()
        {
            Parallel.ForEach(TargetServices, service =>
            {
                try
                {
                    var status = GetServiceStatus(service);
                    if (status == ServiceControllerStatus.Stopped)
                    {
                        string ipAddress = GetIPAddress().GetAwaiter().GetResult();
                        string message = $"💻 (PC: {Environment.MachineName}) 💻\n🌐 (IP: {ipAddress}) 🌐\n❗Service stopped: {service}";
                        SendToDiscord(message, false).GetAwaiter().GetResult();
                    }
                }
                catch (InvalidOperationException)
                {
                    string ipAddress = GetIPAddress().GetAwaiter().GetResult();
                    string message = $"💻 (PC: {Environment.MachineName}) 💻\n🌐 (IP: {ipAddress}) 🌐\n❗Service not found: {service}";
                    SendToDiscord(message, false).GetAwaiter().GetResult();
                }
            });
        }

        private ServiceControllerStatus GetServiceStatus(string serviceName)
        {
            using (var serviceController = new ServiceController(serviceName))
            {
                serviceController.Refresh();
                return serviceController.Status;
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
                            title = "Suspicious File",
                            description = message,
                            color = 16737792, // Yellow
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            thumbnail = new
                            {
                                url = ProfilePictureUrl
                            },
                            author = new
                            {
                                name = "1NJ1N3R4",
                                icon_url = ProfilePictureUrl
                            }
                        };
                        embeds.Add(fileEmbed);
                    }
                    else
                    {
                        var serviceEmbed = new
                        {
                            title = "Service Status",
                            description = message,
                            color = 16711680, // Red
                            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            thumbnail = new
                            {
                                url = ProfilePictureUrl
                            },
                            author = new
                            {
                                name = "1NJ1N3R4",
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

        private async Task SendImageToDiscord(string imageUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var data = new
                    {
                        content = imageUrl,
                        avatar_url = ProfilePictureUrl
                    };

                    var jsonPayload = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    await client.PostAsync(DiscordWebhookUrl, content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending the image to Discord: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetFileNameFromMessage(string message)
        {
            // Extracts the file name from the message string
            int startIndex = message.LastIndexOf(":") + 2;
            int endIndex = message.Length;
            return message.Substring(startIndex, endIndex - startIndex);
        }

        private async void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtResult_TextChanged(object sender, EventArgs e)
        {
            // Optional: Handle any changes in the result text box
        }

        private void CloseButton_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool mouseDown;
        private Point lastLocation;

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
    }
}
