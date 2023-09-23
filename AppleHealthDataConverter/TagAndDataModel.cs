using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleHealthDataConverter
{
    internal class TagAndDataModel
    {
        public TagAndDataModel()
        {

        }

        public TagAndDataModel(string tag, string data)
        {
            Data = data;
            Tag = tag;
        }

        public string Data { get; set; } = "";
        public string Tag { get; set; } = "";
        public override string ToString() { return $"{Tag}: {Data}"; }
    }
}
