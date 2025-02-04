using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows;

namespace PassThis
{
    public static class ExceptionHandler
    {
        // For functions that return a value
        public static T ExecuteWithHandling<T>(Func<T> function)
        {
            try
            {
                return function();
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + $"Error: {ex.Message}");               
                return default!;
            }
        }

        // For methods that return void
        public static void ExecuteWithHandling(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + $"Error: {ex.Message}");
            }
        }
    }
}
