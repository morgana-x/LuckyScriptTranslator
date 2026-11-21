using LuckyScriptHackyTranslator;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
            return;

        if (args.Length == 1) // gen translation editing file
        {
            CreateTranslationFile(args[0]);
            return;
        }

        if (args.Length == 2) // gen new script  from og + translation file
        {
            string sdFile;
            string txtFile;

            if (args[0].EndsWith(".sd"))
            {
                sdFile = args[0];
                txtFile = args[1];
            }
            else
            {
                sdFile = args[1];
                txtFile = args[0];
            }
            
            TranslateScriptFromFile(sdFile, txtFile);

        }
    }
    
    static void CreateTranslationFile(string scriptPath, string? outPath = null)
    {
        if (outPath == null)
            outPath = scriptPath.Replace(".sd", "") + ".txt";
        
        using BinaryReader br = new BinaryReader(new FileStream(scriptPath, FileMode.Open));
        var entries  =  DialogueReader.FindDialogues(br);
        br.Close();
        
        using StreamWriter writer = new StreamWriter(new FileStream(outPath, FileMode.Create));
        DialogueEntry.WriteEntries(writer, entries);
        writer.Close();
    }

    static void TranslateScriptFromFile(string scriptPath, string translationFilePath, string? outScriptPath = null)
    {
        if (outScriptPath == null)
        {
            outScriptPath = scriptPath.Replace(".sd", "") + "_translated.sd";
        }
        
        MemoryStream virtualScript = new MemoryStream();
        virtualScript.Write(File.ReadAllBytes(scriptPath));
        virtualScript.Position = 0;
        
        using StreamReader reader = new StreamReader(new FileStream(translationFilePath, FileMode.Open));
        var entries = DialogueEntry.ReadEntries(reader);
        reader.Close();
        
        DialogueReplacer.ReplaceDialogues(virtualScript, entries);
        
        File.WriteAllBytes(outScriptPath, virtualScript.ToArray());
    }
}