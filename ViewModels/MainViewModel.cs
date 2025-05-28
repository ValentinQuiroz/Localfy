using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localfy.Models;
using Localfy.Services;
using Localfy.Views;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;

namespace Localfy.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
       
        private readonly PlaylistService playlistService = new();
        private readonly AudioPlayerService playerService = new();
        private readonly DispatcherTimer songTimer = new();

        //Flag to prevent the playerService to play a null reference 
        private bool firstSongSelection = false;
        public MainViewModel() 
        {
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
        private float volume = (float)0.5;

        private int sortNameValue = 0;
        private int sortDurationValue = 0;



        [RelayCommand]
        private void DeletePlaylist(Playlist playlist)
        {
            if (playlist == null) return;
            if (MessageBox.Show($"Delete playlist \"{playlist.Name}\"?", "Confirm delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                playlistService.DeletePlaylist(playlist);
                StopSong();
                LoadMainWindow();
            }
        }


        [RelayCommand]
        private void SortByDuration()
        {
            Debug.WriteLine(" SORTING DURATION M M M M M");

            switch (sortDurationValue)
            {
                case 0:
                    //Ascending 
                    CurrentSongsDisplay = new ObservableCollection<Song>(CurrentSongsDisplay.OrderBy(s => s.Duration));
                    sortDurationValue = 1;
                    break;
                case 1:
                    //Descending sort
                    sortNameValue = 0;
                    CurrentSongsDisplay = new ObservableCollection<Song>(CurrentSongsDisplay.OrderByDescending(s => s.Duration));
                    sortDurationValue = 2;
                    break;
                case 2:
                    //Default sort
                    sortNameValue = 0;
                    CurrentSongsDisplay = currentSongs;
                    sortDurationValue = 0;
                    break;
            }
        }

        [RelayCommand]
        private void SortByName()
        { 
            Debug.WriteLine(" SORTING NAMES M M M M M");

            switch (sortNameValue)
            {
                case 0:
                    //Ascending 
                    sortDurationValue = 0;
                    CurrentSongsDisplay = new ObservableCollection<Song>(CurrentSongsDisplay.OrderBy(s => s.Title));
                    sortNameValue = 1;
                    break;
                case 1:
                    //Descending sort
                    sortDurationValue = 0;
                    CurrentSongsDisplay = new ObservableCollection<Song>(CurrentSongsDisplay.OrderByDescending(s => s.Title));
                    sortNameValue = 2;
                    break;
                case 2:
                    //Default sort
                    sortDurationValue = 0;
                    CurrentSongsDisplay = currentSongs;
                    sortNameValue = 0;
                    break;
            }

        }


        [RelayCommand]
        private void DeleteSong(Song song)
        {
            if(SelectedPlaylist != null)
            {
                Debug.WriteLine(song.Title);
                Debug.WriteLine(SelectedPlaylist.Songs.Remove(song));
                
                playlistService.SavePlaylist(SelectedPlaylist);
                LoadPlaylist();
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
            if (playerService.isPlaying())
            {
                if (draggingStoppedAt != -1)
                {
                    playerService.SeekTo(draggingStoppedAt);
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
            songTimer.Interval = TimeSpan.FromSeconds(1);
            songTimer.Tick += OnTimerTick;
            songTimer.Start();
        }
        private void StopTimer()
        {
            songTimer.Stop();
            UpdateFooter();
        }
        
        //Keeps the trackbar updated every 1 second
        private void OnTimerTick(object? sender, EventArgs e)
        {
            TimeSpan current = playerService.CurrentTime;
            if (playerService.isPlaying())
            {

                //Prevents the currentSongTimerPosition to update while dragging
                if (IsDraggingSlider == false) CurrentSongTimerPosition = current.TotalSeconds;

                CurrentSongTimerPositionDisplay = FormatTime(current);
            }
            //Resets the progressbar and stops the timer from clicking if the audio reachs the end
            else StopTimer();
            
        }


        partial void OnVolumeChanged(float value)
        {
            playerService.SetVolume(value);
        }
        partial void OnSelectedPlaylistChanged(Playlist? value)
        {
            LoadPlaylist();
        }

        partial void OnSelectedSongChanged(Song? oldValue, Song? newValue)
        {
            if (newValue != null)
            {
                //When the first song to play is selected the flag allows to not loose reference along the execution
                if (firstSongSelection == false) firstSongSelection = true;
                //If a song is already playing prevents the footer to alter
                if (!playerService.isPlaying())
                {
                    UpdateFooter();
                }
            }
            else if (newValue == null && firstSongSelection == true) SelectedSong = oldValue;
        }
        

        [RelayCommand]
        private void LoadMainWindow()
        {
            
            Playlists = new ObservableCollection<Playlist>(playlistService.GetAllPlaylist());

            Debug.WriteLine(Playlists.Count);
            if (Playlists.Count > 0)
            {
                SelectedPlaylist = Playlists[0];
                LoadPlaylist();
            }
            //if there's no playlists create one by default
            else
            {
                MessageBox.Show("Please create a playlist");
                CreateNewPlaylist();
            }

        }

        [RelayCommand]
        private void LoadPlaylist()
        {
            if (SelectedPlaylist == null) return;
            currentSongs = playlistService.GetPlaylist(SelectedPlaylist.Id).Songs;
            CurrentSongsDisplay = currentSongs;
            UpdatePlaylistInfo();
        }

        private void UpdatePlaylistInfo()
        {
            TimeSpan totalPlaytime = playlistService.GetTotalPlayTime(SelectedPlaylist.Id);
            int totalHours = (int)totalPlaytime.TotalHours;
            int minutes = totalPlaytime.Minutes;

            HeaderTitle = SelectedPlaylist.Name;
            HeaderContent = $"{SelectedPlaylist.Songs.Count} songs, {totalHours} h {minutes} min";

        }


        [RelayCommand]
        private void CreateNewPlaylist()
        {
            CreatePlaylistWindow createPlaylistWindow = new CreatePlaylistWindow();
            if(createPlaylistWindow.ShowDialog() == true) LoadMainWindow();
        }


        [RelayCommand]
        private void PlaySong()
        {
            if (firstSongSelection == false) return;
            if (SelectedSong.Title == CurrentSongTitle && playerService.isPlaying()) return;

            StopSong();
            playerService.Play(SelectedSong.FilePath);
            playerService.SetVolume(Volume);
            StartTimer();
            UpdateFooter();
            firstSongSelection = true;
        }

        [RelayCommand]
        private void StopSong()
        {
            playerService.Stop();
            StopTimer();
        }


        [RelayCommand]
        private void AddSong()
        {
            if (SelectedPlaylist == null) return;

            OpenFileDialog ofd = new()
            {
                Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                foreach (var file in ofd.FileNames)
                {
                    try
                    {
                        var reader = new AudioFileReader(file);
                        var song = new Song(
                            System.IO.Path.GetFileName(file),
                            reader.TotalTime,
                            file
                        );
                        //avoids duplicated files
                        if (SelectedPlaylist.Songs.Contains(song)) continue;
                        SelectedPlaylist.Songs.Add(song);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid file" + file + " " + ex.Message);
                    }
                }
                playlistService.SavePlaylist(SelectedPlaylist);

                LoadPlaylist();
            }
        }

        private void UpdateFooter()
        {
            if(SelectedSong != null)
            {
                CurrentSongTitle = SelectedSong.Title;
                CurrentSongTotalDuration = SelectedSong.Duration.TotalSeconds;
                CurrentSongTotalDurationDisplay = FormatTime(SelectedSong.Duration);
                CurrentSongTimerPosition = 0;
                CurrentSongTimerPositionDisplay = "00:00";
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

