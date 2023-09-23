using System.Collections.Generic;

namespace AppleHealthDataConverter
{
    internal class DataMeasurementsClass
    {
        public List<BMIModel> BMI = new();
        public List<WeightModel> Weight = new();
        public List<BodyFatPercentageModel> BodyFatPercentage = new();
        public List<LeanBodyMassModel> LeanBodyMass = new();
        public List<StepCountModel> StepCount = new();
        public List<WalkingSpeedModel> WalkingSpeed = new();
        public List<WalkingStepLengthModel> WalkingStepLength = new();
    }
}
