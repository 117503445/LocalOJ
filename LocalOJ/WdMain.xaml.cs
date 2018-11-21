using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
namespace LocalOJ
{
    public class TestData
    {
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public string ActualOutput { get; set; }
        public StatusCodes StatusCode { get; set; }
    }
    public enum StatusCodes
    {
        UnTested,
        Right,
        Wrong,
        TimeOut
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WdMain : Window
    {
        private FileInfo file_exe = new FileInfo(App.Path_File + "ExampleProgram.exe");
        public FileInfo File_exe
        {
            get { return file_exe; }
            set
            {
                file_exe = value;
                BtnEXEPath.Content = value.FullName;
            }
        }
        private FileSystemWatcher watcher;
        private FileInfo file_test = new FileInfo(App.Path_File + "ExampleProgram.json");
        public FileInfo File_test
        {
            get { return file_test; }
            set
            {
                file_test = value;
                BtnTestPath.Content = value.FullName;
                string json = LoadJsonFromDisk();
                datas = JsonConvert.DeserializeObject<List<TestData>>(json);
            }
        }
        private List<TestData> datas;
        public WdMain()
        {
            InitializeComponent();
            watcher = new FileSystemWatcher()
            {
                Path = App.Path_File,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.exe",
                EnableRaisingEvents = true
            };
            watcher.Changed += Watcher_Changed;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BtnEXEPath.Content = File_exe;
            BtnTestPath.Content = File_test;
            //string json = CreateTestJson();

            string json = LoadJsonFromDisk();
            datas = JsonConvert.DeserializeObject<List<TestData>>(json);
            await RunTest();
            UpdateGUI();
            //Console.WriteLine("Show");
            //foreach (var data in datas)
            //{
            //    Console.WriteLine($"<{data.ActualOutput}>");
            //}
        }
        private async void Watcher_Changed(object sender, FileSystemEventArgs e)
        {

            string json = LoadJsonFromDisk();
            datas = JsonConvert.DeserializeObject<List<TestData>>(json);
            await RunTest();
            UpdateGUI();
            await Dispatcher.InvokeAsync(() =>
            {
                Topmost = true;
                Topmost = false;
            });


        }
        private string LoadJsonFromDisk()
        {
            string json = File.ReadAllText(File_test.FullName);
            return json;
        }
        private void UpdateGUI(List<TestData> datas = null)
        {
            Dispatcher.InvokeAsync(() =>
           {
               if (datas == null)
               {
                   datas = this.datas;
               }
               StkMain.Children.RemoveRange(0, StkMain.Children.Count);
               foreach (var data in datas)
               {
                   UniformGrid ug = new UniformGrid()
                   {
                       Rows = 1
                   };
                   TextBox tb0 = new TextBox() { TextWrapping = TextWrapping.Wrap, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Text = data.Input };
                   TextBox tb1 = new TextBox() { TextWrapping = TextWrapping.Wrap, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Text = data.ExpectedOutput };
                   TextBox tb2 = new TextBox() { TextWrapping = TextWrapping.Wrap, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Text = data.ActualOutput, IsReadOnly = true };
                   TextBox tb3 = new TextBox() { TextWrapping = TextWrapping.Wrap, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Text = data.StatusCode.ToString(), IsReadOnly = true };
                   ug.Children.Add(tb0);
                   ug.Children.Add(tb1);
                   ug.Children.Add(tb2);
                   ug.Children.Add(tb3);
                   StkMain.Children.Add(ug);
               }
           });



        }
        /// <summary>
        /// 创建测试数据
        /// </summary>
        /// <returns></returns>
        private string CreateTestJson()
        {
            List<TestData> testDatas = new List<TestData>();
            TestData t1 = new TestData() { Input = "Hello World\r\n1 2 ", ExpectedOutput = @"I received <Hello World> ,the length is 11
3" };//Right
            TestData t2 = new TestData() { Input = "Hello World\r\n1 2  ", ExpectedOutput = @"I received <Goodbye World> ,the length is 11
3" };//Wrong
            TestData t3 = new TestData() { Input = "Hello World\r\n1 2", ExpectedOutput = @"I received <Hello World> ,the length is 11
3" };//TimeOut
            testDatas.Add(t1);
            testDatas.Add(t2);
            testDatas.Add(t3);
            string result = JsonConvert.SerializeObject(testDatas);
            //Console.WriteLine(result);
            return result;
        }
        private async Task RunTest()
        {
            List<Task<string>> tasks = new List<Task<string>>();
            List<Process> ps = new List<Process>();
            foreach (var item in datas)
            {
                Process p = new Process();
                tasks.Add(ExecuteAsync(File_exe, item.Input, p));
                ps.Add(p);
            }
            await Task.Delay(1000);
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].IsCompleted)
                {
                    datas[i].ActualOutput = await tasks[i];
                    if (datas[i].ActualOutput.Trim(new char[] { ' ', '\r', '\n' }) == datas[i].ExpectedOutput)
                    {
                        datas[i].StatusCode = StatusCodes.Right;
                    }
                    else
                    {
                        datas[i].StatusCode = StatusCodes.Wrong;
                    }
                }
                else
                {
                    datas[i].StatusCode = StatusCodes.TimeOut;
                }
            }
            foreach (var item in ps)
            {
                try
                {
                    item.Kill();
                    item.Close();
                }
                catch (Exception)
                {
                }
            }
        }
        /// <summary>
        /// 调用exe文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="input"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private async Task<string> ExecuteAsync(FileInfo path, string input, Process p = null)
        {
            if (p == null) p = new Process();
            string output = "";
            await Task.Run(() =>
            {
                p.StartInfo.FileName = path.FullName;
                p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.StandardInput.Write(input);
                p.StandardInput.AutoFlush = true;
                p.WaitForExit();//关键，等待外部程序退出后才能往下执行
                output = p.StandardOutput.ReadToEnd();
                p.Close();
            });
            return output;
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
            {
                FileInfo path = new FileInfo(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
                if (path.Extension == ".exe")
                {
                    File_exe = path;
                }
                else if (path.Extension == ".json")
                {
                    File_test = path;
                }
            }
            catch (Exception)
            {
            }
        }
        private void BtnEXEPath_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "可执行文件|*.exe"
            };
            if (dialog.ShowDialog() == true)
            {
                File_exe = new FileInfo(dialog.FileName);
            }
        }
        private void BtnTestPath_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "测试数据文件|*.json"
            };
            if (dialog.ShowDialog() == true)
            {
                File_test = new FileInfo(dialog.FileName);
            }
        }
    }
}
