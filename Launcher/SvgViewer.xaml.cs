using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for SvgViewer.xaml
    /// </summary>
    public partial class SvgViewer : INotifyPropertyChanged
    {
        private double _initWidth;
        private double _initHeight;

        private double _zoomFactor = 1.0;

        public SvgViewer()
        {
            InitializeComponent();
            DataContext = this;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public double ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                if (Math.Abs(_zoomFactor - value) > 0.0001)
                {
                    _zoomFactor = value;
                    OnPropertyChanged();
                    Zoom();
                }
            }
        }

        public void LoadImage(string file)
        {
            var uri = new Uri(file);
            //var converted = uri.AbsoluteUri;
            _viewBox.Source = uri;
            _viewBox.AutoSize = false;

            _viewBox.Loaded += _viewBox_Loaded;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Zoom()
        {
            _viewBox.Height = _initHeight * ZoomFactor;
            _viewBox.Width = _initWidth * ZoomFactor;
        }

        private void _viewBox_Loaded(object sender, RoutedEventArgs e)
        {
            _initWidth = _viewBox.ActualWidth;
            _initHeight = _viewBox.ActualHeight;
        }
    }
}