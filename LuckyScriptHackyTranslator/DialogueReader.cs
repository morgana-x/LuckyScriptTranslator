namespace LuckyScriptHackyTranslator;

public class DialogueReader
{
    public static DialogueEntry? FindNextDialogue(BinaryReader r)
    {
        while (r.BaseStream.Position < r.BaseStream.Length && r.ReadByte() != 0x04)
        {
            
        }

        if (r.BaseStream.Position >= r.BaseStream.Length)
            return null;

        int length = r.ReadUInt16();

        if (length < 1)
            return null;
        
        byte[] bytes = r.ReadBytes(length);
        
        for (int i = 0; i < length - 1; i++)
            if (bytes[i] == 0)
                return null;

        string msg = Util.ShiftJis.GetShiftJis().GetString(bytes);

        if (msg.Trim().IsWhiteSpace() || msg.Trim().Length == 0)
            return null;


        r.BaseStream.Position = r.BaseStream.Position - length - 2;
        byte[] fullOGBytes = r.ReadBytes(length + 2);

        return new DialogueEntry(fullOGBytes, msg);
    }
    
    public static List<DialogueEntry> FindDialogues(BinaryReader r)
    {
        List<DialogueEntry> entries = new List<DialogueEntry>();
        
        // Go to opcode section
        r.BaseStream.Seek(40, SeekOrigin.Begin);

        while (r.BaseStream.Position < r.BaseStream.Length)
        {
            var d = FindNextDialogue(r);
            if (d != null)
                entries.Add(d);
        }

        return entries;
    }

  
}