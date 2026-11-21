using LuckyScriptHackyTranslator;

public class Program
{

    static string? SkipCommentsWhiteSpace(StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == null)
                break;
            if (line.TrimStart().StartsWith("#"))
                continue;
            if (line.Trim() == "" || line.Trim().IsWhiteSpace())
                continue;
            
            return line;
        }

        return null;
    }
    
    public static void Main(string[] args)
    {
        if (args.Length == 0)
            return;

        if (args.Length == 1) // gen translation editing file
        {
            FileStream fs = new FileStream(args[0], FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            var entries =  DialogueFinder.FindDialogues(br);
            br.Close();
            
            FileStream writeStream = new FileStream(args[0].Replace(".sd", "") + ".txt", FileMode.Create);
            
            StreamWriter writer = new StreamWriter(writeStream);

            foreach (var entry in entries)
            {
                writer.WriteLine($"# Entry {entries.IndexOf(entry)}");
                writer.WriteLine(Convert.ToHexString(entry.Original));
                
                string cleanLine = entry.Translation.Replace("\n", "\\n").Replace("\r", "\\r");
                writer.WriteLine($"# Original Japanese: {cleanLine}");
                writer.WriteLine($"{cleanLine}");
                
                writer.WriteLine("\n");
            }
            
            writer.Close();
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
            
            MemoryStream temp = new MemoryStream();
            temp.Write(File.ReadAllBytes(sdFile));
            temp.Position = 0;
            
            List<DialogueEntry> entries = new List<DialogueEntry>();
            
            StreamReader sr = new StreamReader(txtFile);
            while (!sr.EndOfStream)
            {
                string? pattern = SkipCommentsWhiteSpace(sr);
             
                if (pattern == null)
                    break;
                
                Console.WriteLine(pattern);
                
                string? translation = SkipCommentsWhiteSpace(sr);
             
                if (translation == null)
                    break;
                
                Console.WriteLine(translation);
                
                entries.Add(new DialogueEntry(Convert.FromHexString(pattern.Trim()), translation));
            }
            
            DialogueReplacer.ReplaceDialogues(temp, entries);
            
            FileStream  writeStream = new FileStream(sdFile.Replace(".sd", "") + "_translated.sd", FileMode.Create, FileAccess.Write);
            temp.Position = 0;
            temp.CopyTo(writeStream);
            temp.Close();
            writeStream.Close();

        }
    }
}