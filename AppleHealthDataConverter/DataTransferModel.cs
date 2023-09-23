using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleHealthDataConverter
{
    internal class DataTransferModel
    {
        public DateTime Time { get; set; } = DateTime.MinValue;
        public float Value { get; set; } = -1;
        public DataTransferModel(string input)
        {
            List<TagAndDataModel> TagAndData = new();

            StringBuilder sb = new();
            bool inbetweenSpeechMarksMode = false;
            string thisTag = string.Empty;
            for (int i = 0; i < input.Length; i++)
            {
                string nextletter = input.Substring(i, 1);

                if (nextletter == " ")
                    if (!inbetweenSpeechMarksMode)
                        sb = new StringBuilder();

                if (nextletter == "\"")
                {
                    inbetweenSpeechMarksMode = !inbetweenSpeechMarksMode;

                    if (!inbetweenSpeechMarksMode)
                    {
                        TagAndData.Add(new TagAndDataModel(thisTag, sb.ToString()));
                    }

                    sb = new StringBuilder();
                    continue;
                }

                if (nextletter == "=")
                {
                    thisTag = sb.ToString().Trim();
                    sb = new StringBuilder();
                    continue;
                }

                sb.Append(nextletter);
            }


            //find the creation time:
            Time = DateTime.Parse(TagAndData.First(x => x.Tag == "startDate").Data);

            //find the value
            Value = float.Parse(TagAndData.First(x => x.Tag == "value").Data);
        }
    }
}
