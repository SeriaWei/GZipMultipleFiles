using System;
using System.Collections.ObjectModel;

namespace EasyZip
{
    [Serializable]
    public class ZipFileInfoCollection : Collection<ZipFileInfo>
    {
        public ZipFileInfoCollection()
        {

        }
        public ZipFileInfoCollection(string token)
        {
            this.Token = token;
        }
        public string Token { get; set; }
    }
}