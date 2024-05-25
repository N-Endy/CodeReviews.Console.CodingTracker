
using System.Globalization;
using CodingTracker.ConsoleInteraction;

namespace CodingTracker.Utilities
{
    public class Utils : IUtils
    {
        private readonly IUserInteraction? _userInteraction;

        public Utils(IUserInteraction? userInteraction)
        {
            _userInteraction = userInteraction;
        }

        public string GetDateInput(string message)
        {
            _userInteraction.ShowMessageTimeout(message);
            string dateInput = _userInteraction.GetUserInput();
            return dateInput;
        }

        public string GetSessionDuration(DateTime startTime, DateTime endTime)
        {
            string duration = endTime.Subtract(startTime).ToString(@"d\.hh\:mm\:ss");
            return duration;
        }

        public DateTime ValidatedStartTime()
        {
            DateTime finalDate = DateTime.Now;
            bool isValidDate = false;

            _userInteraction.ShowMessageTimeout("\n[Green]Do you want start time to be Now? (Y for YES/Any key for NO): [/]");
            string answer = _userInteraction.GetUserInput();

            if (answer.ToLower() == "y")
            {
                finalDate = DateTime.Now;
                isValidDate = true;
            }

            
            // Validation loop for date input.
            while (!isValidDate)
            {
                string date = GetDateInput("\n[Green]Please enter a start date (Format: dd-MM-yy hh:mm:ss): [/]");

                isValidDate = DateTime.TryParseExact(date, "dd-MM-yy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out finalDate);

                if (!isValidDate)
                {
                    _userInteraction.ShowMessageTimeout("[Red]Please enter a valid date.[/]");
                }
            }
            return finalDate;
        }

        public DateTime ValidatedEndTime()
        {
            DateTime finalDate = DateTime.Now;
            bool isValidDate = false;

            
            // Validation loop for date input.
            while (!isValidDate)
            {
                string date = GetDateInput("\n[Green]Please enter an end date (Format: dd-MM-yy hh:mm:ss): [/]");
                isValidDate = DateTime.TryParseExact(date, "dd-MM-yy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out finalDate);

                if (!isValidDate)
                {
                    _userInteraction.ShowMessageTimeout("[Red]Please enter a valid date.[/]");
                }
            }
            return finalDate;
        }

        public List<DateTime> ValidatedTimes()
        {
            DateTime startTime = ValidatedStartTime();
            DateTime endTime = ValidatedEndTime();

            if (endTime.CompareTo(startTime) <= 0)
            {
                _userInteraction.ShowMessageTimeout("\n[Red]The End Time must be after the Start Time[/]\n");
                ValidatedTimes();
            }

            return [startTime, endTime];
        }
    }
}