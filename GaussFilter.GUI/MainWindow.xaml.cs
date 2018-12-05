using GaussFilter.Algorithm;
using GaussFilter.Core.GaussMask;
using GaussFilter.Model;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;

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
                int minSize = new [] { newBitmap.Width, newBitmap.Height }.Min();
                if (minSize >= 50)
                {
                    context.Image = newBitmap;
                    int newKernel = (int)((double)minSize / 10d);
                    context.MaximumKernelSize = (newKernel % 2 == 0) ? newKernel + 1 : newKernel;
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
            var frame = context.FrameSize;
            var radius = context.GaussRadius;
            var image = context.Image;
            context.Image = null;
            Stopwatch stopwatch = new Stopwatch();

            context.ElapsedTime = "Trwa filtrowanie...";
            context.ProgressBar = 0;
            ProgressBar.Visibility = Visibility.Visible;
            stopwatch.Start();
            if (context.CSharpImplementationMode)
            {
                context.FilteredImage = await RunCSharpImplementationFilter(frame, radius, image);
            }
            else if (context.AssemblerImplementationMode)
            {
                context.FilteredImage = await RunAssemblyImplementationFilter(frame, radius, image);
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

        private async Task<Bitmap> RunAssemblyImplementationFilter(int frame, double radius, Bitmap image)
        {
            return await Task.Run(() =>
            {
                return new GaussFilterAssembly(frame,radius,image,new StandardGaussMaskProvider(), dispatcher).ApplyAssemblyFilter(image);
            });
        }
        private async Task<Bitmap> RunCSharpImplementationFilter(int frame, double radius, Bitmap image)
        {
           
            return await Task.Run(() =>
            {
                var gaussFilter = new GaussFilterCSharp(frame, radius, image, new StandardGaussMaskProvider(), dispatcher);
                gaussFilter.ApplyUnsafe();
                return gaussFilter.FilteredImage;
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
