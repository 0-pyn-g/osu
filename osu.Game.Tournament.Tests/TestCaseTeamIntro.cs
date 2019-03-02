// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Tournament.Screens.Ladder.Components;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseTeamIntro : LadderTestCase
    {
        [Cached]
        private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();

        [BackgroundDependencyLoader]
        private void load()
        {
            var pairing = new MatchPairing();
            pairing.Team1.Value = Ladder.Teams.FirstOrDefault(t => t.Acronym == "USA");
            pairing.Team2.Value = Ladder.Teams.FirstOrDefault(t => t.Acronym == "JPN");
            pairing.Grouping.Value = Ladder.Groupings.FirstOrDefault(g => g.Name.Value == "Finals");
            currentMatch.Value = pairing;

            Add(new TeamIntroScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            });
        }
    }
}
