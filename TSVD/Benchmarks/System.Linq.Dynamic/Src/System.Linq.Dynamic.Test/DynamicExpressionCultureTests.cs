using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;
//using Microsoft.Coyote.SystematicTesting;

namespace Linq.Dynamic.Test
{
    [TestClass]
    public class DynamicExpressionCultureTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-PT");
        }

        [TestMethod]
        public void Parse_DoubleLiteral_ReturnsDoubleExpression()
        {
            var expression = (ConstantExpression)DynamicExpression.Parse(typeof(double), "1.0");
            Assert.AreEqual(typeof(double), expression.Type);
            Assert.AreEqual(1.0, expression.Value);
        }

        [TestMethod]
        [Microsoft.Coyote.SystematicTesting.Test]
        public static void Parse_FloatLiteral_ReturnsFloatExpression()
        {
            const int numOfTasks = 10;

            var properties = new[] { new DynamicProperty("prop2", typeof(string)) };

            var tasks = new List<Task>(numOfTasks);

            for (int i = 0; i < numOfTasks; i++)
            {
                tasks.Add(Task.Run(() => DynamicExpression.CreateClass(properties)));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
