﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleHealthDataConverter
{
    internal class WalkingStepLengthModel
    {
        //blank constructor
        public WalkingStepLengthModel()
        {

        }

        //constructor from input data
        public WalkingStepLengthModel(string input)
        {
            DataTransferModel dtm = new DataTransferModel(input);
            MeasurementTime = dtm.Time;
            Value = dtm.Value;
        }

        public DateTime MeasurementTime { get; set; } = DateTime.MinValue;
        public float Value { get; set; } = -1;

        public override string ToString()
        {
            return $"{Value}: {MeasurementTime}";
        }
    }
}
