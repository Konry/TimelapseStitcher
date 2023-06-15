using CommandLine;
using TimelapseStitcher;

namespace TimelapseStitcherTests;

public class OptionsValidations
{
    private static IEnumerable<IEnumerable<string>> MissingInputParameters()
    {
        // empty list
        yield return new List<string>();
        // in is missing
        yield return new List<string>()
        {
            "-o",
            @".\out",
            "-f",
            @".\bin",
            "-i"
        };
    }
        
    [TestCaseSource(nameof(MissingInputParameters))]
    public void MissingInputParameter(IEnumerable<string> arguments)
    {
        // Arrange
        // Act
        // Assert
        Parser.Default.ParseArguments<Options>(arguments).WithParsed(o =>
        {
            Assert.Fail("Required options are not set");
        }).WithNotParsed( o => 
        {
            Assert.Pass("Parsing successfully failed!");
        });
    }
    
    [Test]
    public void MinimalInputIsGiven()
    {
        // Arrange
        var arguments = new List<string>();
        InitNecessaryOptions(ref arguments);
        // Act
        // Assert
        Parser.Default.ParseArguments<Options>(arguments).WithParsed(o =>
        {
            Assert.Pass("Parsing successfully!");
        }).WithNotParsed( o => 
        {
            foreach (var error in o)
            {
                TestContext.WriteLine(error);
            }
            Assert.Fail("Parsing failed!");
        });
    }

    [TestCase("2023-11-03")]
    [TestCase("2023 11 03")]
    [TestCase("2023 11 03")]
    [TestCase("11.03.2023")]
    [TestCase("11-03-2023")]
    [TestCase("11 03 2023")]
    public void DateStringValidation(string dateString)
    {
        var arguments = new List<string>();
        InitNecessaryOptions(ref arguments);
        arguments.Add("-d");
        arguments.Add(dateString);

        Parser.Default.ParseArguments<Options>(arguments).WithParsed(o =>
        {
            Assume.That(o.Date, Is.Not.Null);
            
            var dateToBe = new DateOnly(2023, 11, 03);
            Assert.That(o.Date!.Value, Is.EqualTo(dateToBe));
        }).WithNotParsed( o => 
        {
            foreach (var error in o)
            {
                TestContext.WriteLine(error);
            }
            Assert.Fail("Parsing failed!");
        });
    }

    private void InitNecessaryOptions(ref List<string> arguments)
    {
        arguments.Add("-f");
        
        
        if (Environment.Is64BitProcess == false)
        {
            arguments.Add(@"runtimes\win7-x86\bin");
        }
        else
        {
            arguments.Add(@"runtimes\win7-x64\bin");
        }
        
        arguments.Add("-i");
        arguments.Add(@"C:\temp\Test");
        arguments.Add("-o");
        arguments.Add(@"C:\temp\Test\Out test");
    }
}