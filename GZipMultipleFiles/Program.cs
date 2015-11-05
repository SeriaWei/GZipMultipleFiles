using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EasyZip;
using System.Xml.Serialization;

namespace GZipMultipleFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            ZipFile file = new ZipFile();
            file.AddFile(new FileInfo(@"E:\GitHub\CMS\Easy.CMS.Web\Modules\Section\Views\SectionTemplate.Column.cshtml"));
            file.AddFile(new FileInfo(@"E:\GitHub\CMS\Easy.CMS.Web\Modules\Section\Views\Thumbnail\SectionTemplate.Column.png"));

            var fs = File.Create("templates.gz");
            byte[] buffer = file.ToMemoryStream().ToArray();
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
            fs.Dispose();
        }
    }
}
