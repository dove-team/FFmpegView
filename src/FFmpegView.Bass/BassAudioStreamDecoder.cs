using ManagedBass;

namespace FFmpegView.Bass
{
    public unsafe class BassAudioStreamDecoder : AudioStreamDecoder
    {
        private Errors error;
        private int decodeStream;
        public Errors LastError => error;
        public override void PauseCore()
        {
            ManagedBass.Bass.ChannelPause(decodeStream);
        }
        public override void StopCore()
        {
            ManagedBass.Bass.ChannelStop(decodeStream);
        }
        public override void Prepare()
        {
            if (decodeStream != 0)
                ManagedBass.Bass.StreamFree(decodeStream);
            decodeStream = ManagedBass.Bass.CreateStream(SampleRate, Channels, BassFlags.Mono, StreamProcedureType.Push);
            if (!ManagedBass.Bass.ChannelPlay(decodeStream, true))
                error = ManagedBass.Bass.LastError;
        }
        public override void PlayNextFrame(byte[] bytes)
        {
            if (ManagedBass.Bass.StreamPutData(decodeStream, bytes, bytes.Length) == -1)
                error = ManagedBass.Bass.LastError;
        }
    }
}