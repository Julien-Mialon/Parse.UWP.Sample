// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Threading.Tasks;
using System.Threading;

namespace Parse.Internal {
  interface IParseConfigController {
    /// <summary>
    /// Gets the current config controller.
    /// </summary>
    /// <value>The current config controller.</value>
    IParseCurrentConfigController CurrentConfigController { get; }

    /// <summary>
    /// Fetchs the config from the server asynchrounously.
    /// </summary>
    /// <returns>The config async.</returns>
    /// <param name="sessionToken">Session token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ParseConfig> FetchConfigAsync(String sessionToken, CancellationToken cancellationToken);
  }
}
