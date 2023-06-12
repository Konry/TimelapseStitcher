using CommandLine;
using Serilog.Events;

namespace TimelapseStitcher;

[Serializable]
public class Options
{
    [Option('f', "folder", Required = true, HelpText = "The folder where the files are.")]
    public string? Folder { get; set; }
    
    [Option('o', "output-folder", Required = true, HelpText = "The folder where the files are send to.")]
    public string? OutputFolder { get; set; }
    
    [Option( "ffmpeg-path", Required = true, HelpText = "The path to ffmpeg executable.")]
    public string? FfmpegPath { get; set; }
    
    [Option('l', "loglevel",
        Default = LogEventLevel.Debug,
        HelpText = "Lines to be printed from the beginning or end of the file.")]
    public LogEventLevel LogLevel { get; set; }
}