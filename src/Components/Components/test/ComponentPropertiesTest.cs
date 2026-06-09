// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Test.Helpers;

namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Tests for ComponentProperties.SetParameters behavior, focusing on:
/// - Bug #19638: Missing event callback detection with CaptureUnmatchedValues
/// - Parameter matching and assignment logic
/// - Edge cases for two-way binding detection
/// </summary>
public class ComponentPropertiesTest
{
    #region Bug #19638: Core Fix - Missing EventCallback Detection

    /// <summary>
    /// Core fix test: When component has CaptureUnmatchedValues AND [Parameter] Prop1 but NO Prop1Changed,
    /// passing Prop1Changed should throw, not be silently captured.
    /// </summary>
    [Fact]
    public void SetProperties_ThrowsForMissingEventCallback_BasicCase()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
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
    /// Verifies error message does NOT reference CaptureUnmatchedValues property (Attributes).
    /// The error is specifically about the missing Changed callback.
    /// </summary>
    [Fact]
    public void SetProperties_ErrorMessageDoesNotReferenceCaptureUnmatchedProperty()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["Prop1"] = true,
                ["Prop1Changed"] = EventCallback.Empty
            })));

        // Assert
        Assert.DoesNotContain("Attributes", ex.Message);
    }

    #endregion

    #region Bug #19638: Case-Insensitivity

    /// <summary>
    /// Verifies "Changed" suffix detection is case-insensitive.
    /// Razor compiler may generate different casings.
    /// </summary>
    [Theory]
    [InlineData("prop1changed")]
    [InlineData("PROP1CHANGED")]
    [InlineData("PrOp1ChAnGeD")]
    public void SetProperties_ThrowsCaseInsensitiveChangedSuffix(string changedParamName)
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["Prop1"] = true,
                [changedParamName] = EventCallback.Empty
            })));

        // Assert
        Assert.Contains("Prop1Changed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Bug #19638: Multiple Properties Missing Changed

    /// <summary>
    /// Verifies first missing Changed callback encountered throws.
    /// Tests parameter processing order behavior.
    /// </summary>
    [Fact]
    public void SetProperties_ThrowsForFirstMissingChangedCallback()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithMultiplePropsNoChangedCallbacks();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(
            () => component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
            {
                ["Prop1"] = true,
                ["Prop1Changed"] = EventCallback.Empty,
                ["Prop2"] = "value",
                ["Prop2Changed"] = EventCallback.Empty
            })));

        // Assert
        Assert.Contains("Prop1Changed", ex.Message);
    }

    #endregion

    #region Bug #19638: Base Property Existence Check (Precision)

    /// <summary>
    /// CRITICAL: Verifies that properties ending with "Changed" but with NO corresponding base property
    /// are NOT flagged as binding errors - they should be captured normally.
    /// This is the key precision improvement in the fix.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesPropertyEndingWithChanged_NoMatchingBaseProperty()
    {
        // Arrange - Component with a property that happens to end with "Changed"
        // but has no corresponding base property that would make it a binding pattern
        var renderer = new TestRenderer();
        var component = new ComponentWithNonBindingChangedProperty();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act - Pass IsChanged parameter (should be captured, not thrown)
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["IsChanged"] = true
        }));

        // Assert - Should be captured in Attributes
        Assert.True(component.ExtraAttributes.ContainsKey("IsChanged"));
        Assert.Equal(true, component.ExtraAttributes["IsChanged"]);
    }

    /// <summary>
    /// Verifies that completely non-existent *Changed parameters are captured,
    /// not thrown - this avoids false positives for arbitrary naming.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesNonExistentChangedParameter()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["CompletelyNonExistentChanged"] = EventCallback.Empty
        }));

        // Assert - Captured, not thrown
        Assert.True(component.ExtraAttributes.ContainsKey("CompletelyNonExistentChanged"));
    }

    /// <summary>
    /// Verifies that a property like "UserChanged" (not related to binding) is captured.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesPropertyWithChangedSuffix_NotBindingPattern()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithNonBindingChangedProperty();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["UserChanged"] = "some value"
        }));

        // Assert
        Assert.True(component.ExtraAttributes.ContainsKey("UserChanged"));
    }

    #endregion

    #region Bug #19638: Valid Unmatched Parameters Capture

    /// <summary>
    /// Verifies unknown parameters (without Changed suffix) are properly captured.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesUnknownParameter()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["data-custom"] = "value"
        }));

        // Assert
        Assert.NotNull(component.ExtraAttributes);
        Assert.True(component.ExtraAttributes.ContainsKey("data-custom"));
    }

    /// <summary>
    /// Verifies captured parameter value is correct.
    /// </summary>
    [Fact]
    public void SetProperties_CapturedParameterValueIsCorrect()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["CustomAttr"] = 42
        }));

        // Assert
        Assert.Equal(42, component.ExtraAttributes["CustomAttr"]);
    }

    /// <summary>
    /// Verifies multiple unknown parameters are all captured.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesMultipleUnknownParameters()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["attr1"] = "value1",
            ["attr2"] = "value2"
        }));

        // Assert
        Assert.Equal(2, component.ExtraAttributes.Count);
        Assert.Equal("value1", component.ExtraAttributes["attr1"]);
        Assert.Equal("value2", component.ExtraAttributes["attr2"]);
    }

    #endregion

    #region Bug #19638: Without CaptureUnmatchedValues

    /// <summary>
    /// Verifies error still thrown when component has no CaptureUnmatchedValues.
    /// </summary>
    [Fact]
    public void SetProperties_ThrowsForMissingCallback_WithoutCaptureUnmatchedValues()
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

    #endregion

    #region Bug #19638: Mixed Binding and Non-Binding Scenarios

    /// <summary>
    /// CRITICAL: When a parameter ends with "Changed" but NO corresponding base property exists,
    /// it should be captured as an attribute, NOT thrown. This is the key precision improvement.
    /// ComponentWithNonBindingChangedProperty has IsChanged and UserChanged, but NO Prop1.
    /// Passing Prop1Changed should be captured since Prop1 doesn't exist.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesChangedSuffix_WhenBasePropertyDoesNotExist()
    {
        // Arrange - ComponentWithNonBindingChangedProperty has: IsChanged, UserChanged (NO Prop1)
        var renderer = new TestRenderer();
        var component = new ComponentWithNonBindingChangedProperty();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act - Prop1Changed should be captured since Prop1 doesn't exist as a parameter
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["Prop1Changed"] = EventCallback.Empty
        }));

        // Assert - Should be captured in ExtraAttributes (not thrown)
        Assert.True(component.ExtraAttributes.ContainsKey("Prop1Changed"));
        Assert.Equal(EventCallback.Empty, component.ExtraAttributes["Prop1Changed"]);
    }

    /// <summary>
    /// Verifies that when a component has a valid binding pair (Prop1 + Prop1 with CaptureUnmatchedValues),
    /// and the callback is properly provided, the binding works correctly.
    /// </summary>
    [Fact]
    public void SetProperties_Succeeds_WhenBindingPairAndCallbackBothExist()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithBindingPairWithCallback();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act - Both Prop1 and Prop1Changed are present, should succeed
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["Prop1"] = true,
            ["Prop1Changed"] = EventCallback.Empty
        }));

        // Assert - Should not throw, parameters set correctly
        Assert.True(component.Prop1);
        Assert.NotNull(component.Prop1Changed);
    }

    #endregion

    #region Bug #19638: Edge Cases - Empty and Null Values

    /// <summary>
    /// Edge case: Verifies that a parameter named exactly "Changed" (which would result in
    /// an empty base property name) is captured, not thrown. The substring operation
    /// would produce an empty string for basePropertyName, which won't match any parameter.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesParameterNamedExactlyChanged()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act - "Changed" parameter results in empty basePropertyName
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["Changed"] = "some value"
        }));

        // Assert - Should be captured (empty base property name won't match any parameter)
        Assert.True(component.ExtraAttributes.ContainsKey("Changed"));
        Assert.Equal("some value", component.ExtraAttributes["Changed"]);
    }

    /// <summary>
    /// Verifies empty string as value is captured correctly.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesEmptyStringValue()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["emptyValue"] = string.Empty
        }));

        // Assert
        Assert.Equal(string.Empty, component.ExtraAttributes["emptyValue"]);
    }

    /// <summary>
    /// Verifies null value is captured correctly.
    /// </summary>
    [Fact]
    public void SetProperties_CapturesNullValue()
    {
        // Arrange
        var renderer = new TestRenderer();
        var component = new ComponentWithCaptureUnmatchedValues();
        var componentId = renderer.AssignRootComponentId(component);
        renderer.RenderRootComponent(componentId);

        // Act
        component.SetParameters(ParameterView.FromDictionary(new Dictionary<string, object>
        {
            ["nullValue"] = null!
        }));

        // Assert
        Assert.True(component.ExtraAttributes.ContainsKey("nullValue"));
        Assert.Null(component.ExtraAttributes["nullValue"]);
    }

    #endregion

    #region Test Component Classes

    /// <summary>
    /// Test component with CaptureUnmatchedValues and one [Parameter] property,
    /// but NO corresponding Changed event callback. Used to verify bug #19638 fix.
    /// </summary>
    private class ComponentWithCaptureUnmatchedValues : IComponent
    {
        public Dictionary<string, object> ExtraAttributes { get; set; } = new();

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes
        {
            get => ExtraAttributes;
            set => ExtraAttributes = value;
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
    /// Test component with CaptureUnmatchedValues and a property that happens to end with "Changed"
    /// but is NOT a binding pattern (no corresponding base property exists).
    /// </summary>
    private class ComponentWithNonBindingChangedProperty : IComponent
    {
        public Dictionary<string, object> ExtraAttributes { get; set; } = new();

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes
        {
            get => ExtraAttributes;
            set => ExtraAttributes = value;
        }

        // This is a regular bool property that happens to end with "Changed"
        // It is NOT part of two-way binding - there's no "Is" property to bind to
        [Parameter]
        public bool IsChanged { get; set; }

        [Parameter]
        public string UserChanged { get; set; } = string.Empty;

        // Note: No Prop1 or Prop1Changed - this component has different properties

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
    /// and NO corresponding Changed event callback.
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
    /// </summary>
    private class ComponentWithMultiplePropsNoChangedCallbacks : IComponent
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> ExtraAttributes { get; set; } = new();

        [Parameter]
        public bool Prop1 { get; set; }

        [Parameter]
        public string Prop2 { get; set; } = string.Empty;

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

    /// <summary>
    /// Test component with CaptureUnmatchedValues and a complete binding pair
    /// (Prop1 plus Prop1Changed). Used to verify binding succeeds when both exist.
    /// </summary>
    private class ComponentWithBindingPairWithCallback : IComponent
    {
        public Dictionary<string, object> ExtraAttributes { get; set; } = new();

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes
        {
            get => ExtraAttributes;
            set => ExtraAttributes = value;
        }

        [Parameter]
        public bool Prop1 { get; set; }

        [Parameter]
        public EventCallback<bool> Prop1Changed { get; set; }

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
