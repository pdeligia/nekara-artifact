﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.IO;
using System.Runtime.Serialization;

using Microsoft.CoyoteActors.IO;
using Microsoft.CoyoteActors.TestingServices.Coverage;

namespace Microsoft.CoyoteActors.TestingServices
{
    /// <summary>
    /// The Coyote testing reporter.
    /// </summary>
    internal sealed class Reporter
    {
#if NET46
        /// <summary>
        /// Emits the testing coverage report.
        /// </summary>
        /// <param name="report">TestReport</param>
        /// <param name="processId">Optional process id that produced the report</param>
        /// <param name="isDebug">Is a debug report</param>
        internal static void EmitTestingCoverageReport(TestReport report, uint? processId = null, bool isDebug = false)
        {
            string file = Path.GetFileNameWithoutExtension(report.Configuration.AssemblyToBeAnalyzed);
            if (isDebug && processId != null)
            {
                file += "_" + processId;
            }

            string directory = CodeCoverageInstrumentation.OutputDirectory;
            if (isDebug)
            {
                directory += $"Debug{Path.DirectorySeparatorChar}";
                Directory.CreateDirectory(directory);
            }

            EmitTestingCoverageOutputFiles(report, directory, file);
        }
#endif

        /// <summary>
        /// Returns (and creates if it does not exist) the output directory with an optional suffix.
        /// </summary>
        internal static string GetOutputDirectory(string userOutputDir, string assemblyPath, string suffix = "", bool createDir = true)
        {
            string directoryPath;

            if (!string.IsNullOrEmpty(userOutputDir))
            {
                directoryPath = userOutputDir + Path.DirectorySeparatorChar;
            }
            else
            {
                var subpath = Path.GetDirectoryName(assemblyPath);
                if (subpath.Length == 0)
                {
                    subpath = ".";
                }

                directoryPath = subpath +
                    Path.DirectorySeparatorChar + "Output" + Path.DirectorySeparatorChar +
                    Path.GetFileName(assemblyPath) + Path.DirectorySeparatorChar;
            }

            if (suffix.Length > 0)
            {
                directoryPath += suffix + Path.DirectorySeparatorChar;
            }

            if (createDir)
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        /// <summary>
        /// Emits all the testing coverage related output files.
        /// </summary>
        /// <param name="report">TestReport containing CoverageInfo</param>
        /// <param name="directory">Output directory name, unique for this run</param>
        /// <param name="file">Output file name</param>
        private static void EmitTestingCoverageOutputFiles(TestReport report, string directory, string file)
        {
            var codeCoverageReporter = new ActivityCoverageReporter(report.CoverageInfo);
            var filePath = $"{directory}{file}";

            string graphFilePath = $"{filePath}.dgml";
            Output.WriteLine($"..... Writing {graphFilePath}");
            codeCoverageReporter.EmitVisualizationGraph(graphFilePath);

            string coverageFilePath = $"{filePath}.coverage.txt";
            Output.WriteLine($"..... Writing {coverageFilePath}");
            codeCoverageReporter.EmitCoverageReport(coverageFilePath);

            string serFilePath = $"{filePath}.sci";
            Output.WriteLine($"..... Writing {serFilePath}");
            using (var fs = new FileStream(serFilePath, FileMode.Create))
            {
                var serializer = new DataContractSerializer(typeof(CoverageInfo));
                serializer.WriteObject(fs, report.CoverageInfo);
            }
        }
    }
}
