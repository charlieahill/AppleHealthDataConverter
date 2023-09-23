using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleHealthDataConverter
{
    internal class StepsPerDayModel
    {
        public StepsPerDayModel()
        {

        }

        //constructor from input data
        public StepsPerDayModel(DateTime day, float steps)
        {
            MeasurementTime = day;
            Value = steps;
        }

        public DateTime MeasurementTime { get; set; } = DateTime.MinValue;
        public float Value { get; set; } = -1;

        public override string ToString()
        {
            return $"{Value}: {MeasurementTime}";
        }
    }
}
