#!/usr/bin/env bash

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

set -exo pipefail

THIS_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

dotnet build ${THIS_DIR}/Framework/Uncontrolled/Coyote.sln
dotnet build ${THIS_DIR}/Benchmarks/Uncontrolled/ChainReplication/ChainReplication.csproj
dotnet build ${THIS_DIR}/Benchmarks/Uncontrolled/FailureDetector/FailureDetector.csproj
dotnet build ${THIS_DIR}/Benchmarks/Uncontrolled/Paxos/Paxos.csproj
dotnet build ${THIS_DIR}/Benchmarks/Uncontrolled/Raft/Raft.csproj

dotnet build ${THIS_DIR}/Framework/Coyote/Coyote.sln
dotnet build ${THIS_DIR}/Benchmarks/Coyote/ChainReplication/ChainReplication.csproj
dotnet build ${THIS_DIR}/Benchmarks/Coyote/FailureDetector/FailureDetector.csproj
dotnet build ${THIS_DIR}/Benchmarks/Coyote/Paxos/Paxos.csproj
dotnet build ${THIS_DIR}/Benchmarks/Coyote/Raft/Raft.csproj

dotnet build ${THIS_DIR}/Framework/TPL_N/Coyote.sln
dotnet build ${THIS_DIR}/Benchmarks/TPL_N/ChainReplication/ChainReplication.csproj
dotnet build ${THIS_DIR}/Benchmarks/TPL_N/FailureDetector/FailureDetector.csproj
dotnet build ${THIS_DIR}/Benchmarks/TPL_N/Paxos/Paxos.csproj
dotnet build ${THIS_DIR}/Benchmarks/TPL_N/Raft/Raft.csproj
dotnet ${THIS_DIR}/packages/microsoft.coyote.test/1.3.0/lib/net5.0/coyote.dll rewrite ${THIS_DIR}/Benchmarks/TPL_N/tpl.nekara.json

dotnet build ${THIS_DIR}/Framework/Coyote_N/Coyote.sln
dotnet build ${THIS_DIR}/Benchmarks/Coyote_N/ChainReplication/ChainReplication.csproj
dotnet build ${THIS_DIR}/Benchmarks/Coyote_N/FailureDetector/FailureDetector.csproj
dotnet build ${THIS_DIR}/Benchmarks/Coyote_N/Paxos/Paxos.csproj
dotnet build ${THIS_DIR}/Benchmarks/Coyote_N/Raft/Raft.csproj
