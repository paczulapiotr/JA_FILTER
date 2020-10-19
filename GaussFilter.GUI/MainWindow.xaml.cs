using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GaussFilter.Algorithm;
using GaussFilter.Model;

namespace GaussFilter
{

    public partial class MainWindow : Window
    {
        private Action<float> dispatcher;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new GaussDataContext();
            dispatcher = (newValue) => Dispatcher.InvokeAsync(() =>
            {
                (this.DataContext as GaussDataContext).ProgressBar = newValue;
            });
        }



        private void Input_Path_Find(object sender, RoutedEventArgs e)
        {

            var openFile = new OpenFileDialog
            {
                Filter = "1.JPeg Image|*.jpg|2.Bitmap Image|*.bmp",
                Title = "Please select an image file."
            };
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                DataContext is GaussDataContext context)
            {
                context.InputFilePath = openFile.FileName;
                var newBitmap = new Bitmap(openFile.FileName);
                int minSize = new[] { newBitmap.Width, newBitmap.Height }.Min();
                if (minSize >= 50)
                {
                    context.Image = newBitmap;
                    PrintImageOnGUI(context.Image);
                }
                else
                {
                    context.Image = null;
                    System.Windows.Forms.MessageBox.Show("Picture is too small. Min height/weight is 50x50.");
                }
            }
        }


        private async void Filter_Button_Click(object sender, RoutedEventArgs e)
        {
            Find_Btn.IsEnabled = false;

            var context = (DataContext as GaussDataContext);
            context.FilteredImage = null;
            var image = context.Image;
            context.Image = null;
            Stopwatch stopwatch = new Stopwatch();

            context.ElapsedTime = "Trwa filtrowanie...";
            context.ProgressBar = 0;
            ProgressBar.Visibility = Visibility.Visible;
            stopwatch.Start();
            if (context.CSharpImplementationMode)
            {
                context.FilteredImage = await RunCSharpLaplaceFilter(image);
            }
            else if (context.AssemblerImplementationMode)
            {
                context.FilteredImage = await RunAssemblyImplementationFilter(image);
            }
            stopwatch.Stop();
            ProgressBar.Visibility = Visibility.Hidden;
            context.ElapsedTime = stopwatch.ElapsedMilliseconds.ToString();
            context.Image = image;
            PrintImageOnGUI(context.FilteredImage);
            Find_Btn.IsEnabled = true;

        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "1.JPeg Image|*.jpg|2.Bitmap Image|*.bmp",
                Title = "Save an Image File"
            };
            var result = saveFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var context = (DataContext as GaussDataContext);
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        context.FilteredImage.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
                        break;
                    case 2:
                        context.FilteredImage.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                        break;
                    default:
                        break;
                }

            }

        }

        private async Task<Bitmap> RunAssemblyImplementationFilter(Bitmap image)
        {
            return await Task.Run(() =>
            {
                return new LaplaceFilterAssembly(image).ApplyAssemblyFilter(image);
            });
        }

        private int[] _laplaceMask1 = new int[] {
                        0, -1, 0,
                        -1, 4, -1,
                        0, -1, 0 };

        private int[] _laplaceMask2 = new int[] {
                        -1, -1, -1,
                        -1,  8, -1,
                        -1, -1, -1 };

        private int[] _laplaceMask3 = new int[] {
                        1, -2, 1,
                        -2,  4, -2,
                        1, -2, 1 };

        private async Task<Bitmap> RunCSharpLaplaceFilter(Bitmap image)
        {
            return await Task.Run(() =>
            {
                var laplaceFilter = new LaplaceFilterCSharp(_laplaceMask2, image, dispatcher);
                laplaceFilter.ApplyUnsafe();

                return laplaceFilter.FilteredImage;
            });
        }

        private void PrintImageOnGUI(Bitmap image)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                bitmapimage.Freeze();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    (DataContext as GaussDataContext).ImageSource = bitmapimage;
                }));
            }

        }

    }
}
