using System.ComponentModel;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace GaussFilter.Model
{

    class GaussDataContext : INotifyPropertyChanged
    {
        private const int MIN_FRAME_SIZE = 3;
        private const double MIN_GAUSS_RADIUS = 0.5;
        public GaussDataContext()
        {
            _frameSize = MIN_FRAME_SIZE;
            _radius = MIN_GAUSS_RADIUS;
            CSharpImplementationMode = true;
        }
        private string _elapsedTime;
        private double _radius;
        private int _frameSize;
        private string _inputFilePath;
        private BitmapImage _imageSource;
        private Bitmap _image;
        private Bitmap _filteredImage;


        public bool SaveEnabled
        {
            get => _filteredImage != null;
        }
        public bool FilterEnabled { get => Image != null; }
        public bool AssemblerImplementationMode { get; set; }
        public bool CSharpImplementationMode { get; set; }
        public int FrameSize
        {
            get => _frameSize; set
            {
                _frameSize = value;
                OnPropertyChanged("FrameSize");
            }
        }
        public double GaussRadius
        {
            get => _radius; set
            {
                _radius = value;
                OnPropertyChanged("GaussRadius");
            }
        }
        public string InputFilePath
        {
            get => _inputFilePath; set
            {
                _inputFilePath = value;
                OnPropertyChanged("InputFilePath");
            }
        }
        public string ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                OnPropertyChanged("ElapsedTime");
            }
        }
        public Bitmap Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged("FilterEnabled");
            }
        }
        public Bitmap FilteredImage
        {
            get => _filteredImage;
            set
            {
                _filteredImage = value;
                OnPropertyChanged("FilteredImage");
                OnPropertyChanged("SaveEnabled");
            }
        }
        public BitmapImage ImageSource
        {
            get => _imageSource; set
            {
                _imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (propertyName != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }
    }
}
