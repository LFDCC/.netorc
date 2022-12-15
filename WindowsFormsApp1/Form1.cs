using OpenCvSharp;

using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.LocalV3;
using Sdcb.PaddleOCR.Models.Online;

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async Task<long> 图片侦测(string url)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //byte[] sampleImageData;
            //string sampleImageUrl = @"https://www.tp-link.com.cn/content/images2017/gallery/4288_1920.jpg";
            //using (HttpClient http = new HttpClient())
            //{
            //    Console.WriteLine("Download sample image from: " + sampleImageUrl);
            //    sampleImageData = await http.GetByteArrayAsync(sampleImageUrl);
            //}

            using (var detector = new PaddleOcrDetector(LocalDetectionModel.ChineseV3, PaddleDevice.Mkldnn()))
            {
                using (var src = Cv2.ImRead(url))
                //using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
                {
                    var rects = detector.Run(src);
                    using (var visualized = PaddleOcrDetector.Visualize(src, rects, Scalar.Red, thickness: 2))
                    {
                        stopwatch.Stop();
                        var filename = Guid.NewGuid().ToString("N") + ".jpg";
                        var outputFile = Path.Combine(Environment.CurrentDirectory, filename);
                        Console.WriteLine("OutputFile: " + filename);
                        visualized.ImWrite(filename);
                        var bitmap = new Bitmap(filename);
                        this.pictureBox1.Image = bitmap;
                    }
                }
            }
            return await Task.FromResult(stopwatch.ElapsedMilliseconds);
        }

        private async Task<long> 图片识别和侦测(string url)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var model = LocalFullModels.ChineseV3;//本地模型
            //FullOcrModel model = await OnlineFullModels.ChineseV3.DownloadAsync();//在线模型
            //byte[] sampleImageData;
            //string sampleImageUrl = @"https://www.tp-link.com.cn/content/images2017/gallery/4288_1920.jpg";
            //using (HttpClient http = new HttpClient())
            //{
            //    Console.WriteLine("Download sample image from: " + sampleImageUrl);
            //    sampleImageData = await http.GetByteArrayAsync(sampleImageUrl);
            //}

            using (var all = new PaddleOcrAll(model, PaddleDevice.Mkldnn(), device =>
            {
                device.UseGpu = true;                
            })
            {
                AllowRotateDetection = true, /* 允许识别有角度的文字 */
                Enable180Classification = false, /* 允许识别旋转角度大于90度的文字 */
            })
            {
                // Load local file by following code:
                using (var src = Cv2.ImRead(url))
                //using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
                {
                    var result = all.Run(src);
                    stopwatch.Stop();
                    this.richTextBox1.Text = stopwatch.ElapsedMilliseconds + "\r\n" + result.Text;

                }
            }

            return await Task.FromResult(stopwatch.ElapsedMilliseconds);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files(*.*)|*.*",
                RestoreDirectory = true
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;
            this.richTextBox1.Text = "";
            var picAddress = ofd.FileName;
            var total = 0L;
            //for (int i = 0; i < 10; i++)
            //{
            //    total += await 图片识别和侦测(picAddress);
            //}
            total += await 图片识别和侦测(picAddress);
            var avg = total / 10;

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files(*.*)|*.*",
                RestoreDirectory = true
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;
            var picAddress = ofd.FileName;
            await 图片侦测(picAddress);
        }
    }
}
