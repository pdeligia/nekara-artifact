#!/usr/bin/env bash

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

set -e

MODE=$1

THIS_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

if [ "$MODE" == "build" ]; then
  git submodule sync --recursive
  git submodule update --init --recursive
  bash ${THIS_DIR}/Nekara/scripts/build.sh
  # bash ${THIS_DIR}/CoyoteActors/artifact.sh build
elif [ "$MODE" == "run" ]; then
  bash ${THIS_DIR}/CoyoteActors/artifact.sh run
else
  echo "Error: mode parameter is missing; please choose 'build' or 'run'."
  exit 1
fi

exit 0
