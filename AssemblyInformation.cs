using System.Diagnostics;
using System.Reflection;

namespace TimelapseStitcher;

public static class AssemblyInformation
{
    public static string? GetProductName()
    {
        return GetAssemblyInformation().ProductName;
    }

    public static string? GetCompanyName()
    {
        return GetAssemblyInformation().CompanyName;
        
    }

    private static FileVersionInfo GetAssemblyInformation()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        if (fvi == null)
        {
            throw new Exception($"Could not read {nameof(FileVersionInfo)} of assembly {assembly.FullName}");
        }
        return fvi;
    }
}