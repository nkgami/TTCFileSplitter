TTC to TTF File Convert Library for C# .NET
====

## Description
Simple TTC to TTF File Convert Library for C# .NET to split font files into one or multiple TTF files.

## Requirement
.NET Standard 2.0


## Usage
```
using TTCFileSplitter;

string inputFilePath = @"C:\temp\msgothic.ttc"; //Path to TTC file.
string outputDirPath = @"C:\temp"; //Path to output directory.
//These are used for output ttf file names.
//Refer https://learn.microsoft.com/en-us/typography/opentype/spec/name for more information about PlatformID and LanguageID.
UInt16 fileNamePlatformID = 3;
UInt16 fileNameLanguageID = 0x0411;
TTC2TTFConvert convert = new TTC2TTFConvert(inputFilePath, outputDirPath,  fileNamePlatformID, fileNameLanguageID);
if (!convert.SimpleSplit())
{
    Console.WriteLine(convert.GetErrorMessage());
}
Console.WriteLine("Done.");
```

Oputput file name is {No}-{Platform / Language Specific Font Family Name}.ttf

For example, "2-MS UI Gothic.ttf"

Sample: sources/TTCFileSplitterSample

## Licence

[MIT](LICENCE)

## Author

[nkgami](https://github.com/nkgami)