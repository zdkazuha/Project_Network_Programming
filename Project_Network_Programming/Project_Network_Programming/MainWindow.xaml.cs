using DbController;
using Db_Controller.Entities;
using Microsoft.Win32;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;

namespace Final_Project_Network_Programming
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<MessageList> Files { get; set; } = new ObservableCollection<MessageList>();
        public ObservableCollection<MessageInfo> Messages { get; set; } = new ObservableCollection<MessageInfo>();

        public static Db_functional context = new Db_functional();
        IPEndPoint server;
        TcpClient client;
        NetworkStream ns = null;

        StreamWriter sw;
        StreamReader sr;

        string UserName = "";
        string CLOSE = "$<close>";

        public static bool isListener = true;

        public User User;

        public MainWindow(User user)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            User = user;
            UserName = user.Username;

            this.DataContext = this;
            server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040);
        }

        private void SendBtn(object sender, RoutedEventArgs e)
        {
            string Text = msgTextBox.Text;
            msgTextBox.Text = "";
            if (string.IsNullOrWhiteSpace(Text))
            {
                MessageBox.Show("Будь ласка введіть повідомлення!");
                return;
            }
            if (sw == null)
                return;

            string message = $"{User.Username}:{Text}";
            sw.WriteLine(message);
            sw.Flush();
        }
        private async void JoinBtn(object sender, RoutedEventArgs e)
        {
            GroupName groupName = new GroupName(User);
            groupName.ShowDialog();

            if (groupName.ResultUser != null)
            {
                User = groupName.ResultUser;
                UserName = User.Username;

                if (context == null)
                {
                    MessageBox.Show("Не вдалося підключитися до бази даних.");
                    return;
                }

                context.SaveChanges();
            }

            try
            {
                if (client == null)
                {
                    client = new TcpClient();
                }

                if (server == null)
                {
                    MessageBox.Show("Сервер не ініціалізовано.");
                    return;
                }

                client.Connect(server);
                ns = client.GetStream();

                if (ns == null)
                {
                    MessageBox.Show("Не вдалося створити мережевий потік.");
                    return;
                }

                sw = new StreamWriter(ns);
                sr = new StreamReader(ns);

                isListener = true;
                Listener();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при з'єднанні: " + ex.Message);
                return;
            }

            try
            {
                if (sw == null)
                {
                    MessageBox.Show("StreamWriter не ініціалізовано.");
                    return;
                }

                await sw.WriteLineAsync($"{User.Username}:Hello server:");
                await sw.FlushAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при надсиланні: " + ex.Message);
            }
        }
        private void LeaveBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sw == null)
                {
                    MessageBox.Show("Ви ще не приєдналися до сервера!");
                    return;
                }
                User = context.GetUser(User.Username);


                sw.WriteLine($"{UserName}:{CLOSE}");
                sw.Flush();

                client.Close();
                ns.Close();
                sw.Close();
                sr.Close();

                sw = null;
                isListener = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void Listener()
        {
            await Task.Run(async () =>
            {
                while (isListener)
                {
                    try
                    {
                        string? fullMessage = await sr.ReadLineAsync();
                        if (fullMessage != null)
                        {
                            if (fullMessage.Contains(":FILE:"))
                            {
                                var parts1 = fullMessage.Split(':', 4);
                                if (parts1.Length == 4)
                                {
                                    string senderName = parts1[0];
                                    string fileName = parts1[2];
                                    int length = int.Parse(parts1[3]);

                                    await ReceiveFileFromServer(senderName, fileName, length);
                                }
                                continue;
                            }

                            if (fullMessage.Contains(":Invite:"))
                            {                                
                                var parts1 = fullMessage.Split(':', 5);
                                if (parts1.Length == 5)
                                {
                                    string senderName = parts1[0];
                                    string command = parts1[1];
                                    string UserName = parts1[2];
                                    string ChatName = parts1[3];
                                    int ChatId = int.Parse(parts1[4]);

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        ConfirmationGroup configuration = new ConfirmationGroup(senderName, User, ChatName, ChatId);
                                        configuration.ShowDialog();

                                        if (configuration.ResultUser != null)
                                        {
                                            context.AddUserToGroup(User, ChatName);
                                            context.SaveChanges();
                                            MessageBox.Show($"Вітаємо {User.Username} у групі {ChatName}");
                                        }
                                        else
                                            MessageBox.Show("Запрошення відхилено.");
                                    });

                                    continue;
                                }
                                else
                                    MessageBox.Show("Формат запрошення некоректний: очікувалося 5 частин.");

                            }

                            int index = fullMessage.IndexOf(':');
                            if (index == -1)
                                continue;

                            var parts = fullMessage.Split(':', 2);
                            if (parts.Length != 2)
                                continue;

                            string userName = parts[0];
                            string message = parts[1];

                            Dispatcher.Invoke(() =>
                            {
                                Messages.Add(new MessageInfo($"{userName} ::", message));
                            });

                            if (userName == "Сервер" && message == "Сервер переповнений!")
                            {
                                sw = null;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            });
        }
        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MessageList item)
                Files.Remove(item);
        }
        private async void BrowseBtn(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                Files.Add(new MessageList { FilePath = dialog.FileName });
                await SendFile(dialog.FileName);
            }
        }
        private async Task SendFile(string filePath)
        {
            if (sw == null || ns == null)
                return;

            byte[] fileBytes = File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);

            sw.WriteLine($"{UserName}:FILE:{fileName}:{fileBytes.Length}");
            sw.Flush();

            await ns.WriteAsync(fileBytes, 0, fileBytes.Length);
            await ns.FlushAsync();

            //MessageBox.Show("Файл успішно відправлено.");

            Files.Clear();
        }
        private async Task ReceiveFileFromServer(string senderName, string fileName, int length)
        {
            try
            {
                byte[] buffer = new byte[length];
                int totalRead = 0;

                while (totalRead < length)
                {
                    int read = await ns.ReadAsync(buffer, totalRead, length - totalRead);
                    if (read == 0)
                        break;
                    totalRead += read;
                }

                string saveFolder = Path.Combine(Environment.CurrentDirectory, "ReceivedFiles");
                Directory.CreateDirectory(saveFolder);

                string savePath = Path.Combine(saveFolder, fileName);
                await File.WriteAllBytesAsync(savePath, buffer);

                Dispatcher.Invoke(() =>
                {
                    string extension = Path.GetExtension(fileName).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                        Messages.Add(new MessageInfo($"{senderName} (ФОТО)", $"отримано фото: {fileName}", savePath));
                    else
                        Messages.Add(new MessageInfo($"{senderName} (ФАЙЛ)", $"отримано файл: {fileName}"));
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Помилка отримання файлу: {ex.Message}");
                });
            }
        }
        private void OpenFileOrImage(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label && label.DataContext is MessageInfo messageInfo)
            {
                string filePath = messageInfo.ImagePath;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
        }
        private void InviteToPrivateGroupBtn(object sender, RoutedEventArgs e)
        {
            InviteToPrivateChat inviteToPrivateChat = new InviteToPrivateChat(sw, User);
            inviteToPrivateChat.ShowDialog();

            if (inviteToPrivateChat.ResultUser != null)
            {
                context.AddUserToGroup(User, inviteToPrivateChat.ResultUser.Group.Name);
                context.SaveChanges();

                MessageBox.Show($"Вітаємо {User.Username} у групі {inviteToPrivateChat.ResultUser.Group.Name}");
            }
            else
                MessageBox.Show("Вас не вийшло підєднати");
        }
        private void ClearBtn(object sender, RoutedEventArgs e)
        {
            Messages.Clear();
            Files.Clear();
        }
        private void msgTextBoxEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendBtn(sender, e);
            }
        }
        private void OpenContactsBtn(object sender, RoutedEventArgs e)
        {
            ContactWindow contactWindow = new ContactWindow(User);
            contactWindow.Show();
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class MessageList
    {
        public string FilePath { get; set; }
    }
    public class MessageInfo
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public string ImagePath { get; set; }
        private DateTime time;
        public string Time => time.ToLongTimeString();
        public MessageInfo(string userName, string message, string imagePath = null)
        {
            UserName = userName;
            Message = message;
            ImagePath = imagePath;
            time = DateTime.Now;
        }
        public override string ToString()
        {
            return $"{Message,-20} {Time} ";
        }
    }
}
