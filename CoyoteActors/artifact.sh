#!/usr/bin/env bash

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

set -e

MODE=$1
RUNS=$2

THIS_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

if [ "$MODE" == "build" ]; then
  # dotnet build ${THIS_DIR}/Framework/Uncontrolled/Coyote.sln
  # dotnet build ${THIS_DIR}/Benchmarks/Uncontrolled/ChainReplication/ChainReplication.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Uncontrolled/FailureDetector/FailureDetector.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Uncontrolled/Paxos/Paxos.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Uncontrolled/Raft/Raft.csproj

  # dotnet build ${THIS_DIR}/Framework/Coyote/Coyote.sln
  # dotnet build ${THIS_DIR}/Benchmarks/Coyote/ChainReplication/ChainReplication.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Coyote/FailureDetector/FailureDetector.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Coyote/Paxos/Paxos.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Coyote/Raft/Raft.csproj

  # dotnet build ${THIS_DIR}/Framework/TPL_N/Coyote.sln
  # dotnet build ${THIS_DIR}/Benchmarks/TPL_N/ChainReplication/ChainReplication.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/TPL_N/FailureDetector/FailureDetector.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/TPL_N/Paxos/Paxos.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/TPL_N/Raft/Raft.csproj
  # dotnet ${THIS_DIR}/packages/microsoft.coyote.test/1.3.0/lib/net5.0/coyote.dll rewrite ${THIS_DIR}/Benchmarks/TPL_N/tpl.nekara.json

  dotnet build ${THIS_DIR}/Framework/Coyote_N/Coyote.sln
  dotnet build ${THIS_DIR}/Benchmarks/Coyote_N/ChainReplication/ChainReplication.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Coyote_N/FailureDetector/FailureDetector.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Coyote_N/Paxos/Paxos.csproj
  # dotnet build ${THIS_DIR}/Benchmarks/Coyote_N/Raft/Raft.csproj
elif [ "$MODE" == "run" ]; then
  mkdir -p ${THIS_DIR}/Results
# echo "Running ChainReplication [Uncontrolled]"
# dotnet ${THIS_DIR}/Benchmarks/Uncontrolled/bin/net5.0/ChainReplication.dll

# echo "Running FailureDetector [Uncontrolled]"
# dotnet ${THIS_DIR}/Benchmarks/Uncontrolled/bin/net5.0/FailureDetector.dll

# echo "Running Paxos [Uncontrolled]"
# dotnet ${THIS_DIR}/Benchmarks/Uncontrolled/bin/net5.0/Paxos.dll

# echo "Running Raft [Uncontrolled]"
# dotnet ${THIS_DIR}/Benchmarks/Uncontrolled/bin/net5.0/Raft.dll

# echo "Running ChainReplication [Coyote]"
# dotnet ${THIS_DIR}/Benchmarks/Coyote/bin/net5.0/ChainReplication.dll

# echo "Running FailureDetector [Coyote]"
# dotnet ${THIS_DIR}/Benchmarks/Coyote/bin/net5.0/FailureDetector.dll

# echo "Running Paxos [Coyote]"
# dotnet ${THIS_DIR}/Benchmarks/Coyote/bin/net5.0/Paxos.dll

# echo "Running Raft [Coyote]"
# dotnet ${THIS_DIR}/Benchmarks/Coyote/bin/net5.0/Raft.dll

# echo "Running ChainReplication [TPL_N]"
# dotnet ${THIS_DIR}/Benchmarks/TPL_N/bin/net5.0/ChainReplication.dll

# echo "Running FailureDetector [TPL_N]"
# dotnet ${THIS_DIR}/Benchmarks/TPL_N/bin/net5.0/FailureDetector.dll

# echo "Running Paxos [TPL_N]"
# dotnet ${THIS_DIR}/Benchmarks/TPL_N/bin/net5.0/Paxos.dll

# echo "Running Raft [TPL_N]"
# dotnet ${THIS_DIR}/Benchmarks/TPL_N/bin/net5.0/Raft.dll

  echo "Running ChainReplication [Coyote_N]"
  dotnet ${THIS_DIR}/Benchmarks/Coyote_N/bin/net5.0/ChainReplication.dll $RUNS

  # echo "Running FailureDetector [Coyote_N]"
  # dotnet ${THIS_DIR}/Benchmarks/Coyote_N/bin/net5.0/FailureDetector.dll

  # echo "Running Paxos [Coyote_N]"
  # dotnet ${THIS_DIR}/Benchmarks/Coyote_N/bin/net5.0/Paxos.dll

  # echo "Running Raft [Coyote_N]"
  # dotnet ${THIS_DIR}/Benchmarks/Coyote_N/bin/net5.0/Raft.dll
else
  echo "Error: mode parameter is missing; please choose 'build' or 'run'."
  exit 1
fi

exit 0
