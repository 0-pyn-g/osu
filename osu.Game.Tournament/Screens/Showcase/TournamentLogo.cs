// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Tournament.Screens.Showcase
{
    public class TournamentLogo : CompositeDrawable
    {
        public TournamentLogo()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Margin = new MarginPadding { Vertical = 5 };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChild = new Sprite
            {
                Texture = textures.Get("game-screen-logo"),
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            };
        }
    }
}
