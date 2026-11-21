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
}