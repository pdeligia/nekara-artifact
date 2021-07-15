﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Common;

namespace FluentAssertions.Execution
{
    internal static class TestFrameworkProvider
    {
        #region Private Definitions

        private static readonly Dictionary<string, ITestFramework> frameworks = new Dictionary<string, ITestFramework>
        {
            ["gallio"] = new GallioTestFramework(),
            ["mspec"] = new MSpecFramework(),
            ["nspec"] = new NSpec1Framework(),
            ["nspec2"] = new NSpecFramework(),
            ["nspec3"] = new NSpecFramework(),
            ["mbunit"] = new MbUnitTestFramework(),
            ["nunit"] = new NUnitTestFramework(),
            ["mstest"] = new MSTestFramework(),
            ["mstestv2"] = new MSTestFrameworkV2(),
            ["xunit"] = new XUnitTestFramework(),
            ["xunit2"] = new XUnit2TestFramework(),

            ["fallback"] = new FallbackTestFramework()
        };

        private static ITestFramework testFramework;

        #endregion

        public static void Throw(string message)
        {
            if (testFramework == null)
            {
                testFramework = DetectFramework();
            }

            testFramework.Throw(message);
        }

        private static ITestFramework DetectFramework()
        {
            ITestFramework detectedFramework = AttemptToDetectUsingAppSetting()
                ?? AttemptToDetectUsingDynamicScanning();

            return detectedFramework;
        }

        private static ITestFramework AttemptToDetectUsingAppSetting()
        {
            string frameworkName = Services.Configuration.TestFrameworkName;
            if (!string.IsNullOrEmpty(frameworkName) && frameworks.ContainsKey(frameworkName.ToLower()))
            {
                ITestFramework framework = frameworks[frameworkName.ToLower()];
                if (!framework.IsAvailable)
                {
                    throw new Exception(
                        "FluentAssertions was configured to use " + frameworkName +
                        " but the required test framework assembly could not be found");
                }

                return framework;
            }

            return null;
        }

        private static ITestFramework AttemptToDetectUsingDynamicScanning()
        {
            return frameworks.Values.FirstOrDefault(framework => framework.IsAvailable);
        }
    }
}
