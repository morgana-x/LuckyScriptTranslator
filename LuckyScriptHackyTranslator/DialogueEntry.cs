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
                
            string cleanLineOg =  Util.ShiftJis.GetShiftJis().GetString(entry.Original.Skip(2).ToArray()).Replace("\n", "\\n").Replace("\r", "\\r");
            writer.WriteLine($"# Original Japanese: {cleanLineOg}");

            string cleanLineTranslation = entry.Translation.Replace("\n", "\\n").Replace("\r", "\\r"); 
            writer.WriteLine($"{cleanLineTranslation}");
                
            writer.WriteLine("\n");
        }
    }
    
    static internal bool _containsEntry(List<DialogueEntry> entries, DialogueEntry entry)
    {
        foreach (var e in entries)
            if (e.Original.SequenceEqual(entry.Original))
                return true;
        return false;
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
    
    internal static void _mergeEntries(ref List<DialogueEntry> entries, List<DialogueEntry> inp)
    {
        foreach (var i in inp)
        {
            if (!_containsEntry(entries, i))
            {
                entries.Add(i);
                continue;
            }
            
            foreach (var e in entries)
                if (e.Original.SequenceEqual(i.Original) && 
                    !Util.ShiftJis.GetShiftJis().GetString(i.Original.Skip(2).ToArray()).TrimEnd().SequenceEqual(i.Translation.TrimEnd()))
                    e.Translation = i.Translation;
        }
    }
    
    internal static List<DialogueEntry> _readTranslationFiles(IEnumerable<string> translationPaths)
    {
        List<DialogueEntry> result = new List<DialogueEntry>();
        
        foreach (var translation in translationPaths)
        {
            using StreamReader reader = new StreamReader(new FileStream(translation, FileMode.Open));
            _mergeEntries(ref result, DialogueEntry.ReadEntries(reader));
            reader.Close();
        }

        return result;
    }
}