# #!/usr/bin/env bash

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

set -e

MODE=$1
RUNS=$2

THIS_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

if [ "$MODE" == "build" ]; then
  # dotnet build ${THIS_DIR}/Framework/Coyote/Coyote.sln

  echo "Building K8s-Client benchmark"
  dotnet build ${THIS_DIR}/Benchmarks/csharp/kubernetes-client.sln

  echo "Building DateTimeExtensions benchmark"
  dotnet build ${THIS_DIR}/Benchmarks/DateTimeExtensions/DateTimeExtensions.sln

  echo "Building FluentAssertions benchmark"
  dotnet build ${THIS_DIR}/Benchmarks/fluentassertions/FluentAssertions.sln

  echo "Building System.Linq.Dynamic benchmark"
  dotnet build ${THIS_DIR}/Benchmarks/System.Linq.Dynamic/Src/System.Linq.Dynamic.sln

  echo "Building Radical benchmark"
  dotnet build ${THIS_DIR}/Benchmarks/Radical/Radical.sln

  echo "Building Thunderstruck benchmark"
  dotnet build ${THIS_DIR}/Benchmarks/Thunderstruck/Thunderstruck.sln

  # echo "Press ENTER to exit..."
  # read abcd
  bashRead-Host "Press ENTER to exit..."

elif [ "$MODE" == "run" ]; then
  mkdir -p ${THIS_DIR}/Results

  # echo "Running ChainReplication [Coyote]"
  # dotnet ${THIS_DIR}/Benchmarks/Coyote/bin/net5.0/ChainReplication.dll $RUNS

else
  echo "Error: mode parameter is missing; please choose 'build' or 'run'."
  exit 1
fi

exit 0