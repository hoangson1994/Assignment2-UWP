using FFmpegInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Assignmennt2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    // --- Implement interface INotifyPropertyChanged to update view when soucre change ---
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private FFmpegInteropMSS FFmpegMSS;
        private ObservableCollection<StorageFile> videos = new ObservableCollection<StorageFile>();       
        public ObservableCollection<StorageFile> Videos
        {
            get => videos;
            set
            {
                if (videos != value)
                {
                    videos = value;
                    OnPropertyChanged();

                }
            }
        }
        private ObservableCollection<StorageFile> musics = new ObservableCollection<StorageFile>();

        public ObservableCollection<StorageFile> Musics
        {
            get => musics;
            set
            {
                if (musics != value)
                {
                    musics = value;
                    OnPropertyChanged();

                }
            }
        }

        int onPlay = 0;
        private bool isPlaying = false;

        public MainPage()
        {
            this.InitializeComponent();
            volumeSlider.Value = 100;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        // ---- Video Player -----
        private async void ChooseVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderPicker oFolderPicker = new FolderPicker();
                oFolderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                oFolderPicker.FileTypeFilter.Add(".mp4");
                StorageFolder folder = await oFolderPicker.PickSingleFolderAsync();

                IReadOnlyList<StorageFile> listFiles = await folder.GetFilesAsync();


                if (listFiles != null)
                {
                    ObservableCollection<StorageFile> videos = new ObservableCollection<StorageFile>();
                    foreach (var f in listFiles)
                    {
                        if (f.FileType == ".MP4" || f.FileType == ".mp4")
                        {

                            videos.Add(f);
                        }
                    }

                    Videos = videos;

                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.InnerException);
            }
        }

        private async void Video_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile fileSelected =  (StorageFile)((StackPanel)sender).Tag;
            if (fileSelected != null)
            {
                if(VideoPlayer.CanPause == true)
                {
                    try
                    {
                        VideoPlayer.Pause();

                    }
                    catch(Exception)
                    {

                    }
                }
                else
                {
                    try
                    {
                        VideoPlayer.Stop();
                    }
                    catch (Exception)
                    {

                    }
                    
                }

                IRandomAccessStream readStream = await fileSelected.OpenAsync(FileAccessMode.Read);

                try
                {
                    FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, true, true);
                    MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();
                    if(mss != null)
                    {
                        VideoPlayer.AreTransportControlsEnabled = true;

                        VideoPlayer.TransportControls.IsFastForwardButtonVisible = true;
                        VideoPlayer.TransportControls.IsFastForwardEnabled = true;

                        VideoPlayer.TransportControls.IsFastRewindButtonVisible = true;
                        VideoPlayer.TransportControls.IsFastRewindEnabled = true;

                        VideoPlayer.TransportControls.IsNextTrackButtonVisible = true;                  
                        VideoPlayer.TransportControls.IsPreviousTrackButtonVisible = true;

                        VideoPlayer.TransportControls.IsPlaybackRateButtonVisible = true;
                        VideoPlayer.TransportControls.IsPlaybackRateEnabled = true;

                        VideoPlayer.TransportControls.IsSkipBackwardButtonVisible = true;
                        VideoPlayer.TransportControls.IsSkipBackwardEnabled = true;

                        VideoPlayer.TransportControls.IsSkipForwardButtonVisible = true;
                        VideoPlayer.TransportControls.IsSkipForwardEnabled = true;

                        VideoPlayer.TransportControls.IsStopButtonVisible = true;
                        VideoPlayer.TransportControls.IsStopEnabled = true;

                        VideoPlayer.TransportControls.IsRightTapEnabled = true;

                        VideoPlayer.SetMediaStreamSource(mss);

                        VideoPlayer.Play();
                    }
                    else
                    {
                        var msg = new MessageDialog("error");
                        await msg.ShowAsync();
                    }
                }
                catch(Exception)
                {

                }
            }
        }

        // --- Music Player  ---

        private async void ChooseMusic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderPicker oFolderPicker = new FolderPicker();
                oFolderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                oFolderPicker.FileTypeFilter.Add(".mp3");
                StorageFolder folder = await oFolderPicker.PickSingleFolderAsync();

                IReadOnlyList<StorageFile> listFiles = await folder.GetFilesAsync();


                if (listFiles != null)
                {
                    ObservableCollection<StorageFile> newM = new ObservableCollection<StorageFile>();
                    foreach (var f in listFiles)
                    {
                        Debug.WriteLine(f.FileType);
                        if (f.FileType == ".mp3")
                        {

                            newM.Add(f);
                        }
                    }


                    Musics = newM;                 

                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.InnerException);
            }
        }
        private void timer_Tick(object sender, object e)
        {
            if (MusicPlayer.NaturalDuration.HasTimeSpan)
            {
                Progress.Minimum = 0;
                Progress.Maximum = MusicPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Progress.Value = MusicPlayer.Position.TotalSeconds;
            }
        }

        private async void Music_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile selectedSong = (StorageFile)((StackPanel)sender).Tag;
            MusicPlayer.SetSource(await selectedSong.OpenAsync(FileAccessMode.Read), selectedSong.ContentType);
            onPlay = ListMusic.SelectedIndex;
            this.nowPlaying.Text = selectedSong.Name;
            playSong();
        }    

        private async void loadSong(StorageFile currentSong)
        {
            this.nowPlaying.Text = currentSong.Name;
            MusicPlayer.SetSource(await currentSong.OpenAsync(FileAccessMode.Read), currentSong.ContentType);
        }

        private void playSong()
        {
            MusicPlayer.Play();
            PlayButton.Icon = new SymbolIcon(Symbol.Pause);
            isPlaying = true;
        }

        private void pauseSong()
        {
            MusicPlayer.Pause();
            PlayButton.Icon = new SymbolIcon(Symbol.Play);
            isPlaying = false;

        }

        private void resumeSong()
        {
            if (!isPlaying)
            {
                playSong();
            }
        }

        private void playBack(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Stop();
            if (onPlay > 0)
            {
                onPlay = onPlay - 1;

            }
            else
            {
                onPlay = Musics.Count - 1;
            }
            loadSong(Musics[onPlay]);
            playSong();
            ListMusic.SelectedIndex = onPlay;
        }

        private void playNext(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Stop();
            if (onPlay < Musics.Count - 1)
            {
                onPlay = onPlay + 1;
            }
            else
            {
                onPlay = 0;
            }
            loadSong(Musics[onPlay]);
            playSong();
            ListMusic.SelectedIndex = onPlay;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                pauseSong();

            }
            else
            {
                playSong();
            }
        }

        private void volumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider vol = sender as Slider;

            if (vol != null)
            {
                MusicPlayer.Volume = vol.Value / 100;

                this.volume.Text = vol.Value.ToString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
