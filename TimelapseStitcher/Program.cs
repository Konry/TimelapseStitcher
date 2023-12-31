﻿using CommandLine;
using FFMpegCore;
using FFMpegCore.Exceptions;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using TimelapseStitcher;

var levelSwitch = new LoggingLevelSwitch
{
    MinimumLevel = LogEventLevel.Warning
};

var commonApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
if (!commonApplicationFolder.EndsWith(Path.DirectorySeparatorChar))
{
    commonApplicationFolder += Path.DirectorySeparatorChar;
}

var pathToLogFile = commonApplicationFolder + AssemblyInformation.GetCompanyName() + Path.DirectorySeparatorChar +
                    AssemblyInformation.GetProductName() + Path.DirectorySeparatorChar;
if (!Directory.Exists(pathToLogFile))
{
    try
    {
        Directory.CreateDirectory(pathToLogFile);
    }
    catch (Exception)
    {
        // logging in common application folder does not work, just ignore
        pathToLogFile = "";
    }
}

var log = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch).WriteTo.Console().WriteTo
    .File($@"{pathToLogFile}log-.txt", fileSizeLimitBytes: 100L * 1024 * 1024, rollingInterval: RollingInterval.Month).CreateLogger();

Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
{
    // parsing successful; go ahead and run the app
    levelSwitch.MinimumLevel = o.LogLevel;
    log.Information("{ProductName} - {Version}", AssemblyInformation.GetProductName(), AssemblyInformation.GetProductVersion());
    log.Information("Using following options");
    log.Information("checking folder {Folder}", o.Folder );
    log.Information("write output to {OutputFolder}", o.OutputFolder);
    if (o.Date != null)
    {
        log.Information("check timelapse from date {Date}", o.Date);
    }
    log.Information("LogLevel {LogLevel}", o.LogLevel);
    if (!o.OutputFolder!.EndsWith(Path.DirectorySeparatorChar))
    {
        o.OutputFolder += Path.DirectorySeparatorChar;
    }

    log.Information("Check folder for videos");
    
    DateOnly dateOnly = o.Date ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    var files = new FolderSearcher(log).GetAllFilesFrom(dateOnly, o.Folder!);
    if (files.Count == 0)
    {
        log.Warning("No files for the {Date}", dateOnly.ToString());
        return;
    }
    
    log.Information("Found {AmountOfFiles} in folder {Folder}", files.Count, o.Folder);

    log.Information("Using ffmpeg path {Path}", o.FfmpegPath);
    GlobalFFOptions.Configure(new FFOptions
    {
        BinaryFolder = o.FfmpegPath!
    });


    var t = Task.Run(() =>
    {
        log.Information("Starting to join the timelapse");
        var outputFileFormat = o.OutputFolder + $"TimeLapse-{dateOnly.ToString("yyyy-MM-dd")}.mp4";

        var errorCounter = 0;
        while (errorCounter < 100)
        {
            try
            {
                FFMpeg.Join(outputFileFormat, files.ToArray());
                log.Information("Join process finished {OutputFileNameAndFolder}", outputFileFormat);
            }
            catch (FFMpegException e)
            {
                errorCounter++;
                var toRemove = "";
                foreach (var file in files.Where(file => e.Message.Contains(file)))
                {
                    toRemove = file;
                }

                if (toRemove != "")
                {
                    files.Remove(toRemove);
                    log.Warning("Remove file {FileName} to try again, if stitching is running", toRemove);
                }
                else
                {
                    log.Warning("No file to remove found, cancel ");
                    break;
                }
                log.Error("ffmpeg does not work with error {Error}", e.Message);
                continue;
            }
            catch (Exception ex)
            {
                log.Error("Unhandled exception raised {Error}", ex.Message);
            }

            break;
        }
    });
    t.Wait();
}).WithNotParsed(e =>
{
    // parsing unsuccessful; deal with parsing errors
    log.Error("Parsing of commandline unsuccessful, please check input");

    foreach (var error in e)
    {
        log.Error("{ErrorMessage}", error.ToString());
    }
});