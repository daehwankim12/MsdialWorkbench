﻿using CompMs.Graphics.Base;
using CompMs.Graphics.Core.Base;
using System;
using System.Windows;
using System.Windows.Input;

namespace CompMs.Graphics.Behavior
{
    public static class MoveByDragBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled", typeof(bool), typeof(MoveByDragBehavior),
                new PropertyMetadata(BooleanBoxes.FalseBox, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject d) =>
            (bool)d.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject d, bool value) =>
            d.SetValue(IsEnabledProperty, BooleanBoxes.Box(value));

        static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is FrameworkElement fe) {
                if ((bool)e.OldValue)
                    OnDetaching(fe);
                if ((bool)e.NewValue)
                    OnAttached(fe);
            }
        }

        static void OnAttached(FrameworkElement fe) {
            fe.MouseLeftButtonDown += MoveOnMouseLeftButtonDown;
            fe.MouseMove += MoveOnMouseMove;
            fe.MouseLeftButtonUp += MoveOnMouseLeftButtonUp;
        }

        static void OnDetaching(FrameworkElement fe) {
            fe.MouseLeftButtonDown -= MoveOnMouseLeftButtonDown;
            fe.MouseMove -= MoveOnMouseMove;
            fe.MouseLeftButtonUp -= MoveOnMouseLeftButtonUp;
        }

        public static readonly DependencyProperty CurrentPointProperty =
            DependencyProperty.RegisterAttached(
                "CurrentPoint", typeof(Point), typeof(MoveByDragBehavior),
                new PropertyMetadata(default));

        public static Point GetCurrentPoint(DependencyObject d) =>
            (Point)d.GetValue(CurrentPointProperty);

        public static void SetCurrentPoint(DependencyObject d, Point value) =>
            d.SetValue(CurrentPointProperty, value);

        public static readonly DependencyProperty MovingProperty =
            DependencyProperty.RegisterAttached(
                "Moving", typeof(bool), typeof(MoveByDragBehavior),
                new PropertyMetadata(BooleanBoxes.FalseBox));

        public static bool GetMoving(DependencyObject d) =>
            (bool)d.GetValue(MovingProperty);

        public static void SetMoving(DependencyObject d, bool value) =>
            d.SetValue(MovingProperty, BooleanBoxes.Box(value));

        static void MoveOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement fe) {
                SetCurrentPoint(fe, e.GetPosition(fe));
                SetMoving(fe, true);
                fe.CaptureMouse();
            }
        }

        static void MoveOnMouseMove(object sender, MouseEventArgs e) {
            if (sender is FrameworkElement fe) {
                if (GetMoving(fe)) {
                    var previous = GetCurrentPoint(fe);
                    var current = e.GetPosition(fe);
                    SetCurrentPoint(fe, current);

                    var haxis = ChartBaseControl.GetHorizontalAxis(fe);
                    if (haxis != null) {
                        var flippedX = ChartBaseControl.GetFlippedX(fe);
                        var rangeX = haxis.Range;
                        var prev = haxis.TranslateFromRenderPoint(previous.X / fe.ActualWidth, flippedX);
                        var curr = haxis.TranslateFromRenderPoint(current.X / fe.ActualWidth, flippedX);
                        var delta = prev - curr;
                        rangeX = new Range(rangeX.Minimum + delta, rangeX.Maximum + delta);
                        if (haxis.InitialRange.Contains(rangeX)) {
                            haxis.Range = rangeX;
                        }
                    }

                    var vaxis = ChartBaseControl.GetVerticalAxis(fe);
                    if (vaxis != null) {
                        var flippedY = ChartBaseControl.GetFlippedY(fe);
                        var rangeY = vaxis.Range;
                        var prev = vaxis.TranslateFromRenderPoint(previous.Y / fe.ActualHeight, flippedY);
                        var curr = vaxis.TranslateFromRenderPoint(current.Y / fe.ActualHeight, flippedY);
                        var delta = prev - curr;
                        rangeY = new Range(rangeY.Minimum + delta, rangeY.Maximum + delta);
                        if (vaxis.InitialRange.Contains(rangeY)) {
                            vaxis.Range = rangeY;
                        }
                    }
                }
            }
        }

        static void MoveOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement fe) {
                if (GetMoving(fe)) {
                    SetMoving(fe, false);
                    fe.ReleaseMouseCapture();
                }
            }
        }
    }
}