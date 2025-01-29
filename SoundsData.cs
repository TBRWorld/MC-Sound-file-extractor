using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC_Sound_Texture_Extractor
{
    public class SoundsData
    {
        public string hash;
        public string fullFilePath;

        public SoundsData(string hash, string fullFilePath)
        {
            this.hash = hash;
            this.fullFilePath = fullFilePath;
        }

        public string name;
        public string folderPath;

        public void GetName()
        {
            //seperate fullFilePath to get file name and file folder path
            name = Path.GetFileName(fullFilePath);
            folderPath = Path.GetDirectoryName(fullFilePath);
        }
    }
}
