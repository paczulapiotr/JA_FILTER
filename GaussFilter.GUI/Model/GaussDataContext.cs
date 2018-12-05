using GaussFilter.Core.ProgressNotifier;
using System;
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
        private double _progressBar;
        private int _maximumKernelSize;

        public int MaximumKernelSize
        {
            get { return _maximumKernelSize; }
            set
            {
                _maximumKernelSize = value;
                OnPropertyChanged(nameof(MaximumKernelSize));
            }
        }


        public double ProgressBar
        {
            get { return _progressBar; }
            set
            {
                _progressBar = value;
                OnPropertyChanged(nameof(ProgressBar));
            }
        }
        public bool SaveEnabled
        {
            get => _filteredImage != null;
        }
        public bool FilterEnabled
        {
            get => _image != null;
        }
        public bool AssemblerImplementationMode { get; set; }
        public bool CSharpImplementationMode { get; set; }
        public int FrameSize
        {
            get => _frameSize; set
            {
                _frameSize = value;
                OnPropertyChanged(nameof(FrameSize));
            }
        }
        public double GaussRadius
        {
            get => _radius; set
            {
                _radius = value;
                OnPropertyChanged(nameof(GaussRadius));
            }
        }
        public string InputFilePath
        {
            get => _inputFilePath; set
            {
                _inputFilePath = value;
                OnPropertyChanged(nameof(InputFilePath));
            }
        }
        public string ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }
        public Bitmap Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
                OnPropertyChanged(nameof(FilterEnabled));
            }
        }
        public Bitmap FilteredImage
        {
            get => _filteredImage;
            set
            {
                _filteredImage = value;
                OnPropertyChanged(nameof(FilteredImage));
                OnPropertyChanged(nameof(SaveEnabled));
            }
        }
        public BitmapImage ImageSource
        {
            get => _imageSource; set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
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
