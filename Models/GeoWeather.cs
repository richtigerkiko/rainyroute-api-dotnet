    public class GeoWeather
    {
        public double TotalDistance { get; set; }
        public double DistanceFromLastPoint { get; set; }
        public GeoCoordinates Coordinates { get; set; }
        public double TotalDuration { get; set; }
        public double DurationFromLastPoint { get; set; }
        public Direction Direction { get; set; }
        public Hour? WeatherForecastAtDuration { get; set; }
        public List<Hour> CompleteForecast { get; set; }

        public GeoWeather(GeoCoordinates coordinates, double distanceFromLastPoint, double durationFromLastPoint, GeoWeather? LastPoint)
        {
            Coordinates = coordinates;

            DistanceFromLastPoint = distanceFromLastPoint;
            TotalDistance = (LastPoint?.TotalDistance ?? 0) + DistanceFromLastPoint;

            DurationFromLastPoint = durationFromLastPoint;
            TotalDuration = (LastPoint?.TotalDuration ?? 0) + DurationFromLastPoint;

            Direction = CalculateDirection((LastPoint?.Coordinates ?? new GeoCoordinates()), Coordinates);
        }

        // From https://stackoverflow.com/questions/35104991/relative-cardinal-direction-of-two-coordinates merci!
        private Direction CalculateDirection(GeoCoordinates coordinatesStart, GeoCoordinates coordinatesDestination)
        {
            double angle = Math.Atan2(coordinatesDestination.Longitude - coordinatesStart.Longitude, coordinatesDestination.Latitude - coordinatesStart.Latitude);
            angle += Math.PI;
            angle /= Math.PI / 4;
            int halfQuarter = Convert.ToInt32(angle);
            halfQuarter %= 8;
            return (Direction)halfQuarter;
        }

        public void FillHour (List<Hour> weatherForecast, DateTime routeStartDate){
            var dateTimeAtDuration = routeStartDate.AddSeconds(TotalDuration);

            WeatherForecastAtDuration = weatherForecast.Where(x => (DateTime.ParseExact(x.Time, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture).Hour) == dateTimeAtDuration.Hour).FirstOrDefault();
            CompleteForecast = weatherForecast;
        }
    }