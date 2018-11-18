using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
namespace LocalOJ
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WdMain : Window
    {
        public FileInfo File_exe { get; set; } = new FileInfo(@"C:\Users\117503445\Desktop\build\19.exe");

        public WdMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string json = @"['8
1 8 4 7 6 5 3 2 ','6 1 2 3 4 5 ','8
1 8 4 7 6 5 3 2 ']";
            List<string> s = JsonConvert.DeserializeObject<List<string>>(json);
            foreach (var item in s)
            {
                Console.WriteLine($"input<{item}>");

                Console.WriteLine($"end<{item}>,output<{ Execute(item).Result}>");
            }

        }
        private async Task<string> Execute(string input)
        {
            string output = "";
            await Task.Run(() =>
            {
                using (Process exep = new Process())
                {
                    exep.StartInfo.FileName = File_exe.FullName;
                    exep.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                    exep.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                    exep.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                    exep.StartInfo.CreateNoWindow = true;
                    exep.StartInfo.UseShellExecute = false;
                    exep.Start();
                    exep.StandardInput.Write(input);
                    exep.StandardInput.AutoFlush = true;
                    exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
                    output = exep.StandardOutput.ReadToEnd();
                    exep.Close();
                }
            });


            return output;
        }
    }
}
