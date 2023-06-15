using CommandLine;
using Serilog.Events;

namespace TimelapseStitcher;

[Serializable]
public class Options
{
    [Option('d', "date", Required = false, HelpText = "The date to generate the timelapse from. Format has to be Year-Month-Day or Month-Day-Year.", Default = null)]
    public DateOnly? Date { get; set; }
    
    [Option('i', "input", Required = true, HelpText = "The input folder where the files are.")]
    public string? Folder { get; set; }
    
    [Option('o', "output-folder", Required = true, HelpText = "The output folder where the files are send to.")]
    public string? OutputFolder { get; set; }
    
    [Option( 'f',"ffmpeg-path", Required = true, HelpText = "The path to ffmpeg executable.")]
    public string? FfmpegPath { get; set; }
    
    [Option('l', "loglevel",
        Default = LogEventLevel.Debug,
        HelpText = "Lines to be printed from the beginning or end of the file.")]
    public LogEventLevel LogLevel { get; set; }
}