using GaussFilter.Algorithm;
using GaussFilter.Core;
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
                //openFile.InitialDirectory = @"C:\";
                Title = "Please select an image file."
            };
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                DataContext is GaussDataContext context)
            {
                context.InputFilePath = openFile.FileName;
                context.FilteredImage = null;
                context.Image = new Bitmap(openFile.FileName);

                PrintImageOnGUI(context.Image);


            }
        }


        private async void Filter_Button_Click(object sender, RoutedEventArgs e)
        {

            Filter_Btn.IsEnabled = false;
            Find_Btn.IsEnabled = false;

            var context = (DataContext as GaussDataContext);
            context.FilteredImage = null;
            var frame = context.FrameSize;
            var radius = context.GaussRadius;
            var image = context.Image;
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
            PrintImageOnGUI(context.FilteredImage);
            Filter_Btn.IsEnabled = true;
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
