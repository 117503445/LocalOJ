using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Newtonsoft.Json;

namespace LocalOJ
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WdMain : Window
    {
        private FileInfo file_exe = new FileInfo(App.Path_File + "ExampleProgram.exe");
        /// <summary>
        /// exe文件的路径
        /// </summary>
        public FileInfo File_exe
        {
            get { return file_exe; }
            set
            {
                watcher.Path = value.Directory.FullName;
                file_exe = value;
                BtnEXEPath.Content = value.FullName;
                File_test = new FileInfo(value.Directory.FullName + "\\" + Path.GetFileNameWithoutExtension(value.Name) + ".json");
            }
        }
        private FileInfo file_test = new FileInfo(App.Path_File + "ExampleProgram.json");
        /// <summary>
        /// json测试文件的路径
        /// </summary>
        public FileInfo File_test
        {
            get { return file_test; }
            set
            {
                //Console.WriteLine("Set Start");
                file_test = value;
                BtnTestPath.Content = value.FullName;
                DiskToDatas();
                Task.Run(async () =>
                {
                    DatasToGUI();
                    await RunTestAsync();
                    //Console.WriteLine("Run Finished");
                    DatasToGUI();
                    //Console.WriteLine("Update Finished");
                });
                //Console.WriteLine("Set Finish");
            }
        }
        /// <summary>
        /// 储存测试数据信息
        /// </summary>
        private List<TestData> datas;
        /// <summary>
        /// 监视exe文件
        /// </summary>
        private FileSystemWatcher watcher;
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

            DiskToDatas();
            DatasToGUI();
            await RunTestAsync();
            DatasToGUI();
            //Console.WriteLine("Show");
            //foreach (var data in datas)
            //{
            //    Console.WriteLine($"<{data.ActualOutput}>");
            //}
        }
        /// <summary>
        /// exe被编译
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Watcher_Changed(object sender, FileSystemEventArgs e)
        {

            DiskToDatas();

            await RunTestAsync();
            DatasToGUI();
            await Dispatcher.InvokeAsync(() =>
            {
                Topmost = true;
                Topmost = false;
            });


        }
        /// <summary>
        /// 把文件路径拖入应用窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            //Console.WriteLine("Drag");
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
        /// <summary>
        /// 撤回数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (lastDatas != null)
            {
                datas = lastDatas;
                DatasToGUI();
                DatasToDisk();
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
        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            datas.Add(new TestData());
            DatasToDisk();
            DatasToGUI();
        }
        /// <summary>
        /// 删除某一测试数据
        /// </summary>
        /// <param name="index"></param>
        private void DeleteData(int index)
        {
            lastDatas = new List<TestData>();
            foreach (var data in datas)

            {
                lastDatas.Add(data);
            }
            datas.RemoveAt(index);
            DatasToDisk();
            DatasToGUI();
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

        private void GUIToDatas()
        {
            //Console.WriteLine("GUIToDatas Start");
            datas.Clear();

            foreach (var ugs in StkMain.Children)
            {
                TestData data = new TestData(); UniformGrid ug = (UniformGrid)ugs;
                data.Input = ((TextBox)ug.Children[0]).Text;
                data.ExpectedOutput = ((TextBox)ug.Children[1]).Text;
                data.ActualOutput = ((TextBox)ug.Children[2]).Text;
                datas.Add(data);
            }
            //Console.WriteLine("GUIToDatas End");
        }
        private void DatasToGUI()
        {
            //Console.WriteLine("DatasToGUI Start");
            Dispatcher.InvokeAsync(() =>
            {
                StkMain.Children.RemoveRange(0, StkMain.Children.Count);
                for (int i = 0; i < datas.Count; i++)
                {
                    UniformGrid ug = new UniformGrid()
                    {
                        Rows = 1,
                        Tag = i
                    };
                    ug.MouseRightButtonUp += (s, e) =>
                    {
                        if (s is UniformGrid u)
                        {
                            DeleteData((int)u.Tag);
                        }
                    };

                    TextBox tb0 = new TextBox() { TextWrapping = TextWrapping.Wrap, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Text = datas[i].Input };
                    TextBox tb1 = new TextBox() { TextWrapping = TextWrapping.Wrap, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Text = datas[i].ExpectedOutput };
                    TextBox tb2 = new TextBox() { TextWrapping = TextWrapping.Wrap, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Text = datas[i].ActualOutput, IsReadOnly = true };
                    TextBox tb3 = new TextBox() { TextWrapping = TextWrapping.Wrap, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Text = datas[i].StatusCode.ToString(), IsReadOnly = true };
                    ug.Children.Add(tb0);
                    ug.Children.Add(tb1);
                    ug.Children.Add(tb2);
                    ug.Children.Add(tb3);
                    StkMain.Children.Add(ug);
                }
            });
            //Console.WriteLine("DatasToGUI End");
        }
        private void DiskToDatas()
        {
            //Console.WriteLine("DiskToDatas Start");
            string json = "";
            try
            {
                json = File.ReadAllText(File_test.FullName);
            }
            catch (Exception)
            {
            }
            datas = JsonConvert.DeserializeObject<List<TestData>>(json);
            if (datas == null) datas = new List<TestData>();
            //Console.WriteLine("DiskToDatas End");
        }
        private void DatasToDisk()
        {
            //Console.WriteLine("DatasToDisk Start");
            if (datas != null)
            {
                string json = JsonConvert.SerializeObject(datas);
                File.WriteAllText(File_test.FullName, json);
            }
            //Console.WriteLine("DatasToDisk End");
        }
        /// <summary>
        /// 运行所有测试
        /// </summary>
        /// <returns></returns>
        private async Task RunTestAsync()
        {
            //Console.WriteLine("RunTest Start");
            if (datas == null)
            {
                return;
            }
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

            foreach (var p in ps)//试图关闭超时线程
            {
                try
                {
                    p.Kill();
                    p.Close();
                }
                catch (Exception)
                {
                }
            }

            string s = DateTime.Now.ToLocalTime().ToString("MM/dd HH:mm:ss");
            Dispatcher.Invoke(() => { TbkTestTime.Text = $"Last test time : {s}"; });
            //Console.WriteLine("RunTest End");
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
            if (input.Length > 0 && input[input.Length - 1] != ' ') input += ' ';
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
        /// <summary>
        /// 用于撤回,储存上一次datas的状态
        /// </summary>
        private List<TestData> lastDatas = null;
        /// <summary>
        ///  F5 时保存并运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                GUIToDatas();
                DatasToDisk();
                await RunTestAsync();
                DatasToGUI();
            }
        }
    }
}
