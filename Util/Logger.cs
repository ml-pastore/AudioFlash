using System;
using System.IO;

public interface ILogger: IDisposable
{
    string LogFile{set; get;}
    void Write(string s);
 
}

public class Logger : ILogger, IDisposable
{

    private StreamWriter _sw;
    private string _logFile;
    public string LogFile{
        set{
            _logFile = value;
            }
        get{
            return _logFile;
        }
    }
    public void Write(string s)
    {
          _sw = new StreamWriter(_logFile, true);
         _sw.WriteLine(s);  
         _sw.Close();
    }
 
    public void Dispose()
    {

        //Free managed resources too
        if (_sw != null)
        {
            _sw.Close();
        }
    }
    

}