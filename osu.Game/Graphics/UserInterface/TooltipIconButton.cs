﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using System;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An icon with an action upon click that can be disabled.
    /// </summary>
    public class TooltipIconButton : Container, IHasTooltip
    {
        private readonly SpriteIcon icon;
        private SampleChannel sampleHover;
        private SampleChannel sampleClick;

        /// <summary>
        /// The action to fire upon click if <see cref="IsEnabled"/> is set to true.
        /// </summary>
        public Action Action;

        private bool isEnabled;

        /// <summary>
        /// If set to true, upon click the <see cref="Action"/> will execute.
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                icon.FadeTo(value ? 1 : 0.5f, 250);
            }
        }

        public IconUsage Icon
        {
            get => icon.Icon;
            set => icon.Icon = value;
        }

        public TooltipIconButton()
        {
            isEnabled = true;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                icon = new SpriteIcon
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.8f),
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (isEnabled)
            {
                sampleClick?.Play();
                Action?.Invoke();
            }

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (isEnabled)
                sampleHover?.Play();
            return base.OnHover(e);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
            sampleClick = audio.Sample.Get(@"UI/generic-select-soft");
        }

        public string TooltipText { get; set; }
    }
}
