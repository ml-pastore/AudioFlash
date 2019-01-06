using System;
using System.Collections.Generic;
using System.Linq;
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

public class SoundUtil
{
    public string GetSoundProp(string sndProp, string sndDefault)
    {
        string ret = sndProp.ToUpper().Trim() == "DEFAULT" ? sndDefault : sndProp;
        return ret;
    }

    public string GetRandSpeakerName(string lang, List<Speaker> speakers)
    {

        List<Speaker> spkrs = speakers.Where(x => x.Language.ToUpper().Trim() == lang.ToUpper().Trim()).ToList();
        int cnt = spkrs.Count;

        if(cnt == 0)
            return "DEFAULT";

        var rand = new Random();
        var spkr = spkrs[rand.Next(spkrs.Count)].Voice;

        return spkr;
        
    }


}   
public class Speaker
{
    public string Language {set; get;}
    public string Voice {set; get;}

 


}
