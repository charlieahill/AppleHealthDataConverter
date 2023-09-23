using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleHealthDataConverter
{
    internal class HistogramModel
    {
        public int BandCenter { get; set; } = 0;
        public int Count { get; set; } = 0;
        public HistogramModel(int center, int count)
        {
            Count = count;
            BandCenter = center;

        }
    }
}
