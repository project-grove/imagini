namespace Imagini.Audio
{
    /// <summary>
    /// Defines audio sample formats.
    /// </summary>
    public enum AudioSampleFormat
    {
        
        U8 = 0x0008,
        S8 = 0x8008,
        U16LSB = 0x0010,
        S16LSB = 0x8010,
        U16MSB = 0x1010,
        S16MSB = 0x9010,
        U16 = U16LSB,
        S16 = S16LSB,
        S32LSB = 0x8020,
        S32MSB = 0x9020,
        S32 = S32LSB,
        F32LSB = 0x8120,
        F32MSB = 0x9120,
        F32 = F32LSB,
    }
}