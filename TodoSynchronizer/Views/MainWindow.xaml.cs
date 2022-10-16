using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using TodoSynchronizer.Core.Config;
using TodoSynchronizer.Core.Helpers;
using TodoSynchronizer.Core.Models;
using TodoSynchronizer.Core.Models.CanvasModels;
using TodoSynchronizer.Core.Services;
using TodoSynchronizer.Core.Yaml;
using TodoSynchronizer.ViewModels;
using Wpf.Ui.Extensions;
using YamlDotNet.Serialization;
using File = System.IO.File;

namespace TodoSynchronizer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [INotifyPropertyChanged]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        [ObservableProperty]
        private string message;

        [ObservableProperty]
        private ObservableCollection<string> items;

        public CanvasLoginViewModel CanvasLoginViewModel { get; set; } = new CanvasLoginViewModel();
        public TodoLoginViewModel TodoLoginViewModel { get; set; } = new TodoLoginViewModel();

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ReadConfig();
            ButtonProgressAssist.SetIsIndicatorVisible(GoButton, true);
            ButtonProgressAssist.SetIsIndeterminate(GoButton, true);
            Thread t = new Thread(() => { Go(); });
            t.Start();
        }

        private void Go()
        {
            Items = new ObservableCollection<string>();
            SyncService sync = new SyncService();
            sync.OnReportProgress += OnReportProgress;
            sync.Go();
        }

        private void GoDida()
        {
            Items = new ObservableCollection<string>();
            DidaSyncService sync = new DidaSyncService();
            sync.OnReportProgress += OnReportProgress;
            sync.Go();
        }

        private void OnReportProgress(SyncState state)
        {
            Message = state.Message;
            this.Dispatcher.Invoke(() => {
                Items.Add(Message);
            });
            if (state.State == SyncStateEnum.Finished)
            {
                Finish();
            }
        }

        private async void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            ReadConfig();
            //FileStream fs = new FileStream(@"C:\Users\111\Downloads\98568049_p0.jpg", FileMode.Open);
            //AttachmentInfo info = new AttachmentInfo();
            //info.AttachmentType = AttachmentType.File;
            //info.Size = fs.Length;
            //info.Name = "98568049_p0.png";

            //MemoryStream stream = new MemoryStream();
            //byte[] buffer = new byte[1024];
            //int bytesRead = 0;
            //while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
            //{
            //    stream.Write(buffer, 0, bytesRead);
            //}

            //TodoService.UploadAttachment("AQMkADAwATM0MDAAMS0xM2UxLWVlAGIxLTAwAi0wMAoALgAAA_qILJaukoNEkdO_6z6BimcBAFr-8yPLKcJMlzMhuV24V6IAA3FhX8EAAAA=", "AQMkADAwATM0MDAAMS0xM2UxLWVlAGIxLTAwAi0wMAoARgAAA_qILJaukoNEkdO_6z6BimcHAFr-8yPLKcJMlzMhuV24V6IAA3FhX8EAAABa--MjyynCTJczIblduFeiAANxYbM1AAAA", info, stream);
        }

        private static void ReadConfig()
        {
            var yml = File.ReadAllText(@"C:\Users\111\source\repos\TodoSynchronizer\config.yaml");
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .WithTypeInspector(n => new IgnoreCaseTypeInspector(n))
                .IgnoreUnmatchedProperties()
                .Build();
            SyncConfig.Default = deserializer.Deserialize<SyncConfig>(yml);
        }

        public void Finish()
        {
            this.Dispatcher.Invoke(() =>{
                ButtonProgressAssist.SetIsIndicatorVisible(GoButton, false);
                ButtonProgressAssist.SetIsIndeterminate(GoButton, false);
            });
        }

        private void ButtonSetting_Click(object sender, RoutedEventArgs e)
        {
            var m = new SettingsWindow();
            m.Show();
        }

        private void ButtonDida_Click(object sender, RoutedEventArgs e)
        {
            DidaService.Login(File.ReadAllText(@"C:\Users\111\Downloads\dida.txt"));
            ReadConfig();
            ButtonProgressAssist.SetIsIndicatorVisible(GoButton, true);
            ButtonProgressAssist.SetIsIndeterminate(GoButton, true);
            Thread t = new Thread(() => { GoDida(); });
            t.Start();
        }
    }
}
