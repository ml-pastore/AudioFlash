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
             _sw = new StreamWriter(value, true);
            _logFile = value;
            }
        get{
            return _logFile;
        }
    }
    public void Write(string s)
    {
         _sw.WriteLine(s);  
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