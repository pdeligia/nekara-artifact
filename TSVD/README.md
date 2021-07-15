# Testing TSVD Benchmarks
This folder contains artifact of Nekara testing the open-source [TSVD](https://github.com/microsoft/TSVD) C# benchmarks. 
These benchmarks uses legacy (old) .NET framework and it requires windows OS to build and test it. 
All reported bugs are related to shared access on a common variable (mostly a Dictionary Variable) and are race bugs.

## Benchmark Details
The following are the list of TSVD C# benchmarks where the bugs are reproduced by Nekara. 
  - [DataTimeExtention](https://github.com/joaomatossilva/DateTimeExtensions/pull/86) - Wrong API call to Dictionary Variable. It creates issue only during concurrent thread access.
  - [FluentAssertion](https://github.com/fluentassertions/fluentassertions/issues/862), [K8s-client](https://github.com/kubernetes-client/csharp/pull/212), [Radical](https://github.com/RadicalFx/Radical/issues/108) and [Thunderstruck](https://github.com/19WAS85/Thunderstruck/issues/3) - Race condition on a shared variable.
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
