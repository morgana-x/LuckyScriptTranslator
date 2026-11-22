namespace LuckyScriptHackyTranslator;

public class DialogueEntry
{
    public byte[] Original;

    public string Translation;

    public DialogueEntry(byte[] bytes, string translation)
    {
        Original = bytes;
        Translation = translation;
    }
    
    public static void WriteEntries(StreamWriter writer, List<DialogueEntry> entries)
    {
        foreach (var entry in entries)
        {
            writer.WriteLine($"# Entry {entries.IndexOf(entry)}");
            writer.WriteLine(Convert.ToHexString(entry.Original));
                
            string cleanLine = entry.Translation.Replace("\n", "\\n").Replace("\r", "\\r");
            writer.WriteLine($"# Original Japanese: {cleanLine}");
            writer.WriteLine($"{cleanLine}");
                
            writer.WriteLine("\n");
        }
    }

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
    
    public static List<DialogueEntry> ReadEntries(StreamReader sr)
    {
        List<DialogueEntry> entries = new List<DialogueEntry>();
        
        while (!sr.EndOfStream)
        {
            string? pattern = SkipCommentsWhiteSpace(sr);
             
            if (pattern == null)
                break;
                
            string? translation = SkipCommentsWhiteSpace(sr);
             
            if (translation == null)
                break;
            
            entries.Add(new DialogueEntry(Convert.FromHexString(pattern.Trim()), translation));
        }

        return entries;
    }
}