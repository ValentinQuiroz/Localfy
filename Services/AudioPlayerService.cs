using NAudio.Wave;
using System.Diagnostics;

namespace Localfy.Services
{
    
    public class AudioPlayerService
    {
        private WaveOutEvent? waveOut;

        private AudioFileReader? audioFile;
        public TimeSpan CurrentTime => audioFile?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan TotalTime => audioFile?.TotalTime ?? TimeSpan.Zero;

        private bool _isPaused = false;
        
        public bool isPlaying()
        {
            //if(waveOut == null || waveOut.PlaybackState == PlaybackState.Stopped || waveOut.PlaybackState == PlaybackState.Paused) Debug.WriteLine("Not playing");
            //else Debug.WriteLine("Playing");
            if (audioFile == null || waveOut == null) return false; 
            return waveOut?.PlaybackState == PlaybackState.Playing;

        }

        public bool isPaused()
        {
            return _isPaused;
        }

        public void Play(string filePath)
        {
            Stop();
            waveOut = new WaveOutEvent();
            audioFile = new AudioFileReader(filePath);
            waveOut.Init(audioFile);
            waveOut.Play();

            _isPaused = false;
        }

        public void Stop() 
        {
            if(waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            _isPaused = false;
        }
        public void SeekTo(double seconds)
        {
            if(audioFile != null && waveOut != null)
            {
                TimeSpan newPosition = TimeSpan.FromSeconds(seconds);
                if(newPosition <= audioFile.TotalTime)
                {
                    audioFile.CurrentTime = newPosition;

                    //Buffer cleaning to avoid playback issues when seeking a paused song
                    if (_isPaused)
                    {
                        waveOut.Dispose();
                        waveOut = new WaveOutEvent();
                        waveOut.Init(audioFile);
                    }

                    if (audioFile.CurrentTime == TotalTime) Stop();
                }
            }
        }
        public void SetVolume(float volume)
        {
            if(waveOut != null) waveOut.Volume = volume;
        }

        public void Pause()
        {
            if (audioFile != null && waveOut != null)
            {
                waveOut.Pause();
                _isPaused = true;
            }
        }

        public void Resume()
        {
            if (audioFile != null && waveOut != null) 
            { 
                waveOut.Play();
                _isPaused = false;
            } 
        }
    }
}
