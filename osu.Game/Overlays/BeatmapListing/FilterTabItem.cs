﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing
{
    public class FilterTabItem<T> : TabItem<T>
    {
        protected virtual float TextSize => 13;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private readonly OsuSpriteText text;

        public FilterTabItem(T value)
            : base(value)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            AddRangeInternal(new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: TextSize, weight: FontWeight.Regular),
                    Text = CreateText(value)
                },
                new HoverClickSounds()
            });

            Enabled.Value = true;
        }

        protected virtual string CreateText(T value) => (value as Enum)?.GetDescription() ?? value.ToString();

        [BackgroundDependencyLoader]
        private void load()
        {
            updateState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        private void updateState() => text.FadeColour(Active.Value ? Color4.White : getStateColour(), 200, Easing.OutQuint);

        private Color4 getStateColour() => IsHovered ? colourProvider.Light1 : colourProvider.Light3;
    }
}
