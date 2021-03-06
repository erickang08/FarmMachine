using System.IO;
using System.Text;
using CefSharp.Wpf;
using Serilog;

namespace FarmMachine.MonitorStrategy.Services
{
  public class LibraryLoader
  {
    private string[] _files;
    private StringBuilder _jsBuilder;

    public LibraryLoader()
    {
      _files = new[]
      {
        "Libs\\tv.lib.js"
      };
      _jsBuilder = new StringBuilder();
    }

    public void Init()
    {
      Log.Information("Loading js library: ");
      
      foreach (var file in _files)
      {
        Log.Information($"+ {file}");
        
        var fileSource = ReadFromFile(file);
        
        _jsBuilder.AppendLine(fileSource);
      }
    }

    public void Execute(ChromiumWebBrowser browser)
    {
      browser.ExecuteScriptAsync(_jsBuilder.ToString());
    }

    private string ReadFromFile(string pathToFile)
    {
      var source = string.Empty;
      
      using (var stream = new FileStream(pathToFile, FileMode.Open))
      {
        using (var reader = new StreamReader(stream))
        {
          source = reader.ReadToEnd();
        }
      }

      return source;
    }
  }
}