using System.Runtime.InteropServices;

namespace NSubsys;

/// <summary>
/// Utility class for handling PE (Portable Executable) files.
/// </summary>
internal class PeUtility : IDisposable
{
    /// <summary>
    /// Enum representing the subsystem type of the PE file.
    /// </summary>
    public enum SubSystemType : ushort
    {
        /// <summary>
        /// Windows GUI subsystem.
        /// </summary>
        IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,

        /// <summary>
        /// Windows CUI subsystem.
        /// </summary>
        IMAGE_SUBSYSTEM_WINDOWS_CUI = 3
    }

    /// <summary>
    /// Struct representing the DOS header of the PE file.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_DOS_HEADER
    {
        /// <summary>
        /// Offset to the new PE header.
        /// </summary>
        [FieldOffset(60)]
        public uint e_lfanew;
    }

    /// <summary>
    /// Struct representing the optional header of the PE file.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_OPTIONAL_HEADER
    {
        /// <summary>
        /// Subsystem type of the PE file.
        /// </summary>
        [FieldOffset(68)]
        public ushort Subsystem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PeUtility"/> class.
    /// </summary>
    /// <param name="filePath">The path to the PE file.</param>
    public PeUtility(string filePath)
    {
        Stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
        var reader = new BinaryReader(Stream);
        var dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

        // Seek the new PE Header and skip NtHeadersSignature (4 bytes) & IMAGE_FILE_HEADER struct (20bytes).
        Stream.Seek(dosHeader.e_lfanew + 4 + 20, SeekOrigin.Begin);

        MainHeaderOffset = Stream.Position;
        optionalHeader = FromBinaryReader<IMAGE_OPTIONAL_HEADER>(reader);
    }

    /// <summary>
    /// Reads in a block from a file and converts it to the struct type specified by the template parameter.
    /// </summary>
    /// <typeparam name="T">The type of the struct.</typeparam>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The struct read from the binary reader.</returns>
    public static T FromBinaryReader<T>(BinaryReader reader) where T : struct
    {
        // Read in a byte array
        var bytes = reader.ReadBytes(Marshal.SizeOf<T>());

        // Pin the managed memory while, copy it out the data, then unpin it
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        var theStructure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        handle.Free();

        return theStructure;
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="PeUtility"/> class.
    /// </summary>
    public void Dispose() => Stream.Dispose();

    /// <summary>
    /// Gets the optional header.
    /// </summary>
    public IMAGE_OPTIONAL_HEADER OptionalHeader => optionalHeader;

    /// <summary>
    /// Gets the PE file stream for read/write functions.
    /// </summary>
    public FileStream Stream { get; }

    /// <summary>
    /// Gets the main header offset.
    /// </summary>
    public long MainHeaderOffset { get; }

    IMAGE_OPTIONAL_HEADER optionalHeader;
}
