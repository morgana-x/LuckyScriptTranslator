namespace LuckyScriptHackyTranslator;

public class DialogueReplacer
{
    private static long _findDialogue(Stream stream, DialogueEntry e)
    {
        stream.Position = 0;
        return Util.IndexOf(stream, e.Original);
    }

    public static void MoveChunk(Stream stream, long start, long newPos)
    {
        stream.Position = start;
        MemoryStream temp = new MemoryStream();
        stream.CopyTo(temp);

        stream.Position = newPos;

        temp.Position = 0;
        temp.CopyTo(stream);
        
        temp.Dispose();
    }

    public static void ReplaceDialogues(Stream stream, List<DialogueEntry> entries)
    {
        foreach (var e in entries)
        {
            long pos = _findDialogue(stream, e);
            
            if (pos == -1)
            {
                Program.Log($"Couldn't find entry {Convert.ToHexString(e.Original)} {e.Translation}, are you using the vanilla original script???");
                continue;
            }

            byte[] bytes = Util.ShiftJis.GetShiftJis().GetBytes(e.Translation);
            
            if (bytes.SequenceEqual(e.Original.Skip(2)))
                continue;
            
            using MemoryStream temp = new MemoryStream();
            temp.Write(BitConverter.GetBytes((ushort)bytes.Length));
            temp.Write(bytes);
            
            int offset =  e.Original.Length - ((int)temp.Length);
            
            if (offset != 0)
            {
                var start = pos + e.Original.Length;
                MoveChunk(stream, start, start - offset );
            }

            stream.Position = pos;
            temp.Position = 0;
            temp.CopyTo(stream);
        }
    }
}