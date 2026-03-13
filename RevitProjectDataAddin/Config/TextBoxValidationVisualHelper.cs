using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitProjectDataAddin
{
    internal static class TextBoxValidationVisualHelper
    {
        private sealed class VisualState
        {
            public object NormalBackgroundValue { get; set; }
            public object NormalBorderBrushValue { get; set; }
            public object NormalBorderThicknessValue { get; set; }
            public Thickness ResolvedNormalBorderThickness { get; set; }
            public bool IsInvalid { get; set; }
        }

        private static readonly ConditionalWeakTable<TextBox, VisualState> States =
            new ConditionalWeakTable<TextBox, VisualState>();

        public static void Initialize(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            Apply(textBox, States.GetValue(textBox, CreateState));
        }

        public static void SetInvalid(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            var state = States.GetValue(textBox, CreateState);
            state.IsInvalid = true;
            Apply(textBox, state);
        }

        public static void CaptureNormalAppearance(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            var state = States.GetValue(textBox, CreateState);
            UpdateNormalAppearance(textBox, state);
            Apply(textBox, state);
        }

        public static void SetValid(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            var state = States.GetValue(textBox, CreateState);
            state.IsInvalid = false;
            Apply(textBox, state);
        }

        public static void Refresh(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            Apply(textBox, States.GetValue(textBox, CreateState));
        }

        private static VisualState CreateState(TextBox textBox)
        {
            var state = new VisualState();
            UpdateNormalAppearance(textBox, state);
            return state;
        }

        private static void Apply(TextBox textBox, VisualState state)
        {
            RestoreValue(textBox, Control.BackgroundProperty, state.NormalBackgroundValue);

            if (state.IsInvalid)
            {
                textBox.BorderBrush = Brushes.Red;
                textBox.BorderThickness = CreateInvalidThickness(state.ResolvedNormalBorderThickness);
                return;
            }

            RestoreValue(textBox, Control.BorderBrushProperty, state.NormalBorderBrushValue);
            RestoreValue(textBox, Control.BorderThicknessProperty, state.NormalBorderThicknessValue);
        }

        private static Thickness CreateInvalidThickness(Thickness originalThickness)
        {
            return new Thickness(
                Math.Max(2.0, originalThickness.Left),
                Math.Max(2.0, originalThickness.Top),
                Math.Max(2.0, originalThickness.Right),
                Math.Max(2.0, originalThickness.Bottom));
        }

        private static void UpdateNormalAppearance(TextBox textBox, VisualState state)
        {
            state.NormalBackgroundValue = textBox.ReadLocalValue(Control.BackgroundProperty);
            state.NormalBorderBrushValue = textBox.ReadLocalValue(Control.BorderBrushProperty);
            state.NormalBorderThicknessValue = textBox.ReadLocalValue(Control.BorderThicknessProperty);
            state.ResolvedNormalBorderThickness = EnsureVisibleThickness(textBox.BorderThickness);
        }

        private static Thickness EnsureVisibleThickness(Thickness thickness)
        {
            if (thickness.Left <= 0 &&
                thickness.Top <= 0 &&
                thickness.Right <= 0 &&
                thickness.Bottom <= 0)
            {
                return new Thickness(1.0);
            }

            return thickness;
        }

        private static void RestoreValue(DependencyObject target, DependencyProperty property, object value)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                target.ClearValue(property);
                return;
            }

            target.SetValue(property, value);
        }
    }
}
