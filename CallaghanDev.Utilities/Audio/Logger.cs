using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace CallaghanDev.Utilities.Audio
{
    class AudioRecorder
    {
        private WasapiLoopbackCapture capture;
        private WaveFileWriter writer;

        public AudioRecorder()
        {
            capture = new WasapiLoopbackCapture();
            capture.DataAvailable += OnDataAvailable;
            capture.RecordingStopped += OnRecordingStopped;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            writer?.Dispose();
            writer = null;
            capture.Dispose();
        }

        public void StartRecording(string outputFilePath)
        {
            writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);
            capture.StartRecording();
        }

        public void StopRecording()
        {
            capture.StopRecording();
        }
    }
}
