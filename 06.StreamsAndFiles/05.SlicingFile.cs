using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class SlicingFile
{
    private static List<string> files = new List<string>();
    private static MatchCollection matches;
    private static void Main()
    {
        string inputFile = @"../../starozlato.pdf";
        string directory = @"../../";
        int parts = 5;

        Slice(inputFile, directory, parts);

        Assemble(files, directory);
    }

    private static void Assemble(List<string> files, string destinationDirectory)
    {
        // creating the file path for the reconstructed file
        string fileOutputPath = destinationDirectory + "assembled" + "." + matches[0].Groups[2];
        var fsSource = new FileStream(fileOutputPath, FileMode.Create);

        fsSource.Close();
        using (fsSource = new FileStream(fileOutputPath, FileMode.Append))
        {
            // reading the file paths of the parts from the files list
            foreach (var filePart in files)
            {
                using (var partSource = new FileStream(filePart, FileMode.Open))
                {
                    // Create a byte array of the content of the current file
                    Byte[] bytePart = new byte[4096];
                    while (true)
                    {
                        int readBytes = partSource.Read(bytePart, 0, bytePart.Length);
                        if (readBytes == 0)
                        {
                            break;
                        }

                        // Write the bytes to the reconstructed file
                        fsSource.Write(bytePart, 0, readBytes);
                    }
                }
            }
        }
    }

    private static void Slice(string sourceFile, string destinationDirectory, int parts)
    {
        using (var source = new FileStream(sourceFile, FileMode.Open))
        {
            long partSize = (long)Math.Ceiling((double)source.Length / parts);

            // The offset at which to start reading from the source file            
            string currPartPath;
            FileStream fsPart;
            long sizeRemaining = source.Length;

            // extracting name and extension of the input file
            string pattern = @"(\w+)(?=\.)\.(?<=\.)(\w+)";
            Regex pairs = new Regex(pattern);
            matches = pairs.Matches(sourceFile);
            for (int i = 0; i < parts; i++)
            {
                currPartPath = destinationDirectory + String.Format(@"Part-{0}", i) + "." + matches[0].Groups[2];
                files.Add(currPartPath);

                // reading one part size
                using (fsPart = new FileStream(currPartPath, FileMode.Create))
                {
                    long currentPieceSize = 0;
                    byte[] buffer = new byte[4096];
                    while (currentPieceSize < partSize)
                    {
                        int readBytes = source.Read(buffer, 0, buffer.Length);
                        if (readBytes == 0)
                        {
                            break;
                        }

                        // creating one part size file
                        fsPart.Write(buffer, 0, readBytes);
                        currentPieceSize += readBytes;
                    }
                }

                // calculating the remaining file size which iis still too be read
                sizeRemaining = (int)source.Length - (i * partSize);
                if (sizeRemaining < partSize)
                {
                    partSize = sizeRemaining;
                }             
            }
        }
    }
}
