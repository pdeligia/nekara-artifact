# Write-Host "Running ChainReplication [Uncontrolled]"
# .\Benchmarks\Uncontrolled\bin\net5.0\ChainReplication.exe

# Write-Host "Running FailureDetector [Uncontrolled]"
# .\Benchmarks\Uncontrolled\bin\net5.0\FailureDetector.exe

# Write-Host "Running Paxos [Uncontrolled]"
# .\Benchmarks\Uncontrolled\bin\net5.0\Paxos.exe

# Write-Host "Running Raft [Uncontrolled]"
# .\Benchmarks\Uncontrolled\bin\net5.0\Raft.exe

# Write-Host "Running ChainReplication [Coyote]"
# .\Benchmarks\Coyote\bin\net5.0\ChainReplication.exe

# Write-Host "Running FailureDetector [Coyote]"
# .\Benchmarks\Coyote\bin\net5.0\FailureDetector.exe

# Write-Host "Running Paxos [Coyote]"
# .\Benchmarks\Coyote\bin\net5.0\Paxos.exe

# Write-Host "Running Raft [Coyote]"
# .\Benchmarks\Coyote\bin\net5.0\Raft.exe

Write-Host "Running ChainReplication [TPL_N]"
.\Benchmarks\TPL_N\bin\net5.0\ChainReplication.exe

Write-Host "Running FailureDetector [TPL_N]"
.\Benchmarks\TPL_N\bin\net5.0\FailureDetector.exe

Write-Host "Running Paxos [TPL_N]"
.\Benchmarks\TPL_N\bin\net5.0\Paxos.exe

Write-Host "Running Raft [TPL_N]"
.\Benchmarks\TPL_N\bin\net5.0\Raft.exe
