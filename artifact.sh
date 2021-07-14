#!/usr/bin/env bash

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

set -e

EXPERIMENT=${1:-6}
RUNS=${2:-10000}

THIS_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"

if [ "$EXPERIMENT" == "memcached" ]; then
  ...
elif [ "$EXPERIMENT" == "coyote" ]; then
  bash ${THIS_DIR}/CoyoteActors/artifact.sh run $RUNS
else
  echo "Error: unknown experiment; please choose 'memcached' or 'coyote'."
  exit 1
fi

exit 0
