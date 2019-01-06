using System.Collections.Generic;
using System.Text;
using System.Linq;
using NAudio.Wave;

public class FileInput
{
    public string authFile { get; set; }
    public List<string> CSVFiles { get; set; } // path + file mask
}

public class FileOutput
{
    public string SoundFolder { get; set; }
    public string WAVPrefix { get; set; }
    public  bool SplitQAFiles {set; get;}
    public int StartOutNum { get; set; }
    public string LogFolder { get; set; }
}

