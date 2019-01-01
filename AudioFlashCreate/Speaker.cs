using System;

public interface ISound
{
    string SpeakVersion {set ; get;}
    string OutputFormat {set ; get;}
    string Language {set ; get;}
    string Speaker {set ; get;}
    string ProsodyRate {set ; get;}
}


public class SoundOutput : ISound
{

    public SoundOutput(ISoundDefault def)
    {
        this.SpeakVersion = def.SpeakVersion;
        this.OutputFormat = def.OutputFormat;
        this.Language = def.Language;
        this.ProsodyRate = def.ProsodyRate;
        
        this.Speaker = def.Speaker;
        

    }

    public string SpeakVersion {set ; get;}
    public string OutputFormat {set ; get;}
    public string Language {set ; get;}
    public string Speaker {set ; get;}
    public string ProsodyRate {set ; get;}

}
   
public class Speaker
{
    public string Language {set; get;}
    public string Voice {set; get;}

}
