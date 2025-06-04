using NAudio.Wave;
using System.Diagnostics;

namespace Localfy.Services
{
    
    internal class AudioPlayerService
    {
        private WaveOutEvent? waveOut;

        private AudioFileReader? audioFile;
        public TimeSpan CurrentTime => audioFile?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan TotalTime => audioFile?.TotalTime ?? TimeSpan.Zero;

        public void Play(string filePath)
        {
            Stop();
            waveOut = new WaveOutEvent();
            audioFile = new AudioFileReader(filePath);
            waveOut.Init(audioFile);
            waveOut.Play();
        }

        public bool isPlaying()
        {
            if(waveOut == null || waveOut.PlaybackState == PlaybackState.Stopped) Debug.WriteLine("Not playing");
            else Debug.WriteLine("Playing");
            if (waveOut == null || waveOut.PlaybackState == PlaybackState.Stopped)
                return false;
            else
                return true;
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

        }
        public void SeekTo(double seconds)
        {
            if(audioFile != null && waveOut != null)
            {
                TimeSpan newPosition = TimeSpan.FromSeconds(seconds);
                if(newPosition <= audioFile.TotalTime)
                {
                    audioFile.CurrentTime = newPosition;

                    if (audioFile.CurrentTime == TotalTime) Stop();
                }
            }
        }
        public void SetVolume(float volume)
        {
            if(audioFile != null) audioFile.Volume = volume;
        }

        public void Pause()
        {
            if (audioFile != null && waveOut != null) waveOut.Pause();
        }
    }
}
