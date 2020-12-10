Write-Host "Running ChainReplication [Uncontrolled]"
.\Benchmarks\Uncontrolled\bin\net5.0\ChainReplication.exe

Write-Host "Running FailureDetector [Uncontrolled]"
.\Benchmarks\Uncontrolled\bin\net5.0\FailureDetector.exe

Write-Host "Running Paxos [Uncontrolled]"
.\Benchmarks\Uncontrolled\bin\net5.0\Paxos.exe

Write-Host "Running Raft [Uncontrolled]"
.\Benchmarks\Uncontrolled\bin\net5.0\Raft.exe
