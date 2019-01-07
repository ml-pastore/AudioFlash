using System;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;


namespace AudioFlash
{
    class Program
    {

        public enum RetCodes {Success, InvalidParm, ConfigFileNotFound, InvalidConfigInit, NoRecs}

        static async Task Main(string[] args)
        {

            string configName = ""; 
            
            if(args.Length > 0)
                configName = args[0];
            else
                configName = "_samples/_bin/config_sample.json";
            
            Console.WriteLine($"{configName}");
            
            if(! File.Exists(configName))
            {
                Console.WriteLine($"File does not exist: {configName}");
                Environment.Exit((int) RetCodes.ConfigFileNotFound);
            }
        
            IConfig c = new Config();
          
            try
            {
                c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configName));
                c.Authentication = JsonConvert.DeserializeObject<Authentication>(File.ReadAllText(c.FileInPut.authFile));
            }
            catch(Exception e)
            {
                Environment.Exit((int) RetCodes.InvalidConfigInit);
            }

            ILogger lg = new Logger();
            lg.LogFile = String.Concat(c.FileOutPut.LogFolder, "/runLog.txt");
            lg.Write(new string('-', 50));
            lg.Write($"{DateTime.Now}");
           
            IEnumerable<CSVInput> allQuests = CSVInput.GetAllQuestions(c);
            
            // build wav output
            int fileCntr = c.FileOutPut.StartOutNum;
            int numRecs = allQuests.Where(x => x.IsActive.ToUpper() == "TRUE").Count();

            if(numRecs == 0)
              Environment.Exit((int) RetCodes.NoRecs);

            int numZerosCalc = Convert.ToInt32(Math.Log10(numRecs)+1);
            int numZeros =  Math.Max(c.FileOutPut.OutNumPad,numZerosCalc); 
            SoundUtil sndUtil = new SoundUtil();

            const int QUESTION = 0;
            const int ANSWER = 1;
            
            TTS_QA[] ttsQA = {new TTS_QA(), new TTS_QA()};
            string outFileNameBase = "";
            
            foreach (CSVInput ln in allQuests.Where(x => x.IsActive.ToUpper() == "TRUE"))
            {
                
                lg.Write("-".PadLeft(20,'-'));
                lg.Write($"Src file: {ln.SourceCSVFile}, Line#: {ln.FileLineNum}");

                outFileNameBase = string.Format(@"{0}/{1}.wav",c.FileOutPut.SoundFolder
                    , c.FileOutPut.WAVPrefix.Replace("{#}", fileCntr.ToString().PadLeft(numZeros,'0')));
                
                int pauseSec = Convert.ToInt32(ln.AnswerWaitSeconds) * 1000;
                string pauseText = $"<break time=\"{pauseSec}ms\"/>";

                string question = $"{ln.Question} {pauseText}";
        
                ttsQA[QUESTION] = new TTS_QA{QAText = question,
                    Lang = ln.QuesLang, ProsodyRate = ln.QuesProsodyRate, OutFile = outFileNameBase.Replace(".wav","_ques.wav")};

                ttsQA[ANSWER] =new TTS_QA{QAText = ln.Answer,
                    Lang = ln.AnsLang, ProsodyRate = ln.AnsProsodyRate, OutFile = outFileNameBase.Replace(".wav","_resp.wav")}; // append "resp" for sorting purposes if Q/A files are separate
                
                foreach(TTS_QA qa in ttsQA)
                {
                    TextToSpeech t = new TextToSpeech();

                    ISound sndOut = new SoundOutput(c.SoundDefault);
                    t.Sound = sndOut;

                    t.TextIn = qa.QAText;
                    t.Sound.Language = sndUtil.GetSoundProp(qa.Lang, c.SoundDefault.Language);
                    
                    // random speaker from avail list
                    string spkr = sndUtil.GetRandSpeakerName(t.Sound.Language, c.SoundDefault.Speakers);
                    t.Sound.Speaker = sndUtil.GetSoundProp(spkr, c.SoundDefault.Speaker);
                    t.Sound.ProsodyRate = sndUtil.GetSoundProp(qa.ProsodyRate, c.SoundDefault.ProsodyRate);

                    t.FileOut =  qa.OutFile ;

                    if(! string.IsNullOrEmpty(t.TextIn))
                    {
                        lg.Write($"Spkr: {t.Sound.Speaker}, Text: {t.TextIn.Substring(0, Math.Min(t.TextIn.Length, 20))} ... -> {t.FileOut}");
                    }

                    while(File.Exists(t.FileOut))
                        File.Delete(t.FileOut);

                    await t.OutPutTTS(c.Authentication);
                }

                if(! c.FileOutPut.SplitQAFiles) // need to merge them 
                {

                    var ques = new AudioFileReader(ttsQA[QUESTION].OutFile);
                    var ans = new AudioFileReader(ttsQA[ANSWER].OutFile);
                    
                    File.Delete(outFileNameBase);

                    ConcatenatingSampleProvider  playlist = new ConcatenatingSampleProvider(new[] { ques, ans});
                    
                    lg.Write($"\n---> Merging {ttsQA[QUESTION].OutFile} and {ttsQA[ANSWER].OutFile} to {outFileNameBase}");
                    
                    WaveFileWriter.CreateWaveFile16(outFileNameBase, playlist);

                    ques.Dispose();
                    ans.Dispose();
                    
                    File.Delete(ttsQA[QUESTION].OutFile);
                    File.Delete(ttsQA[ANSWER].OutFile);
                }
                    
              
                fileCntr++;
            }
            

            lg.Write("Done");
            lg.Dispose();

            Console.WriteLine($"Done");
            Environment.Exit((int) RetCodes.Success);
        
        }

     } // class Program
   
} // namespace AudioFlash
