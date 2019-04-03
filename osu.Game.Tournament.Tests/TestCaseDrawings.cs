﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Drawings;
using osu.Game.Tournament.Screens.Drawings.Components;
using osu.Framework.Allocation;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseDrawings : ScreenTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new DrawingsScreen
            {
                TeamList = new TestTeamList(),
            });
        }

        private class TestTeamList : ITeamList
        {
            public IEnumerable<TournamentTeam> Teams { get; } = new[]
            {
                new TournamentTeam
                {
                    FlagName = "GB",
                    FullName = "United Kingdom",
                    Acronym = "UK"
                },
                new TournamentTeam
                {
                    FlagName = "FR",
                    FullName = "France",
                    Acronym = "FRA"
                },
                new TournamentTeam
                {
                    FlagName = "CN",
                    FullName = "China",
                    Acronym = "CHN"
                },
                new TournamentTeam
                {
                    FlagName = "AU",
                    FullName = "Australia",
                    Acronym = "AUS"
                },
                new TournamentTeam
                {
                    FlagName = "JP",
                    FullName = "Japan",
                    Acronym = "JPN"
                },
                new TournamentTeam
                {
                    FlagName = "RO",
                    FullName = "Romania",
                    Acronym = "ROM"
                },
                new TournamentTeam
                {
                    FlagName = "IT",
                    FullName = "Italy",
                    Acronym = "PIZZA"
                },
                new TournamentTeam
                {
                    FlagName = "VE",
                    FullName = "Venezuela",
                    Acronym = "VNZ"
                },
                new TournamentTeam
                {
                    FlagName = "US",
                    FullName = "United States of America",
                    Acronym = "USA"
                },
            };
        }
    }
}
