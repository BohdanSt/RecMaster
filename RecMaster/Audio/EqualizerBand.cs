namespace RecMaster.Audio
{
    class EqualizerBand
    {
        public float Frequency;
        public float Bandwidth;
        public float Gain;

        public EqualizerBand(float frequency, float bandwidth, float gain)
        {
            Frequency = frequency;
            Bandwidth = bandwidth;
            Gain = gain;
        }
    }
}
