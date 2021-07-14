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

The artifact includes source code and scripts for automatically running a subset of the experiments
presented in the paper. This subset was selected using the following two practical criteria:
- The corresponding benchmarks must be publicly available as open-source (and not be proprietary
  and/or closed-source).
- It must be possible to build and run the corresponding benchmarks on a Linux container (and not
  only on a proprietary OS such as Windows and macOS) so that they can be easily and widely
  distributed.

With the above two criteria in mind, the following experiments (and corresponding benchmarks) were
included in this artifact:
- Finding bugs with Nekara in [Memcached](https://github.com/memcached/memcached) (see Table II in
  page 5). Memcached is available as open-source on GitHub, is written in C and can build and run on
  the artifact's Linux container.
- Comparison of systematic testing with Nekara against [Coyote](https://github.com/microsoft/Coyote)
  on the [Coyote Actors](https://microsoft.github.io/coyote/#concepts/actors/overview/) programming
  model (see Table VI in page 9). Coyote is available as open-source on GitHub and is written in C#
  and .NET Core, which makes it available cross-platform, including on the artifact's Linux
  container.

Based on the above selection criteria, the following experiments and benchmarks could not be included:
- Finding bugs in [Verona](https://github.com/microsoft/verona) (i.e. Zevio in the anonymized
  version of the paper, pages 5-6). The experiment uses an earlier branch of Verona that is
  Microsoft-internal and has not been open-sourced yet.
- Finding bugs in CSCS and ECSS (see Section VI-A and VI-B in pages 7-9). These systems are
  proprietary and Microsoft-internal.
- Reproducing bugs found by [TSVD](https://github.com/microsoft/TSVD) (see Table VII in page 10).
  These benchmarks use the legacy .NET Framework which is only available on Windows machines.
- Reproducing bugs found by [Maple](http://web.eecs.umich.edu/~nsatish/papers/OOPSLA-12-Maple.pdf)
  (see Table VII in page 10). These benchmarks use the legacy .NET Framework which is only available
  on Windows machines.

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
