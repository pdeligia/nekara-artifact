# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

FROM ubuntu:18.04

RUN echo "APT::Acquire::Retries \"5\";" | tee /etc/apt/apt.conf.d/80-retries

# Install prerequisites.
RUN apt-get update && apt-get -y --no-install-recommends install \
  ca-certificates software-properties-common curl git wget build-essential

RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

# Install the .NET 5.0 SDK.
RUN apt-get update; \
  apt-get install -y apt-transport-https && \
  apt-get update && \
  apt-get install -y dotnet-sdk-5.0

# Install CMake.
RUN curl --retry 5 --retry-connrefused https://cmake.org/files/v3.15/cmake-3.15.7-Linux-x86_64.sh -o cmake.sh && \
    chmod +x cmake.sh && \
    ./cmake.sh --prefix=/usr --exclude-subdir --skip-license && \
    rm cmake.sh
