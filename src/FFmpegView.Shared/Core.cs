using FFmpeg.AutoGen;
using PCLUntils;
using PCLUntils.Objects;
using PCLUntils.Plantform;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace FFmpegView
{
    public sealed class Core
    {
        private Core() { }
        private static Core instance;
        public static Core Instance
        {
            get
            {
                instance ??= new();
                return instance;
            }
        }
        internal bool IsInitialize { get; private set; }
        public unsafe void Initialize(string libffmpegDirectoryPath = null)
        {
            try
            {
                if (libffmpegDirectoryPath.IsEmpty())
                {
                    var platform = string.Empty;
                    switch (PlantformUntils.System)
                    {
                        case Platforms.Linux:
                            platform = $"linux-{PlantformUntils.ArchitectureString}";
                            break;
                        case Platforms.MacOS:
                            platform = $"osx-{PlantformUntils.ArchitectureString}";
                            break;
                        case Platforms.Windows:
                            platform = PlantformUntils.IsArmArchitecture ? "win-arm64" : "win-x86";
                            break;
                    }
                    libffmpegDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libffmpeg", platform);
                }
                if (Directory.Exists(libffmpegDirectoryPath))
                {
                    Debug.WriteLine($"FFmpeg binaries found in: {libffmpegDirectoryPath}");
                    ffmpeg.RootPath = libffmpegDirectoryPath;
                    ffmpeg.avdevice_register_all();
                    ffmpeg.avformat_network_init();
                    ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);
                    av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
                    {
                        if (level > ffmpeg.av_log_get_level()) return;
                        var lineSize = 1024;
                        var lineBuffer = stackalloc byte[lineSize];
                        var printPrefix = 1;
                        ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                        var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                        Console.Write(line);
                    };
                    ffmpeg.av_log_set_callback(logCallback);
                    IsInitialize = true;
                }
                else
                {
                    IsInitialize = false;
                    Debug.WriteLine($"cannot found FFmpeg binaries from path:\"{libffmpegDirectoryPath}\"");
                }
            }
            catch (Exception ex)
            {
                IsInitialize = false;
                Debug.WriteLine(ex.Message);
            }
        }
    }
}