using PCLUntils.Objects;
using PCLUntils.Plantform;
using PCLUntils;
using System;
using System.IO;
using System.Diagnostics;

namespace FFmpegView.Bass
{
    public sealed class BassCore
    {
        internal static bool IsInitialize { get; private set; } = false;
        private BassCore() { }
        public static void Initialize()
        {
            InitDll();
            IsInitialize = ManagedBass.Bass.Init();
        }
        private static bool InitDll()
        {
            bool canInit = true;
            try
            {
                string sourceFileName = string.Empty, dllPath = string.Empty;
                switch (PlantformUntils.System)
                {
                    case Platforms.Linux:
                        {
                            dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libbass.so");
                            if (!File.Exists(dllPath))
                            {
                                var platform = $"linux-{PlantformUntils.ArchitectureString}";
                                sourceFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libBass", platform, "libbass.so");
                            }
                            break;
                        }
                    case Platforms.MacOS:
                        {
                            dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libbass.dylib");
                            if (!File.Exists(dllPath))
                                sourceFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libBass", "osx", "libbass.dylib");
                            break;
                        }
                    case Platforms.Windows:
                        {
                            dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bass.dll");
                            if (!File.Exists(dllPath))
                            {
                                var platform = $"win-{PlantformUntils.ArchitectureString}";
                                if (platform.Equals("win-arm", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    canInit = false;
                                    Debug.WriteLine("Bass cannot run in win-arm platform.Stop init.");
                                }
                                else
                                    sourceFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libBass", platform, "bass.dll");
                            }
                            break;
                        }
                }
                if (sourceFileName.IsNotEmpty() && canInit)
                {
                    if (File.Exists(sourceFileName))
                        File.Copy(sourceFileName, dllPath, true);
                    else
                        canInit = false;
                }
            }
            catch (Exception ex)
            {
                canInit = false;
                Debug.WriteLine(ex.Message);
            }
            return canInit;
        }
    }
}