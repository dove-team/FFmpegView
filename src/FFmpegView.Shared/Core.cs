using FFmpeg.AutoGen;
using PCLUntils;
using PCLUntils.Objects;
using PCLUntils.Plantform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FFmpegView
{
    public sealed class Core
    {
        internal static bool IsInitialize { get; private set; }
        private static AVHWDeviceType current;
        public static AVHWDeviceType Current => current;
        /// <summary>
        /// init ffmpeg
        /// </summary>
        /// <param name="libffmpegDirectoryPath">ffmpeg libs folder path</param>
        /// <param name="logLevel">value from ffmpeg.AV_LOG_***</param>
        public static unsafe void Initialize(string libffmpegDirectoryPath = null, int logLevel = ffmpeg.AV_LOG_VERBOSE)
        {
            if (IsInitialize) return;
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
                    ffmpeg.av_log_set_level(logLevel);
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
        public static AVHWDeviceType GetHWDecoder(int inputDecoderNumber = 0)
        {
            var type = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            var availableHWDecoders = new Dictionary<int, AVHWDeviceType>();
            var number = 0;
            while ((type = ffmpeg.av_hwdevice_iterate_types(type)) != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                availableHWDecoders.Add(number, type);
                number++;
            }
            if (availableHWDecoders.Count == 0)
            {
                Console.WriteLine("Your system have no hardware decoders.");
                current = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE;
            }
            else
            {
                var decoderNumber = availableHWDecoders.SingleOrDefault(t => t.Value == AVHWDeviceType.AV_HWDEVICE_TYPE_DXVA2).Key;
                if (decoderNumber == 0)
                    decoderNumber = availableHWDecoders.FirstOrDefault().Key;
                availableHWDecoders.TryGetValue(inputDecoderNumber == 0 ? decoderNumber : inputDecoderNumber, out current);
            }
            return current;
        }
    }
}