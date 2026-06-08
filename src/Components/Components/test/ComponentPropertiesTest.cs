// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Test.Helpers;

namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Tests for ComponentProperties.SetParameters behavior, focusing on:
/// - Bug #19638: Missing event callback detection with CaptureUnmatchedValues
/// - Parameter matching and assignment logic
/// - Error handling for various edge cases
/// </summary>
public class ComponentPropertiesTest
{
    #region Bug #19638: Missing EventCallback Detection Tests

    /// <summary>
    /// Verifies basic fix for #19638: When a component has CaptureUnmatchedValues AND a
    /// [Parameter] Prop1 but NO Prop1Changed event callback, passing Prop1Changed should
    /// throw, not be silently captured.
    /// </summary>
    [Fact]
    public void SetProperties_ThrowsForMissingEventCallback_WhenComponentHasCaptureUnmatchedValues()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValuesAndMissingChangedCallback();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["Prop1"] = true,
                ["Prop1Changed"] = EventCallback.Empty
            })));

        Assert.Contains("Prop1Changed", ex.Message);
    }

    /// <summary>
    /// Verifies that error message does NOT reference the CaptureUnmatchedValues property
    /// (Attributes) since the error is specifically about the missing Changed callback.
    /// Split from main test for clearer debugging.
    /// </summary>
    [Fact]
    public void SetProperties_ThrowsForMissingEventCallback_DoesNotReferenceCaptureUnmatchedProperty()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValuesAndMissingChangedCallback();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["Prop1"] = true,
                ["Prop1Changed"] = EventCallback.Empty
            })));

        Assert.DoesNotContain("Attributes", ex.Message);
    }

    /// <summary>
    /// Verifies the "Changed" suffix detection is case-insensitive (OrdinalIgnoreCase).
    /// This covers the scenario where compiler generates different casing.
    /// </summary>
    [Theory]
    [InlineData("prop1changed")]
    [InlineData("PROP1CHANGED")]
    [InlineData("PrOp1ChAnGeD")]
    public void SetProperties_ThrowsForMissingEventCallback_CaseInsensitiveChangedSuffix(
        string changedParamName)
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValuesAndMissingChangedCallback();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["Prop1"] = true,
                [changedParamName] = EventCallback.Empty
            })));

        // Should contain the normalized parameter name in the error
        Assert.Contains("Prop1Changed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that unknown parameters WITHOUT "Changed" suffix are properly captured
    /// in ExtraAttributes when CaptureUnmatchedValues is present.
    /// </summary>
    [Fact]
    public void SetProperties_DoesNotThrowForUnknownNonChangedParameter_WhenComponentHasCaptureUnmatchedValues()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValuesAndMissingChangedCallback();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["UnknownParam"] = "some value"
        }));

        // Assert: Should be captured in Attributes
        Assert.NotNull(component.Attributes);
        Assert.Single(component.Attributes);
        Assert.True(component.Attributes.ContainsKey("UnknownParam"));
    }

    /// <summary>
    /// Verifies single assertion: Attributes contains the unknown parameter value.
    /// </summary>
    [Fact]
    public void SetProperties_UnknownParameter_CapturedValueIsCorrect()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValuesAndMissingChangedCallback();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["CustomAttr"] = 42
        }));

        // Assert: Single assertion for value verification
        Assert.Equal(42, component.Attributes["CustomAttr"]);
    }

    /// <summary>
    /// Verifies that without CaptureUnmatchedValues, the "Changed" suffix still throws.
    /// </summary>
    [Fact]
    public void SetProperties_ThrowsForMissingEventCallback_WhenComponentHasNoCaptureUnmatchedValues()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithoutCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["Prop1"] = true,
                ["Prop1Changed"] = EventCallback.Empty
            })));

        Assert.Contains("Prop1Changed", ex.Message);
    }

    /// <summary>
    /// Verifies that when there are multiple properties each missing their Changed callback,
    /// the first one encountered throws.
    /// </summary>
    [Fact]
    public void SetProperties_ThrowsForFirstMissingChangedCallback_WhenMultiplePropsMissingChanged()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithMultiplePropsNoChangedCallbacks();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["Prop1"] = true,
                ["Prop1Changed"] = EventCallback.Empty,
                ["Prop2"] = "value",
                ["Prop2Changed"] = EventCallback.Empty
            })));

        Assert.Contains("Prop1Changed", ex.Message);
    }

    /// <summary>
    /// Verifies that a parameter ending with "Changed" but where no matching property exists
    /// (e.g., for a non-bindable property name) correctly throws due to the "Changed" detection
    /// found in the CaptureUnmatchedValues branch.
    /// </summary>
    [Fact]
    public void SetProperties_ThrowsForChangedSuffix_NoMatchingPropertyButExistsInDictionary()
    {
        // Arrange: Component with CaptureUnmatchedValues but parameter name ends with "Changed"
        // for a property that doesn't exist at all
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValuesAndMissingChangedCallback();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act & Assert: Should throw because it ends with "Changed"
        // even though "NonExistentChanged" has no corresponding "NonExistent" property
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["NonExistentChanged"] = EventCallback.Empty
            })));

        Assert.Contains("NonExistentChanged", ex.Message);
    }

    #endregion

    #region Test Component Classes

    /// <summary>
    /// Test component with CaptureUnmatchedValues and one [Parameter] property,
    /// but NO corresponding Changed event callback. Used to verify bug #19638 fix.
    /// </summary>
    private class ComponentWithCaptureUnmatchedValuesAndMissingChangedCallback : IComponent
    {
        public Dictionary<string, object> Attributes { get; set; } = new();

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> ExtraAttributes
        {
            get => Attributes;
            set => Attributes = value;
        }

        [Parameter]
        public bool Prop1 { get; set; }

        // Intentional: Missing [Parameter] public EventCallback<bool> Prop1Changed { get; set; }

        private RenderHandle _renderHandle;

        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            _renderHandle.Render(builder => { });
            return Task.CompletedTask;
        }

        public void SetParameters(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
        }
    }

    /// <summary>
    /// Test component WITHOUT CaptureUnmatchedValues but with one [Parameter] property,
    /// and NO corresponding Changed event callback. Used to verify Changed detection
    /// works in the non-CaptureUnmatchedValues branch.
    /// </summary>
    private class ComponentWithoutCaptureUnmatchedValues : IComponent
    {
        [Parameter]
        public bool Prop1 { get; set; }

        // Intentional: Missing [Parameter] public EventCallback<bool> Prop1Changed { get; set; }

        private RenderHandle _renderHandle;

        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            _renderHandle.Render(builder => { });
            return Task.CompletedTask;
        }

        public void SetParameters(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
        }
    }

    /// <summary>
    /// Test component with multiple [Parameter] properties, none with Changed callbacks.
    /// Used to verify that the first missing Changed callback encountered throws.
    /// </summary>
    private class ComponentWithMultiplePropsNoChangedCallbacks : IComponent
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> ExtraAttributes { get; set; } = new();

        [Parameter]
        public bool Prop1 { get; set; }

        [Parameter]
        public string Prop2 { get; set; }

        // Intentional: Missing all *Changed event callbacks

        private RenderHandle _renderHandle;

        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
            _renderHandle.Render(builder => { });
            return Task.CompletedTask;
        }

        public void SetParameters(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);
        }
    }

    #endregion
}
