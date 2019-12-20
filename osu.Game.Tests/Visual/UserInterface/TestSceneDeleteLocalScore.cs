// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneDeleteLocalScore : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Placeholder),
            typeof(MessagePlaceholder),
            typeof(RetrievalFailurePlaceholder),
            typeof(UserTopScoreContainer),
            typeof(Leaderboard<BeatmapLeaderboardScope, ScoreInfo>),
            typeof(LeaderboardScore),
        };

        private readonly FailableLeaderboard leaderboard;

        private readonly DialogOverlay dialogOverlay;

        public TestSceneDeleteLocalScore()
        {
            Add(dialogOverlay = new DialogOverlay
            {
                Depth = -1
            });

            leaderboard = new FailableLeaderboard
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(550f, 450f),
                Scope = BeatmapLeaderboardScope.Local,
                Beatmap = new BeatmapInfo
                {
                    ID = 1,
                    Metadata = new BeatmapMetadata
                    {
                        ID = 1,
                        Title = "TestSong",
                        Artist = "TestArtist",
                        Author = new User
                        {
                            Username = "TestAuthor"
                        },
                    },
                    Version = "Insane"
                },
            };

            AddStep("Insert Local Scores", () => reset());
        }

        private void reset()
        {
            leaderboard.initialLoad = true;
            leaderboard.RefreshScores();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(dialogOverlay);
            Add(leaderboard);
        }

        private class FailableLeaderboard : BeatmapLeaderboard
        {
            private List<ScoreInfo> scoreList;

            private Random rnd;

            public bool initialLoad;

            public void DeleteScore(ScoreInfo score)
            {
                scoreList.Remove(score);
                RefreshScores();
            }

            public FailableLeaderboard()
            {
                initialLoad = true;
            }

            public void SetRetrievalState(PlaceholderState state)
            {
                PlaceholderState = state;
            }

            protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
            {
                if (initialLoad)
                {
                    rnd = new Random();

                    scoreList = Enumerable.Range(1, 50).Select(createScore).ToList();
                    Scores = scoreList.OrderByDescending(s => s.TotalScore).ToArray();

                    initialLoad = false;
                }
                else
                {
                    Scores = scoreList.OrderByDescending(s => s.TotalScore).ToArray();
                }

                return null;
            }

            private ScoreInfo createScore(int id) => new ScoreInfo
            {
                ID = id,
                Accuracy = rnd.NextDouble(),
                PP = rnd.Next(1, 1000000),
                TotalScore = rnd.Next(1, 1000000),
                MaxCombo = rnd.Next(1, 1000),
                Rank = ScoreRank.XH,
                User = new User { Username = "TestUser" },
            };

            protected override LeaderboardScore CreateDrawableScore(ScoreInfo model, int index)
            {
                model.Beatmap = Beatmap;
                return new TestLeaderboardScore(model, index, this, IsOnlineScope);
            }
        }

        private class TestLeaderboardScore : LeaderboardScore
        {
            private DialogOverlay dialogOverlay;

            private readonly FailableLeaderboard leaderboard;

            public TestLeaderboardScore(ScoreInfo score, int rank, FailableLeaderboard leaderboard, bool allowHighlight = true)
                : base(score, rank, allowHighlight)
            {
                this.leaderboard = leaderboard;
            }

            protected override void DeleteLocalScore(ScoreInfo score)
            {
                dialogOverlay?.Push(new TestLocalScoreDeleteDialog(score, leaderboard));
            }

            [BackgroundDependencyLoader]
            private void load(DialogOverlay dialogOverlay)
            {
                this.dialogOverlay = dialogOverlay;
            }
        }

        private class TestLocalScoreDeleteDialog : PopupDialog
        {
            public readonly PopupDialogOkButton ConfirmButton;

            public readonly PopupDialogCancelButton CancelButton;

            public TestLocalScoreDeleteDialog(ScoreInfo score, FailableLeaderboard leaderboard)
            {
                Debug.Assert(score != null);

                string accuracy = string.Format(score.Accuracy % 1 == 0 ? @"{0:P0}" : @"{0:P2}", score.Accuracy);

                BodyText = $@"{score} {Environment.NewLine} Rank: {score.Rank} - Max Combo: {score.MaxCombo} - {accuracy}";
                Icon = FontAwesome.Solid.Eraser;
                HeaderText = @"Deleting this local score. Are you sure?";
                Buttons = new PopupDialogButton[]
                {
                    ConfirmButton = new PopupDialogOkButton
                    {
                        Text = @"Yes. Please.",
                        Action = () => leaderboard.DeleteScore(score)
                    },
                    CancelButton = new PopupDialogCancelButton
                    {
                        Text = @"No, I'm still attached.",
                    },
                };
            }
        }
    }
}
