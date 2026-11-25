namespace LuckyScriptHackyTranslator;

public class DialogueReader
{
    public static DialogueEntry? FindNextDialogue(BinaryReader r, bool ignoreEnglish = false)
    {
        while (r.BaseStream.Position < r.BaseStream.Length && r.ReadByte() != 0x04)
        {
            
        }

        if (r.BaseStream.Position >= r.BaseStream.Length)
            return null;

        var start = r.BaseStream.Position;
        
        int length = r.ReadUInt16();

        if (length < 1)
        {
            r.BaseStream.Position = start;
            return null;
        }

        byte[] bytes = r.ReadBytes(length);

        bool hasJapanese = false;
        for (int i = 0; i < length - 1; i++)
        {
            if (bytes[i] == 0)
            {
                r.BaseStream.Position = start;
                return null;
            }

            if (bytes[i] >= 0x81)
                hasJapanese = true;
        }

        if (!hasJapanese && ignoreEnglish)
            return null;

        string msg = Util.ShiftJis.GetShiftJis().GetString(bytes);

        if (msg.Trim().IsWhiteSpace() || msg.Trim().Length == 0)
        {
            r.BaseStream.Position = start;
            return null;
        }


        r.BaseStream.Position = r.BaseStream.Position - length - 2;
        byte[] fullOGBytes = r.ReadBytes(length + 2);

        return new DialogueEntry(fullOGBytes, msg);
    }
    
    public static List<DialogueEntry> FindDialogues(BinaryReader r, bool ignoreEnglish = false)
    {
        List<DialogueEntry> entries = new List<DialogueEntry>();
        
        // Go to opcode section
        r.BaseStream.Seek(40, SeekOrigin.Begin);

        while (r.BaseStream.Position < r.BaseStream.Length)
        {
            var d = FindNextDialogue(r, ignoreEnglish);
            if (d != null && !DialogueEntry._containsEntry(entries, d))
                entries.Add(d);
        }

        return entries;
    }

  
}