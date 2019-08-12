﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchBeatmapPanel : MultiplayerComposite
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private GetBeatmapSetRequest request;
        private DirectGridPanel panel;

        public MatchBeatmapPanel()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            CurrentItem.BindValueChanged(item =>
            {
                request?.Cancel();

                if (panel != null)
                {
                    panel.FadeOut(200);
                    panel.Expire();
                    panel = null;
                }

                var onlineId = item.NewValue?.Beatmap.OnlineBeatmapID;

                if (onlineId.HasValue)
                {
                    request = new GetBeatmapSetRequest(onlineId.Value, BeatmapSetLookupType.BeatmapId);
                    request.Success += beatmap =>
                    {
                        panel = new DirectGridPanel(beatmap.ToBeatmapSet(rulesets));
                        LoadComponentAsync(panel, AddInternal);
                    };
                    api.Queue(request);
                }
            }, true);
        }
    }
}
