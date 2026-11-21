# Lucky Script Translator (hacky edition!!)
A very stoopid but functional script translation utility for Lucky star no Mori (https://archive.org/details/lucky-star-no-mori)

Reccomend using something like https://codeberg.org/morgana/IDXLib to extract and repack the idx files first

# Usage
## Drag and Drop
### Gen Translation Text File
Drag and dropping just the script .sd file (eg: konata_script12.sd) will generate the translation text file for translating purposes
### Apply translation file
Drag and drop both the original .sd file and the .txt file to generate a new sd file (eg: will become konata_script12_translated.sd) with the translations applied

## Command arguments
```
-s, --script       (Required) Script Filepath
-t --translation   (Optional) Translation Filepath
-o, --out          (Optional) Out Filepath
-i --ignoreEnglish (Optional) Ignore English Only strings (usually filenames) when generating the translation text file
```

Supplying just a script will generate a translation filepath, optionally can supply the out path

Supplying both a script and translation file, will generate the newly translated script, optionally can supply the outpath