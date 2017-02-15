using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CodeCafeIRC.ui
{
    public class DockPanelSplitter : Control
    {
        public static readonly DependencyProperty ProportionalResizeProperty =
            DependencyProperty.Register("ProportionalResize", typeof(bool), typeof(DockPanelSplitter),
                new UIPropertyMetadata(true));

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(DockPanelSplitter),
                new UIPropertyMetadata(4.0, ThicknessChanged));

        private Point StartDragPoint;

        static DockPanelSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockPanelSplitter),
                new FrameworkPropertyMetadata(typeof(DockPanelSplitter)));

            // override the Background property
            BackgroundProperty.OverrideMetadata(typeof(DockPanelSplitter), new FrameworkPropertyMetadata(Brushes.Transparent));

            // override the Dock property to get notifications when Dock is changed
            DockPanel.DockProperty.OverrideMetadata(typeof(DockPanelSplitter),
                new FrameworkPropertyMetadata(Dock.Left, DockChanged));
        }

        public DockPanelSplitter()
        {
            Loaded += DockPanelSplitterLoaded;
            Unloaded += DockPanelSplitterUnloaded;

            UpdateHeightOrWidth();
        }

        /// <summary>
        ///     Resize the target element1 proportionally with the parent container
        ///     Set to false if you don't want the element1 to be resized when the parent is resized.
        /// </summary>
        public bool ProportionalResize
        {
            get { return (bool) GetValue(ProportionalResizeProperty); }
            set { SetValue(ProportionalResizeProperty, value); }
        }

        /// <summary>
        ///     Height or width of splitter, depends of orientation of the splitter
        /// </summary>
        public double Thickness
        {
            get { return (double) GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        public bool IsHorizontal
        {
            get
            {
                Dock dock = DockPanel.GetDock(this);
                return dock == Dock.Top || dock == Dock.Bottom;
            }
        }

        public int PositionFactorPercentage
        {
            set
            {
                Loaded += (sender, args) =>
                {
                    float percentage;
                    if (value > 0 && value < 100)
                        percentage = (float)100 / value;
                    else if (value == 100)
                        percentage = value;
                    else
                        throw new ArgumentOutOfRangeException("value");

                    DockPanel dp = Parent as DockPanel;
                    if (IsHorizontal)
                        SetTargetHeight(dp.ActualHeight / percentage);
                    else
                        SetTargetWidth(dp.ActualWidth / percentage);
                    previousParentWidth = dp.ActualWidth;
                    previousParentHeight = dp.ActualHeight;
                };
            }
        }

        private void DockPanelSplitterLoaded(object sender, RoutedEventArgs e)
        {
            Panel dp = Parent as Panel;
            if (dp == null) return;

            // Subscribe to the parent's size changed event
            dp.SizeChanged += ParentSizeChanged;

            // Store the current size of the parent DockPanel
            previousParentWidth = dp.ActualWidth;
            previousParentHeight = dp.ActualHeight;

            // Find the target elements
            UpdateTargetElements();

            ParentSizeChanged(null, null);
        }

        private void DockPanelSplitterUnloaded(object sender, RoutedEventArgs e)
        {
            Panel dp = Parent as Panel;
            if (dp == null) return;

            // Unsubscribe
            dp.SizeChanged -= ParentSizeChanged;
        }

        private static void DockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockPanelSplitter) d).UpdateHeightOrWidth();
        }

        private static void ThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockPanelSplitter) d).UpdateHeightOrWidth();
        }

        private void UpdateHeightOrWidth()
        {
            if (IsHorizontal)
            {
                Height = Thickness;
                Width = double.NaN;
            }
            else
            {
                Width = Thickness;
                Height = double.NaN;
            }
        }

        /// <summary>
        ///     Update the target elements (the element1 and element2 the DockPanelSplitter works on)
        /// </summary>
        private void UpdateTargetElements()
        {
            Panel dp = Parent as Panel;
            if (dp == null) return;

            int i = dp.Children.IndexOf(this);

            // The splitter cannot be the first child of the parent DockPanel
            // The splitter works on the 'older' sibling 
            /*****
            // EDITED BY ALEXANDER B TO WORK ON OLDER SIBLING AND NEWER SIBLING SIMULTANEOUSLY
            ******/
            if (i > 0 && dp.Children.Count > 0)
                element1 = dp.Children[i - 1] as FrameworkElement;
            if (i < dp.Children.Count - 1)
                element2 = dp.Children[i + 1] as FrameworkElement;
        }

        private void SetTargetWidth(double newWidth)
        {
            if (newWidth < element1.MinWidth)
                newWidth = element1.MinWidth;
            if (newWidth > element1.MaxWidth)
                newWidth = element1.MaxWidth;

            Panel dp = Parent as Panel;

            /*****
            // EDITED BY ALEXANDER B TO WORK ON OLDER SIBLING AND NEWER SIBLING SIMULTANEOUSLY
            // Test against second element
            ******/
            double panelWidth = dp.ActualWidth;
            double remainingWidth = panelWidth - newWidth;
            if (remainingWidth <= element2.MinWidth)
                newWidth = Math.Abs(panelWidth - element2.MinWidth);

            Dock dock = DockPanel.GetDock(this);
            MatrixTransform t = element1.TransformToAncestor(dp) as MatrixTransform;
            if (dock == Dock.Left && newWidth > dp.ActualWidth - t.Matrix.OffsetX - Thickness)
                newWidth = dp.ActualWidth - t.Matrix.OffsetX - Thickness;

            element1.Width = newWidth;
        }

        private void SetTargetHeight(double newHeight)
        {
            if (newHeight < element1.MinHeight)
                newHeight = element1.MinHeight;
            if (newHeight > element1.MaxHeight)
                newHeight = element1.MaxHeight;

            Panel dp = Parent as Panel;

            /*****
            // EDITED BY ALEXANDER B TO WORK ON OLDER SIBLING AND NEWER SIBLING SIMULTANEOUSLY
            // Test against second element
            ******/
            double panelHeight = dp.ActualHeight;
            double remainingHeight = panelHeight - newHeight;
            if (remainingHeight <= element2.MinHeight)
                newHeight = Math.Abs(panelHeight - element2.MinHeight);

            Dock dock = DockPanel.GetDock(this);
            MatrixTransform t = element1.TransformToAncestor(dp) as MatrixTransform;
            if (dock == Dock.Top && newHeight > dp.ActualHeight - t.Matrix.OffsetY - Thickness)
                newHeight = dp.ActualHeight - t.Matrix.OffsetY - Thickness;

            element1.Height = newHeight;
        }

        private void ParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!ProportionalResize) return;

            DockPanel dp = Parent as DockPanel;
            if (dp == null) return;

            double sx = dp.ActualWidth / previousParentWidth;
            double sy = dp.ActualHeight / previousParentHeight;

            if (!double.IsInfinity(sx))
                SetTargetWidth(element1.Width * sx);
            if (!double.IsInfinity(sy))
                SetTargetHeight(element1.Height * sy);

            previousParentWidth = dp.ActualWidth;
            previousParentHeight = dp.ActualHeight;
        }

        private double AdjustWidth(double dx, Dock dock)
        {
            if (dock == Dock.Right)
                dx = -dx;

            width += dx;
            SetTargetWidth(width);

            return dx;
        }

        private double AdjustHeight(double dy, Dock dock)
        {
            if (dock == Dock.Bottom)
                dy = -dy;

            height += dy;
            SetTargetHeight(height);

            return dy;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!IsEnabled) return;
            Cursor = IsHorizontal ? Cursors.SizeNS : Cursors.SizeWE;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;

            if (!IsMouseCaptured)
            {
                StartDragPoint = e.GetPosition(Parent as IInputElement);
                UpdateTargetElements();
                if (element1 != null)
                {
                    width = element1.ActualWidth;
                    height = element1.ActualHeight;
                    CaptureMouse();
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Point ptCurrent = e.GetPosition(Parent as IInputElement);
                Point delta = new Point(ptCurrent.X - StartDragPoint.X, ptCurrent.Y - StartDragPoint.Y);
                Dock dock = DockPanel.GetDock(this);

                if (IsHorizontal)
                    delta.Y = AdjustHeight(delta.Y, dock);
                else
                    delta.X = AdjustWidth(delta.X, dock);

                bool isBottomOrRight = (dock == Dock.Right || dock == Dock.Bottom);

                // When docked to the bottom or right, the position has changed after adjusting the size
                if (isBottomOrRight)
                    StartDragPoint = e.GetPosition(Parent as IInputElement);
                else
                    StartDragPoint = new Point(StartDragPoint.X + delta.X, StartDragPoint.Y + delta.Y);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
            base.OnMouseUp(e);
        }

        #region Private fields

        /*****
        // EDITED BY ALEXANDER B FOR PARAKEET 2 TO WORK ON OLDER SIBLING AND NEWER SIBLING SIMULTANEOUSLY
        ******/
        private FrameworkElement element1; // element1 to resize (target element1)
        private FrameworkElement element2; // element1 to resize (target element1)
        private double height; // current desired height of the element1, can be less than minheight
        private double previousParentHeight; // current height of parent element1, used for proportional resize
        private double previousParentWidth; // current width of parent element1, used for proportional resize
        private double width; // current desired width of the element1, can be less than minwidth

        #endregion
    }
}