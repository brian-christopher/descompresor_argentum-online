using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace descompresor_ao
{
    public static class Zlib
    {
        [DllImport("zlib.dll", EntryPoint = "uncompress")]
        private static extern int Uncompress(byte[] dest, ref uint destLen, byte[] source, int sourceLen);

        private class FILEHEADER
        {
            public int lngNumFiles { get; set; }                 //How many files are inside?
            public int lngFileSize { get; set; }                  //How big is this file? (Used to check integrity)
            public int lngFileVersion { get; set; }                //The resource version (Used to patch)
        }

        //This structure will describe each file contained
        //in our binary file
        private class INFOHEADER
        {
            public int lngFileSize { get; set; }                //'How big is this chunk of stored data?
            public int lngFileStart { get; set; }               //'Where does the chunk start?
            public string? strFileName { get; set; }         //'What's the name of the file this data came from?
            public int lngFileSizeUncompressed { get; set; }    //'How big is the file compressed
        }

        private static List<INFOHEADER> GetFileEntries(string path)
        {
            using var stream = File.Open(path, FileMode.Open);
            using var reader = new BinaryReader(stream);

            var header = new FILEHEADER();
            var entries = new List<INFOHEADER>();

            header.lngNumFiles = reader.ReadInt32();
            header.lngFileSize = reader.ReadInt32();
            header.lngFileVersion = reader.ReadInt32();

            for (int i = 0; i < header.lngNumFiles; i++)
            {
                var fileInfo = new INFOHEADER();
                fileInfo.lngFileSize = reader.ReadInt32();
                fileInfo.lngFileStart = reader.ReadInt32();
                fileInfo.strFileName = Encoding.Latin1.GetString(reader.ReadBytes(16));
                fileInfo.lngFileSizeUncompressed = reader.ReadInt32();
                
                entries.Add(fileInfo);
            } 
            return entries;
        }

        private static byte[] GetFileRawData(string filename, INFOHEADER fileInfo)
        {
            using var stream = File.Open(filename, FileMode.Open);
            using var reader = new BinaryReader(stream);

            stream.Position = fileInfo.lngFileStart - 1; //vb6
            return reader.ReadBytes(fileInfo.lngFileSize);
        }

        public static void ExtractAllFiles(string filename, string outpath)
        {
            foreach (var entry in GetFileEntries(filename))
            {
                var compressedData = GetFileRawData(filename, entry);
                var uncompressedData = new byte[entry.lngFileSizeUncompressed];
                var uncompressedSize = (uint)entry.lngFileSizeUncompressed; 

                var errorCode = Uncompress(uncompressedData, ref uncompressedSize, compressedData, compressedData.Length);
                if(errorCode != 0)
                {
                    throw new ExternalException($"error al descomprimir el archivo \n{entry}"); 
                }
 
                File.WriteAllBytes(outpath + $"/{entry.strFileName}", uncompressedData);
            }
        }
    }
}