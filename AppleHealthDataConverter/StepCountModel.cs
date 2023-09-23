using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleHealthDataConverter
{
    internal class StepCountModel
    {
        //blank constructor
        public StepCountModel()
        {

        }

        //constructor from input data
        public StepCountModel(string input)
        {
            DataTransferModel dtm = new(input);
            MeasurementTime = dtm.Time;
            Value = dtm.Value;

            if (Value > 5000) { Value = 0; Console.WriteLine("!DATATRIM!"); }
        }

        public DateTime MeasurementTime { get; set; } = DateTime.MinValue;
        public float Value { get; set; } = -1;

        public override string ToString()
        {
            return $"{Value}: {MeasurementTime}";
        }
    }
}
