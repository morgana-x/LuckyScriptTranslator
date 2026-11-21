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
        
        //  stream.SetLength(newPos + temp.Length);
        
        temp.Dispose();
    }

    public static void ReplaceDialogues(Stream stream, List<DialogueEntry> entries)
    {
        foreach (var e in entries)
        {
            long pos = _findDialogue(stream, e);
            
            if (pos == -1)
            {
                throw new($"Couldn't find entry {Convert.ToHexString(e.Original)} {e.Translation}, are you using the vanilla original script???");
                // continue;
            }

            byte[] bytes = Util.ShiftJis.GetShiftJis().GetBytes(e.Translation);
            
            using MemoryStream temp = new MemoryStream();
            temp.Write(BitConverter.GetBytes((ushort)bytes.Length));
            temp.Write(bytes);

            
            int offset =  e.Original.Length - ((int)temp.Length);
            
            if (offset != 0)
            {
                Console.WriteLine(e.Translation);
                Console.WriteLine("Offset: " + offset);
                Console.WriteLine($"Original:  {Convert.ToHexString(e.Original)} \n \t New:  {Convert.ToHexString(temp.ToArray())}");
                var start = pos + e.Original.Length;
                MoveChunk(stream, start, start - offset );
            }

            stream.Position = pos;
            temp.Position = 0;
            temp.CopyTo(stream);
        }
    }
}