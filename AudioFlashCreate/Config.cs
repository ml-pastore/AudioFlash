using System;

public interface IConfig
{
    Authentication Authentication {set; get;}
    string SpeakVersion {set ; get;}
    string OutputFormat {set ; get;}
    string Voice {set ; get;}
    FileOutput FileOutPut {set; get;}
    FileInput FileInPut {set; get;}
}
public class Config : IConfig
{

    public Authentication Authentication {set; get;}
    public string SpeakVersion {set ; get;}
    public string OutputFormat {set ; get;}
    public string Voice {set ; get;}
    public FileOutput FileOutPut {set; get;}
    public FileInput FileInPut {set; get;}

}

public class FileInput
{
    public string authFile { get; set; }
}

public class FileOutput
{
    public string SoundFolder { get; set; }
    public string TextFolder { get; set; }
    public string LogFolder { get; set; }
}


