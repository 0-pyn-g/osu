// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableMatchPairing : CompositeDrawable
    {
        public readonly MatchPairing Pairing;
        private readonly bool editor;
        protected readonly FillFlowContainer<DrawableMatchTeam> Flow;
        private readonly Drawable selectionBox;
        private readonly Drawable currentMatchSelectionBox;
        private Bindable<MatchPairing> globalSelection;

        [Resolved(CanBeNull = true)]
        private LadderEditorInfo editorInfo { get; set; }

        [Resolved(CanBeNull = true)]
        private LadderInfo ladderInfo { get; set; }

        public DrawableMatchPairing(MatchPairing pairing, bool editor = false)
        {
            Pairing = pairing;
            this.editor = editor;

            AutoSizeAxes = Axes.Both;

            Margin = new MarginPadding(5);

            InternalChildren = new[]
            {
                selectionBox = new Container
                {
                    CornerRadius = 5,
                    Masking = true,
                    Scale = new Vector2(1.05f),
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Colour = Color4.YellowGreen,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                currentMatchSelectionBox = new Container
                {
                    CornerRadius = 5,
                    Masking = true,
                    Scale = new Vector2(1.05f),
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Colour = Color4.OrangeRed,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                Flow = new FillFlowContainer<DrawableMatchTeam>
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2)
                }
            };

            boundReference(pairing.Team1).BindValueChanged(_ => updateTeams());
            boundReference(pairing.Team2).BindValueChanged(_ => updateTeams());
            boundReference(pairing.Team1Score).BindValueChanged(_ => updateWinConditions());
            boundReference(pairing.Team2Score).BindValueChanged(_ => updateWinConditions());
            boundReference(pairing.Round).BindValueChanged(_ =>
            {
                updateWinConditions();
                Changed?.Invoke();
            });
            boundReference(pairing.Completed).BindValueChanged(_ => updateProgression());
            boundReference(pairing.Progression).BindValueChanged(_ => updateProgression());
            boundReference(pairing.LosersProgression).BindValueChanged(_ => updateProgression());
            boundReference(pairing.Losers).BindValueChanged(_ =>
            {
                updateTeams();
                Changed?.Invoke();
            });
            boundReference(pairing.Current).BindValueChanged(_ => updateCurrentMatch(), true);
            boundReference(pairing.Position).BindValueChanged(pos =>
            {
                if (!IsDragged)
                    Position = new Vector2(pos.NewValue.X, pos.NewValue.Y);
                Changed?.Invoke();
            }, true);

            updateTeams();
        }

        /// <summary>
        /// Fired when somethign changed that requires a ladder redraw.
        /// </summary>
        public Action Changed;

        private readonly List<IUnbindable> refBindables = new List<IUnbindable>();

        private T boundReference<T>(T obj)
            where T : IBindable
        {
            obj = (T)obj.GetBoundCopy();
            refBindables.Add(obj);
            return obj;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            foreach (var b in refBindables)
                b.UnbindAll();
        }

        private void updateCurrentMatch()
        {
            if (Pairing.Current.Value)
                currentMatchSelectionBox.Show();
            else
                currentMatchSelectionBox.Hide();
        }

        private bool selected;

        public bool Selected
        {
            get => selected;

            set
            {
                if (value == selected) return;

                selected = value;

                if (selected)
                {
                    selectionBox.Show();
                    if (editor)
                        editorInfo.Selected.Value = Pairing;
                    else
                        ladderInfo.CurrentMatch.Value = Pairing;
                }
                else
                    selectionBox.Hide();
            }
        }

        private void updateProgression()
        {
            if (!Pairing.Completed.Value)
            {
                // ensure we clear any of our teams from our progression.
                // this is not pretty logic but should suffice for now.
                if (Pairing.Progression.Value != null && Pairing.Progression.Value.Team1.Value == Pairing.Team1.Value)
                    Pairing.Progression.Value.Team1.Value = null;

                if (Pairing.Progression.Value != null && Pairing.Progression.Value.Team2.Value == Pairing.Team2.Value)
                    Pairing.Progression.Value.Team2.Value = null;

                if (Pairing.LosersProgression.Value != null && Pairing.LosersProgression.Value.Team1.Value == Pairing.Team1.Value)
                    Pairing.LosersProgression.Value.Team1.Value = null;

                if (Pairing.LosersProgression.Value != null && Pairing.LosersProgression.Value.Team2.Value == Pairing.Team2.Value)
                    Pairing.LosersProgression.Value.Team2.Value = null;
            }
            else
            {
                transferProgression(Pairing.Progression?.Value, Pairing.Winner);
                transferProgression(Pairing.LosersProgression?.Value, Pairing.Loser);
            }

            Changed?.Invoke();
        }

        private void transferProgression(MatchPairing destination, TournamentTeam team)
        {
            if (destination == null) return;

            bool progressionAbove = destination.ID < Pairing.ID;

            Bindable<TournamentTeam> destinationTeam;

            // check for the case where we have already transferred out value
            if (destination.Team1.Value == team)
                destinationTeam = destination.Team1;
            else if (destination.Team2.Value == team)
                destinationTeam = destination.Team2;
            else
            {
                destinationTeam = progressionAbove ? destination.Team2 : destination.Team1;
                if (destinationTeam.Value != null)
                    destinationTeam = progressionAbove ? destination.Team1 : destination.Team2;
            }

            destinationTeam.Value = team;
        }

        private void updateWinConditions()
        {
            if (Pairing.Round.Value == null) return;

            var instaWinAmount = Pairing.Round.Value.BestOf.Value / 2;

            Pairing.Completed.Value = Pairing.Round.Value.BestOf.Value > 0
                                      && (Pairing.Team1Score.Value + Pairing.Team2Score.Value >= Pairing.Round.Value.BestOf.Value || Pairing.Team1Score.Value > instaWinAmount || Pairing.Team2Score.Value > instaWinAmount);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateTeams();

            if (editorInfo != null)
            {
                globalSelection = editorInfo.Selected.GetBoundCopy();
                globalSelection.BindValueChanged(s =>
                {
                    if (s.NewValue != Pairing) Selected = false;
                });
            }
        }

        private void updateTeams()
        {
            if (LoadState != LoadState.Loaded)
                return;

            // todo: teams may need to be bindable for transitions at a later point.

            if (Pairing.Team1.Value == null || Pairing.Team2.Value == null)
                Pairing.CancelMatchStart();

            if (Pairing.ConditionalPairings.Count > 0)
            {
                foreach (var conditional in Pairing.ConditionalPairings)
                {
                    var team1Match = conditional.Acronyms.Contains(Pairing.Team1Acronym);
                    var team2Match = conditional.Acronyms.Contains(Pairing.Team2Acronym);

                    if (team1Match && team2Match)
                        Pairing.Date.Value = conditional.Date.Value;
                }
            }

            Flow.Children = new[]
            {
                new DrawableMatchTeam(Pairing.Team1.Value, Pairing, Pairing.Losers.Value),
                new DrawableMatchTeam(Pairing.Team2.Value, Pairing, Pairing.Losers.Value)
            };

            SchedulerAfterChildren.Add(() => Scheduler.Add(updateProgression));
            updateWinConditions();
        }

        protected override bool OnMouseDown(MouseDownEvent e) => e.Button == MouseButton.Left && editorInfo != null;

        protected override bool OnDragStart(DragStartEvent e) => editorInfo != null;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (Selected && editorInfo != null && e.Key == Key.Delete)
            {
                Remove();
                return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (editorInfo == null || Pairing is ConditionalMatchPairing)
                return false;

            Selected = true;
            return true;
        }

        protected override bool OnDrag(DragEvent e)
        {
            if (base.OnDrag(e)) return true;

            Selected = true;
            this.MoveToOffset(e.Delta);

            var pos = Position;
            Pairing.Position.Value = new Point((int)pos.X, (int)pos.Y);
            return true;
        }

        public void Remove()
        {
            Selected = false;
            Pairing.Progression.Value = null;
            Pairing.LosersProgression.Value = null;

            ladderInfo.Pairings.Remove(Pairing);
        }
    }
}
