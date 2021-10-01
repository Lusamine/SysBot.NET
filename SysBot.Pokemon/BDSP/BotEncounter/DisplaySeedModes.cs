namespace SysBot.Pokemon
{
    public enum DisplaySeedMode
    {
        /// <summary>
        /// Copy out the global RNG state as a 128-bit value.
        /// </summary>
        Bit128,

        /// <summary>
        /// Copy out the global RNG state as 2 64-bit values.
        /// </summary>
        Bit64,

        /// <summary>
        /// Copy out the global RNG state as 2 64-bit values in the order PokeFinder expects.
        /// </summary>
        Bit64PokeFinder,

        /// <summary>
        /// Copy out the global RNG state as 4 32-bit values in the order CaptureSight displays.
        /// </summary>
        Bit32,
    }
}
