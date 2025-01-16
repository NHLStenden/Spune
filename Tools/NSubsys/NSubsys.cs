
using System.Runtime.InteropServices;

using static System.FormattableString;

namespace NSubsys;

/// <summary>
/// Main class for handling subsystem changes in PE files.
/// </summary>
public class NSubsys
{
    /// <summary>
    /// Gets or sets the target file path.
    /// </summary>
    public string TargetFile { get; set; } = string.Empty;

    /// <summary>
    /// Executes the subsystem change process.
    /// </summary>
    /// <returns>True if the process is successful; otherwise, false.</returns>
    public bool Execute()
    {
        var fileInfo = new FileInfo(TargetFile);

        if (!fileInfo.Exists)
            Console.WriteLine(Invariant($"File doesn't exist! Path: '{TargetFile}'"));

        if (fileInfo.Extension.Equals("exe", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine("This tool only supports PE .exe files.");

        return ProcessFile(fileInfo.FullName);
    }

    /// <summary>
    /// Processes the specified PE file to change its subsystem.
    /// </summary>
    /// <param name="exeFilePath">The path to the PE file.</param>
    /// <returns>True if the process is successful; otherwise, false.</returns>
    static bool ProcessFile(string exeFilePath)
    {
        Console.WriteLine("NSubsys Subsystem Changer for Windows PE files.");
        Console.WriteLine(Invariant($"[NSubsys] Target EXE `{exeFilePath}`."));

        using var utility = new PeUtility(exeFilePath);
        PeUtility.SubSystemType subsysVal;
        var subsysOffset = utility.MainHeaderOffset;

        subsysVal = (PeUtility.SubSystemType)utility.OptionalHeader.Subsystem;
        subsysOffset += Marshal.OffsetOf<PeUtility.IMAGE_OPTIONAL_HEADER>("Subsystem").ToInt32();

        switch (subsysVal)
        {
            case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI:
                Console.WriteLine("Executable file is already a Win32 App!");
                return true;
            case PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_CUI:
                Console.WriteLine("Console app detected...");
                Console.WriteLine("Converting...");

                var subsysSetting = BitConverter.GetBytes((ushort)PeUtility.SubSystemType.IMAGE_SUBSYSTEM_WINDOWS_GUI);

                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(subsysSetting);

                if (utility.Stream.CanWrite)
                {
                    utility.Stream.Seek(subsysOffset, SeekOrigin.Begin);
                    utility.Stream.Write(subsysSetting, 0, subsysSetting.Length);
                    Console.WriteLine("Conversion Complete...");
                }
                else
                {
                    Console.WriteLine("Can't write changes!");
                    Console.WriteLine("Conversion Failed...");
                }

                return true;
            default:
                Console.WriteLine(Invariant($"Unsupported subsystem : {Enum.GetName(typeof(PeUtility.SubSystemType), subsysVal)}."));
                return false;
        }
    }
}
