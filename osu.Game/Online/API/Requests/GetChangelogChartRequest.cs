﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetChangelogChartRequest : APIRequest<APIChangelogChart>
    {
        private readonly string updateStream;

        public GetChangelogChartRequest() => updateStream = null;

        public GetChangelogChartRequest(string updateStreamName) => updateStream = updateStreamName;

        protected override string Target => $@"changelog/{(!string.IsNullOrEmpty(updateStream) ?
            updateStream + "/" : "")}chart-config";
        protected override string Uri => $@"https://houtarouoreki.github.io/fake-api/{Target}"; // for testing
    }
}
