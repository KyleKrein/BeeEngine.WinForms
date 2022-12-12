namespace BeeEngine;

[Flags]
public enum LogTo : byte
{
    /// <summary>
    /// No log output
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Console log output
    /// </summary>
    Console = 0x01,

    /// <summary>
    /// File log output
    /// </summary>
    File = 0x02
}
