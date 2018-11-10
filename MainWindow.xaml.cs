using GaussFilter.Model;
using System.Windows;
using System.Windows.Forms;

namespace GaussFilter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new GaussDataContext();
        }

        private void Input_Path_Find(object sender, RoutedEventArgs e)
        {

            var openFile = new OpenFileDialog();
            openFile.Filter = "bitmap files (*.bmp)|*.bmp|jpg files (*.jpg)|*.jpg";
            //openFile.InitialDirectory = @"C:\";
            openFile.Title = "Please select an image file.";
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                DataContext is GaussDataContext context)
            {
                context.InputFilePath = openFile.FileName;
            }
        }

        private void Output_Path_Find(object sender, RoutedEventArgs e)
        {
            var openFile = new FolderBrowserDialog();

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                DataContext is GaussDataContext context)
            {
                context.OutputFilePath = openFile.SelectedPath;
            }

        }
    }
}
