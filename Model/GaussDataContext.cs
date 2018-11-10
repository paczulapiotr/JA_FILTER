using System.ComponentModel;

namespace GaussFilter.Model
{

    class GaussDataContext : INotifyPropertyChanged
    {
        private const int MIN_FRAME_SIZE = 3;
        private const double MIN_GAUSS_RADIUS = 0.5;
        public GaussDataContext()
        {
            FrameSize = MIN_FRAME_SIZE;
            GaussRadius = MIN_GAUSS_RADIUS;
            CSharpImplementationMode = true;

        }
        private string _inputFilePath;
        private string _outputFilePath;

        public bool AssemblerImplementationMode { get; set; }
        public bool CSharpImplementationMode { get; set; }
        public int FrameSize { get; set; }
        public double GaussRadius { get; set; }
        public string InputFilePath { get=> _inputFilePath; set{
                _inputFilePath = value;
                OnPropertyChanged("InputFilePath");
            } }
        public string OutputFilePath { get=> _outputFilePath; set {
                _outputFilePath = value;
                OnPropertyChanged("OutputFilePath");
            } }
        public long ElapsedTime { get; set; }

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
