using GaussFilter.Core;
using GaussFilter.Core.GaussMask;
using GaussFilter.Model;
using GaussFilter.Model.ExternDllParameters;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GaussFilter
{

    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new GaussDataContext();
        }

        private void Input_Path_Find(object sender, RoutedEventArgs e)
        {

            var openFile = new OpenFileDialog
            {
                Filter = "bitmap files (*.bmp)|*.bmp|jpg files (*.jpg)|*.jpg",
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
            var context = (DataContext as GaussDataContext);
            var frame = context.FrameSize;
            var radius = context.GaussRadius;
            var image = context.Image;
            Stopwatch stopwatch = new Stopwatch();

            context.ElapsedTime = "Trwa filtrowanie...";

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

            context.ElapsedTime = stopwatch.ElapsedMilliseconds.ToString();
            PrintImageOnGUI(context.FilteredImage);

        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp",
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
                //var value = AssemblyCode(5, 5);
                return image;
            });
        }
        private async Task<Bitmap> RunCSharpImplementationFilter(int frame, double radius, Bitmap image)
        {
            return await Task.Run(() =>
            {
            var gaussFilter = new Algorithm.GaussFilter(frame, radius, image, new StandardGaussMaskProvider());
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

        private unsafe void Button_Click(object sender, RoutedEventArgs e)
        {
            Bitmap img = (DataContext as GaussDataContext).Image;
            PrintImageOnGUI(new ImplementationManager().ApplyAssemblyFilter(img));
        }
    }
}
