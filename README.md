# Nekara: Generalized Concurrency Testing (Artifact)

This repository contains the artifact accompanying the paper "Nekara: Generalized Concurrency
Testing" that was accepted in ASE 2021.

The artifact is packaged as a Docker image that runs on the Ubuntu 18.04 Linux distribution. It can
be accessed and used through the "Open in VS Code" feature of GitHub, which allows GitHub
repositories to be easily cloned on a pre-configured (with all dependencies!) containerized
development environment using VS Code. This allows the artifact to be built and run on any machine
(Windows, Linux or macOS) that has Docker and VS Code installed.

The artifact [documentation](INSTALL.md) will guide you through the process of setting all this up,
as well as running the artifact and reproducing the results from the paper.

A copy of the paper is included [here](paper.pdf) in pdf format.

**Note:** It is recommended to first read the paper before trying out the artifact, because the
artifact documentation primarily focuses on the technicalities of using it. Any other details are
described in the paper. 

## Contents of the artifact

The artifact includes source code and scripts for automatically building and running the
non-proprietary experiments presented in the paper.

The following experiments (and corresponding benchmarks) are included and buiding/running them is
fully-automated:
- Finding bugs with Nekara in [Memcached](https://github.com/memcached/memcached) (see Table II and
  III in page 5).
- Comparison of systematic testing with Nekara against [Coyote](https://github.com/microsoft/Coyote)
  on the [Coyote Actors](https://microsoft.github.io/coyote/#concepts/actors/overview/) programming
  model (see Table VI in page 9).
- Reproducing bugs found by [Maple](http://web.eecs.umich.edu/~nsatish/papers/OOPSLA-12-Maple.pdf)
  (see Table VII in page 10). **Note:** The SpiderMonkey benchmark is not included in the artifact,
  as it requires an older version of Ubuntu than 18.04 that is used by the artifact container.

The source code and scripts for the [TSVD](https://github.com/microsoft/TSVD) experiment (see Table
VII in page 10) are also included, but they cannot be automatically build and run in the provided
artifact Docker container, because these benchmarks use the legacy .NET Framework that is available
only for Windows. For this reason we mark this experiment as **optional** for the purposes of this
artifact. You can still run it with some manual effort, but it requires a Windows environment and
installing corresponding dependencies.

The following experiments could not be included in the artifact due to being based on
Microsoft-internal source code:
- Finding bugs in [Verona](https://github.com/microsoft/verona) (i.e. Zevio in the anonymized
  version of the paper, pages 5-6). Verona itself is open-sourced on GitHub, but the experiment is
  based on an earlier branch of Verona that is Microsoft-internal and has not been open-sourced.
- Finding bugs in CSCS and ECSS (see Section VI-A and VI-B in pages 7-9). These systems are
  proprietary, closed-source and Microsoft-internal.

The artifact also includes the source code for the
[Nekara](https://github.com/microsoft/coyote-scheduler) systematic testing library presented in the
paper as a [git submodule](Nekara). Nekara has been open sourced under the name `coyote-scheduler`.
The Nekara scheduling API (see Figure 1 in page 2 of the paper) can be found in this [header
file](https://github.com/microsoft/coyote-scheduler/blob/main/include/coyote/scheduler.h). Small
examples of how Nekara can be used to instrument C++ code (similar to the ones presented in Figures
2, 3 and 4 of the paper) can be found
[here](https://github.com/microsoft/coyote-scheduler/tree/main/test/integration).

**Optional:** The Nekara library can be used on its own outside the scope of this artifact. This is
optional and not part of the artifact documentation, which focuses on paper benchmarks and
experiments, and how to reproduce the results. To try out the Nekara library outside of the
artifact, clone its [repository](https://github.com/microsoft/coyote-scheduler) and follow the
documentation and build instructions in the
[README.md](https://github.com/microsoft/coyote-scheduler#readme).

## Setting up and running the artifact

Please read the [INSTALL.md](INSTALL.md) file for documentation on how to setup the artifact dev
environment (which automatically builds the artifact), as well as how to run it and reproduce
experiments from the paper.
