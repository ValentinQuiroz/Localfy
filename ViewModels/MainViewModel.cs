using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localfy.Models;
using Localfy.Services;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace Localfy.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IFileDialogService _fileDialogService;

        private readonly PlaylistService _playlistService;
        private readonly AudioPlayerService _audioPlayerService;
        private readonly DispatcherTimer _dispatcherTimer;

        //Flag to prevent the playerService to play a null reference 
        private bool firstSongSelection = false;
        public MainViewModel(IDialogService dialogService, IFileDialogService fileDialogService, PlaylistService playlistService, AudioPlayerService audioPlayerService, DispatcherTimer dispatcherTimer) 
        {
            _dialogService = dialogService;
            _fileDialogService = fileDialogService;
            _playlistService = playlistService;
            _audioPlayerService = audioPlayerService;
            _dispatcherTimer = dispatcherTimer;
            LoadMainWindow();
        }

        private double draggingStoppedAt = -1;
        [ObservableProperty]
        private bool isDraggingSlider;

        //TopBar properties
        [ObservableProperty]
        private string searchBarFilter;

        //Main body properties
        [ObservableProperty]
        private ObservableCollection<Playlist> playlists;

        [ObservableProperty]
        private Playlist? selectedPlaylist;

        private ObservableCollection<Song> currentSongs = new();
        [ObservableProperty]
        private ObservableCollection<Song> currentSongsDisplay;

        [ObservableProperty]
        private Song? selectedSong;

        [ObservableProperty]
        private string sortNameIcon = "Minimize";
        [ObservableProperty]
        private string sortDurationIcon = "Minimize";
        public enum SortDirection
        {
            Default,
            Ascending,
            Descending
        }

        private SortDirection _nameSortDirection = SortDirection.Default;
        private SortDirection _durationSortDirection = SortDirection.Default;

        //Header properties
        [ObservableProperty]
        private string headerTitle;

        [ObservableProperty]
        private string headerContent;

        //Footer properties
        [ObservableProperty]
        private string currentSongTitle;
        [ObservableProperty]
        private double currentSongTimerPosition;

        [ObservableProperty]
        private double currentSongTotalDuration;

        [ObservableProperty]
        private string currentSongTimerPositionDisplay;

        [ObservableProperty]
        private string currentSongTotalDurationDisplay;

        [ObservableProperty]
        private string playPauseIcon = "Play";

        [ObservableProperty]
        private string volumeIcon = "VolumeMedium";

        [ObservableProperty]
        private float volume = (float)0.5;

        


        [RelayCommand]
        private void PreviousSong()
        {
            if (CurrentSongsDisplay == null || CurrentSongsDisplay.Count == 0|| SelectedSong == null) return;

            int currentIndex = CurrentSongsDisplay.IndexOf(SelectedSong);
            
            if (currentIndex > 0)
            {
                Song previousSong = CurrentSongsDisplay[currentIndex - 1];
                PlaySong(previousSong);
            }
        }


        [RelayCommand]
        private void NextSong()
        {
            if (CurrentSongsDisplay == null || CurrentSongsDisplay.Count <= 0 || SelectedSong == null) return;

            int currentIndex = CurrentSongsDisplay.IndexOf(SelectedSong);

            if (currentIndex >= 0 && currentIndex < CurrentSongsDisplay.Count - 1)
            {
                Song nextSong = CurrentSongsDisplay[currentIndex + 1];
                PlaySong(nextSong);
            }

        }


        [RelayCommand]
        private void PlayPauseSong()
        {
            if (firstSongSelection == false) return;

            if (_audioPlayerService.isPlaying())
            {
                _audioPlayerService.Pause();
                PlayPauseIcon = "Play";
                StopTimer();
            }
            else if(_audioPlayerService.isPaused())
            {
                _audioPlayerService.Resume();
                PlayPauseIcon = "Pause";
                _audioPlayerService.SetVolume(Volume);
                StartTimer();
            }
            else
            {
                PlaySong(SelectedSong);
            }
        }


        [RelayCommand]
        private async Task DeletePlaylist(Playlist playlist)
        {
            if (playlist == null) return;

            bool confirm =await _dialogService.ShowConfirmationAsync("Delete Playlist",
                $"Are you sure you want to delete the playlist \"{playlist.Name}\"?");

            if (confirm)
            {
                _playlistService.DeletePlaylist(playlist);
                LoadMainWindow();
                if (_audioPlayerService.isPlaying() || _audioPlayerService.isPlaying()) return;
                UpdateFooter();
            }
        }


        [RelayCommand]
        private void SortByDuration()
        {
            if (SelectedPlaylist == null)
            {
                _dialogService.ShowMessageAsync("Error", "Please select a playlist first.");
                return;
            }

            _nameSortDirection = SortDirection.Default;
            SortNameIcon = "Minimize";

            _durationSortDirection = NextSortDirection(_durationSortDirection);
            SortDurationIcon = GetIconForDirection(_durationSortDirection);

            ApplySorting();
        }

        [RelayCommand]
        private void SortByName()
        {
            if (SelectedPlaylist == null)
            {
                _dialogService.ShowMessageAsync("Error", "Please select a playlist first.");
                return;
            }

            _durationSortDirection = SortDirection.Default;
            SortDurationIcon = "Minimize";

            _nameSortDirection = NextSortDirection(_nameSortDirection);
            SortNameIcon = GetIconForDirection(_nameSortDirection);
            ApplySorting();

        }
        private SortDirection NextSortDirection(SortDirection current)
        {
            return current switch
            {
                SortDirection.Default => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.Default,
                _ => SortDirection.Default
            };
        }

        private string GetIconForDirection(SortDirection direction)
        {
            return direction switch
            {
                SortDirection.Ascending => "KeyboardArrowUp",
                SortDirection.Descending => "KeyboardArrowDown",
                _ => "Minimize"
            };
        }
        private void ApplySorting()
        {
            IEnumerable<Song> sorted = CurrentSongsDisplay;

            if (_nameSortDirection == SortDirection.Ascending)
                sorted = sorted.OrderBy(s => s.Title);
            else if (_nameSortDirection == SortDirection.Descending)
                sorted = sorted.OrderByDescending(s => s.Title);
            else if (_durationSortDirection == SortDirection.Ascending)
                sorted = sorted.OrderBy(s => s.Duration);
            else if (_durationSortDirection == SortDirection.Descending)
                sorted = sorted.OrderByDescending(s => s.Duration);

            CurrentSongsDisplay = new ObservableCollection<Song>(sorted);
        }

        [RelayCommand]
        private async Task DeleteSong(Song song)
        {
            bool confirm = await _dialogService.ShowConfirmationAsync("Delete Song",
               $"Are you sure you want to delete the song \"{song.Title}\"?");

            if (SelectedPlaylist != null && confirm == true)
            {
                SelectedPlaylist.Songs.Remove(song);
                _playlistService.SavePlaylist(SelectedPlaylist);
                LoadPlaylist();

                //////// TEMP HARD CODED FOOTER UPDATE////////
                if (CurrentSongsDisplay.Count > 0 && song.Title == CurrentSongTitle)
                {
                    StopSong();
                    SelectedSong = CurrentSongsDisplay.FirstOrDefault();
                    UpdateFooter();
                }
                else if(CurrentSongsDisplay.Count <= 0)
                {
                    StopSong();
                    firstSongSelection = false;
                    UpdateFooter();
                }

                ///////
            }
        } 

        partial void OnSearchBarFilterChanged(string value)
        {
            if (value.Trim() == string.Empty)
            {
                LoadPlaylist();
                return;
            }

            ObservableCollection<Song> filteredSongs = new();

            foreach (Song song in currentSongs)
            {
                if (song.Title.ToLower().Contains(value.ToLower()))
                {
                    filteredSongs.Add(song);
                }
            }
            if (filteredSongs.Count > 0) CurrentSongsDisplay = filteredSongs;
        }




//<<<<---------------------------SLIDER---------------------------

//The slider drag & drop is triggered when the IsDraggingSlider property triggers from true to false (OnIsDraggingSliderChanged).
//Otherwise it means the slider value change was a tap

        [RelayCommand]
        public void SliderDragStarted() { IsDraggingSlider = true; }

        [RelayCommand]
        public void SliderDragStopped(double value) 
        {
            string action = AnalyzeSlideOrTap();
            draggingStoppedAt = value;

            if (action == "slide") IsDraggingSlider = false;
            else if (action == "tap") SeekToCurrentTimerPosition();
        }
        
// Allows the slider new value to update on the method OnIsDraggingSliderChanged
        private void SeekToCurrentTimerPosition()
        {
            if (_audioPlayerService.isPlaying() || _audioPlayerService.isPaused())
            {
                if (draggingStoppedAt != -1)
                {
                    _audioPlayerService.SeekTo(draggingStoppedAt);
                    CurrentSongTimerPosition = draggingStoppedAt;
                    draggingStoppedAt = -1;
                }
            }
        }
        partial void OnIsDraggingSliderChanged(bool oldValue, bool newValue)
        {
            if (IsDraggingSlider == false) SeekToCurrentTimerPosition();
        }

//---------------------------SLIDER--------------------------->>>>

        private void StartTimer()
        {
            //songTimer.Interval = TimeSpan.FromSeconds(1);
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(300);
            _dispatcherTimer.Tick += OnTimerTick;
            _dispatcherTimer.Start();
        }
        private void StopTimer()
        {
            _dispatcherTimer.Stop();
        }
        
        //Keeps the trackbar updated every 0.3 ms
        private void OnTimerTick(object? sender, EventArgs e)
        {
            TimeSpan current = _audioPlayerService.CurrentTime;
            if (_audioPlayerService.isPlaying())
            {

                //Prevents the currentSongTimerPosition to update while dragging
                if (IsDraggingSlider == false) CurrentSongTimerPosition = current.TotalSeconds;

                CurrentSongTimerPositionDisplay = FormatTime(current);
            }
            //Resets the progressbar and stops the timer from clicking if the audio reachs the end
            else 
            { 
                StopTimer();
                UpdateFooter();
            }
            
            
        }


        partial void OnVolumeChanged(float value)
        {
            SetVolumenIcon(value);

            if (_audioPlayerService == null) return;
            if (_audioPlayerService.isPlaying() || _audioPlayerService.isPaused()) 
            {
                _audioPlayerService.SetVolume(value);
            }
        }

        private void SetVolumenIcon(float value)
        {
            if (value == 0) VolumeIcon = "VolumeMute";
            else if (value < 0.3) VolumeIcon = "VolumeLow";
            else if (value < 0.7) VolumeIcon = "VolumeMedium";
            else VolumeIcon = "VolumeHigh";
        }

        partial void OnSelectedPlaylistChanged(Playlist? value)
        {
            LoadPlaylist();
            if (SelectedPlaylist == null) CurrentSongsDisplay = null;
        }

        partial void OnSelectedSongChanged(Song? oldValue, Song? newValue)
        {
            if (newValue != null)
            {
                if (firstSongSelection == false) firstSongSelection = true;
            }
            else if (newValue == null && firstSongSelection == true) SelectedSong = oldValue;
        }
        

        [RelayCommand]
        private void LoadMainWindow()
        {
            
            Playlists = new ObservableCollection<Playlist>(_playlistService.GetAllPlaylist());

            if (Playlists.Count > 0)
            {
                SelectedPlaylist = Playlists[0];
                LoadPlaylist();
            }
            else
            {
                StopSong();
                firstSongSelection = false;
                SelectedPlaylist = null;
                _dialogService.ShowMessageAsync("Welcome", "Please create a playlist");
            }

        }

        [RelayCommand]
        private void LoadPlaylist()
        {
            if (SelectedPlaylist == null) return;
            currentSongs = _playlistService.GetPlaylist(SelectedPlaylist.Id).Songs;
            CurrentSongsDisplay = currentSongs;
            UpdatePlaylistInfo();
        }

        private void UpdatePlaylistInfo()
        {
            TimeSpan totalPlaytime = _playlistService.GetTotalPlayTime(SelectedPlaylist.Id);
            int totalHours = (int)totalPlaytime.TotalHours;
            int minutes = totalPlaytime.Minutes;

            HeaderTitle = SelectedPlaylist.Name;
            HeaderContent = $"{SelectedPlaylist.Songs.Count} songs, {totalHours} h {minutes} min";

        }


        [RelayCommand]
        private async Task CreateNewPlaylist()
        {
            if (await _dialogService.ShowNewPlaylistDialogAsync()) LoadMainWindow();
        }

        [RelayCommand]
        private async Task EditPlaylist(Playlist playlist)
        {
            if (await _dialogService.ShowEditPlaylistDialogAsync(playlist)) LoadMainWindow();
        }


        [RelayCommand]
        private void PlaySong(Song? song)
        {
            if (firstSongSelection == false || song == null) return;

            StopSong();
            SelectedSong = song;

            _audioPlayerService.Play(SelectedSong.FilePath);
            _audioPlayerService.SetVolume(Volume);
            StartTimer();
            PlayPauseIcon = "Pause";
            UpdateFooter();
            firstSongSelection = true;
            SelectedSong = null;
        }

        [RelayCommand]
        private void StopSong()
        {
            _audioPlayerService.Stop();
            PlayPauseIcon = "Play";
            StopTimer();
            UpdateFooter();
        }


        [RelayCommand]
        private void AddSong()
        {

            if (SelectedPlaylist == null)
            {
                _dialogService.ShowMessageAsync("Error", "Please select a playlist first.");
                return;
            }

            string[]? files = _fileDialogService.OpenAudioFiles();

            if (files == null) return;

            foreach (var file in files)
            {
                try
                {
                    var song = CreateSongFromFilepath(file);
                    //avoids duplicated files
                    if (SelectedPlaylist.Songs.Contains(song)) continue;
                    SelectedPlaylist.Songs.Add(song);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessageAsync("Error", "Invalid file: " + file + "\n" + ex.Message);
                }
            }
            _playlistService.SavePlaylist(SelectedPlaylist);
            LoadPlaylist();
        }

        private Song CreateSongFromFilepath(string filePath)
        {
            var reader = new AudioFileReader(filePath);
            return new Song(Path.GetFileName(filePath), reader.TotalTime, filePath );
        }

        private void UpdateFooter()
        {
            if(SelectedSong != null && firstSongSelection == true)
            {
                CurrentSongTitle = SelectedSong.Title;
                CurrentSongTotalDuration = SelectedSong.Duration.TotalSeconds;
                CurrentSongTotalDurationDisplay = FormatTime(SelectedSong.Duration);
                CurrentSongTimerPosition = 0;
                CurrentSongTimerPositionDisplay = "00:00";
            }
            else
            {
                CurrentSongTitle = string.Empty;
                CurrentSongTimerPosition = 0;
                CurrentSongTotalDuration = 0;
                CurrentSongTimerPositionDisplay = "00:00";
                CurrentSongTotalDurationDisplay = "00:00";
            }
        }

//Sets the time format for display purposes: mm:ss || h:mm:ss
        private string FormatTime(TimeSpan time)
        {
            return time.TotalHours >= 1
                ? time.ToString(@"h\:mm\:ss") 
                : time.ToString(@"mm\:ss");
        }

//If StartDraggingSlider event was not triggered, it means that the slider value change was a tap
        private string AnalyzeSlideOrTap()
        {
            if (IsDraggingSlider == true) return "slide";
            else return "tap";
        }
       
    }
}

