using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WPFVideoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Properties

        // Collections
        private ObservableCollection<VideoItem> _playlist = new ObservableCollection<VideoItem>();
        public ObservableCollection<VideoItem> Playlist
        {
            get { return _playlist; }
            set
            {
                _playlist = value;
                OnPropertyChanged(nameof(Playlist));
            }
        }

        private ObservableCollection<VideoItem> _playHistory = new ObservableCollection<VideoItem>();
        public ObservableCollection<VideoItem> PlayHistory
        {
            get { return _playHistory; }
            set
            {
                _playHistory = value;
                OnPropertyChanged(nameof(PlayHistory));
            }
        }

        // Timer for updating position
        private DispatcherTimer _timer;
        private bool _isDraggingSlider = false;
        private bool _isMuted = false;
        private bool _isPlaying = false;

        // Current video being played
        private VideoItem _currentVideo;
        public VideoItem CurrentVideo
        {
            get { return _currentVideo; }
            set
            {
                _currentVideo = value;
                OnPropertyChanged(nameof(CurrentVideo));
            }
        }

        #endregion

        #region Commands

        // Media Commands
        public ICommand PlayPauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand PreviousCommand { get; private set; }
        public ICommand MuteCommand { get; private set; }

        // Playlist Commands
        public ICommand AddVideoCommand { get; private set; }
        public ICommand AddFolderCommand { get; private set; }

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize commands
            InitializeCommands();

            // Setup timer for tracking playback position
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Set default volume
            mediaPlayer.Volume = 0.5;
        }

        #endregion

        #region Command Implementations

        private void InitializeCommands()
        {
            // Media Control Commands
            PlayPauseCommand = new RelayCommand(TogglePlayPause, param => Playlist.Count > 0);
            StopCommand = new RelayCommand(StopMedia, CanStopMedia);
            NextCommand = new RelayCommand(PlayNextMedia, CanPlayNextMedia);
            PreviousCommand = new RelayCommand(PlayPreviousMedia, CanPlayPreviousMedia);
            MuteCommand = new RelayCommand(ToggleMute, param => true);

            // Playlist Commands
            AddVideoCommand = new RelayCommand(AddVideoToPlaylist, param => true);
            AddFolderCommand = new RelayCommand(AddFolderToPlaylist, param => true);
        }

        private void TogglePlayPause(object parameter)
        {
            if (_isPlaying)
            {
                PauseMedia();
            }
            else
            {
                PlayMedia();
            }
        }

        private void PlayMedia()
        {
            if (CurrentVideo != null)
            {
                mediaPlayer.Play();
                _isPlaying = true;
                btnPlayPause.Content = "⏸"; // Pause symbol
            }
            else if (Playlist.Count > 0)
            {
                PlayVideoItem(Playlist[0]);
            }
        }

        private void PauseMedia()
        {
            if (mediaPlayer.CanPause)
            {
                mediaPlayer.Pause();
                _isPlaying = false;
                btnPlayPause.Content = "▶"; // Play symbol
            }
        }

        private bool CanStopMedia(object parameter)
        {
            return mediaPlayer.Source != null;
        }

        private void StopMedia(object parameter)
        {
            mediaPlayer.Stop();
            sliderProgress.Value = 0;
            _isPlaying = false;
            btnPlayPause.Content = "▶"; // Play symbol
        }

        private bool CanPlayNextMedia(object parameter)
        {
            return CurrentVideo != null && Playlist.Count > 1;
        }

        private void PlayNextMedia(object parameter)
        {
            if (CurrentVideo == null || Playlist.Count <= 1) return;

            int currentIndex = Playlist.IndexOf(CurrentVideo);
            if (currentIndex < Playlist.Count - 1)
            {
                PlayVideoItem(Playlist[currentIndex + 1]);
            }
            else
            {
                // Loop back to the first video if at the end
                PlayVideoItem(Playlist[0]);
            }
        }

        private bool CanPlayPreviousMedia(object parameter)
        {
            return CurrentVideo != null && Playlist.Count > 1;
        }

        private void PlayPreviousMedia(object parameter)
        {
            if (CurrentVideo == null || Playlist.Count <= 1) return;

            int currentIndex = Playlist.IndexOf(CurrentVideo);
            if (currentIndex > 0)
            {
                PlayVideoItem(Playlist[currentIndex - 1]);
            }
            else
            {
                // Loop back to the last video if at the beginning
                PlayVideoItem(Playlist[Playlist.Count - 1]);
            }
        }

        private void ToggleMute(object parameter)
        {
            _isMuted = !_isMuted;
            mediaPlayer.IsMuted = _isMuted;
            btnMute.Content = _isMuted ? "🔇" : "🔊";
        }

        private void AddVideoToPlaylist(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.mkv;*.avi;*.wmv;*.mov;*.flv;*.webm|All Files|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    Playlist.Add(new VideoItem(file));
                }

                // If this is the first video and nothing is playing, start playback
                if (CurrentVideo == null && Playlist.Count > 0)
                {
                    PlayVideoItem(Playlist[0]);
                }
            }
        }

        private void AddFolderToPlaylist(object parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string[] videoExtensions = { ".mp4", ".mkv", ".avi", ".wmv", ".mov", ".flv", ".webm" };
                string[] files = Directory.GetFiles(dialog.SelectedPath)
                    .Where(file => videoExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToArray();

                foreach (string file in files)
                {
                    Playlist.Add(new VideoItem(file));
                }

                // If this is the first video and nothing is playing, start playback
                if (CurrentVideo == null && Playlist.Count > 0)
                {
                    PlayVideoItem(Playlist[0]);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && !_isDraggingSlider)
            {
                // Update position slider
                double position = mediaPlayer.Position.TotalSeconds;
                double duration = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

                // Convert position to percentage (0-100)
                sliderProgress.Value = (position / duration) * 100;

                // Update duration text
                TimeSpan currentPosition = mediaPlayer.Position;
                TimeSpan totalDuration = mediaPlayer.NaturalDuration.TimeSpan;
                txtDuration.Text = $"{currentPosition:hh\\:mm\\:ss} / {totalDuration:hh\\:mm\\:ss}";
            }
        }

        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                sliderProgress.Value = 0;
                _isPlaying = true;
                btnPlayPause.Content = "⏸"; // Pause symbol
            }
        }

        private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextMedia(null);
        }

        private void mediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"Media failed to load: {e.ErrorException.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _isPlaying = false;
            btnPlayPause.Content = "▶"; // Play symbol
        }

        private void sliderProgress_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSlider = true;
        }

        private void sliderProgress_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSlider = false;

            // Only seek if we have a valid video
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                // Convert percentage (0-100) to time position
                double seekPercentage = sliderProgress.Value / 100.0;
                double durationInSeconds = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                TimeSpan newPosition = TimeSpan.FromSeconds(seekPercentage * durationInSeconds);

                mediaPlayer.Position = newPosition;
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Volume = sliderVolume.Value;

                // Update mute button state if needed
                if (sliderVolume.Value == 0 && !_isMuted)
                {
                    _isMuted = true;
                    mediaPlayer.IsMuted = true;
                    btnMute.Content = "🔇";
                }
                else if (sliderVolume.Value > 0 && _isMuted)
                {
                    _isMuted = false;
                    mediaPlayer.IsMuted = false;
                    btnMute.Content = "🔊";
                }
            }
        }

        private void TogglePlaylist_Click(object sender, RoutedEventArgs e)
        {
            gridPlaylist.Visibility = gridPlaylist.Visibility == Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }

        private void ToggleHistory_Click(object sender, RoutedEventArgs e)
        {
            gridHistory.Visibility = gridHistory.Visibility == Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }

        private void lstPlaylist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstPlaylist.SelectedItem != null)
            {
                PlayVideoItem((VideoItem)lstPlaylist.SelectedItem);
            }
        }

        private void lstHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstHistory.SelectedItem != null)
            {
                VideoItem selectedItem = (VideoItem)lstHistory.SelectedItem;

                // Check if the item is still in the playlist, if not add it
                if (!Playlist.Any(item => item.FilePath == selectedItem.FilePath))
                {
                    Playlist.Add(selectedItem);
                }

                PlayVideoItem(selectedItem);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Plays the specified video item
        /// </summary>
        private void PlayVideoItem(VideoItem videoItem)
        {
            if (videoItem == null) return;

            try
            {
                // Set the current video
                CurrentVideo = videoItem;

                // Set media source
                mediaPlayer.Source = new Uri(videoItem.FilePath);

                // Start playback
                mediaPlayer.Play();
                _isPlaying = true;
                btnPlayPause.Content = "⏸"; // Pause symbol

                // Add to history if not already there
                if (!PlayHistory.Any(item => item.FilePath == videoItem.FilePath))
                {
                    PlayHistory.Insert(0, videoItem);
                }
                else
                {
                    // If already in history, move to top
                    int index = PlayHistory.IndexOf(
                        PlayHistory.FirstOrDefault(item => item.FilePath == videoItem.FilePath));

                    if (index > 0)
                    {
                        PlayHistory.Move(index, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _isPlaying = false;
                btnPlayPause.Content = "▶"; // Play symbol
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #region RelayCommand Implementation

    /// <summary>
    /// Basic implementation of ICommand for binding in XAML
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    #endregion
}