using System.Text;

namespace RaceAidParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "RaceAID.txt");
            string outputCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "RaceAID.csv");

            List<RacingDriverRecord> records = ParseFile(filePath);

            //Console.WriteLine($"Driver,Frame,Lap,Time");

            var uniqueRecords = records
                //.Where(x => x.Index == 6)
                .OrderBy(record => record.Index)
                .ThenBy(record => record.LapsComplete)
                .ThenBy(record => record.TotalRaceTime)
                .GroupBy(record => new { record.Driver, record.LapsComplete })
                .Select(group => group.Last())
                .ToList();

            // Group by driver
            var driverGroups = uniqueRecords
                .GroupBy(record => record.Driver)
                .ToDictionary(group => group.Key, group => group.ToDictionary(r => r.LapsComplete, r => r));

            // Find the maximum number of laps to determine the columns
            int maxLaps = uniqueRecords.Max(r => r.LapsComplete);

            // Print the header
            var csvBuilder = new StringBuilder();

            Console.Write("Driver");
            csvBuilder.Append("Driver");
            for (int i = 0; i <= maxLaps; i++)
            {
                Console.Write($",Lap{i:D2}");
                csvBuilder.Append($",Lap{i:D2}");
            }
            Console.WriteLine();
            csvBuilder.AppendLine();

            //// Display the parsed records (optional, for testing purposes)
            //foreach (var record in uniqueRecords)
            //{
            //    Console.WriteLine($"{record.Index:D2}|{record.Driver},{record.Frame},{record.LapsComplete},{record.TotalRaceTime}");
            //}

            // Print each driver's data
            foreach (var driver in driverGroups.Keys)
            {
                var driverRecords = driverGroups[driver];
                var record = driverRecords.Values.First();
                Console.Write($"{record.Index}|{record.Driver}");
                csvBuilder.Append($"{record.Index}|{record.Driver}");

                for (int i = 0; i <= maxLaps; i++)
                {
                    if (driverRecords.TryGetValue(i, out var lapRecord))
                    {
                        Console.Write($",{lapRecord.TotalRaceTime}");
                        csvBuilder.Append($",{lapRecord.TotalRaceTime}");
                    }
                    else
                    {
                        Console.Write(","); // No data for this lap
                        csvBuilder.Append(",");
                    }
                }
                Console.WriteLine();
                csvBuilder.AppendLine();
            }

            string? outputDirectory = Path.GetDirectoryName(outputCsvPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            File.WriteAllText(outputCsvPath, csvBuilder.ToString());
        }

        static List<RacingDriverRecord> ParseFile(string filePath)
        {
            List<RacingDriverRecord> records = new List<RacingDriverRecord>();

            string[] fileLines = File.ReadAllLines(filePath);
            List<string> recordLines = new List<string>();
            var separatorCount = 0;

            foreach (string fileLine in fileLines)
            {
                if (fileLine.StartsWith("+++++--"))
                {
                    separatorCount++;

                    if ((recordLines.Count > 0) && (separatorCount > 2))
                    {
                        records.Add(ParseRecord(recordLines));
                        recordLines.Clear();
                        separatorCount = 0;
                    }
                }
                else
                {
                    recordLines.Add(fileLine);
                }
            }

            return records;
        }

        static RacingDriverRecord ParseRecord(List<string> recordLines)
        {
            RacingDriverRecord record = new RacingDriverRecord();

            foreach (string line in recordLines)
            {
                if (line.Contains(" - index "))
                {
                    var parts = line.Split(new[] { " - index " }, StringSplitOptions.None);
                    record.Driver = parts[0];
                    record.Index = int.Parse(parts[1]);
                }
                else if (line.Contains(": "))
                {
                    var parts = line.Split(new[] { ": " }, StringSplitOptions.None);
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "laps_complete":
                            record.LapsComplete = int.Parse(value);
                            break;
                        case "frame":
                            record.Frame = int.Parse(value);
                            break;
                        case "true_speed":
                            record.TrueSpeed = double.Parse(value);
                            break;
                        case "points":
                            record.Points = int.Parse(value);
                            break;
                        case "speed":
                            record.Speed = double.Parse(value);
                            break;
                        case "fuel":
                            record.Fuel = int.Parse(value);
                            break;
                        case "actual_fuel":
                            record.ActualFuel = double.Parse(value);
                            break;
                        case "fuel_consumption_rate_per_lap":
                            record.FuelConsumptionRatePerLap = double.Parse(value);
                            break;
                        case "standard_fuel_consumption_rate_per_lap":
                            record.StandardFuelConsumptionRatePerLap = double.Parse(value);
                            break;
                        case "fuel_consumed_this_lap":
                            record.FuelConsumedThisLap = double.Parse(value);
                            break;
                        case "tyre_dry_type":
                            record.TyreDryType = int.Parse(value);
                            break;
                        case "tyre_set_chosen":
                            record.TyreSetChosen = int.Parse(value);
                            break;
                        case "tyre_wear_stored":
                            record.TyreWearStored = int.Parse(value);
                            break;
                        case "tyre_scrubbed":
                            record.TyreScrubbed = int.Parse(value);
                            break;
                        case "tyres_temperature":
                            record.TyresTemperature = int.Parse(value);
                            break;
                        case "precalc_tyre_degrad_modifiers":
                            record.PrecalcTyreDegradModifiers = int.Parse(value);
                            break;
                        case "tyre_laps_done":
                            record.TyreLapsDone = int.Parse(value);
                            break;
                        case "total_tyre_degradation":
                            record.TotalTyreDegradation = double.Parse(value);
                            break;
                        case "precalc_tyre_grip_modifiers":
                            record.PrecalcTyreGripModifiers = int.Parse(value);
                            break;
                        case "in_trouble":
                            record.InTrouble = int.Parse(value);
                            break;
                        case "retirement_reason":
                            record.RetirementReason = int.Parse(value);
                            break;
                        case "status":
                            record.Status = int.Parse(value);
                            break;
                        case "special_status":
                            record.SpecialStatus = int.Parse(value);
                            break;
                        case "overtaking":
                            record.Overtaking = int.Parse(value);
                            break;
                        case "being_overtaken":
                            record.BeingOvertaken = int.Parse(value);
                            break;
                        case "car_overtaking":
                            record.CarOvertaking = int.Parse(value);
                            break;
                        case "precalc_car_behind":
                            record.PrecalcCarBehind = int.Parse(value);
                            break;
                        case "precalc_car_ahead":
                            record.PrecalcCarAhead = int.Parse(value);
                            break;
                        case "precalc_stall":
                            record.PrecalcStall = int.Parse(value);
                            break;
                        case "precalc_engine_reliability":
                            record.PrecalcEngineReliability = int.Parse(value);
                            break;
                        case "precalc_engine_heat":
                            record.PrecalcEngineHeat = int.Parse(value);
                            break;
                        case "precalc_brakes":
                            record.PrecalcBrakes = int.Parse(value);
                            break;
                        case "precalc_suspension":
                            record.PrecalcSuspension = int.Parse(value);
                            break;
                        case "precalc_clutch":
                            record.PrecalcClutch = int.Parse(value);
                            break;
                        case "precalc_electronics":
                            record.PrecalcElectronics = int.Parse(value);
                            break;
                        case "precalc_gearbox":
                            record.PrecalcGearbox = int.Parse(value);
                            break;
                        case "precalc_hydraulics":
                            record.PrecalcHydraulics = int.Parse(value);
                            break;
                        case "precalc_throttle":
                            record.PrecalcThrottle = int.Parse(value);
                            break;
                        case "precalc_rear_wing":
                            record.PrecalcRearWing = int.Parse(value);
                            break;
                        case "precalc_driver_mistake":
                            record.PrecalcDriverMistake = int.Parse(value);
                            break;
                        case "driver_mistake":
                            record.DriverMistake = int.Parse(value);
                            break;
                        case "driver_condition":
                            record.DriverCondition = int.Parse(value);
                            break;
                        case "car_condition[P_OVERALL_CONDITION]":
                            record.CarConditionPOverallCondition = int.Parse(value);
                            break;
                        case "car_condition[P_FRONT_WING]":
                            record.CarConditionPFrontWing = int.Parse(value);
                            break;
                        case "car_condition[P_REAR_WING]":
                            record.CarConditionPRearWing = int.Parse(value);
                            break;
                        case "car_condition[P_ENGINE]":
                            record.CarConditionPEngine = int.Parse(value);
                            break;
                        case "car_condition[P_BRAKES]":
                            record.CarConditionPBrakes = int.Parse(value);
                            break;
                        case "car_condition[P_CLUTCH]":
                            record.CarConditionPClutch = int.Parse(value);
                            break;
                        case "car_condition[P_ELECTRONICS]":
                            record.CarConditionPElectronics = int.Parse(value);
                            break;
                        case "car_condition[P_GEARS]":
                            record.CarConditionPGears = int.Parse(value);
                            break;
                        case "car_condition[P_HYDRAULICS]":
                            record.CarConditionPHydraulics = int.Parse(value);
                            break;
                        case "car_condition[P_SUSPENSION]":
                            record.CarConditionPSuspension = int.Parse(value);
                            break;
                        case "car_condition[P_THROTTLE]":
                            record.CarConditionPThrottle = int.Parse(value);
                            break;
                        case "preset_start_condition[P_OVERALL_CONDITION]":
                            record.PresetStartConditionPOverallCondition = int.Parse(value);
                            break;
                        case "preset_start_condition[P_FRONT_WING]":
                            record.PresetStartConditionPFrontWing = int.Parse(value);
                            break;
                        case "preset_start_condition[P_REAR_WING]":
                            record.PresetStartConditionPRearWing = int.Parse(value);
                            break;
                        case "preset_start_condition[P_ENGINE]":
                            record.PresetStartConditionPEngine = int.Parse(value);
                            break;
                        case "preset_start_condition[P_BRAKES]":
                            record.PresetStartConditionPBrakes = int.Parse(value);
                            break;
                        case "preset_start_condition[P_CLUTCH]":
                            record.PresetStartConditionPClutch = int.Parse(value);
                            break;
                        case "preset_start_condition[P_ELECTRONICS]":
                            record.PresetStartConditionPElectronics = int.Parse(value);
                            break;
                        case "preset_start_condition[P_GEARS]":
                            record.PresetStartConditionPGears = int.Parse(value);
                            break;
                        case "preset_start_condition[P_HYDRAULICS]":
                            record.PresetStartConditionPHydraulics = int.Parse(value);
                            break;
                        case "preset_start_condition[P_SUSPENSION]":
                            record.PresetStartConditionPSuspension = int.Parse(value);
                            break;
                        case "preset_start_condition[P_THROTTLE]":
                            record.PresetStartConditionPThrottle = int.Parse(value);
                            break;
                        case "car_problem[0]":
                            record.CarProblem0 = int.Parse(value);
                            break;
                        case "last_lap_time":
                            record.LastLapTime = int.Parse(value);
                            break;
                        case "total_race_time":
                            record.TotalRaceTime = int.Parse(value);
                            break;
                        case "fastest_lap_time":
                            record.FastestLapTime = int.Parse(value);
                            break;
                        case "pulses_per_lap":
                            record.PulsesPerLap = int.Parse(value);
                            break;
                        case "pulses_last_lap":
                            record.PulsesLastLap = int.Parse(value);
                            break;
                        case "do_ease_off":
                            record.DoEaseOff = int.Parse(value);
                            break;
                        case "do_flying_lap":
                            record.DoFlyingLap = int.Parse(value);
                            break;
                        case "do_block":
                            record.DoBlock = int.Parse(value);
                            break;
                        case "do_overtake":
                            record.DoOvertake = int.Parse(value);
                            break;
                        case "do_letcarpass":
                            record.DoLetCarPass = int.Parse(value);
                            break;
                        case "on_flying_lap":
                            record.OnFlyingLap = int.Parse(value);
                            break;
                        case "num_flying_lap":
                            record.NumFlyingLap = int.Parse(value);
                            break;
                        case "driver_orders[car][ACCELERATION]":
                            record.DriverOrdersCarAcceleration = int.Parse(value);
                            break;
                        case "driver_orders[car][BRAKING]":
                            record.DriverOrdersCarBraking = int.Parse(value);
                            break;
                        case "driver_orders[car][TOP_SPEED]":
                            record.DriverOrdersCarTopSpeed = int.Parse(value);
                            break;
                        case "driver_orders[car][KERB_USE]":
                            record.DriverOrdersCarKerbUse = int.Parse(value);
                            break;
                        case "driver_orders[car][OFF_RACING_LINE]":
                            record.DriverOrdersCarOffRacingLine = int.Parse(value);
                            break;
                        case "driver_orders[car][LINE_DEFENCE]":
                            record.DriverOrdersCarLineDefence = int.Parse(value);
                            break;
                        case "driver_orders[car][OVERTAKE_POSITION]":
                            record.DriverOrdersCarOvertakePosition = int.Parse(value);
                            break;
                        case "driver_orders[car][OVERTAKE_BACKMARKERS]":
                            record.DriverOrdersCarOvertakeBackmarkers = int.Parse(value);
                            break;
                        case "track_weather":
                            record.TrackWeather = int.Parse(value);
                            break;
                        case "race_weather":
                            record.RaceWeather = int.Parse(value);
                            break;
                        case "track_temperature_level":
                            record.TrackTemperatureLevel = int.Parse(value);
                            break;
                        case "track_rubber_level":
                            record.TrackRubberLevel = int.Parse(value);
                            break;
                        case "Base Points":
                            record.BasePoints = int.Parse(value);
                            break;
                        case "Points after Driver Orders":
                            record.PointsAfterDriverOrders = int.Parse(value);
                            break;
                        case "Points after Tyre Stiffness":
                            record.PointsAfterTyreStiffness = int.Parse(value);
                            break;
                        case "Points after Fuel":
                            record.PointsAfterFuel = int.Parse(value);
                            break;
                        case "Points after Tyre Wear":
                            record.PointsAfterTyreWear = int.Parse(value);
                            break;
                        case "Points after Tyre Grip":
                            record.PointsAfterTyreGrip = int.Parse(value);
                            break;
                        case "Points after Flying Lap":
                            record.PointsAfterFlyingLap = int.Parse(value);
                            break;
                        case "Speed Conversion":
                            record.SpeedConversion = double.Parse(value);
                            break;
                        case "Speed after weather":
                            record.SpeedAfterWeather = double.Parse(value);
                            break;
                        case "Speed after Driver Commands":
                            record.SpeedAfterDriverCommands = double.Parse(value);
                            break;
                        case "Speed after Car in Trouble":
                            record.SpeedAfterCarInTrouble = double.Parse(value);
                            break;
                        case "Speed after Passing Margin":
                            record.SpeedAfterPassingMargin = double.Parse(value);
                            break;
                    }
                }
            }

            return record;
        }
    }
}
