using System;
using System.Collections.Generic;

public interface IConfig
{
    IAuthentication Authentication {set; get;}
    SoundDefault SoundDefault {set; get;}
    FileOutput FileOutPut {set; get;}
    FileInput FileInPut {set; get;}
}
public class Config : IConfig
{

    public IAuthentication Authentication {set; get;}
    public SoundDefault SoundDefault {set; get;}
    public FileOutput FileOutPut {set; get;}
    public FileInput FileInPut {set; get;}
}


public interface ISoundDefault : ISound
{
    List<Speaker> Speakers {set ; get;}
}
public class SoundDefault : ISoundDefault
{
    public string SpeakVersion {set ; get;}
    public string OutputFormat {set ; get;}
    public string Language {set ; get;}
    public string Speaker {set ; get;}
    public List<Speaker> Speakers {set ; get;}
    public string ProsodyRate {set ; get;}
    
}




