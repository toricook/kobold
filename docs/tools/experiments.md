# Experiments Sandbox

Interactive testing environment for Kobold features using Rapid.NET.

## Overview

Kobold.Experiments provides a sandbox for testing features with real-time parameter tweaking via an automatically generated UI.

## Features

- **Rapid.NET Integration** - Automatic UI from code attributes
- **Real-time Updates** - Change parameters without recompiling
- **Visual Output** - See results immediately
- **Current Experiments:**
  - Cellular Automata Cave Generation

## Usage

### Run Experiments

```bash
cd Kobold.Experiments
dotnet run
```

A WPF window appears with:
- **Left Panel:** Parameter controls
- **Right Panel:** Visual output

### Current: Cellular Automata

Test cave generation with configurable parameters:
- Map dimensions (width, height)
- Smoothing iterations
- Wall probability
- Birth/death thresholds
- Random seed
- Cave connection settings

Adjust parameters and click "Run" to see updated results instantly.

## Creating New Experiments

Add a new script class:

```csharp
[Script("My Experiment", "Description")]
public class MyExperimentScript
{
    [IntSlider(1, 100, "Width", "Map width")]
    public int Width { get; set; } = 50;

    [FloatSlider(0, 1, "Density", "Initial density")]
    public float Density { get; set; } = 0.5f;

    [Button("Generate", "Run generation")]
    public void Generate()
    {
        // Your experiment code
    }
}
```

Rapid.NET automatically generates UI from attributes.

## See Also

- **[Procedural Generation](../procedural/)** - Algorithms being tested
- **[Rapid.NET Documentation](https://github.com/PixelDough/Rapid.NET)** - UI framework

---

**Back:** [Tools Overview](index.md) ←
