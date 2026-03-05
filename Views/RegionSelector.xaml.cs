using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenDrop.Views;

public partial class RegionSelector : Window
{
    private Point _startPoint;
    private bool _isSelecting;
    private Rectangle? _selectionRect;

    public Rect? SelectedRegion { get; private set; }

    public RegionSelector()
    {
        InitializeComponent();
        
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        KeyDown += OnKeyDown;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(this);
        _isSelecting = true;

        _selectionRect = new Rectangle
        {
            Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 212)),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            Fill = Brushes.Transparent
        };

        Canvas.SetLeft(_selectionRect, _startPoint.X);
        Canvas.SetTop(_selectionRect, _startPoint.Y);
        SelectionCanvas.Children.Add(_selectionRect);

        Mouse.Capture(this);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isSelecting || _selectionRect == null) return;

        var currentPoint = e.GetPosition(this);
        
        var x = Math.Min(_startPoint.X, currentPoint.X);
        var y = Math.Min(_startPoint.Y, currentPoint.Y);
        var width = Math.Abs(currentPoint.X - _startPoint.X);
        var height = Math.Abs(currentPoint.Y - _startPoint.Y);

        Canvas.SetLeft(_selectionRect, x);
        Canvas.SetTop(_selectionRect, y);
        _selectionRect.Width = width;
        _selectionRect.Height = height;

        SizeLabel.Content = $"{(int)width} x {(int)height}";
        Canvas.SetLeft(SizeLabel, x);
        Canvas.SetTop(SizeLabel, y - 25);
        SizeLabel.Visibility = Visibility.Visible;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting) return;

        _isSelecting = false;
        Mouse.Capture(null);

        var endPoint = e.GetPosition(this);

        var x = Math.Min(_startPoint.X, endPoint.X) + Left;
        var y = Math.Min(_startPoint.Y, endPoint.Y) + Top;
        var width = Math.Abs(endPoint.X - _startPoint.X);
        var height = Math.Abs(endPoint.Y - _startPoint.Y);

        if (width > 10 && height > 10)
        {
            SelectedRegion = new Rect(x, y, width, height);
            DialogResult = true;
        }
        else
        {
            DialogResult = false;
        }

        Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }
}
