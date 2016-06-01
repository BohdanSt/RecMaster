using System;
using NAudio.Dsp;

namespace RecMaster.Audio
{
    class EqualizerForWrite
    {
        private readonly EqualizerBand[] bands;
        private readonly BiQuadFilter[,] filters;
        private readonly int channels;
        private readonly int bandCount;
        private readonly int sampleRate;
        private bool updated;

        public EqualizerForWrite(EqualizerBand[] bands, int channels, int sampleRate)
        {
            this.bands = bands;
            this.channels = channels;
            this.sampleRate = sampleRate;
            bandCount = bands.Length;
            filters = new BiQuadFilter[channels, bands.Length];
            CreateFilters();
        }

        private void CreateFilters()
        {
            for (int bandIndex = 0; bandIndex < bandCount; bandIndex++)
            {
                var band = bands[bandIndex];
                for (int n = 0; n < channels; n++)
                {
                    if (filters[n, bandIndex] == null)
                        filters[n, bandIndex] = BiQuadFilter.PeakingEQ(sampleRate, band.Frequency, band.Bandwidth, band.Gain);
                    else
                        filters[n, bandIndex].SetPeakingEq(sampleRate, band.Frequency, band.Bandwidth, band.Gain);
                }
            }
        }

        public void Update()
        {
            updated = true;
            CreateFilters();
        }

        public void Transform(byte[] bbuffer, int offset, int count)
        {
            if (updated)
            {
                CreateFilters();
                updated = false;
            }

            for (int n = 0; n < count; n += 4)
            {
                int ch = n % channels;

                for (int band = 0; band < bandCount; band++)
                {
                    byte[] transformed = BitConverter.GetBytes(filters[ch, band].Transform(BitConverter.ToSingle(bbuffer, offset + n)));
                    Buffer.BlockCopy(transformed, 0, bbuffer, offset + n, 4);
                }
            }
        }
    }
}
