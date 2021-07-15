using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Coyote.Tasks;
using DateTimeExtensions.WorkingDays;
using Microsoft.Coyote.SystematicTesting;
using System.Diagnostics;
using System.Threading.Tasks;
using Task = Microsoft.Coyote.Tasks.Task;

namespace DateTimeExtensions.Tests
{

    public class ThreadSafeTests
    {

        [Microsoft.Coyote.SystematicTesting.Test]
        public static void AddWorkingDays_MultipleThreads_CanCalculateAsync()
        {
            //Arrange
            var culture = new WorkingDayCultureInfo("en-US");
            var startDate = new DateTime(2018, 5, 1);

            List<Task> Tasks = new List<Task>();

            foreach (var i in Enumerable.Range(1, 2))
            {
                Tasks.Add(Task.Run( () => startDate.AddWorkingDays(i, culture) ));
            }

            Task.WhenAll(Tasks);        
        }

        [Test]
        public void CheckEaster_MultipleThreads()
        {
            Parallel.ForEach(Enumerable.Range(1, 10), (i) => ascencion_is_39_days_after_easter());
        }

        private void ascencion_is_39_days_after_easter()
        {
            var year = 2025;
            var easterDate = new DateTime(2025, 4, 20);

            var ascencionHoliday = ChristianHolidays.Ascension;
            var ascencion = ascencionHoliday.GetInstance(year);
            Debug.Assert(ascencion.HasValue);
            Debug.Assert(DayOfWeek.Thursday == ascencion.Value.DayOfWeek);

            //source: http://en.wikipedia.org/wiki/Ascension_Day
            // Ascension Day is traditionally celebrated on a Thursday, the fortieth day of Easter
            // again, easter day is included
            Debug.Assert(easterDate.AddDays(39) == ascencion.Value);
        }
    }
}
