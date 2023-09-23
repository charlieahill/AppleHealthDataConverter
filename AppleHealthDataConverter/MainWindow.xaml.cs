using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace AppleHealthDataConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            bgwInterpretData.WorkerReportsProgress = true;
            bgwInterpretData.ProgressChanged += BgwInterpretData_ProgressChanged;
            bgwInterpretData.DoWork += BgwInterpretData_DoWork;
            bgwInterpretData.RunWorkerCompleted += BgwInterpretData_RunWorkerCompleted;
        }

        private readonly BackgroundWorker bgwInterpretData = new();

        /// <summary>
        /// When user clicks the load data button - they are able to locate the data file to import.
        /// File is then read, and then processed
        /// </summary>
        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            if (ofd.ShowDialog() == true)
            {
                string[]? input = ReadInData(ofd.FileName);
                if (input == null) { return; }

                bgwInterpretData.RunWorkerAsync(input);
            }
        }

        /// <summary>
        /// Run the data analysis in a parrallel thread and report progress from there
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BgwInterpretData_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (e.Argument == null) { return; }
            bgwInterpretData.ReportProgress(0, "Beginning data interpretation...");
            e.Result = InterpretData(input: (string[])e.Argument);
        }

        /// <summary>
        /// In case of progress changed - echo the new progress to the user
        /// </summary>
        private void BgwInterpretData_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            AddTextToUserOutput($"{e.UserState}");
        }

        /// <summary>
        /// Adds the following text to the user output
        /// </summary>
        /// <param name="text">Text to add to the output</param>
        private void AddTextToUserOutput(string text)
        {
            ProgramOutputTextBox.Text += $"{text}{Environment.NewLine}";
        }

        /// <summary>
        /// Message to user when the interpretation is completed
        /// </summary>
        private void BgwInterpretData_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            AddTextToUserOutput($"==================INTERPRETATION COMPLETED.=======================");

            AnalyseData(e.Result);
        }

        /// <summary>
        /// Now the data is understood, perform the analyis
        /// </summary>
        private void AnalyseData(object? result)
        {
            if (result == null) return;
            DataMeasurementsClass DataMeasurements = (DataMeasurementsClass)result;

            float StepsToday = DataMeasurements.StepCount.Where(x => x.MeasurementTime.Date == DateTime.Today.AddDays(-5)).Sum(x => x.Value);
            //AddTextToUserOutput($"e.g. Steps 5 days ago was {StepsToday}");

            //Earliest step data
            //DateTime earliestStepData = DataMeasurements.StepCount.OrderBy(x => x.MeasurementTime).First().MeasurementTime;
            //AddTextToUserOutput($"Earliest step count data is {earliestStepData}");

            /*DateTime indexedDate = earliestStepData.Date;
            List<StepsPerDayModel> stepsPerDay = new();
            do
            {
                stepsPerDay.Add(new StepsPerDayModel(indexedDate, DataMeasurements.StepCount.Where(x => x.MeasurementTime.Date == indexedDate).Sum(x => x.Value)));
                indexedDate = indexedDate.AddDays(1);
            } while (indexedDate <= DateTime.Today);

            foreach (StepsPerDayModel day in stepsPerDay.OrderByDescending(x => x.Value))
            {
                AddTextToUserOutput(day.ToString());
            }

            //histogram bands of 10
            float maxNumberOfSteps = stepsPerDay.OrderByDescending(x => x.Value).First().Value;

            List<HistogramModel> histogramOutput = new();
            for (int i = 50; i <= maxNumberOfSteps + 100; i += 100)
            {
                int count = 0;
                foreach (StepsPerDayModel day in stepsPerDay)
                {
                    if ((day.Value > (i - 51)) && (day.Value < (i + 50)))
                    {
                        count++;
                    }
                }
                histogramOutput.Add(new HistogramModel(i, count));
            }

            foreach (HistogramModel histogramBand in histogramOutput)
                AddTextToUserOutput($"{histogramBand.BandCenter},{histogramBand.Count}");*/

            //All we have currently implemented is the results of each weigh in so...
            StringBuilder outputText = new();

            foreach (WeightModel weightMeasurement in DataMeasurements.Weight.OrderBy(x => x.MeasurementTime))
            {
                string BMIString = "";
                if (DataMeasurements.BMI.Where(x => x.MeasurementTime == weightMeasurement.MeasurementTime).Count() > 0)
                    BMIString = DataMeasurements.BMI.First(x => x.MeasurementTime == weightMeasurement.MeasurementTime).Value.ToString();

                string BodyFatPercentage = "";
                if (DataMeasurements.BodyFatPercentage.Where(x => x.MeasurementTime == weightMeasurement.MeasurementTime).Count() > 0)
                    BodyFatPercentage = DataMeasurements.BodyFatPercentage.First(x => x.MeasurementTime == weightMeasurement.MeasurementTime).Value.ToString();

                outputText.AppendLine($"{weightMeasurement.MeasurementTime.Date.ToShortDateString()},{weightMeasurement.MeasurementTime.ToShortTimeString()},{weightMeasurement.Value},{BMIString},{BodyFatPercentage}");
            }

            Clipboard.SetText(outputText.ToString());
            AddTextToUserOutput(outputText.ToString());
            AddTextToUserOutput("Data copied to clipboard");
        }

        /// <summary>
        /// Interpretation logic for understanding the health data
        /// </summary>
        /// <param name="input">The file read from the xml export from health data</param>
        private DataMeasurementsClass InterpretData(string[] input)
        {
            //Create an array to store any data measurements interpretated from the data
            DataMeasurementsClass DataMeasurements = new();

            //Create a debug array of lines of input that are not interpretated / understood
            List<string> sbOutputUnusedLines = new();

            //For every string of input - see if it is understood...
            foreach (string line in input)
            {
                if (IsMeasurementCurrentlyIgnored(line))
                    continue;

                //BMI
                if (IsBMIMeasurement(line))
                {
                    DataMeasurements.BMI.Add(new BMIModel(line));
                    continue;
                }

                //Weight
                if (IsWeightMeasurement(line))
                {
                    DataMeasurements.Weight.Add(new WeightModel(line));
                    continue;
                }

                //Body Fat Percentage
                if (IsBodyFatPercentageMeasurement(line))
                {
                    DataMeasurements.BodyFatPercentage.Add(new BodyFatPercentageModel(line));
                    continue;
                }

                //Lean Body Mass
                if (IsLeanBodyMassMeasurement(line))
                {
                    DataMeasurements.LeanBodyMass.Add(new LeanBodyMassModel(line));
                    continue;
                }

                //Step Count
                if (IsStepCountMeasurement(line))
                {
                    DataMeasurements.StepCount.Add(new StepCountModel(line));
                    continue;
                }

                //Walking speed
                if (IsWalkingSpeedMeasurement(line))
                {
                    DataMeasurements.WalkingSpeed.Add(new WalkingSpeedModel(line));
                    continue;
                }

                //Walking step length
                if (IsWalkingStepLength(line))
                {
                    DataMeasurements.WalkingStepLength.Add(new WalkingStepLengthModel(line));
                    continue;
                }

                //Add all unknown lines to the debug items list
                sbOutputUnusedLines.Add(line);
            }

            if (sbOutputUnusedLines.Count > 1)
            {
                bgwInterpretData.ReportProgress(0, $"Number of unused lines: {sbOutputUnusedLines.Count}");
                bgwInterpretData.ReportProgress(0, sbOutputUnusedLines[0]);
            }
            else
                bgwInterpretData.ReportProgress(0, "All lines are recognised, and interpreted.");

            return DataMeasurements;
        }

        private static bool IsMeasurementCurrentlyIgnored(string line)
        {
            //Distance walking / running
            if (IsDistanceWalkingRunningMeasurement(line))
                return true; //discard data - not currently used

            //Basal energy burned
            if (IsBasalEnergyBurnedMeasurement(line))
                return true; //discard data - not currently used

            //Active energy burned
            if (IsActiveEnergyBurnedMeasurement(line))
                return true; //discard data - not currently used

            //Active energy burned
            if (IsFlightsClimbedMeasurement(line))
                return true; //discard data - not currently used

            //Headphones Audio Exposure Measurement
            if (IsHeadphonesAudioExposureMeasurement(line))
                return true; //discard data - not currently used

            //Double Walking Support Measurement
            if (IsDoubleWalkingSupportMeasurement(line))
                return true; //discard data - not currently used

            //Walking Asymmetry Percentage Measurement
            if (IsWalkingAsymmetryPercentageMeasurement(line))
                return true; //discard data - not currently used

            //Sleep goal
            if (IsSleepGoalMeasurement(line))
                return true; //discard data - not currently used

            //Apple walking steadiness
            if (IsAppleWalkingSteadinessMeasurement(line))
                return true; //discard data - not currently used

            //Sleep analysis
            if (IsSleepAnalysisMeasurement(line))
                return true; //discard data - not currently used

            if (IsKnownFromKnownCodeLines(line))
                return true; //discard data - not currently used

            if (IsMarkedAsXMLComment(line))
                return true; //discard data - not currently used

            if (DoesNotStartWithOpenAngleBracket(line))
                return true; //discard data - not currently used

            return false;
        }

        private static string[]? ReadInData(string filepath)
        {
            try
            {
                return File.ReadAllLines(filepath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to read data {Environment.NewLine}{Environment.NewLine}{ex}");
                return null;
            }
        }

        static bool IsBMIMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierBodyMassIndex"))
                return true;

            return false;
        }

        static bool IsWeightMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierBodyMass"))
                return true;

            return false;
        }

        static bool IsBodyFatPercentageMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierBodyFatPercentage"))
                return true;

            return false;
        }

        static bool IsLeanBodyMassMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierLeanBodyMass"))
                return true;

            return false;
        }

        static bool IsStepCountMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierStepCount"))
                return true;

            return false;
        }

        static bool IsWalkingSpeedMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierWalkingSpeed"))
                return true;

            return false;
        }

        static bool IsWalkingStepLength(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierWalkingStepLength"))
                return true;

            return false;
        }

        static bool IsDistanceWalkingRunningMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierDistanceWalkingRunning"))
                return true;

            return false;
        }

        static bool IsBasalEnergyBurnedMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierBasalEnergyBurned"))
                return true;

            return false;
        }

        static bool IsActiveEnergyBurnedMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierActiveEnergyBurned"))
                return true;

            return false;
        }

        static bool IsFlightsClimbedMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierFlightsClimbed"))
                return true;

            return false;
        }

        static bool IsHeadphonesAudioExposureMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierHeadphoneAudioExposure"))
                return true;

            return false;
        }

        static bool IsDoubleWalkingSupportMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierWalkingDoubleSupportPercentage"))
                return true;

            return false;
        }

        static bool IsWalkingAsymmetryPercentageMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierWalkingAsymmetryPercentage"))
                return true;

            return false;
        }

        static bool IsSleepGoalMeasurement(string input)
        {
            if (input.Contains("HKDataTypeSleepDurationGoal"))
                return true;

            return false;
        }

        static bool IsAppleWalkingSteadinessMeasurement(string input)
        {
            if (input.Contains("HKQuantityTypeIdentifierAppleWalkingSteadiness"))
                return true;

            return false;
        }

        private static bool IsSleepAnalysisMeasurement(string input)
        {
            if (input.Contains("HKCategoryTypeIdentifierSleepAnalysis"))
                return true;

            return false;
        }

        static bool IsMarkedAsXMLComment(string input)
        {
            if (input.Length < 4)
                return false;

            if (input[..4] == "<!--")
                return true;

            else return false;
        }

        static bool IsKnownFromKnownCodeLines(string input)
        {
            if (input.Contains("<?xml version="))
                return true;

            if (input.Contains("!DOCTYPE"))
                return true;

            if (input.Contains("!ELEMENT"))
                return true;

            if (input.Contains("!ATTLIST"))
                return true;

            if (input.Contains("en_GB"))
                return true;

            if (input.Contains("ExportDate"))
                return true;

            if (input.Contains("HKCharacteristicTypeIdentifierDateOfBirth"))
                return true;

            if (input.Contains("MetadataEntry"))
                return true;

            if (input.Contains("/Record>"))
                return true;

            //Height data is only via manual input and not reliable
            if (input.Contains("HKQuantityTypeIdentifierHeight"))
                return true;

            if (input.Contains("ActivitySummary"))
                return true;

            if (input.Contains("/HealthData"))
                return true;

            return false;
        }

        private static bool DoesNotStartWithOpenAngleBracket(string input)
        {
            if (input.Length < 1)
                return false;

            if (input.Trim()[..1] == "<")
                return false;

            else return true;
        }
    }
}
