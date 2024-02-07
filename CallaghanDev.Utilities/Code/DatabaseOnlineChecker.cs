using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.Code
{
    public class DatabaseOnlineChecker
    {
        private readonly string _connectionString;
        private Timer _timer;

        public delegate void SuccessfullConnection();
        public SuccessfullConnection onSuccess;

        public DatabaseOnlineChecker(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void StartChecking(TimeSpan interval)
        {
            _timer = new Timer(CheckDatabase, null, TimeSpan.Zero, interval);
        }

        private void CheckDatabase(object state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // If we reach here, database is running
                    _timer.Dispose();  // Stop the timer once database is available
                    onSuccess?.Invoke();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void StopChecking()
        {
            _timer?.Dispose();
        }
    }
}
