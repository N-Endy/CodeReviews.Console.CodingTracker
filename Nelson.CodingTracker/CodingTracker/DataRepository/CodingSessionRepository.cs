using System.Configuration;
using System.Globalization;
using CodingTracker.ConsoleInteraction;
using CodingTracker.IDataRepository;
using CodingTracker.Models;
using CodingTracker.Utilities;
using Dapper;
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace CodingTracker.DataRepository
{
    public class CodingSessionRepository : ICodingSessionRepository
    {
        readonly string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString") ?? "";
        private readonly IUserInteraction? _userInteraction;
        private readonly IUtils _utils;

        public CodingSessionRepository(IUserInteraction? userInteraction, IUtils utils)
        {
            _userInteraction = userInteraction;
            _utils = utils;
        }

        public void GetFromDatabase()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            const string selectQuery = @"SELECT * FROM CodingSession";

            // Use Dapper to execute the query and get a list of sessions
            var sessions = connection.Query<CodingSession>(selectQuery).ToList();

            // Display the sessions using Spectre.Console
            if (sessions.Count != 0)
            {
                var table = new Table();
                table.AddColumn(new TableColumn("Id").RightAligned());
                table.AddColumn("StartTime");
                table.AddColumn("EndTime");
                table.AddColumn("Duration");

                foreach (var session in sessions)
                {
                    table.AddRow(
                        session.Id.ToString(),
                        session.StartTime.ToString("dd-MM-yyyy HH:mm:ss"),
                        session.EndTime.ToString("dd-MM-yyyy HH:mm:ss"),
                        session.Duration
                    );
                }
                _userInteraction.ShowMessage(table);
            }
            else
            {
                _userInteraction.ShowMessageTimeout("\n\n[Red]There are no coding sessions stored in the database.[/]");
            }
        }

        public void InsertSessionToDatabase(DateTime startTime, DateTime endTime, string duration)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Define the object with properties for insertion
            var session = new CodingSession
            {
                StartTime = startTime,
                EndTime = endTime,
                Duration = duration
            };

            const string insertQuery = @"
                INSERT INTO CodingSession (StartTime, EndTime, Duration)
                VALUES (@StartTime, @EndTime, @Duration)";

            // Use Dapper to execute the query
            connection.Execute(insertQuery, session);
        }

        public void UpdateEndTimeInDatabase(int id, DateTime endTime)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string checkQuery = "SELECT EXISTS(SELECT 1 From CodingSession WHERE Id = @Id)";
            var exists = connection.ExecuteScalar<int>(checkQuery, new { Id = id });

            if (exists == 0)
            {
                _userInteraction.ShowMessageTimeout($"[Red]\n\nHabit with ID {id} does not exist.\n\n[/]");
                return;
            }

            // Retrieve the start time
            string startTimeQuery = "SELECT StartTime FROM CodingSession WHERE Id = @Id";
            var startTime = connection.ExecuteScalar<DateTime>(startTimeQuery, new { Id = id });

            // Calculate the new duration
            var newDuration = _utils.GetSessionDuration(startTime, endTime);

            // Update the EndTime and Duration
            const string updateQuery = @"
                UPDATE CodingSession
                SET EndTime = @EndTime, Duration = @Duration
                WHERE Id = @Id";

            connection.Execute(updateQuery, new { Id = id, EndTime = endTime, Duration = newDuration });
            _userInteraction.ShowMessageTimeout($"[Green]\n\nHabit with ID {id} has been updated.\n\n[/]");
        }

        public void UpdateStartTimeInDatabase(int id, DateTime startTime)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string checkQuery = "SELECT EXISTS(SELECT 1 From CodingSession WHERE Id = @Id)";
            var exists = connection.ExecuteScalar<int>(checkQuery, new { Id = id });

            if (exists == 0)
            {
                _userInteraction.ShowMessageTimeout("[Red]\n\nHabit with ID {id} does not exist.\n\n[/]");
                return;
            }

            const string updateQuery = @"
                UPDATE CodingSession
                SET StartTime = @StartTime
                WHERE Id = @Id";

            connection.Execute(updateQuery, new { Id = id, StartTime = startTime });
            _userInteraction.ShowMessageTimeout($"[Green]\n\nHabit with ID {id} has been updated.\n\n[/]");
        }
    }
}