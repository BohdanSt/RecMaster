using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using NAudio.Wave;

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

            float[] buffer = new float[count];
            for (int i = 0; i < count; i++)
                buffer[i] = bbuffer[i];

            for (int n = 0; n < count; n++)
            {
                int ch = n % channels;

                for (int band = 0; band < bandCount; band++)
                {
                    buffer[offset + n] = filters[ch, band].Transform(buffer[offset + n]);
                }
            }

            for (int i = 0; i < count; i++)
            {
                bbuffer[i] = (byte)((buffer[i] >= 0f)?(buffer[i]):(buffer[i] * 1));
            }
        }
    }
}
