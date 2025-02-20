﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Avalonia.Xaml.Interactions.Draggable;

/// <summary>
/// 
/// </summary>
public class CanvasDragBehavior : Behavior<Control>
{
    private bool _enableDrag;
    private Point _start;
    private IControl? _parent;
    private Control? _draggedContainer;
    private Control? _adorner;

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject is { })
        {
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel);
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject is { })
        {
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
            AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
        }
    }

    private void AddAdorner(Control control)
    {
        var layer = AdornerLayer.GetAdornerLayer(control);
        if (layer is null)
        {
            return;
        }

        _adorner = new SelectionAdorner()
        {
            [AdornerLayer.AdornedElementProperty] = control
        };

        ((ISetLogicalParent) _adorner).SetParent(control);
        layer.Children.Add(_adorner);
    }

    private void RemoveAdorner(Control control)
    {
        var layer = AdornerLayer.GetAdornerLayer(control);
        if (layer is null || _adorner is null)
        {
            return;
        }

        layer.Children.Remove(_adorner);
        ((ISetLogicalParent) _adorner).SetParent(null);
        _adorner = null;
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed 
            && AssociatedObject?.Parent is { } parent)
        {
            _enableDrag = true;
            _start = e.GetPosition(AssociatedObject.Parent);
            _parent = parent;
            _draggedContainer = AssociatedObject;

            SetDraggingPseudoClasses(_draggedContainer, true);

            // AddAdorner(_draggedContainer);

            e.Pointer.Capture(AssociatedObject);
        }
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (Equals(e.Pointer.Captured, AssociatedObject))
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Released();
            }

            e.Pointer.Capture(null); 
        }
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (Equals(e.Pointer.Captured, AssociatedObject)
            && properties.IsLeftButtonPressed)
        {
            if (_parent is null || _draggedContainer is null || !_enableDrag)
            {
                return;
            }

            var position = e.GetPosition(_parent);
            var deltaX = position.X - _start.X;
            var deltaY = position.Y - _start.Y;
            _start = position;
            var left = Canvas.GetLeft(_draggedContainer);
            var top = Canvas.GetTop(_draggedContainer);
            Canvas.SetLeft(_draggedContainer, left + deltaX);
            Canvas.SetTop(_draggedContainer, top + deltaY);
        }
    }

    private void Released()
    {
        if (_enableDrag)
        {
            if (_parent is { } && _draggedContainer is { })
            {
                // RemoveAdorner(_draggedContainer);
            }

            if (_draggedContainer is { })
            {
                SetDraggingPseudoClasses(_draggedContainer, false);
            }

            _enableDrag = false;
            _parent = null;
            _draggedContainer = null;
        }
    }

    private void SetDraggingPseudoClasses(IControl control, bool isDragging)
    {
        if (isDragging)
        {
            ((IPseudoClasses)control.Classes).Add(":dragging");
        }
        else
        {
            ((IPseudoClasses)control.Classes).Remove(":dragging");
        }
    }
}
