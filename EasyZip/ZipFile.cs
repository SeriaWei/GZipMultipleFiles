using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyZip
{
    public class ZipFile
    {
        private readonly List<FileInfo> _singleFiles = new List<FileInfo>();
        private readonly List<DirectoryInfo> _directories = new List<DirectoryInfo>();
        private string _token;

        public ZipFile()
        {
            
        }

        public ZipFile(string token)
        {
            _token = token;
        }
        public void AddFile(FileInfo file)
        {
            _singleFiles.Add(file);
        }

        public void AddDirectory(DirectoryInfo directory)
        {
            _directories.Add(directory);
        }

        private void CollectDirectory(string root, DirectoryInfo directory, ZipFileInfoCollection zipFiles)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                CollectFile(root, file, zipFiles);
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                CollectDirectory(root, dir, zipFiles);
            }
        }

        private void CollectFile(string root, FileInfo file, ZipFileInfoCollection zipFiles)
        {
            using (FileStream originalFileStream = file.OpenRead())
            {
                if ((File.GetAttributes(file.FullName) &
                   FileAttributes.Hidden) != FileAttributes.Hidden & file.Extension != ".gz")
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[10240];
                        int count = 0;
                        while ((count = originalFileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, count);
                        }
                        zipFiles.Add(new ZipFileInfo
                        {
                            FileBytes = ms.ToArray(),
                            RelativePath = string.IsNullOrEmpty(root) ? "\\" + file.Name : file.DirectoryName.Replace(root, "") + "\\" + file.Name
                        });
                    }
                }
            }
        }
        public void Compress(string path)
        {
            ZipFileInfoCollection zipFiles = new ZipFileInfoCollection(_token);
            foreach (var dir in _directories)
            {
                string root = dir.Parent == null ? dir.FullName : dir.Parent.FullName;
                CollectDirectory(root, dir, zipFiles);
            }
            foreach (FileInfo fileItem in _singleFiles)
            {
                CollectFile(null, fileItem, zipFiles);
            }
            using (GZipStream stream = new GZipStream(File.Create(path), CompressionMode.Compress))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, zipFiles);
            }
        }

        public void Decompress(FileInfo fileToDecompress, string saveDirectory)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    var zipFiles = binaryFormatter.Deserialize(decompressionStream) as ZipFileInfoCollection;
                    foreach (ZipFileInfo zipFile in zipFiles)
                    {
                        string fileName = saveDirectory + zipFile.RelativePath;
                        if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                        }
                        using (FileStream stream = File.Create(fileName))
                        {
                            stream.Write(zipFile.FileBytes, 0, zipFile.FileBytes.Length);
                        }
                    }
                }
            }
        }

        public MemoryStream ToMemoryStream()
        {
            ZipFileInfoCollection zipFiles = new ZipFileInfoCollection(_token);
            foreach (var dir in _directories)
            {
                string root = dir.Parent == null ? dir.FullName : dir.Parent.FullName;
                CollectDirectory(root, dir, zipFiles);
            }
            foreach (FileInfo fileItem in _singleFiles)
            {
                CollectFile(null, fileItem, zipFiles);
            }

            MemoryStream ms = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(ms, zipFiles);
            ms.Position = 0;
            return ms;
        }

        public ZipFileInfoCollection ToFileCollection(Stream stream)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            return binaryFormatter.Deserialize(stream) as ZipFileInfoCollection;
        }
    }
}