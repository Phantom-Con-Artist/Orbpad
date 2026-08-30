using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;

namespace Orbpad;

public partial class ImageViewerWindow : Window
{
    private Bitmap? _bitmap;

    private const double MinimumZoom = 0.10;
    private const double MaximumZoom = 8.00;

    private const double ZoomStep = 0.25;

    private double _zoom = 1.0;

    private bool _isUpdatingLayout;

    public ImageViewerWindow()
    {
        InitializeComponent();
    }

    public ImageViewerWindow(
        Bitmap bitmap,
        string title)
        : this()
    {
        _bitmap = bitmap;

        Title =
            $"Orbpad Image — {title}";

        ImagePreview.Source =
            _bitmap;

        ImageInfoText.Text =
            $"{_bitmap.PixelSize.Width} × " +
            $"{_bitmap.PixelSize.Height} px";

        ApplyZoom(
            preserveView: false);
    }


    // ============================================================
    // ZOOM BUTTONS
    // ============================================================

    private void ZoomIn_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetZoom(
            _zoom + ZoomStep,
            preserveView: true);
    }

    private void ZoomOut_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetZoom(
            _zoom - ZoomStep,
            preserveView: true);
    }

    private void ResetZoom_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetZoom(
            1.0,
            preserveView: true);
    }

    private void Zoom50_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetZoom(
            0.5,
            preserveView: true);
    }

    private void Zoom100_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetZoom(
            1.0,
            preserveView: true);
    }

    private void Zoom200_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SetZoom(
            2.0,
            preserveView: true);
    }


    // ============================================================
    // FIT TO WINDOW
    // ============================================================

    private void FitToWindow_Click(
        object? sender,
        RoutedEventArgs e)
    {
        FitImageToWindow();
    }

    private void FitImageToWindow()
    {
        if (_bitmap is null)
            return;

        double viewportWidth =
            ImageScrollViewer.Viewport.Width;

        double viewportHeight =
            ImageScrollViewer.Viewport.Height;

        if (viewportWidth <= 0 ||
            viewportHeight <= 0)
        {
            return;
        }

        double imageWidth =
            _bitmap.Size.Width;

        double imageHeight =
            _bitmap.Size.Height;

        if (imageWidth <= 0 ||
            imageHeight <= 0)
        {
            return;
        }

        double zoomX =
            viewportWidth / imageWidth;

        double zoomY =
            viewportHeight / imageHeight;

        double fitZoom =
            Math.Min(
                zoomX,
                zoomY);

        fitZoom =
            Math.Clamp(
                fitZoom,
                MinimumZoom,
                MaximumZoom);

        _zoom =
            fitZoom;

        ApplyZoom(
            preserveView: false);

        ImageScrollViewer.Offset =
            new Vector(0, 0);
    }


    // ============================================================
    // MOUSE WHEEL ZOOM
    // ============================================================

    private void ImagePreview_PointerWheelChanged(
        object? sender,
        PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(
                KeyModifiers.Control))
        {
            return;
        }

        double newZoom;

        if (e.Delta.Y > 0)
        {
            newZoom =
                _zoom + ZoomStep;
        }
        else if (e.Delta.Y < 0)
        {
            newZoom =
                _zoom - ZoomStep;
        }
        else
        {
            return;
        }

        SetZoom(
            newZoom,
            preserveView: true);

        e.Handled = true;
    }


    // ============================================================
    // ZOOM ENGINE
    // ============================================================

    private void SetZoom(
        double zoom,
        bool preserveView)
    {
        if (_bitmap is null)
            return;

        double oldZoom =
            _zoom;

        zoom =
            Math.Clamp(
                zoom,
                MinimumZoom,
                MaximumZoom);

        if (Math.Abs(
                zoom - oldZoom) < 0.001)
        {
            return;
        }

        /*
         * Work out which point of the image is currently
         * underneath the center of the viewport.
         *
         * After zooming, we move the scroll offset so that
         * the same point remains underneath the center.
         */

        double viewportWidth =
            ImageScrollViewer.Viewport.Width;

        double viewportHeight =
            ImageScrollViewer.Viewport.Height;

        double oldImageWidth =
            _bitmap.Size.Width * oldZoom;

        double oldImageHeight =
            _bitmap.Size.Height * oldZoom;

        double oldLeft =
            GetImageLeft(
                oldImageWidth,
                viewportWidth);

        double oldTop =
            GetImageTop(
                oldImageHeight,
                viewportHeight);

        double centerX =
            ImageScrollViewer.Offset.X +
            viewportWidth / 2.0;

        double centerY =
            ImageScrollViewer.Offset.Y +
            viewportHeight / 2.0;

        double imagePointX =
            0.5;

        double imagePointY =
            0.5;

        if (oldImageWidth > 0)
        {
            imagePointX =
                (centerX - oldLeft) /
                oldImageWidth;
        }

        if (oldImageHeight > 0)
        {
            imagePointY =
                (centerY - oldTop) /
                oldImageHeight;
        }

        imagePointX =
            Math.Clamp(
                imagePointX,
                0,
                1);

        imagePointY =
            Math.Clamp(
                imagePointY,
                0,
                1);

        _zoom =
            zoom;

        ApplyZoom(
            preserveView: false);

        if (!preserveView)
            return;

        /*
         * Calculate where that same image point now sits.
         */

        double newImageWidth =
            _bitmap.Size.Width * _zoom;

        double newImageHeight =
            _bitmap.Size.Height * _zoom;

        double newLeft =
            GetImageLeft(
                newImageWidth,
                viewportWidth);

        double newTop =
            GetImageTop(
                newImageHeight,
                viewportHeight);

        double newPointX =
            newLeft +
            imagePointX *
            newImageWidth;

        double newPointY =
            newTop +
            imagePointY *
            newImageHeight;

        double targetOffsetX =
            newPointX -
            viewportWidth / 2.0;

        double targetOffsetY =
            newPointY -
            viewportHeight / 2.0;

        targetOffsetX =
            Math.Max(
                0,
                targetOffsetX);

        targetOffsetY =
            Math.Max(
                0,
                targetOffsetY);

        double maxOffsetX =
            Math.Max(
                0,
                ImageScrollViewer.Extent.Width -
                ImageScrollViewer.Viewport.Width);

        double maxOffsetY =
            Math.Max(
                0,
                ImageScrollViewer.Extent.Height -
                ImageScrollViewer.Viewport.Height);

        targetOffsetX =
            Math.Min(
                targetOffsetX,
                maxOffsetX);

        targetOffsetY =
            Math.Min(
                targetOffsetY,
                maxOffsetY);

        ImageScrollViewer.Offset =
            new Vector(
                targetOffsetX,
                targetOffsetY);
    }


    // ============================================================
    // APPLY ZOOM
    // ============================================================

    private void ApplyZoom(
        bool preserveView)
    {
        if (_bitmap is null)
            return;

        if (_isUpdatingLayout)
            return;

        _isUpdatingLayout = true;

        try
        {
            double imageWidth =
                _bitmap.Size.Width *
                _zoom;

            double imageHeight =
                _bitmap.Size.Height *
                _zoom;

            double viewportWidth =
                ImageScrollViewer.Viewport.Width;

            double viewportHeight =
                ImageScrollViewer.Viewport.Height;

            double canvasWidth =
                Math.Max(
                    imageWidth,
                    viewportWidth);

            double canvasHeight =
                Math.Max(
                    imageHeight,
                    viewportHeight);

            ImageCanvas.Width =
                canvasWidth;

            ImageCanvas.Height =
                canvasHeight;

            ImagePreview.Width =
                imageWidth;

            ImagePreview.Height =
                imageHeight;

            double left =
                GetImageLeft(
                    imageWidth,
                    viewportWidth);

            double top =
                GetImageTop(
                    imageHeight,
                    viewportHeight);

            Canvas.SetLeft(
                ImagePreview,
                left);

            Canvas.SetTop(
                ImagePreview,
                top);

            ZoomPercentageButton.Content =
                $"{Math.Round(
                    _zoom * 100)}%";
        }
        finally
        {
            _isUpdatingLayout = false;
        }
    }


    // ============================================================
    // IMAGE POSITIONING
    // ============================================================

    private static double GetImageLeft(
        double imageWidth,
        double viewportWidth)
    {
        if (imageWidth <
            viewportWidth)
        {
            return
                (viewportWidth -
                 imageWidth) / 2.0;
        }

        return 0;
    }

    private static double GetImageTop(
        double imageHeight,
        double viewportHeight)
    {
        if (imageHeight <
            viewportHeight)
        {
            return
                (viewportHeight -
                 imageHeight) / 2.0;
        }

        return 0;
    }


    // ============================================================
    // WINDOW RESIZE
    // ============================================================

    private void ImageScrollViewer_SizeChanged(
        object? sender,
        SizeChangedEventArgs e)
    {
        if (_bitmap is null ||
            _isUpdatingLayout)
        {
            return;
        }

        /*
         * Recalculate the layout when the window changes size.
         * Fit mode is deliberately not forced here; if the user
         * is zoomed to 200%, resizing the window shouldn't suddenly
         * destroy their chosen zoom level.
         */

        ApplyZoom(
            preserveView: false);
    }


    // ============================================================
    // CLOSE
    // ============================================================

    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close();
    }


    // ============================================================
    // CLEANUP
    // ============================================================

    protected override void OnClosed(
        EventArgs e)
    {
        _bitmap?.Dispose();

        _bitmap = null;

        base.OnClosed(e);
    }
}