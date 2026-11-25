namespace LuckyScriptHackyTranslator;

public class DialogueReplacer
{
    private static long _findDialogue(Stream stream, DialogueEntry e)
    {
        return Util.IndexOf(stream, e.Original);
    }

    public static void MoveChunk(Stream stream, long start, long newPos)
    {
        stream.Position = start;
        MemoryStream temp = new MemoryStream();
        stream.CopyTo(temp);
        
        stream.SetLength(stream.Position+1);

        stream.Position = newPos;

        temp.Position = 0;
        temp.CopyTo(stream);
        
        temp.Dispose();
    }

    public static void ReplaceDialogues(Stream stream, List<DialogueEntry> entries)
    {
        foreach (var e in entries)
        {
            stream.Position = 0;
            long pos = 0;

            long prev = 0;
            while (pos >= 0 && stream.Position < stream.Length)
            {
                pos = _findDialogue(stream, e);
                
                if (pos == -1 || pos < prev)
                    break;

                prev = pos;
                
                byte[] bytes = Util.ShiftJis.GetShiftJis().GetBytes(e.Translation);

                if (e.Original.Length > 2 && bytes.SequenceEqual(e.Original.Skip(2)))
                    break;

                if (bytes.Length < 1)
                {
                    Program.Log($"EMPTY entry {Convert.ToHexString(e.Original)}");
                    break;
                }

                if (bytes.Length > e.Original.Length - 2)
                {
                    Program.Log(
                        $"TRUNCATING LARGER entry {Convert.ToHexString(e.Original)} {e.Translation} (Game crashes otherwise :( )");

                    bytes = bytes.SkipLast(bytes.Length - (e.Original.Length - 2)).ToArray();
                }

                // pad shorter sentences
                if (bytes.Length < e.Original.Length - 2)
                {
                    var dynamic = bytes.ToList();
                    while (dynamic.Count() < e.Original.Length - 2)
                        dynamic.Add(0x20); // space
                    bytes = dynamic.ToArray();
                }

                using MemoryStream temp = new MemoryStream();
                temp.Write(BitConverter.GetBytes((ushort)bytes.Length));
                temp.Write(bytes);

                int offset = e.Original.Length - ((int)temp.Length);


                if (offset != 0)
                {
                    var start = pos + e.Original.Length;
                    MoveChunk(stream, start, start - offset);
                }



                stream.Position = pos;
                temp.Position = 0;
                temp.CopyTo(stream);
                
                stream.Position = pos + temp.Length;
                temp.Close();

            }
        }

        uint fileSize = (uint)stream.Length;

        stream.Position = 8;
        // Write new size of file
        stream.Write(BitConverter.GetBytes(fileSize));
    }
    
   
}