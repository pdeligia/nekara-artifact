# Nekara: Generalized Concurrency Testing (Artifact)

This repository contains the artifact for the "Nekara: Generalized Concurrency Testing" paper that
was accepted in ASE 2021.

The artifact is packaged as a Docker image that runs Ubuntu 18.04. It uses the "Open in VS Code"
feature of GitHub that allows GitHub repositories to easily open in VS Code using a Docker
container. This allows you to both run experiments, but also edit the artifact code, on a pre-setup
dev environment with ease from your local machine. We will guide you through the process of setting
this up for your machine [here](#prerequisites).

## Setting up the dev environment

### Installing and running Docker

To build the artifact, you first need to have Docker installed and running on your machine. This can
be done by downloading and installing [Docker
Desktop](https://www.docker.com/products/docker-desktop), which is available for Windows, Linux and
macOS.

**Note:** The artifact was tested with Docker version `20.10.7`, it will possibly work with some
earlier versions, but to be sure install the same Docker version that we did (or a later one).

Next, run the Docker Desktop application to start Docker. You will know it's running if you look in
the activity tray and see the Docker whale icon. Docker might take a few minutes to start. If the
whale icon is animated, it is probably still in the process of starting. You can click on the icon
to see the status.

![Running Docker Desktop](https://code.visualstudio.com/assets/docs/remote/containers-tutorial/docker-status.png)

Once you have Docker installed and running, you can confirm that it is working by running the
following command on a **new** terminal:
```
docker --version
```

You should see something like the following output:
```
Docker version 20.10.7, build f0df350
```

### Installing VS Code and required extension

Now that you installed Docker, proceed to install the latest [VS
Code](https://code.visualstudio.com/), which is available for Windows, Linux and macOS. This can be
done [here](https://code.visualstudio.com/Download).

Next, install the "Remote - Containers" extension that lets you run Visual Studio Code inside a
Docker container. This can be done by clicking
[here](vscode:extension/ms-vscode-remote.remote-containers) which will open up the extension in VS
Code.

![VS Code Extension](Images/vs-code-remote-containers-extension.png)

Next, connect to the Docker container in VS Code by using this
[link](https://open.vscode.dev/pdeligia/nekara-artifact) and selecting the "Clone repo in container
volume" option (see highlighted button on the right side in the image below).

![Open in VS Code](Images/vs-code-open-repo.png)

**Note:** If your browser asks you if you want to allow the website to open VS Code, press allow or open.
Next, if VS Code asks you if you want to allow the extension to open the URI, then press Open again.

VS Code will ask you how to create your container configuration. Select `From 'DockerFile'` (second
option) as in the following image:

![VS Code Configuration](Images/vs-code-configuration.png)

It can take several minutes to build the Docker container before it connects to it.

Once it finishes, you should now be connected to the container and able to see the workspace and an
open terminal:

![VS Code Connected](Images/vs-code-connected.png)

**Note:** If the bash terminal in the lower right panel does not appear, then select `Terminal` on the top panel and then select `New Terminal` (or use the keyboard shortcut ``Ctrl + Shift + ` ``).

Now you are ready to [run the artifact](#running-the-artifact)!

## Running the artifact

To build the artifact, run the following command (which can take several minutes to complete) from
the root `nekara-artifact` directory:
```
bash artifact.sh build
```
You are now ready to reproduce the non-proprietary experiments from the paper!

This artifact includes the **4 non-proprietary experiments** from the paper:
- Finding bugs with Nekara in [Memcached](https://www.memcached.org/) (see Table II in page 5).
- Comparison of systematic testing with Nekara against [Coyote](https://github.com/microsoft/Coyote)
  on the [Coyote Actors](https://microsoft.github.io/coyote/#concepts/actors/overview/) programming
  model (see Table VI in page 9).
- Reproducing bugs found by [TSVD](https://github.com/microsoft/TSVD) (see Table VII in page 10).
- Reproducing bugs found by [Maple](http://web.eecs.umich.edu/~nsatish/papers/OOPSLA-12-Maple.pdf)
  (see Table VII in page 10).

**Note:** The following 3 experiments from the paper were not included because they require (1)
proprietary Microsoft-internal systems in the case of the CSCS and ECSS, and (2) an earlier internal
branch of [Verona](https://github.com/microsoft/verona) (i.e. Zevio in the anonymized version of the
paper) that has not been open-sourced yet.

Below we will give instructions on how to run each experiment, and what results you should get. For
more details in what each experiment is doing, please read the corresponding section in the paper.

### Experiment #1: Memcached (Table II)

To run the artifact experiments for finding bugs in Memcached using Nekara (see Table II in page 5
of the paper), run the following command (which can take several minutes to complete) from the root
`nekara-artifact` directory:
```
bash artifact.sh run memcached
```

### Experiment #2: Coyote (Table VI)

To run the artifact experiments for comparing systematic testing with Nekara against Coyote on the
Coyote Actors programming model (see Table VI in page 9 of the paper), run the following command
(which can take several minutes to complete) from the root `nekara-artifact` directory:
```
bash artifact.sh run coyote 10000
```

**Note:** If the above command is taking too long on your machine, you can reduce the test
iterations (i.e. runs) by changing the `10000` value to a smaller value such as `1000`. This will
complete the experiments much faster, but if you run less than the `10000` test iterations than we
run for the paper experiments then it is very likely that the bug-finding ability of Nekara or
Coyote might regress (e.g. if a bug is found 1/10000 times, it might not be found unless you run the
experiment more times). This is normal and expected due to concurrency/scheduling nondeterminism.

The results from running the above command can be found in the `CoyoteActors/Results` directory.
There you will see multiple JSON files, one for each experiment. Each JSON file is named as
`benchmark_target` where benchmark is a benchmark name from TABLE VI (for example
`ChainReplication`) and target is one of `Coyote`, `Coyote_N` and `TPL_N` (the last two are
instrumented with Nekara, as explained in the paper).

For example, you will see the following JSON file:
```json
// chainreplication_coyote.json
{"BuggyIterations":0.0001,"Time":375190.2749}
```

The name of the file corresponds to the `ChainReplication` benchmark run and the `Coyote` target.
The JSON contents are the following:
- `BuggyIterations`, which is the % of the iterations (i.e. runs) that were buggy, in this case out
  of `10000` test iterations, Coyote uncovered the bug once in the benchmark.
- `Time`, which is the time in seconds it took to to run all the iterations in the benchmark.

**Note:** due to nondeterminism in the concurrent execution, as well as variations in the underlying
OS scheduler and machine that Docker is running, some variation in the results from the paper is
totally normal and expected. However, the overall trend should be similar to the paper, and this is
what running these experiments showcases.

### Experiment #3: TSVD (Table VII)

To run the artifact experiments for reproducing bugs found by TSVD (see Table VII in page 10 of the
paper), run the following command (which can take several minutes to complete) from the root
`nekara-artifact` directory:
```
bash artifact.sh run tsvd
```

### Experiment #4: Maple (Table VII)

To run the artifact experiments for reproducing bugs found by Maple (see Table VII in page 10 of the
paper), run the following command (which can take several minutes to complete) from the root
`nekara-artifact` directory:
```
bash artifact.sh run maple
```

## Troubleshooting

### Issue authenticating to Docker
It is unlikely, but if opening the GitHub repository on a VS Code Docker container fails with a
Docker authentication error, then you can fix this by logging in your Docker account (please create
one, if you do not have one already). You can either login using the Docker Desktop GUI or by
running the following command from your terminal (which will ask for your username and password):
```bash
docker login
# Login Succeeded
```

### The device run out of memory
If you get an error that the device run out of memory while building the Docker container after opening the GitHub repository in VS Code, then it is likely that you can fix this by clearing up Docker images and your Docker cache. You can do this by running:
```bash
docker system prune
docker images
docker rmi $IMAGE_ID
```
