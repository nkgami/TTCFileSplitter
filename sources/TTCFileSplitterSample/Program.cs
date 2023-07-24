using TTCFileSplitter;

namespace TTCFileSplitterSample
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFilePath = @"C:\temp\msgothic.ttc"; //Path to TTC file.
            string outputDirPath = @"C:\temp"; //Path to output directory.
            //These are used for output ttf file names.
            //Refer https://learn.microsoft.com/en-us/typography/opentype/spec/name for more information about PlatformID and LanguageID.
            UInt16 fileNamePlatformID = 3;
            UInt16 fileNameLanguageID = 0x0411;
            TTC2TTFConvert convert = new TTC2TTFConvert(inputFilePath, outputDirPath,  fileNamePlatformID, fileNameLanguageID);
            //Existing ttf files will be overwritten.
            if (!convert.SimpleSplit())
            {
                Console.WriteLine(convert.GetErrorMessage());
            }
            Console.WriteLine("Done.");
        }
    }
}