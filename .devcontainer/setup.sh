#!/usr/bin/env bash

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

set -e

THIS_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

echo "APT::Acquire::Retries \"5\";" | tee /etc/apt/apt.conf.d/80-retries

# Workaround for known WSL issue https://github.com/microsoft/WSL/issues/4114.
if [ $(uname -r | sed -n 's/.*\( *Microsoft *\).*/\1/ip') ]; then
  apt-get -o Acquire::Check-Valid-Until=false -o Acquire::Check-Date=false update
else
  apt-get update
fi

# Install prerequisites.
apt-get -y --no-install-recommends install \
  ca-certificates software-properties-common wget git build-essential cmake ninja-build

# Install the .NET 5.0 SDK.
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Workaround for known WSL issue https://github.com/microsoft/WSL/issues/4114.
if [ $(uname -r | sed -n 's/.*\( *Microsoft *\).*/\1/ip') ]; then
  apt-get -o Acquire::Check-Valid-Until=false -o Acquire::Check-Date=false update
else
  apt-get update
fi

apt-get -y --no-install-recommends install apt-transport-https

# Workaround for known WSL issue https://github.com/microsoft/WSL/issues/4114.
if [ $(uname -r | sed -n 's/.*\( *Microsoft *\).*/\1/ip') ]; then
  apt-get -o Acquire::Check-Valid-Until=false -o Acquire::Check-Date=false update
else
  apt-get update
fi

apt-get -y --no-install-recommends install dotnet-sdk-5.0

# Build the artifact.
git submodule sync --recursive
git submodule update --init --recursive
bash ${THIS_DIR}/../Nekara/scripts/build.sh
bash ${THIS_DIR}/../CoyoteActors/artifact.sh build
