# ModifiersCore

ModifiersCore is a library designed for creating and managing custom gameplay modifiers for Beat Saber.
It allows developers to add custom modifiers, customize their visuals, and control their behavior within the game. 

## Creating a Custom Modifier

To create a custom modifier, you need to implement the ICustomModifier interface. 
One of the most important properties here is `Multiplier`. This property defines how much the final rank will be changed negatively or positively. 
If the modifier does not suppose to change the gameplay logic you can specify `0`. In this case scoring will not be affected.
Otherwise use positive or negative value to increase or decrease the rank respectively.

Example:
```csharp
using ModifiersCore;
using UnityEngine;

class MyCustomModifier : ICustomModifier {
    public string Id => "my_custom_modifier";
    public string Name => "My Custom Modifier";
    public string Description => "A custom modifier that alters gameplay.";
    public Sprite Icon => /* Provide your custom icon */;
    public Color? Color => Color.magenta;
    public Color? MultiplierColor => Color.yellow;
    public float Multiplier => 1.2f;
    public IEnumerable<string>? MutuallyExclusives => null;
    public IEnumerable<string>? Requires => null;
    public IEnumerable<string>? RequiredBy => null;
}
```

If you don't need to implement any logic inside this class you can use CustomModifier implementation instead.

Example:
```csharp
new CustomModifier(
    "my_modifier",
    "My Custom Modifier",
    "Nothing too special",
    /* Provide your custom icon */,
    Color.magenta,
    Color.yellow,
    0.5f
    /* You can omit further dependencies since they are optional */
);
```

## Adding and Registering Modifiers

Once your custom modifier class is created, you can register it using `ModifiersManager.AddModifier`.
This will make your modifier appear in the game's modifiers panel and apply its effects when selected.

Example:
```csharp
using ModifiersCore;
using UnityEngine;

class ModifierRegistrator : MonoBehaviour {
    private void Start() {
        var myModifier = new MyCustomModifier();
        ModifiersManager.AddModifier(myModifier);
    }
}
```

## Checking Modifier States

You can query the state of a modifier (enabled or disabled) by using the `ModifiersManager.GetModifierState` method. 

Example:
```csharp
using ModifiersCore;

class ModifierLogicInstaller : MonoBehaviour {
    private void Start() {
        bool isModifierEnabled = ModifiersManager.GetModifierState("my_custom_modifier");
        if (isModifierEnabled) {
            // Apply custom behavior when the modifier is enabled
            Debug.Log("My Custom Modifier is enabled!");
        } else {
            // Do nothin when the modifier is not enabled
            Debug.Log("My Custom Modifier is not enabled.");
        }
    }
}
```
You can also enable or disable modifiers programmatically using `ModifiersManager.SetModifierState`.

## Listening for Modifier Changes

ModifiersCore provides events for tracking when modifiers are added or removed. 
You can subscribe to these events to react to changes in the modifier state.

Example:
```csharp
using ModifiersCore;
using UnityEngine;

class ModifierStateListener : MonoBehaviour {
    private void Start() {
        ModifiersManager.ModifierAddedEvent += OnModifierAdded;
        ModifiersManager.ModifierRemovedEvent += OnModifierRemoved;
    }

    private void OnModifierAdded(ICustomModifier modifier) {
        Debug.Log($"Modifier added: {modifier.Name}");
    }

    private void OnModifierRemoved(ICustomModifier modifier) {
        Debug.Log($"Modifier removed: {modifier.Name}");
    }
}
```

## Advanced Integration

You can create complex gameplay logic by defining dependencies between modifiers.
For example, some modifiers may require other modifiers to be active or prevent other modifiers from being activated simultaneously.

Example:
```csharp
class ComplexModifier : ICustomModifier {
    public string Id => "complex_modifier";
    public string Name => "Complex Modifier";
    public string Description => "This modifier depends on others.";
    public Sprite Icon => /* Your custom icon */;
    public float Multiplier => 1.5f;
    
    // Ensure this modifier cannot be enabled with "conflicting_modifier"
    public IEnumerable<string>? MutuallyExclusives => new List<string> { "conflicting_modifier" };

    // This modifier requires "required_modifier" to be active
    public IEnumerable<string>? Requires => new List<string> { "required_modifier" };
    
    // Other modifiers may depend on this one
    public IEnumerable<string>? RequiredBy => new List<string> { "dependent_modifier" };
}
```

By defining MutuallyExclusives, Requires, and RequiredBy, you can craft intricate gameplay interactions between different modifiers.
