using System.Text.RegularExpressions;
using Serilog;

namespace TimelapseStitcher;

public class FolderSearcher
{
    private readonly ILogger _log;
    private readonly string _regexPattern = @"\d+_timelapse_(\d+-\d+-\d+_\d+-\d+-\d+)." + FileEnding;

    public FolderSearcher(ILogger log)
    {
        _log = log;
    }

    private const string FileEnding = "mp4";
    public List<string> GetAllFilesFrom(DateOnly date, string folder)
    {
        if (!Directory.Exists(folder))
        {
            throw new Exception($"Folder {folder} does not exist");
        }

        var files = Directory.GetFiles(folder,"*." + FileEnding)
            .Where(path =>
            {
                var matches = Regex.Match(path.ToLowerInvariant(), _regexPattern);;
                if (matches.Groups.Count > 1)
                {
                    try
                    {
                        var datetime = matches.Groups[1].Value;
                        var myDate = DateTime.ParseExact(datetime, "yyyy-MM-dd_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture);
                        var myDateOnly = DateOnly.FromDateTime(myDate);
                        if (myDateOnly == date)
                        {
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Warning("File {FileName} is not used due to a malformed date", Path.GetFileName(path));
                        return false;
                    }
                }
                return false;
            }).OrderBy(x => x).ToList();

        return files;
    }
}