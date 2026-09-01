using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Orb.Engine.Graph;

namespace Orbpad.Orbis.Views;

public partial class GraphViewerWindow : Window
{
    private readonly OrbGraph _graph;

    private Canvas _graphCanvas = null!;
    private MatrixTransform _graphTransform = null!;

    private readonly Dictionary<Guid, Point> _nodePositions = new();

    private double _zoom = 1.0;
    private double _panX;
    private double _panY;

    private bool _isPanning;
    private Point _panStart;
    private double _panOriginX;
    private double _panOriginY;

    // ------------------------------------------------------------
    // View animation (smooth Reset View / Fit Graph transitions).
    // ------------------------------------------------------------

    private DispatcherTimer? _viewAnimationTimer;
    private double _animStartZoom;
    private double _animStartPanX;
    private double _animStartPanY;
    private double _animTargetZoom;
    private double _animTargetPanX;
    private double _animTargetPanY;
    private DateTime _animStartTime;

    private static readonly TimeSpan ViewAnimationDuration =
        TimeSpan.FromMilliseconds(320);

    private const double NodeWidth = 150;
    private const double NodeHeight = 60;

    private const double RelationshipSpacing = 85;
    private const double ArrowLength = 13;
    private const double ArrowWidth = 6.5;

    // ------------------------------------------------------------
    // Design tokens.
    //
    // Centralizing the palette here makes the graph's look
    // consistent and easy to re-tune in one place.
    // ------------------------------------------------------------

    private static readonly Color AccentColor =
        Color.Parse("#22D3EE");

    private static readonly Color AccentColorBright =
        Color.Parse("#7DF0FF");

    private static readonly Color NodeFillTop =
        Color.Parse("#1B2430");

    private static readonly Color NodeFillBottom =
        Color.Parse("#12181F");

    private static readonly Color NodeTitleColor =
        Color.Parse("#E7EDF5");

    private static readonly Color NodeTypeColor =
        Color.Parse("#7C8CA3");

    private static readonly Color EdgeColor =
        Color.Parse("#5B6B80");

    private static readonly Color LabelBackground =
        Color.Parse("#161D27");

    private static readonly Color LabelBorder =
        Color.Parse("#2B3644");

    private static readonly Color LabelTextColor =
        Color.Parse("#C7D2E0");

    private static readonly BoxShadows NodeShadow =
        BoxShadows.Parse("0 10 24 -10 #B0000000");

    private static readonly BoxShadows NodeShadowHover =
        BoxShadows.Parse("0 14 32 -8 #C0000000, 0 0 0 1 #6622D3EE");

    private static readonly BoxShadows LabelShadow =
        BoxShadows.Parse("0 3 8 -2 #80000000");

    public string LoreTitle { get; }

    public string GraphSummary =>
        $"{_graph.Entities.Count} "
        + (_graph.Entities.Count == 1
            ? "entity"
            : "entities")
        + " · "
        + $"{_graph.Relationships.Count} "
        + (_graph.Relationships.Count == 1
            ? "relationship"
            : "relationships");

    // Required by Avalonia's runtime XAML loader.
    // The normal application path uses the OrbGraph constructor below.
    public GraphViewerWindow()
        : this(new OrbGraph(), null)
    {
    }

    public GraphViewerWindow(
        OrbGraph graph,
        string? loreTitle = null)
    {
        _graph =
            graph
            ?? throw new ArgumentNullException(nameof(graph));

        LoreTitle =
            string.IsNullOrWhiteSpace(loreTitle)
                ? "Lore Graph"
                : loreTitle;

        DataContext = this;

        InitializeComponent();

        _graphCanvas =
            this.FindControl<Canvas>("GraphCanvas")
            ?? throw new InvalidOperationException(
                "GraphCanvas could not be found.");

        _graphTransform =
            new MatrixTransform();

        _graphCanvas.RenderTransform =
            _graphTransform;

        Opened += (_, _) =>
        {
            BuildGraph();
            FitGraph();
        };
    }

    // ================================================================
    // GRAPH BUILD
    // ================================================================

    private void BuildGraph()
    {
        _graphCanvas.Children.Clear();
        _nodePositions.Clear();

        var entities =
            _graph.Entities.Values.ToList();

        if (entities.Count == 0)
        {
            return;
        }

        double width =
            Math.Max(
                700,
                _graphCanvas.Bounds.Width);

        double height =
            Math.Max(
                500,
                _graphCanvas.Bounds.Height);

        Point center =
            new(
                width / 2,
                height / 2);

        double radius =
            Math.Min(width, height) * 0.30;

        // ------------------------------------------------------------
        // Initial circular layout
        // ------------------------------------------------------------

        if (entities.Count == 1)
        {
            _nodePositions[entities[0].Id] =
                center;
        }
        else
        {
            for (int i = 0;
                 i < entities.Count;
                 i++)
            {
                double angle =
                    -Math.PI / 2
                    + (2 * Math.PI * i / entities.Count);

                _nodePositions[entities[i].Id] =
                    new Point(
                        center.X
                            + Math.Cos(angle) * radius,

                        center.Y
                            + Math.Sin(angle) * radius);
            }
        }

        // ------------------------------------------------------------
        // Group relationships by their undirected pair.
        //
        // This is important because:
        //
        // Avalon -> Monami
        // Monami -> Avalon
        //
        // belong to the same visual relationship group.
        // ------------------------------------------------------------

        var relationships =
            _graph.Relationships.Values.ToList();

        var relationshipGroups =
            relationships
                .GroupBy(GetUndirectedPairKey)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());

        foreach (var relationship in relationships)
        {
            if (!_nodePositions.TryGetValue(
                    relationship.SourceId,
                    out Point source)
                || !_nodePositions.TryGetValue(
                    relationship.TargetId,
                    out Point target))
            {
                continue;
            }

            // Self relationship.
            if (relationship.SourceId ==
                relationship.TargetId)
            {
                CreateSelfRelationship(
                    relationship,
                    source);

                continue;
            }

            string pairKey =
                GetUndirectedPairKey(
                    relationship);

            var group =
                relationshipGroups[pairKey];

            CreateRelationship(
                relationship,
                source,
                target,
                group);
        }

        // ------------------------------------------------------------
        // Nodes are created last so they sit above edges.
        // ------------------------------------------------------------

        foreach (var entity in entities)
        {
            var node =
                CreateNode(entity);

            Point position =
                _nodePositions[entity.Id];

            Canvas.SetLeft(
                node,
                position.X - NodeWidth / 2);

            Canvas.SetTop(
                node,
                position.Y - NodeHeight / 2);

            _graphCanvas.Children.Add(node);
        }
    }

    // ================================================================
    // RELATIONSHIP RENDERING
    // ================================================================

    private void CreateRelationship(
        OrbRelationship relationship,
        Point source,
        Point target,
        List<OrbRelationship> group)
    {
        Vector connection =
            target - source;

        double distance =
            connection.Length;

        if (distance < 0.001)
        {
            return;
        }

        Vector direction =
            connection.Normalize();

        // ------------------------------------------------------------
        // Calculate the relationship's lane.
        //
        // For two relationships between the same entities:
        //
        //       relationship A
        //          +85
        //
        //       relationship B
        //          -85
        //
        // This guarantees that opposite relationships never share
        // the same visual lane.
        //
        // IMPORTANT: the perpendicular used for the *lane offset*
        // must be canonical for the whole group (not derived from
        // this relationship's own source/target order). Otherwise,
        // for a reverse pair (A->B and B->A), the perpendicular
        // flips AND the lane sign flips, and the two cancel out,
        // putting both curves on the same side. See
        // GetCanonicalPerpendicular.
        // ------------------------------------------------------------

        Vector laneDirection =
            GetCanonicalPerpendicular(
                relationship,
                source,
                target,
                group,
                direction);

        double curveOffset =
            CalculateRelationshipOffset(
                relationship,
                group);

        double maximumOffset =
            Math.Max(
                35,
                distance * 0.45);

        curveOffset =
            Math.Clamp(
                curveOffset,
                -maximumOffset,
                maximumOffset);

        Point midpoint =
            new(
                (source.X + target.X) / 2
                    + laneDirection.X * curveOffset,

                (source.Y + target.Y) / 2
                    + laneDirection.Y * curveOffset);

        // ------------------------------------------------------------
        // Cubic Bezier control points.
        //
        // Using the lane midpoint produces a smooth bow away from
        // other relationships.
        // ------------------------------------------------------------

        Point control1 =
            new(
                source.X
                    + (midpoint.X - source.X) * 0.80,

                source.Y
                    + (midpoint.Y - source.Y) * 0.80);

        Point control2 =
            new(
                target.X
                    + (midpoint.X - target.X) * 0.80,

                target.Y
                    + (midpoint.Y - target.Y) * 0.80);

        // ------------------------------------------------------------
        // Move endpoints toward the node boundary.
        // ------------------------------------------------------------

        Point curveStart =
            MoveAwayFromNode(
                source,
                direction,
                NodeWidth / 2);

        Point curveEnd =
            MoveAwayFromNode(
                target,
                -direction,
                NodeWidth / 2);

        // ------------------------------------------------------------
        // Edge geometry.
        // ------------------------------------------------------------

        var geometry =
            new PathGeometry();

        var figure =
            new PathFigure
            {
                StartPoint = curveStart,
                IsClosed = false,
                IsFilled = false
            };

        figure.Segments!.Add(
            new BezierSegment
            {
                Point1 = control1,
                Point2 = control2,
                Point3 = curveEnd
            });

        geometry.Figures!.Add(figure);

        // ------------------------------------------------------------
        // A wide, faint duplicate of the same geometry sits underneath
        // the crisp stroke to fake a soft glow — cheap, GPU-friendly,
        // and reads as much more polished than a flat line.
        // ------------------------------------------------------------

        var glow =
            new Path
            {
                Data = geometry,

                Stroke =
                    new SolidColorBrush(
                        EdgeColor,
                        0.16),

                StrokeThickness = 6,
                StrokeLineCap = PenLineCap.Round,

                IsHitTestVisible = false
            };

        _graphCanvas.Children.Add(glow);

        var path =
            new Path
            {
                Data = geometry,

                Stroke =
                    new SolidColorBrush(EdgeColor),

                StrokeThickness = 2,
                StrokeLineCap = PenLineCap.Round,
                Opacity = 0.92,

                IsHitTestVisible = false
            };

        _graphCanvas.Children.Add(path);

        // ------------------------------------------------------------
        // Arrowhead
        // ------------------------------------------------------------

        Vector endDirection =
            curveEnd - control2;

        if (endDirection.Length > 0.001)
        {
            CreateArrowhead(
                curveEnd,
                endDirection.Normalize());
        }

        // ------------------------------------------------------------
        // Relationship label.
        //
        // IMPORTANT:
        // The label is deliberately positioned slightly farther out
        // than the mathematical curve midpoint.
        //
        // This prevents labels from touching when two relationships
        // connect the same entities.
        // ------------------------------------------------------------

        double labelT =
            0.50;

        Point labelPosition =
            GetBezierPoint(
                curveStart,
                control1,
                control2,
                curveEnd,
                labelT);

        double labelExtraOffset =
            Math.Sign(curveOffset)
            * 10;

        labelPosition =
            new Point(
                labelPosition.X
                    + laneDirection.X * labelExtraOffset,

                labelPosition.Y
                    + laneDirection.Y * labelExtraOffset);

        string relationshipType =
            string.IsNullOrWhiteSpace(
                relationship.Type)
                ? "related_to"
                : relationship.Type;

        var label =
            new Border
            {
                Background =
                    new SolidColorBrush(
                        LabelBackground,
                        0.92),

                BorderBrush =
                    new SolidColorBrush(LabelBorder),

                BorderThickness =
                    new Thickness(1),

                CornerRadius =
                    new CornerRadius(9),

                BoxShadow = LabelShadow,

                Padding =
                    new Thickness(
                        9,
                        4),

                Child =
                    new TextBlock
                    {
                        Text = relationshipType,

                        FontSize = 11,
                        FontWeight = FontWeight.Medium,
                        LetterSpacing = 0.2,

                        Foreground =
                            new SolidColorBrush(LabelTextColor)
                    },

                Tag = relationship.Id,
                IsHitTestVisible = true
            };

        PlaceLabelCentered(
            label,
            labelPosition);

        label.PointerPressed +=
            (_, e) =>
            {
                SelectRelationship(relationship.Id);
                e.Handled = true;
            };

        _graphCanvas.Children.Add(label);
    }

    // ------------------------------------------------------------
    // Canvas.Left/Top position a control's top-left corner, not
    // its center. Measuring first lets us center the label on the
    // curve point instead of offsetting it down-and-right, which
    // otherwise makes labels look misaligned or drift into
    // neighboring edges/labels.
    // ------------------------------------------------------------

    private static void PlaceLabelCentered(
        Control label,
        Point position)
    {
        label.Measure(
            new Size(
                double.PositiveInfinity,
                double.PositiveInfinity));

        Size desired =
            label.DesiredSize;

        Canvas.SetLeft(
            label,
            position.X - desired.Width / 2);

        Canvas.SetTop(
            label,
            position.Y - desired.Height / 2);
    }

    // ================================================================
    // RELATIONSHIP OFFSET
    // ================================================================

    private static double CalculateRelationshipOffset(
        OrbRelationship relationship,
        List<OrbRelationship> group)
    {
        if (group.Count <= 1)
        {
            return 0;
        }

        // ------------------------------------------------------------
        // Spread every relationship in the group symmetrically around
        // the direct connection, purely by its index in the group.
        //
        // This works uniformly for 2, 3, or more relationships, and
        // — crucially — is independent of each relationship's own
        // source/target order. The sign here is only meaningful
        // relative to GetCanonicalPerpendicular's canonical direction;
        // see that method for why that matters.
        // ------------------------------------------------------------

        int index =
            group.FindIndex(
                item => item.Id == relationship.Id);

        double center =
            (group.Count - 1) / 2.0;

        return
            (index - center)
            * RelationshipSpacing;
    }

    private static Vector GetCanonicalPerpendicular(
        OrbRelationship relationship,
        Point source,
        Point target,
        List<OrbRelationship> group,
        Vector ownDirection)
    {
        if (group.Count <= 1)
        {
            return new Vector(
                -ownDirection.Y,
                ownDirection.X);
        }

        // ------------------------------------------------------------
        // The lane offset from CalculateRelationshipOffset must be
        // applied along a SINGLE, SHARED direction for every
        // relationship in the group.
        //
        // If instead we used each relationship's own source->target
        // direction, a reverse relationship (B -> A instead of
        // A -> B) would flip the perpendicular sign. Combined with
        // a lane offset that also flips sign for reverse
        // relationships, the two flips cancel out and both curves
        // land on the same side — which is the overlap bug.
        //
        // Fix: derive the perpendicular from the group's first
        // relationship's orientation ("canonical" direction), and
        // simply mirror it when this relationship runs the other
        // way. Since both relationships connect the same two nodes,
        // reversing source/target reverses the connecting vector, so
        // we can do this using only the points we already have.
        // ------------------------------------------------------------

        var canonical =
            group[0];

        bool sameOrientation =
            relationship.SourceId == canonical.SourceId
            && relationship.TargetId == canonical.TargetId;

        Vector canonicalDirection =
            sameOrientation
                ? (target - source)
                : (source - target);

        if (canonicalDirection.Length < 0.001)
        {
            canonicalDirection = target - source;
        }

        canonicalDirection =
            canonicalDirection.Normalize();

        return new Vector(
            -canonicalDirection.Y,
            canonicalDirection.X);
    }

    // ================================================================
    // ARROWHEAD
    // ================================================================

    private void CreateArrowhead(
        Point tip,
        Vector direction)
    {
        if (direction.Length < 0.001)
        {
            return;
        }

        direction =
            direction.Normalize();

        Vector perpendicular =
            new(
                -direction.Y,
                direction.X);

        // A slightly indented back edge (instead of a flat base)
        // makes the triangle read as a sleeker "chevron" arrowhead.
        Point back =
            tip
            - direction * ArrowLength;

        Point left =
            back
            + perpendicular * ArrowWidth;

        Point right =
            back
            - perpendicular * ArrowWidth;

        Point notch =
            back
            + direction * (ArrowLength * 0.35);

        var arrow =
            new Polygon
            {
                Points =
                    new Avalonia.Points
                    {
                        tip,
                        left,
                        notch,
                        right
                    },

                Fill =
                    new SolidColorBrush(EdgeColor),

                Opacity = 0.95,

                IsHitTestVisible = false
            };

        _graphCanvas.Children.Add(arrow);
    }

    // ================================================================
    // SELF RELATIONSHIPS
    // ================================================================

    private void CreateSelfRelationship(
        OrbRelationship relationship,
        Point center)
    {
        const double radius = 45;

        var geometry =
            new PathGeometry();

        var figure =
            new PathFigure
            {
                StartPoint =
                    new Point(
                        center.X,
                        center.Y - NodeHeight / 2),

                IsClosed = false,
                IsFilled = false
            };

        figure.Segments!.Add(
            new ArcSegment
            {
                Point =
                    new Point(
                        center.X + 1,
                        center.Y - NodeHeight / 2),

                Size =
                    new Size(
                        radius,
                        radius),

                RotationAngle = 0,

                IsLargeArc = true,

                SweepDirection =
                    SweepDirection.Clockwise
            });

        geometry.Figures!.Add(figure);

        var glow =
            new Path
            {
                Data = geometry,

                Stroke =
                    new SolidColorBrush(
                        EdgeColor,
                        0.16),

                StrokeThickness = 6,
                StrokeLineCap = PenLineCap.Round,

                IsHitTestVisible = false
            };

        _graphCanvas.Children.Add(glow);

        var path =
            new Path
            {
                Data = geometry,

                Stroke =
                    new SolidColorBrush(EdgeColor),

                StrokeThickness = 2,
                StrokeLineCap = PenLineCap.Round,
                Opacity = 0.92,

                IsHitTestVisible = false
            };

        _graphCanvas.Children.Add(path);

        string relationshipType =
            string.IsNullOrWhiteSpace(
                relationship.Type)
                ? "related_to"
                : relationship.Type;

        var label =
            new Border
            {
                Background =
                    new SolidColorBrush(
                        LabelBackground,
                        0.92),

                BorderBrush =
                    new SolidColorBrush(LabelBorder),

                BorderThickness =
                    new Thickness(1),

                CornerRadius =
                    new CornerRadius(9),

                BoxShadow = LabelShadow,

                Padding =
                    new Thickness(
                        9,
                        4),

                Child =
                    new TextBlock
                    {
                        Text = relationshipType,

                        FontSize = 11,
                        FontWeight = FontWeight.Medium,
                        LetterSpacing = 0.2,

                        Foreground =
                            new SolidColorBrush(LabelTextColor)
                    },

                Tag = relationship.Id,
                IsHitTestVisible = true
            };

        PlaceLabelCentered(
            label,
            new Point(
                center.X + radius,
                center.Y - radius));

        label.PointerPressed +=
            (_, e) =>
            {
                SelectRelationship(relationship.Id);
                e.Handled = true;
            };

        _graphCanvas.Children.Add(label);
    }

    // ================================================================
    // BEZIER
    // ================================================================

    private static Point GetBezierPoint(
        Point p0,
        Point p1,
        Point p2,
        Point p3,
        double t)
    {
        double oneMinusT =
            1 - t;

        double a =
            oneMinusT
            * oneMinusT
            * oneMinusT;

        double b =
            3
            * oneMinusT
            * oneMinusT
            * t;

        double c =
            3
            * oneMinusT
            * t
            * t;

        double d =
            t
            * t
            * t;

        return new Point(
            a * p0.X
                + b * p1.X
                + c * p2.X
                + d * p3.X,

            a * p0.Y
                + b * p1.Y
                + c * p2.Y
                + d * p3.Y);
    }

    private static Point MoveAwayFromNode(
        Point point,
        Vector direction,
        double distance)
    {
        if (direction.Length < 0.001)
        {
            return point;
        }

        direction =
            direction.Normalize();

        return
            point
            + direction * distance;
    }

    // ================================================================
    // RELATIONSHIP GROUPING
    // ================================================================

    private static string GetUndirectedPairKey(
        OrbRelationship relationship)
    {
        Guid first =
            relationship.SourceId;

        Guid second =
            relationship.TargetId;

        if (first.CompareTo(second) > 0)
        {
            (first, second) =
                (second, first);
        }

        return
            $"{first:N}:{second:N}";
    }

    // ================================================================
    // ENTITY NODES
    // ================================================================

    private Border CreateNode(
        OrbEntity entity)
    {
        string title =
            string.IsNullOrWhiteSpace(
                entity.Name)
                ? "Unnamed Entity"
                : entity.Name;

        string type =
            string.IsNullOrWhiteSpace(
                entity.Type)
                ? "Entity"
                : entity.Type;

        var borderBrush =
            new SolidColorBrush(AccentColor);

        // ------------------------------------------------------------
        // A subtle top-to-bottom gradient reads as far more "designed"
        // than a flat fill, without being loud about it.
        // ------------------------------------------------------------

        var background =
            new LinearGradientBrush
            {
                StartPoint =
                    new RelativePoint(
                        0, 0, RelativeUnit.Relative),

                EndPoint =
                    new RelativePoint(
                        0, 1, RelativeUnit.Relative),

                GradientStops =
                {
                    new GradientStop(NodeFillTop, 0),
                    new GradientStop(NodeFillBottom, 1)
                }
            };

        // ------------------------------------------------------------
        // Hover pop: a gentle scale + brighter accent glow. The
        // ScaleTransform owns its own Transitions so the scale eases
        // in/out smoothly instead of snapping.
        // ------------------------------------------------------------

        var scale =
            new ScaleTransform(1, 1)
            {
                Transitions =
                    new Transitions
                    {
                        new DoubleTransition
                        {
                            Property =
                                ScaleTransform.ScaleXProperty,
                            Duration =
                                TimeSpan.FromMilliseconds(160),
                            Easing = new CubicEaseOut()
                        },
                        new DoubleTransition
                        {
                            Property =
                                ScaleTransform.ScaleYProperty,
                            Duration =
                                TimeSpan.FromMilliseconds(160),
                            Easing = new CubicEaseOut()
                        }
                    }
            };

        var typeDot =
            new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(AccentColor),
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment =
                    Avalonia.Layout.VerticalAlignment.Center
            };

        var node =
            new Border
            {
                Width = NodeWidth,
                MinHeight = NodeHeight,

                Padding =
                    new Thickness(
                        14,
                        10),

                CornerRadius =
                    new CornerRadius(14),

                Background = background,
                BorderBrush = borderBrush,

                BorderThickness =
                    new Thickness(1.4),

                BoxShadow = NodeShadow,

                RenderTransform = scale,
                RenderTransformOrigin = RelativePoint.Center,

                Cursor =
                    new Cursor(StandardCursorType.Hand),

                Tag = entity.Id,

                Child =
                    new StackPanel
                    {
                        Spacing = 4,

                        Children =
                        {
                            new TextBlock
                            {
                                Text = title,

                                FontWeight =
                                    FontWeight.SemiBold,

                                FontSize = 14,

                                Foreground =
                                    new SolidColorBrush(
                                        NodeTitleColor),

                                TextTrimming =
                                    TextTrimming
                                        .CharacterEllipsis
                            },

                            new StackPanel
                            {
                                Orientation =
                                    Avalonia.Layout.Orientation
                                        .Horizontal,

                                Children =
                                {
                                    typeDot,

                                    new TextBlock
                                    {
                                        Text =
                                            type.ToUpperInvariant(),

                                        FontSize = 10,
                                        FontWeight =
                                            FontWeight.Medium,
                                        LetterSpacing = 0.6,

                                        Foreground =
                                            new SolidColorBrush(
                                                NodeTypeColor),

                                        TextTrimming =
                                            TextTrimming
                                                .CharacterEllipsis
                                    }
                                }
                            }
                        }
                    }
            };

        node.PointerPressed +=
            (_, e) =>
            {
                if (e.Source is Control)
                {
                    SelectEntity(entity.Id);
                    e.Handled = true;
                }
            };

        node.PointerEntered +=
            (_, _) =>
            {
                scale.ScaleX = 1.045;
                scale.ScaleY = 1.045;

                borderBrush.Color = AccentColorBright;
                node.BoxShadow = NodeShadowHover;
            };

        node.PointerExited +=
            (_, _) =>
            {
                scale.ScaleX = 1.0;
                scale.ScaleY = 1.0;

                borderBrush.Color = AccentColor;
                node.BoxShadow = NodeShadow;
            };

        return node;
    }

    // ================================================================
    // PAN
    // ================================================================

    private void GraphCanvas_PointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (e.Source is Control control
            && control != _graphCanvas)
        {
            return;
        }

        _viewAnimationTimer?.Stop();
        ClearInspectorSelection();

        _isPanning = true;

        _graphCanvas.Cursor =
            new Cursor(StandardCursorType.SizeAll);

        _panStart =
            e.GetPosition(this);

        _panOriginX =
            _panX;

        _panOriginY =
            _panY;
    }

    private void GraphCanvas_PointerMoved(
        object? sender,
        PointerEventArgs e)
    {
        if (!_isPanning)
        {
            return;
        }

        Point current =
            e.GetPosition(this);

        _panX =
            _panOriginX
            + current.X
            - _panStart.X;

        _panY =
            _panOriginY
            + current.Y
            - _panStart.Y;

        UpdateTransform();
    }

    private void GraphCanvas_PointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        _isPanning = false;

        _graphCanvas.Cursor = null;
    }

    // ================================================================
    // ZOOM
    // ================================================================

    private void GraphCanvas_PointerWheelChanged(
        object? sender,
        PointerWheelEventArgs e)
    {
        _viewAnimationTimer?.Stop();

        Point mousePosition =
            e.GetPosition(_graphCanvas);

        double oldZoom =
            _zoom;

        _zoom =
            Math.Clamp(
                _zoom
                * (e.Delta.Y > 0
                    ? 1.10
                    : 0.90),
                0.35,
                2.5);

        if (Math.Abs(
                _zoom - oldZoom)
            > 0.0001)
        {
            double scale =
                _zoom / oldZoom;

            _panX =
                mousePosition.X
                - (mousePosition.X - _panX)
                    * scale;

            _panY =
                mousePosition.Y
                - (mousePosition.Y - _panY)
                    * scale;
        }

        UpdateTransform();

        e.Handled = true;
    }

    // ================================================================
    // TRANSFORM
    // ================================================================

    private void UpdateTransform()
    {
        _graphTransform.Matrix =
            Matrix.CreateScale(
                new Vector(
                    _zoom,
                    _zoom))
            * Matrix.CreateTranslation(
                new Vector(
                    _panX,
                    _panY));
    }

    // ================================================================
    // VIEW COMMANDS
    // ================================================================

    private void ResetView_Click(
        object? sender,
        RoutedEventArgs e)
    {
        AnimateViewTo(1.0, 0, 0);
    }

    private void FitGraph_Click(
        object? sender,
        RoutedEventArgs e)
    {
        BuildGraph();
        FitGraph();
    }

    private void FitGraph()
    {
        AnimateViewTo(1.0, 0, 0);
    }

    // ------------------------------------------------------------
    // Smoothly eases zoom/pan to a target instead of snapping,
    // which reads as far more polished on Reset View / Fit Graph.
    // ------------------------------------------------------------

    private void AnimateViewTo(
        double targetZoom,
        double targetPanX,
        double targetPanY)
    {
        _viewAnimationTimer?.Stop();

        _animStartZoom = _zoom;
        _animStartPanX = _panX;
        _animStartPanY = _panY;

        _animTargetZoom = targetZoom;
        _animTargetPanX = targetPanX;
        _animTargetPanY = targetPanY;

        _animStartTime = DateTime.UtcNow;

        _viewAnimationTimer =
            new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

        _viewAnimationTimer.Tick += ViewAnimationTick;
        _viewAnimationTimer.Start();
    }

    private void ViewAnimationTick(
        object? sender,
        EventArgs e)
    {
        double elapsedMs =
            (DateTime.UtcNow - _animStartTime)
                .TotalMilliseconds;

        double t =
            Math.Clamp(
                elapsedMs / ViewAnimationDuration.TotalMilliseconds,
                0,
                1);

        double eased =
            EaseOutCubic(t);

        _zoom =
            _animStartZoom
            + (_animTargetZoom - _animStartZoom) * eased;

        _panX =
            _animStartPanX
            + (_animTargetPanX - _animStartPanX) * eased;

        _panY =
            _animStartPanY
            + (_animTargetPanY - _animStartPanY) * eased;

        UpdateTransform();

        if (t >= 1.0)
        {
            _viewAnimationTimer!.Stop();
            _viewAnimationTimer.Tick -= ViewAnimationTick;
            _viewAnimationTimer = null;
        }
    }

    private static double EaseOutCubic(double t)
    {
        double p = t - 1;

        return
            p * p * p
            + 1;
    }

    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close();
    }
}