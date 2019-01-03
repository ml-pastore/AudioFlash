using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

public class CSVInput
{
    public string Category {set; get;}
    public string IsActive {set; get;}
    public string Question {set; get;}
    public string Answer {set; get;}
    public string AnswerWaitSeconds {set; get;}
    public string QuesLang {set; get;}
    public string QuesProsodyRate {set; get;}
    public string AnsLang {set; get;}
    public string AnsProsodyRate {set; get;}
    public string SourceCSVFile {set; get;}
    

    private string _fileEncode = "iso-8859-1";
    public IEnumerable<CSVInput> GetRecs(string FileName)
    {
        string absPath = Path.GetFullPath(FileName);
        IEnumerable<CSVInput> ret = new List<CSVInput>();

        StreamReader reader = new StreamReader(absPath,  Encoding.GetEncoding(_fileEncode), true);
        var csv = new CsvReader(reader);
        csv.Configuration.HasHeaderRecord = true;
        csv.Configuration.RegisterClassMap<CSVInputMap>();
        ret = csv.GetRecords<CSVInput>();

        return ret;

    }

 
}


public class CSVInputMap : ClassMap<CSVInput>
{
    public CSVInputMap()
    {
        AutoMap();
        Map( m => m.SourceCSVFile).Ignore();
    }
}