using System;
using System.IO;
using System.Text;

namespace TTCFileSplitter
{
    public enum ConverterErrorCode : ushort
    {
        NoError = 0,
        InvalidFileFormat = 1,
        InvalidNumFonts = 2,
        NoOputputFile = 3,
        OutputDirectoryNotExist = 4,
        OtherException = 5,
        FileDoesNotExist = 6
    }
    public class TTFTable
    {
        public char[] tag;
        public UInt32 checkSum;
        public UInt32 offset;
        public UInt32 length;
    }
    public class TTF
    {
        public UInt32 sfntVersion;
        public UInt16 numTables;
        public UInt16 searchRange;
        public UInt16 entrySelector;
        public UInt16 rangeShift;
        public TTFTable[] ttfTable;
    }
    public class NamingTable
    {
        public UInt16 version;
        public UInt16 count;
        public UInt16 storageOffset;
        public NameRecord[] nameRecord;
        public UInt16 langTagCount;
        public LangTagRecord[] langTagRecord;
    }
    public class NameRecord
    {
        public UInt16 platformID;
        public UInt16 encodingID;
        public UInt16 languageID;
        public UInt16 nameID;
        public UInt16 length;
        public UInt16 stringOffset;
    }
    public class LangTagRecord
    {
        public UInt16 length;
        public UInt16 langTagOffset;
    }
    public class TTC2TTFConvert
    {
        private string _ttcFileName;
        private ConverterErrorCode _errorCode;
        private string _exceptionMessage;
        private string _outputDirectory;
        private UInt16 _majorVersion;
        private UInt16 _minorVersion;
        private UInt32 _numFonts;
        private bool _isLittleEndian;
        private UInt16 _fileNamePlatformID;
        private UInt16 _fileNameLanguageID;
        static char[][] listTableTags = new char[][] {
            new char[] {'a','v','a','r'},
            new char[] {'B','A','S','E'},
            new char[] {'C','B','D','T'},
            new char[] {'C','B','L','C'},
            new char[] {'C','F','F',' '},
            new char[] {'C','F','F','2'},
            new char[] {'c','m','a','p'},
            new char[] {'C','O','L','R'},
            new char[] {'C','P','A','L'},
            new char[] {'c','v','a','r'},
            new char[] {'c','v','t',' '},
            new char[] {'D','S','I','G'},
            new char[] {'E','B','D','T'},
            new char[] {'E','B','L','C'},
            new char[] {'E','B','S','C'},
            new char[] {'f','p','g','m'},
            new char[] {'f','v','a','r'},
            new char[] {'g','a','s','p'},
            new char[] {'G','D','E','F'},
            new char[] {'g','l','y','f'},
            new char[] {'G','P','O','S'},
            new char[] {'G','S','U','B'},
            new char[] {'g','v','a','r'},
            new char[] {'h','d','m','x'},
            new char[] {'h','e','a','d'},
            new char[] {'h','h','e','a'},
            new char[] {'h','m','t','x'},
            new char[] {'H','V','A','R'},
            new char[] {'J','S','T','F'},
            new char[] {'k','e','r','n'},
            new char[] {'l','o','c','a'},
            new char[] {'L','T','S','H'},
            new char[] {'M','A','T','H'},
            new char[] {'m','a','x','p'},
            new char[] {'M','E','R','G'},
            new char[] {'m','e','t','a'},
            new char[] {'M','V','A','R'},
            new char[] {'n','a','m','e'},
            new char[] {'O','S','/','2'},
            new char[] {'P','C','L','T'},
            new char[] {'p','o','s','t'},
            new char[] {'p','r','e','p'},
            new char[] {'s','b','i','x'},
            new char[] {'S','T','A','T'},
            new char[] {'S','V','G',' '},
            new char[] {'V','D','M','X'},
            new char[] {'v','h','e','a'},
            new char[] {'v','m','t','x'},
            new char[] {'V','O','R','G'},
            new char[] {'V','V','A','R'}
        };

        //Compare whether char array is same or not
        private bool CompareCharArray(char[] array1, char[] array2)
        {
            if (array1 == null || array2 == null)
            {
                return false;
            }
            if (array1.Length != array2.Length)
            {
                return false;
            }
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckTagName(char[] tagNameArray)
        {
            for (int j = 0; j < listTableTags.Length; j++)
            {
                if (CompareCharArray(listTableTags[j], tagNameArray))
                {
                    return true;
                }
            }
            return false;
        }

        public ConverterErrorCode GetErrorCode()
        {
            return _errorCode;
        }

        public UInt16 GetMajorVersion()
        {
            return _majorVersion;
        }

        public UInt16 MinorMajorVersion()
        {
            return _minorVersion;
        }

        public string GetErrorMessage()
        {
            switch (_errorCode)
            {
                case ConverterErrorCode.NoError:
                    return "";
                case ConverterErrorCode.InvalidFileFormat:
                    return "Invalid file format: ttcTag is not correct.";
                case ConverterErrorCode.InvalidNumFonts:
                    return "Invalid file format: numFonts is 0.";
                case ConverterErrorCode.OutputDirectoryNotExist:
                    return "Output directory does not exist.";
                case ConverterErrorCode.NoOputputFile:
                    return "No output ttf file.";
                case ConverterErrorCode.OtherException:
                    return _exceptionMessage == null ? "" : _exceptionMessage;
                case ConverterErrorCode.FileDoesNotExist:
                    return "File does not exist.";
                default:
                    return "";

            }
        }

        public TTC2TTFConvert(string TtcFFileName, string outputDirectory, UInt16 fileNamePlatformID = 3, UInt16 fileNameLanguageID = 0x0411)
        {
            _ttcFileName = TtcFFileName;
            _outputDirectory = outputDirectory;
            _isLittleEndian = BitConverter.IsLittleEndian;
            _errorCode = ConverterErrorCode.NoError;
            _exceptionMessage = null;
            _fileNamePlatformID = fileNamePlatformID;
            _fileNameLanguageID = fileNameLanguageID;
        }
        //Convert Big Endian Uint16 to Little Endian Uint16
        private UInt16 SwapBytesUInt16(UInt16 value)
        {
            return _isLittleEndian ? (UInt16)((value << 8) | (value >> 8)) : value;
        }
        //Convert Big Endian Uint32 to Little Endian Uint32
        private UInt32 SwapBytesUInt32(UInt32 value)
        {
            return _isLittleEndian ? (UInt32)((SwapBytesUInt16((UInt16)value) << 16) | (SwapBytesUInt16((UInt16)(value >> 16)))) : value;
        }
        public bool SimpleSplit()
        {
            int outputFileCount = 0;
            if (_ttcFileName != null && System.IO.File.Exists(_ttcFileName))
            {
                try
                {
                    using (var stream = File.Open(_ttcFileName, FileMode.Open))
                    {
                        using (var reader = new BinaryReader(stream, Encoding.ASCII, false))
                        {
                            //Read TTC Header
                            char[] ttcTag = reader.ReadChars(4);
                            if (ttcTag.Length != 4 || ttcTag[0] != 't' || ttcTag[1] != 't' || ttcTag[2] != 'c' || ttcTag[3] != 'f')
                            {
                                _errorCode = ConverterErrorCode.InvalidFileFormat;
                                return false;
                            }
                            //Read Version
                            _majorVersion = SwapBytesUInt16(reader.ReadUInt16());
                            _minorVersion = SwapBytesUInt16(reader.ReadUInt16());
                            //Read numFonts
                            _numFonts = SwapBytesUInt32(reader.ReadUInt32());
                            if (_numFonts == 0)
                            {
                                _errorCode = ConverterErrorCode.InvalidNumFonts;
                                return false;
                            }
                            //Read OffsetTable
                            UInt32[] offsetTable = new UInt32[_numFonts];
                            for (int i = 0; i < _numFonts; i++)
                            {
                                offsetTable[i] = SwapBytesUInt32(reader.ReadUInt32());
                            }
                            //Read TTF Tables and Find "name" tag. And then read the name of font.
                            char[] charName = new char[4] { 'n', 'a', 'm', 'e' };
                            int countTTF = 0;
                            foreach (UInt32 offset in offsetTable)
                            {
                                countTTF += 1;
                                reader.BaseStream.Position = offset;
                                TTF ttf = new TTF();
                                string fontFamilyNameString = null;
                                ttf.sfntVersion = SwapBytesUInt32(reader.ReadUInt32());
                                ttf.numTables = SwapBytesUInt16(reader.ReadUInt16());
                                ttf.searchRange = SwapBytesUInt16(reader.ReadUInt16());
                                ttf.entrySelector = SwapBytesUInt16(reader.ReadUInt16());
                                ttf.rangeShift = SwapBytesUInt16(reader.ReadUInt16());
                                ttf.ttfTable = new TTFTable[ttf.numTables];
                                for (int i = 0; i < ttf.numTables; i++)
                                {
                                    ttf.ttfTable[i] = new TTFTable();
                                    ttf.ttfTable[i].tag = reader.ReadChars(4);
                                    ttf.ttfTable[i].checkSum = SwapBytesUInt32(reader.ReadUInt32());
                                    ttf.ttfTable[i].offset = SwapBytesUInt32(reader.ReadUInt32());
                                    ttf.ttfTable[i].length = SwapBytesUInt32(reader.ReadUInt32());
                                    
                                    // If tag is "name"
                                    if (CompareCharArray(charName, ttf.ttfTable[i].tag))
                                    {
                                        NamingTable namingTable = new NamingTable();
                                        reader.BaseStream.Position = ttf.ttfTable[i].offset;
                                        namingTable.version = SwapBytesUInt16(reader.ReadUInt16());
                                        namingTable.count = SwapBytesUInt16(reader.ReadUInt16());
                                        namingTable.storageOffset = SwapBytesUInt16(reader.ReadUInt16());
                                        namingTable.nameRecord = new NameRecord[namingTable.count];
                                        for (int j = 0; j < namingTable.count; j++)
                                        {
                                            namingTable.nameRecord[j] = new NameRecord();
                                            namingTable.nameRecord[j].platformID = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].encodingID = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].languageID = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].nameID = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].length = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].stringOffset = SwapBytesUInt16(reader.ReadUInt16());
                                        }
                                        for (int j = 0; j < namingTable.count; j++)
                                        {
                                            //If Font Family name
                                            if (namingTable.nameRecord[j].nameID == 1 && namingTable.nameRecord[j].platformID == _fileNamePlatformID && namingTable.nameRecord[j].languageID == _fileNameLanguageID)
                                            {
                                                UInt32 stringDataOffset = ttf.ttfTable[i].offset + namingTable.storageOffset + namingTable.nameRecord[j].stringOffset;
                                                reader.BaseStream.Position = stringDataOffset;
                                                byte[] fontFamilyName = reader.ReadBytes(namingTable.nameRecord[j].length);
                                                fontFamilyNameString = System.Text.Encoding.BigEndianUnicode.GetString(fontFamilyName);
                                            }
                                        }
                                    }
                                }
                                if (fontFamilyNameString != null)
                                {
                                    //Write TTF file
                                    if (_outputDirectory != null && System.IO.Directory.Exists(_outputDirectory))
                                    {
                                        string filePath = System.IO.Path.Combine(_outputDirectory, countTTF.ToString() + "-" + fontFamilyNameString + ".ttf");
                                        using (var writeStream = File.Open(filePath, FileMode.Create))
                                        {
                                            using (var writer = new BinaryWriter(writeStream, Encoding.ASCII, false))
                                            {
                                                UInt32[] listNewOffset = new UInt32[ttf.numTables];
                                                byte[] headerDummy = new byte[12 + (16 * ttf.numTables)];
                                                //Prepare header space
                                                for (int j = 0; j < headerDummy.Length; j++)
                                                {
                                                    headerDummy[j] = 0;
                                                }
                                                writer.Write(headerDummy);
                                                //Write TTF Data
                                                for (int j = 0; j < ttf.numTables; j++)
                                                {
                                                    if (!CheckTagName(ttf.ttfTable[j].tag))
                                                    {
                                                        continue;
                                                    }
                                                    reader.BaseStream.Position = ttf.ttfTable[j].offset;
                                                    byte[] writeBuffer = reader.ReadBytes((int)(ttf.ttfTable[j].length));
                                                    listNewOffset[j] = (UInt32)(writer.BaseStream.Position);
                                                    long lengthPadding = ttf.ttfTable[j].length % 4;
                                                    writer.Write(writeBuffer);
                                                    for (int k = 0; k < lengthPadding; k++)
                                                    {
                                                        writer.Write(0);
                                                    }
                                                }
                                                //Write header
                                                writer.BaseStream.Position = 0;
                                                reader.BaseStream.Position = offset;
                                                writer.Write(reader.ReadBytes(12));
                                                for (int j = 0; j < ttf.numTables; j++)
                                                {
                                                    writer.Write(reader.ReadBytes(8));
                                                    reader.ReadBytes(4);
                                                    writer.Write(SwapBytesUInt32(listNewOffset[j]));
                                                    writer.Write(reader.ReadBytes(4));
                                                }
                                                outputFileCount += 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _errorCode = ConverterErrorCode.OutputDirectoryNotExist;
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _exceptionMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        Exception innerEx = ex.InnerException;
                        while (innerEx != null)
                        {
                            _exceptionMessage += "\n" + innerEx.Message;
                            innerEx = innerEx.InnerException;
                        }
                    }
                    _errorCode = ConverterErrorCode.OtherException;
                    return false;
                }
            }
            else
            {
                _errorCode = ConverterErrorCode.FileDoesNotExist;
                return false;
            }
            if (outputFileCount > 0)
            {
                return true;
            }
            else
            {
                _errorCode = ConverterErrorCode.NoOputputFile;
                return false;
            }
        }
        public bool FullDataSplit()
        {
            int outputFileCount = 0;
            if (_ttcFileName != null && System.IO.File.Exists(_ttcFileName))
            {
                try
                {
                    using (var stream = File.Open(_ttcFileName, FileMode.Open))
                    {
                        using (var reader = new BinaryReader(stream, Encoding.ASCII, false))
                        {
                            //Read TTC Header
                            char[] ttcTag = reader.ReadChars(4);
                            if (ttcTag.Length != 4 || ttcTag[0] != 't' || ttcTag[1] != 't' || ttcTag[2] != 'c' || ttcTag[3] != 'f')
                            {
                                _errorCode = ConverterErrorCode.InvalidFileFormat;
                                return false;
                            }
                            //Read Version
                            _majorVersion = SwapBytesUInt16(reader.ReadUInt16());
                            _minorVersion = SwapBytesUInt16(reader.ReadUInt16());
                            //Read numFonts
                            _numFonts = SwapBytesUInt32(reader.ReadUInt32());
                            if (_numFonts == 0)
                            {
                                _errorCode = ConverterErrorCode.InvalidNumFonts;
                                return false;
                            }
                            //Read OffsetTable
                            UInt32[] offsetTable = new UInt32[_numFonts];
                            for (int i = 0; i < _numFonts; i++)
                            {
                                offsetTable[i] = SwapBytesUInt32(reader.ReadUInt32());
                            }
                            //Read TTF Tables and Find "name" tag. And then read the name of font.
                            char[] charName = new char[4] { 'n', 'a', 'm', 'e' };
                            int count = 0;
                            foreach (UInt32 offset in offsetTable)
                            {
                                count += 1;
                                reader.BaseStream.Position = offset;
                                TTF ttf = new TTF();
                                string fontFamilyNameString = null;
                                ttf.sfntVersion = SwapBytesUInt32(reader.ReadUInt32());
                                ttf.numTables = SwapBytesUInt16(reader.ReadUInt16());
                                ttf.searchRange = SwapBytesUInt16(reader.ReadUInt16());
                                ttf.entrySelector = SwapBytesUInt16(reader.ReadUInt16());
                                ttf.rangeShift = SwapBytesUInt16(reader.ReadUInt16());
                                ttf.ttfTable = new TTFTable[ttf.numTables];
                                for (int i = 0; i < ttf.numTables; i++)
                                {
                                    ttf.ttfTable[i] = new TTFTable();
                                    ttf.ttfTable[i].tag = reader.ReadChars(4);
                                    ttf.ttfTable[i].checkSum = SwapBytesUInt32(reader.ReadUInt32());
                                    ttf.ttfTable[i].offset = SwapBytesUInt32(reader.ReadUInt32());
                                    ttf.ttfTable[i].length = SwapBytesUInt32(reader.ReadUInt32());
                                    // If tag is "name"
                                    if (CompareCharArray(charName, ttf.ttfTable[i].tag))
                                    {
                                        NamingTable namingTable = new NamingTable();
                                        reader.BaseStream.Position = ttf.ttfTable[i].offset;
                                        namingTable.version = SwapBytesUInt16(reader.ReadUInt16());
                                        namingTable.count = SwapBytesUInt16(reader.ReadUInt16());
                                        namingTable.storageOffset = SwapBytesUInt16(reader.ReadUInt16());
                                        namingTable.nameRecord = new NameRecord[namingTable.count];
                                        for (int j = 0; j < namingTable.count; j++)
                                        {
                                            namingTable.nameRecord[j] = new NameRecord();
                                            namingTable.nameRecord[j].platformID = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].encodingID = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].languageID = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].nameID = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].length = SwapBytesUInt16(reader.ReadUInt16());
                                            namingTable.nameRecord[j].stringOffset = SwapBytesUInt16(reader.ReadUInt16());
                                        }
                                        for (int j = 0; j < namingTable.count; j++)
                                        {
                                            //If Font Family name
                                            if (namingTable.nameRecord[j].nameID == 1 && namingTable.nameRecord[j].platformID == _fileNamePlatformID && namingTable.nameRecord[j].languageID == _fileNameLanguageID)
                                            {
                                                UInt32 stringDataOffset = ttf.ttfTable[i].offset + namingTable.storageOffset + namingTable.nameRecord[j].stringOffset;
                                                reader.BaseStream.Position = stringDataOffset;
                                                byte[] fontFamilyName = reader.ReadBytes(namingTable.nameRecord[j].length);
                                                fontFamilyNameString = System.Text.Encoding.BigEndianUnicode.GetString(fontFamilyName);
                                            }
                                        }
                                    }
                                }
                                if (fontFamilyNameString != null)
                                {
                                    //Write TTF file
                                    if (_outputDirectory != null && System.IO.Directory.Exists(_outputDirectory))
                                    {
                                        string filePath = System.IO.Path.Combine(_outputDirectory, count.ToString() + "-" + fontFamilyNameString + ".ttf");
                                        using (var writeStream = File.Open(filePath, FileMode.Create))
                                        {
                                            using (var writer = new BinaryWriter(writeStream, Encoding.ASCII, false))
                                            {
                                                UInt32[] listNewOffset = new UInt32[ttf.numTables];
                                                byte[] headerDummy = new byte[12 + (16 * ttf.numTables)];
                                                //Prepare header space
                                                for (int j = 0; j < headerDummy.Length; j++)
                                                {
                                                    headerDummy[j] = 0;
                                                }
                                                writer.Write(headerDummy);
                                                //Write TTF Data
                                                for (int j = 0; j < ttf.numTables; j++)
                                                {
                                                    reader.BaseStream.Position = ttf.ttfTable[j].offset;
                                                    byte[] writeBuffer = reader.ReadBytes((int)(ttf.ttfTable[j].length));
                                                    listNewOffset[j] = (UInt32)(writer.BaseStream.Position);
                                                    long lengthPadding = ttf.ttfTable[j].length % 4;
                                                    writer.Write(writeBuffer);
                                                    for (int k = 0; k < lengthPadding; k++)
                                                    {
                                                        writer.Write(0);
                                                    }
                                                }
                                                //Write header
                                                writer.BaseStream.Position = 0;
                                                reader.BaseStream.Position = offset;
                                                writer.Write(reader.ReadBytes(12));
                                                for (int j = 0; j < ttf.numTables; j++)
                                                {
                                                    writer.Write(reader.ReadBytes(8));
                                                    reader.ReadBytes(4);
                                                    writer.Write(SwapBytesUInt32(listNewOffset[j]));
                                                    writer.Write(reader.ReadBytes(4));
                                                }
                                                outputFileCount += 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _errorCode = ConverterErrorCode.OutputDirectoryNotExist;
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _exceptionMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        Exception innerEx = ex.InnerException;
                        while (innerEx != null)
                        {
                            _exceptionMessage += "\n" + innerEx.Message;
                            innerEx = innerEx.InnerException;
                        }
                    }
                    _errorCode = ConverterErrorCode.OtherException;
                    return false;
                }
            }
            else
            {
                _errorCode = ConverterErrorCode.FileDoesNotExist;
                return false;
            }
            if (outputFileCount > 0)
            {
                return true;
            }
            else
            {
                _errorCode = ConverterErrorCode.NoOputputFile;
                return false;
            }
        }
    }
}
