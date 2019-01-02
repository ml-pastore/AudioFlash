using System;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AudioFlash
{
    class Program
    {

        public enum RetCodes {Success, InvalidParm, ConfigFileNotFound, InvalidConfigInit, NoRecs}

        static async Task Main(string[] args)
        {

            string configName = "_bin/config.json";
            
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

            IEnumerable<CSVInput> allQuests = GetAllQuestions(c);
            
            // build wav output
            int fileCntr = 0;
            int numRecs = allQuests.Where(x => x.IsActive.ToUpper() == "TRUE").Count();

            if(numRecs == 0)
              Environment.Exit((int) RetCodes.NoRecs);

            int numZeros = Convert.ToInt32(Math.Log10(numRecs));
            
            foreach (CSVInput ln in allQuests.Where(x => x.IsActive.ToUpper() == "TRUE"))
            {

                // question and answer might have different output characteristics (lang, voice, prosody rate)

               
                string outFileName = string.Format(@"{0}\{1}.wav",c.FileOutPut.SoundFolder
                    , c.FileOutPut.WAVPrefix.Replace("{#}", fileCntr.ToString().PadLeft(numZeros,'0')));

                List<TTS_QA> QA = new List<TTS_QA>();
                
                QA.Add(new TTS_QA{QAText = ln.Question, Lang = ln.QuesLang, ProsodyRate = ln.QuesProsodyRate, QorA = outFileName.Replace(".wav","_ques.wav")});
                QA.Add(new TTS_QA{QAText = string.Format("<break time={2}{0}ms{2}/> {1}", Convert.ToInt32(ln.AnswerWaitSeconds) * 1000,ln.Answer, '"')
                    , Lang = ln.AnsLang, ProsodyRate = ln.AnsProsodyRate, QorA = outFileName.Replace(".wav","_resp.wav")});

                foreach(TTS_QA qa in QA)
                {
                    TextToSpeech t = new TextToSpeech();

                    ISound sndOut = new SoundOutput(c.SoundDefault);
                    t.Sound = sndOut;

                    // question or answer
                    t.TextIn = qa.QAText;
                    t.Sound.Language = GetSoundProp(qa.Lang, c.SoundDefault.Language);
                    // random speaker from avail list
                    string spkr = GetRandSpeakerName(t.Sound.Language, c.SoundDefault.Speakers);
                    t.Sound.Speaker = GetSoundProp(spkr, c.SoundDefault.Speaker);
                    t.Sound.ProsodyRate = GetSoundProp(qa.ProsodyRate, c.SoundDefault.ProsodyRate);

                    t.FileOut =  qa.QorA ;//string.Format(@"{0}\{1}.wav",c.FileOutPut.SoundFolder, qa.QorA);

                    while(File.Exists(t.FileOut))
                        File.Delete(t.FileOut);

                    await t.testToken(c.Authentication);
                }
              
                // TODO, merge Q & A wav files

                fileCntr++;
            }

          
            ///// SAMPLE //////
            // TextToSpeech t = new TextToSpeech();

            // ISound sndOut = new SoundOutput(c.SoundDefault);
            // t.Sound = sndOut;
            // t.TextIn = "Are you going to Scarborough fair?";
            // t.FileOut = string.Format(@"{0}\sample.wav",c.FileOutPut.SoundFolder);

            // await t.testToken(c.Authentication);

            // ///// SAMPLE //////
            // sndOut = new SoundOutput(c.SoundDefault);
            // sndOut.Language = "de-DE";
            // sndOut.Speaker = "Hedda";
            // t.Sound = sndOut;
            // t.TextIn = "Der, die, oder das?  Aktivität.";
            // t.FileOut = string.Format(@"{0}\sample1.wav",c.FileOutPut.SoundFolder);

            // await t.testToken(c.Authentication);

            // ///// SAMPLE //////
            // sndOut = new SoundOutput(c.SoundDefault);
            // t.TextIn = "Are you STILL going to Scarborough fair?";
            // t.Sound = sndOut;
            // t.FileOut = string.Format(@"{0}\sample2.wav",c.FileOutPut.SoundFolder);

            // await t.testToken(c.Authentication);

            Console.WriteLine($"Done");
            Environment.Exit((int) RetCodes.Success);
        
        }

        class TTS_QA
        {
            public string QAText {set; get;}
            public string Lang {set; get;}
            public string ProsodyRate {set; get;}
            public string QorA {set; get;}
        }

        static string GetSoundProp(string sndProp, string sndDefault)
        {
            string ret = sndProp.ToUpper().Trim() == "DEFAULT" ? sndDefault : sndProp;
            return ret;
        }

         static string GetRandSpeakerName(string lang, List<Speaker> speakers)
        {

            List<Speaker> spkrs = speakers.Where(x => x.Language.ToUpper().Trim() == lang.ToUpper().Trim()).ToList();
            int cnt = spkrs.Count;

            if(cnt == 0)
                return "DEFAULT";

            var rand = new Random();
            var spkr = spkrs[rand.Next(spkrs.Count)].Voice;

            return spkr;
            
        }

        static IEnumerable<CSVInput> GetAllQuestions(IConfig c)
        {

            List<CSVInput> ret = new List<CSVInput>();

            foreach(string inCSVMask in c.FileInPut.CSVFiles)
            {
                Console.WriteLine(inCSVMask);

                string[] parts = inCSVMask.Split('|');
                string[] files = Directory.GetFiles(parts[0], parts[1]);

                foreach(string f in files)
                {
                    Console.WriteLine(f);
                    CSVInput csv = new CSVInput();
                    IEnumerable<CSVInput> tmp = csv.GetRecs(f);
                    ret.AddRange(tmp);
                }
            }

            return ret;
          
        } // GetAllQuestions()

    } // class Program
   
} // namespace AudioFlash
