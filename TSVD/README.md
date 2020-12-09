# Testing TSVD Benchmarks
This folder contains artifact of Nekara testing the open-source [TSVD](https://github.com/microsoft/TSVD) C# benchmarks.
These benchmarks uses legacy (old) .NET framework and it requires windows OS to build and test it.
All reported bugs are related to shared access on a common variable (mostly a Dictionary Variable) and are race bugs.

## Benchmark Details
The following are the list of TSVD C# benchmarks where the bugs are reproduced by Nekara.
  - [DataTimeExtention](https://github.com/joaomatossilva/DateTimeExtensions/pull/86) - Wrong API call to dictionary Variable. It creates issue only during concurrent thread access.
  - [FluentAssertion](https://github.com/fluentassertions/fluentassertions/issues/862) - Race condition on a dictionary variable
  - [K8s-client](https://github.com/kubernetes-client/csharp/pull/212) - Race condition on a dictionary variable
  - [Radical](https://github.com/RadicalFx/Radical/issues/108) - Race condition on a dictionary variable
  - [Thunderstruck](https://github.com/19WAS85/Thunderstruck/issues/3) - Race condition on a dictionary variable.
  - [System.Linq.Dynamic](https://github.com/kahanu/System.Linq.Dynamic/pull/48) - Multiple threads tries to add same key into a dictionary variable because of misplaced RW locks.

We left over few C# benchmarks from TSVD reason being either the benchmarks currently not available or reported bug is not in interest of Nekara (not race bugs).

## Mock Dictionary
All these TSVD benchmarks uses [Dictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-5.0) variable in their source code
and are being concurrently accessed by threads. We created a mock for [Dictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-5.0) variable so Nekara can take the
control over the API calls to these variable. Below is a piece of modelled snippet for API [Dictionary.Add(Tkey, TValue)](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.add?view=net-5.0#System_Collections_Generic_Dictionary_2_Add__0__1_)
where the code inside IF block makes Nekara to take control on this API.

```csharp
        public void Add(TKey key, TValue value)
        {
            if (Controller.IsExecutionControlled)
            {
                var TaskId = (int)Task.CurrentId;
                this.sharedEntry = TaskId;
                this.IsWrite = true;
                Controller.ExploreContextSwitch();

                if (this.sharedEntry != TaskId)
                {
                    throw new Exception("Race in Add()");
                }

                this.IsWrite = false;
            }

            this.InnerDictionary.Add(key, value);
        }
```
## Build and Run the Tests
You should have Windows environment to build the benchmarks, open the solution files (`.sln`) using the [Visual Studio 19](https://visualstudio.microsoft.com/) IDE and select "Build Solution". You might require support for .NET4.8 and .NET5.0 on your machine.

Once each benchmark is built, you can manually run the tests via cmd line as follows:
  - **DataTimeExtention**: *cd [...]\DateTimeExtensions\tests\DateTimeExtensions.Tests\bin\Debug\net48 && dotnet ~\.nuget\packages\microsoft.coyote.cli\1.2.6\tools\net5.0\any\coyote.dll test .\DateTimeExtensions.Tests.dll*
  - **FluentAssertion**: *cd [...]\fluentassertions\Tests\Benchmarks\bin\Release\net48 && dotnet ~\.nuget\packages\microsoft.coyote.cli\1.2.6\tools\net5.0\any\coyote.dll test .\Benchmarks.dll -i 10*
  - **K8s-client**: *cd [...]\csharp\examples\attach\bin\Debug\net5 && dotnet ~\.nuget\packages\microsoft.coyote.cli\1.2.6\tools\net5.0\any\coyote.dll test .\attach.dll*
  - **Radical**: *cd [...]\Radical\src\net35\Test.Radical\bin\Debug && dotnet ~\.nuget\packages\microsoft.coyote.cli\1.2.6\tools\net5.0\any\coyote.dll test .\Test.Radical.dll -i 10*
  - **Thunderstruck**: *cd [...]\Thunderstruck\Thunderstruck.Test\bin\Debug && dotnet ~\.nuget\packages\microsoft.coyote.cli\1.2.6\tools\net5.0\any\coyote.dll test .\Thunderstruck.Test.dll -i 5*
  - **System.Linq.Dynamic**: *cd [...]\System.Linq.Dynamic\Src\System.Linq.Dynamic.Test\bin\Debug && dotnet ~\.nuget\packages\microsoft.coyote.cli\1.2.6\tools\net5.0\any\coyote.dll test .\System.Linq.Dynamic.Test.dll*

## Results
Once Nekara finds the bug it will report in the command line along with the bug message and it's respective schedule number. You can also find the reported bug in the *Output* folder. For example you can find the bug details of *Thunderstruck* bench at `[...]\Thunderstruck\Thunderstruck.Test\bin\Debug\Output\Thunderstruck.Test.dll\CoyoteOutput`.
