using CommandLine;
using LuckyScriptHackyTranslator;

public class Program
{
    class Options
    {
        [Option('s', "script", Required = false, HelpText = "Script File")]
        public IEnumerable<string>? ScriptFile { get; set; }
        
        [Option('t', "translation", Required = false, HelpText = "Translation File")]
        public IEnumerable<string>? TranslationFile { get; set; }
        
        [Option('o', "out", Required = false, HelpText = "Output File")]
        public string? OutFile { get; set; }

        [Option('i', "ignoreenglish", Required = false,
            HelpText = "Ignores english strings (usually filenames etc) when generating the translation text file")]
        public bool IgnoreEnglish { get; set; } = false;
        
        [Option('h', "silent", Required = false)]
        public bool Silent { get; set; } = false;

        [Option('m', "merge", Required = false, HelpText = "Merges the given translation text files, presumed if you only supply multiple translation files")]
        public bool Merge { get; set; } = false;
    }

    private static bool _silent = false;
    
    static void RunOptions(Options opts)
    {
        _silent = opts.Silent;

        var scripts = opts.ScriptFile;
        var translationFiles = opts.TranslationFile;
        
        _prepareScriptList(ref scripts, ".sd");
        _prepareScriptList(ref translationFiles, ".txt");
        
        //handle options
        if (translationFiles == null || translationFiles?.Count() < 1)
            CreateTranslationFile(scripts, opts.OutFile, opts.IgnoreEnglish);
        else if (( opts.Merge ||  scripts == null || scripts?.Count() < 1) &&
                 translationFiles != null && translationFiles.Count() > 0)
            MergeTranslationFiles(translationFiles, opts.OutFile);
        else
            TranslateScriptFromFile(scripts, translationFiles.First(), opts.OutFile);
    }

    public static void Log(string msg)
    {
        if (!_silent)
            Console.WriteLine("[LST] " + msg);
    }
    
    public static void Main(string[] args)
    {
        
        // Drag and drop support
        if (args.Length == 1 && args[0].EndsWith(".sd")) // gen translation editing file
        {
            CreateTranslationFile(new List<string>(){args[0]});
            return;
        }

        if (args.Length == 2 && (args[0].EndsWith(".sd") || args[0].EndsWith(".txt"))) // gen new script  from og + translation file
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
            
            TranslateScriptFromFile(new List<string>(){sdFile}, txtFile);
            return;
        }


        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunOptions);

    }
    

    static void _prepareScriptList(ref IEnumerable<string>? scriptPaths, string ext)
    {
        if (scriptPaths == null)
            return;
        List<string> newScriptPaths = new List<string>();

        foreach (var s in scriptPaths)
        {
            if (!File.Exists(s) && Directory.Exists(s))
            {
                foreach (var f in Directory.GetFiles(s))
                {
                    if (f.EndsWith(ext))
                        newScriptPaths.Add(f);
                }
                continue;
            }

            if (!File.Exists(s))
            {
                throw new Exception("File not found " + s);
            }
            newScriptPaths.Add(s);
        }
        
        scriptPaths = newScriptPaths.ToArray();
    }
    
    static void CreateTranslationFile(IEnumerable<string> scriptPaths, string? outPath = null, bool ignoreEnglish = false)
    {
        if (outPath == null)
            outPath = scriptPaths.First().Replace(".sd", "") + ".txt";
        
        List<DialogueEntry> entries = new List<DialogueEntry>();
        
        foreach (var s in scriptPaths)
        {
            Log("Reading script " + s);
            using BinaryReader br = new BinaryReader(new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            DialogueEntry._mergeEntries(ref entries, DialogueReader.FindDialogues(br, ignoreEnglish));
            br.Close();
        }
        
        using StreamWriter writer = new StreamWriter(new FileStream(outPath, FileMode.Create));
        DialogueEntry.WriteEntries(writer, entries);
        writer.Close();
    }

    static void TranslateScriptFromFile(IEnumerable<string> scriptPaths, string translationFilePath, string? outScriptPath = null)
    {
        if (outScriptPath == null)
            outScriptPath = scriptPaths.Count() == 1 ? scriptPaths.First().Replace(".sd", "") + "_translated.sd" : scriptPaths.First().Replace(".sd", "") + "_translated/";
        
        var entries = DialogueEntry._readTranslationFiles(new string[]{translationFilePath});
        
        MemoryStream virtualScript;

        foreach (var scriptPath in scriptPaths)
        {
            Log("Translating " + scriptPath);
            
            virtualScript = new MemoryStream();
            virtualScript.Write(File.ReadAllBytes(scriptPath));
            virtualScript.Position = 0;

            DialogueReplacer.ReplaceDialogues(virtualScript, entries);
            
            Log("Replaced dialogue");

            string outPath = outScriptPath;
            
            if (Directory.Exists(outScriptPath))
                outPath = Path.Combine(outScriptPath, Path.GetFileName(scriptPath));

            File.WriteAllBytes(outPath, virtualScript.ToArray());
            virtualScript.Close();
            
            Log("Translated " + scriptPath + " to " + outPath);
        }
    }

    static void MergeTranslationFiles(IEnumerable<string> translationPaths, string? outTranslationPath = null)
    {
        if (outTranslationPath == null)
            outTranslationPath = translationPaths.First().Replace(".txt", "") + "_merged.txt";
        
        Log($"Reading translation {translationPaths.Count()} files to merge...");
        
        var entries = DialogueEntry._readTranslationFiles(translationPaths);
        
        Log($"Writing {entries.Count} entries to  {outTranslationPath}...");
        using StreamWriter writer = new StreamWriter(new FileStream(outTranslationPath, FileMode.Create));
        DialogueEntry.WriteEntries(writer, entries);
        writer.Close();
        Log($"Wrote to  {outTranslationPath}!");
    }
}