// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections
{
    public class SkinSection : SettingsSection
    {
        private SkinSettingsDropdown skinDropdown;

        public override string Header => "Skin";

        public override IconUsage Icon => FontAwesome.Solid.PaintBrush;

        private readonly Bindable<SkinInfo> dropdownBindable = new Bindable<SkinInfo> { Default = SkinInfo.Default };
        private readonly Bindable<int> configBindable = new Bindable<int>();

        private static readonly SkinInfo random_skin_info = new RandomSkinInfo();

        private SkinManager skins;
        private List<SkinInfo> usableSkins;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SkinManager skins)
        {
            this.skins = skins;

            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                skinDropdown = new SkinSettingsDropdown(),
                new SettingsSlider<double, SizeSlider>
                {
                    LabelText = "Menu cursor size",
                    Bindable = config.GetBindable<double>(OsuSetting.MenuCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsSlider<double, SizeSlider>
                {
                    LabelText = "Gameplay cursor size",
                    Bindable = config.GetBindable<double>(OsuSetting.GameplayCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsCheckbox
                {
                    LabelText = "Adjust gameplay cursor size based on current beatmap",
                    Bindable = config.GetBindable<bool>(OsuSetting.AutoCursorSize)
                },
                new SettingsCheckbox
                {
                    LabelText = "Beatmap skins",
                    Bindable = config.GetBindable<bool>(OsuSetting.BeatmapSkins)
                },
                new SettingsCheckbox
                {
                    LabelText = "Beatmap hitsounds",
                    Bindable = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds)
                },
            };

            skins.ItemAdded += itemAdded;
            skins.ItemRemoved += itemRemoved;

            config.BindWith(OsuSetting.Skin, configBindable);

            usableSkins = skins.GetAllUsableSkins();

            skinDropdown.Bindable = dropdownBindable;
            resetSkinButtons();

            // Todo: This should not be necessary when OsuConfigManager is databased
            if (skinDropdown.Items.All(s => s.ID != configBindable.Value))
                configBindable.Value = 0;

            configBindable.BindValueChanged(id => dropdownBindable.Value = skinDropdown.Items.Single(s => s.ID == id.NewValue), true);
            dropdownBindable.BindValueChanged(skin =>
            {
                if (v == random_skin_info)
                    randomizeSkin();
                else
                    configBindable.Value = skin.NewValue.ID ?? 0;
            });
        }

        private void randomizeSkin()
        {
            int n = usableSkins.Count;
            if (n > 1)
                configBindable.Value = (configBindable.Value + RNG.Next(n - 1) + 1) % n; // make sure it's always a different one
            else
                configBindable.Value = 0;
        }

        private void itemRemoved(SkinInfo s) => Schedule(() =>
        {
            usableSkins.RemoveAll(i => i.ID == s.ID);
            resetSkinButtons();
        });

        private void itemAdded(SkinInfo s)
        {
            if (existing)
                return;

            Schedule(() =>
            {
                usableSkins.Add(s);
                resetSkinButtons();
            });
        }

        private void resetSkinButtons()
        {
            skinDropdown.Items = usableSkins.Count > 1 ? usableSkins.Concat(new[] { random_skin_info }) : usableSkins;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skins != null)
            {
                skins.ItemAdded -= itemAdded;
                skins.ItemRemoved -= itemRemoved;
            }
        }

        private class SizeSlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.##x");
        }

        private class SkinSettingsDropdown : SettingsDropdown<SkinInfo>
        {
            protected override OsuDropdown<SkinInfo> CreateDropdown() => new SkinDropdownControl();

            private class SkinDropdownControl : DropdownControl
            {
                protected override string GenerateItemText(SkinInfo item) => item.ToString();
            }
        }

        private class RandomSkinInfo : SkinInfo
        {
            public RandomSkinInfo()
            {
                Name = "<Random Skin>";
                ID = -1;
            }

            public override string ToString() => Name;
        }
    }
}
