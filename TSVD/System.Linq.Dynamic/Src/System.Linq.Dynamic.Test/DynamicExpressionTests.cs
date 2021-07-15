using System;
using System.Collections.Generic;
using Microsoft.Coyote.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Linq.Dynamic;

namespace Linq.Dynamic.Test
{
    [TestClass]
    public class DynamicExpressionTests
    {

        [TestMethod]
        public static void CreateClass_TheadSafe()
        {
            const int numOfTasks = 2;

            var properties = new[] { new DynamicProperty("prop1", typeof(string)) };

            var tasks = new List<Task>(numOfTasks);

            for (var i = 0; i < numOfTasks; i++)
            {
                tasks.Add(Task.Run(() => DynamicExpression.CreateClass(properties)));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
