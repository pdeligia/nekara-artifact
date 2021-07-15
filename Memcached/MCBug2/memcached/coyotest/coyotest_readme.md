# CoyoTest
CoyoTest is a systematic concurrency testing framework for Memcached. 

Table of content:
 - Introduction
 - Installation
 - Getting Started
 - Licensing

## Introduction
 Coyotest allows developers to write more expressive test cases aimed at finding and reproducing rare concurrency bugs. Contrary to stress testing, which relies on an unstructured, hit-and-trial method of triggering concurrency bugs, systematic concurrency testing (SCT), a.k.a. stateless model checking, oderly explores every possible thread interleaving with minimum memory overhead.  Coyotest is based on Coyote scheduler, which is a tool developed at Microsoft Research, aimed at facilitating language-agnostic SCT. 
Coyotest works by taking control over all the sources of randomness in Memcached whether it be the thread schedulings, heap allocators, asynchronous callbacks using libevent and networking APIs.
Since the program space of all possible thread interleavings is very large to explore within a reasonable number of iterations, Coyotest provides developers a way to explicitly specify the points of context switches and thus write more expressive test cases.

## Installation
## Getting started
## Contributing
This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a
CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repositories using our CLA.

## Code of Conduct
This project has adopted the [Microsoft Open Source Code of
Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of
Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact
[opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
