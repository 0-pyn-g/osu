﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Screens.Match.Settings
{
    public class RoomSettingsOverlay : FocusedOverlayContainer
    {
        private const float transition_duration = 500;
        private const float field_padding = 45;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<RoomAvailability> availabilityBind = new Bindable<RoomAvailability>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<int?> maxParticipantsBind = new Bindable<int?>();

        private readonly Room room;
        private readonly Container content;
        private readonly OsuSpriteText typeLabel;

        protected readonly OsuTextBox Name, MaxParticipants;
        protected readonly RoomAvailabilityPicker Availability;
        protected readonly GameTypePicker Type;
        protected readonly TriangleButton Apply;

        public RoomSettingsOverlay(Room room)
        {
            this.room = room;
            Masking = true;

            Child = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex(@"28242d"),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 35, Bottom = 75, Horizontal = SearchableListOverlay.WIDTH_PADDING },
                        Children = new[]
                        {
                            new SectionContainer
                            {
                                Padding = new MarginPadding { Right = field_padding / 2 },
                                Children = new[]
                                {
                                    new Section("ROOM NAME")
                                    {
                                        Child = Name = new SettingsTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            OnCommit = (sender, text) => apply(),
                                        },
                                    },
                                    new Section("ROOM VISIBILITY")
                                    {
                                        Child = Availability = new RoomAvailabilityPicker(),
                                    },
                                    new Section("GAME TYPE")
                                    {
                                        Child = new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(7),
                                            Children = new Drawable[]
                                            {
                                                Type = new GameTypePicker
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                },
                                                typeLabel = new OsuSpriteText
                                                {
                                                    TextSize = 14,
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                            new SectionContainer
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Padding = new MarginPadding { Left = field_padding / 2 },
                                Children = new[]
                                {
                                    new Section("MAX PARTICIPANTS")
                                    {
                                        Child = MaxParticipants = new SettingsTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            OnCommit = (sender, text) => apply(),
                                        },
                                    },
                                    new Section("PASSWORD (OPTIONAL)")
                                    {
                                        Child = new SettingsTextBox
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            OnCommit = (sender, text) => apply(),
                                        },
                                    },
                                },
                            },
                        },
                    },
                    Apply = new ApplyButton
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(230, 35),
                        Margin = new MarginPadding { Bottom = 20 },
                        Action = apply,
                    },
                },
            };

            Type.Current.ValueChanged += t => typeLabel.Text = t.Name;

            nameBind.ValueChanged += n => Name.Text = n;
            availabilityBind.ValueChanged += a => Availability.Current.Value = a;
            typeBind.ValueChanged += t => Type.Current.Value = t;
            maxParticipantsBind.ValueChanged += m => MaxParticipants.Text = m?.ToString();

            nameBind.BindTo(room.Name);
            availabilityBind.BindTo(room.Availability);
            typeBind.BindTo(room.Type);
            maxParticipantsBind.BindTo(room.MaxParticipants);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            typeLabel.Colour = colours.Yellow;
        }

        protected override void PopIn()
        {
            // reapply the rooms values if the overlay was completely closed
            if (content.Y == -1)
            {
                nameBind.TriggerChange();
                availabilityBind.TriggerChange();
                typeBind.TriggerChange();
                maxParticipantsBind.TriggerChange();
            }

            content.MoveToY(0, transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            content.MoveToY(-1, transition_duration, Easing.InSine);
        }

        private void apply()
        {
            if (room != null)
            {
                room.Name.Value = Name.Text;
                room.Availability.Value = Availability.Current.Value;
                room.Type.Value = Type.Current.Value;

                int max;
                if (int.TryParse(MaxParticipants.Text, out max))
                    room.MaxParticipants.Value = max;
                else
                    room.MaxParticipants.Value = null;
            }

            Hide();
        }

        private class SettingsTextBox : OsuTextBox
        {
            protected override Color4 BackgroundUnfocused => Color4.Black;
            protected override Color4 BackgroundFocused => Color4.Black;

            protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText
            {
                Text = c.ToString(),
                TextSize = 18,
            };
        }

        private class SectionContainer : FillFlowContainer<Section>
        {
            public SectionContainer()
            {
                RelativeSizeAxes = Axes.Both;
                Width = 0.5f;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(field_padding);
            }
        }

        private class Section : Container
        {
            private readonly Container content;

            protected override Container<Drawable> Content => content;

            public Section(string title)
            {
                AutoSizeAxes = Axes.Y;
                RelativeSizeAxes = Axes.X;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            TextSize = 12,
                            Font = @"Exo2.0-Bold",
                            Text = title.ToUpper(),
                        },
                        content = new Container
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                };
            }
        }

        private class ApplyButton : TriangleButton
        {
            public ApplyButton()
            {
                Text = "Apply";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Yellow;
                Triangles.ColourLight = colours.YellowLight;
                Triangles.ColourDark = colours.YellowDark;
            }
        }
    }
}
