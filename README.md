This is a ffmpeg.autogen binding library, for wpf and avalonia.

> # Avalonia
in Avalonia,To enable extension should be present in your Program.cs file:

```csharp
AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .UseFFmpeg()
        .LogToTrace();
```
AND Initialize USING 
```csharp
Core.Initialize(libffmpegDirectoryPath)
```
OR
```csharp
AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .UseFFmpeg(libffmpegDirectoryPath)
        .LogToTrace();
```
then use the `FFmpegView` in axaml or charp code, and you need handle audio stream [here](#audio). 

> # WPF
in Wpf,To enable extension should be present in your App.xaml.cs file:
```csharp
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        BassCore.Initialize();
        Core.Instance.Initialize();
    }
```
then use the `FFmpegView` in xaml,and you should set audio handle after `InitializeComponent` like:
```csharp
    public MainWindow()
    {
        InitializeComponent();
        playerView.SetAudioHandler(new NAudioStreamDecoder());
    }
```

> # Audio
for Audio Handle,you can use [FFmpegView.Bass](https://www.nuget.org/packages/FFmpegView.Bass) or [FFmpegView.NAudio](https://www.nuget.org/packages/FFmpegView.NAudio)
Bass:
```csharp
    var playerView = this.FindControl<FFmpegView>("playerView");
    playerView.SetAudioHandler(new BassAudioStreamDecoder());
    playerView.Play("http://vfx.mtime.cn/Video/2019/02/04/mp4/190204084208765161.mp4");
```
NAudio:
```csharp
        playerView.SetAudioHandler(new NAudioStreamDecoder());
```

> # MEDIA
you can use the `MediaItem` class the define the resources info,coding like
```csharp
    var mediaItem = new MediaItem(url);
    playerView.Play(mediaItem);
```