# Hentul – System Architecture

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Second Order Memory (SOM)](#2-second-order-memory-som)
   - 2.1 [Purpose](#21-purpose)
   - 2.2 [Network Dimensions and Structure](#22-network-dimensions-and-structure)
   - 2.3 [Signal Types: SPATIAL, TEMPORAL, APICAL](#23-signal-types-spatial-temporal-apical)
   - 2.4 [Sequence Memory](#24-sequence-memory)
   - 2.5 [Bursting](#25-bursting)
   - 2.6 [Training Flow](#26-training-flow)
   - 2.7 [Prediction Flow](#27-prediction-flow)
   - 2.8 [Network Modes and Neuron States](#28-network-modes-and-neuron-states)
   - 2.9 [Schema-Based Connectivity](#29-schema-based-connectivity)
   - 2.10 [Label Tracking: AllTimeSupportedLabels vs SupportedLabels](#210-label-tracking-alltimesupportedlabels-vs-supportedlabels)
   - 2.11 [Neuroplasticity (DISTALNEUROPLASTICITY)](#211-neuroplasticity-distalneuroplasticity)
3. [Orchestrator](#3-orchestrator)
   - 3.1 [Role](#31-role)
   - 3.2 [Vision Scopes](#32-vision-scopes)
   - 3.3 [RecordPixels](#33-recordpixels)
   - 3.4 [Public API Summary](#34-public-api-summary)
4. [Hippocampal Complex](#4-hippocampal-complex)
   - 4.1 [Role](#41-role)
   - 4.2 [Object Lifecycle](#42-object-lifecycle)
   - 4.3 [Core Data Structures](#43-core-data-structures)
   - 4.4 [The Graph (Spatial Index)](#44-the-graph-spatial-index)
   - 4.5 [Key Methods](#45-key-methods)
5. [Vision Stream Processor](#5-vision-stream-processor)
   - 5.1 [PixelEncoder](#51-pixelencoder)
   - 5.2 [SendApical and SendAPicalFeedback](#52-sendapical-and-sendapicalfeedback)
6. [Two-Phase Vision Pipeline](#6-two-phase-vision-pipeline)
   - 6.1 [Phase 1 – BROAD Scan](#61-phase-1--broad-scan)
   - 6.2 [Phase 2 – NARROW Deep Learning](#62-phase-2--narrow-deep-learning)
7. [Full Data-Flow Diagram](#7-full-data-flow-diagram)

---

## 1. System Overview

Hentul is a **biologically-inspired hierarchical visual object recognition system** that models two complementary memory systems found in the mammalian brain:

| Layer | Biological Analogue | Role in Hentul |
|---|---|---|
| **Second Order Memory (SOM)** | Neocortical sequence memory (HTM / Layer 3) | Learns and predicts temporal sequences of visual patterns |
| **Hippocampal Complex (HC)** | Hippocampus / Entorhinal Cortex | Stores *where* objects are in the environment (spatial memory) |

The **Orchestrator** acts as the central coordinator, capturing visual input from the screen, routing it through the **Vision Stream Processor** (which encodes and fires the SOM network), and simultaneously updating the **Hippocampal Complex** with spatial location data.

```
┌────────────────────────────────────────────────────────┐
│                     ORCHESTRATOR                       │
│  ┌──────────────┐   RecordPixels()   ┌─────────────┐  │
│  │  Screen /    │──────────────────▶│  PixelEncoder│  │
│  │  Cursor API  │                   │  (SDR_SOM)   │  │
│  └──────────────┘                   └──────┬───────┘  │
│                                            │           │
│                          ┌─────────────────┴────────┐  │
│                          │  VisionStreamProcessor    │  │
│                          │  (BlockBehaviourManagerSOM│  │
│                          │   Layer 3A / 3B)          │  │
│                          └─────────────┬─────────────┘  │
│                                        │                │
│                          ┌─────────────▼─────────────┐  │
│                          │   HippocampalComplex       │  │
│                          │   (Graph + Objects)        │  │
│                          └───────────────────────────┘  │
└────────────────────────────────────────────────────────┘
```

---

## 2. Second Order Memory (SOM)

### 2.1 Purpose

The Second Order Memory module (`SecondOrderMemory/`) implements a **sparse, biologically-plausible sequence learning network** based on Hierarchical Temporal Memory (HTM) principles. It learns *temporal sequences* of sparse distributed representations (SDRs) and, after training, predicts what pattern should come next given the current input — disambiguating similar-looking sequences through context.

The name "Second Order" refers to the fact that predictions are made not just on the current input but on the *order* of past inputs. The same column can mean different things depending on what fired in the previous timestep.

### 2.2 Network Dimensions and Structure

The SOM network is configured for the vision pipeline as a **1200 × 600 × 5** grid:

| Dimension | Value | Meaning |
|---|---|---|
| **X** | 1200 | Number of column positions along the horizontal axis |
| **Y** | 600 | Number of column positions along the vertical axis |
| **Z** | 5 | Number of neurons per column |

**Column**: A column at position `(X, Y)` contains Z neurons. Columns are the basic spatial unit. When a region of the visual field is active, the corresponding column fires. If no prediction exists for that column, the entire column *bursts*.

**Neuron**: Each neuron sits at position `(X, Y, Z)`. It accumulates voltage from multiple input sources:
- Proximal (feedforward) schema connections: **+100 mV**
- Temporal line depolarisation: **+40 mV**
- Apical line depolarisation: **+40 mV**
- Active distal dendritic synapse: **+20 mV**
- Direct fire: **+101 mV** (clears threshold immediately)

In addition to normal `NORMAL` neurons, the network maintains two special neuron arrays:

| Array | Structure | Purpose |
|---|---|---|
| **TemporalLineArray** | `Neuron[Y, Z]` | One temporal neuron per (Y, Z) slice; each connects to all X columns at that (Y, Z). Carries "what fired last cycle?" context. |
| **ApicalLineArray** | `Neuron[X, Y]` | One apical neuron per column (X, Y); connects to all Z neurons in that column. Carries top-down "what is expected?" feedback. |

---

### 2.3 Signal Types: SPATIAL, TEMPORAL, APICAL

Every SDR fired into the network carries an **input type** (`iType`) that determines how the signal is processed.

#### SPATIAL
- **Meaning**: Primary sensory input — the "what is happening right now" signal.
- **Source**: `PixelEncoder.EncodeBitmap()` → encoded bitmap as a list of active column positions.
- **Processing**: For each active position `(X, Y)` in the SDR, the corresponding column fires its predicted neurons (or bursts if none are predicted). The cycle counter advances only for SPATIAL fires.
- **Connection type created**: Proximal dendritic (via schema), and distal dendritic (via Wiring after firing).
- **Biological analogue**: Feedforward sensory signal from thalamus/V1 arriving at Layer 4 / proximal dendrites.

#### TEMPORAL
- **Meaning**: Sequential context — "what fired one step ago".
- **Source**: `TemporalLineArray[Y, Z]` neurons, activated by firing a TEMPORAL-typed SDR.
- **Processing**: Temporal neurons depolarise (+40 mV) the spatial neurons they connect to, pre-activating the neurons most consistent with the previous timestep. This biases the column to fire the *predicted* neuron rather than bursting.
- **Biological analogue**: Basal dendrite input from recurrent connections in the same cortical layer.

#### APICAL
- **Meaning**: Top-down feedback — "what is the higher layer expecting?".
- **Source**: `ApicalLineArray[X, Y]` neurons, activated by firing an APICAL-typed SDR (same positions as a SPATIAL SDR but with `iType.APICAL`).
- **Processing**: Apical neurons depolarise (+40 mV) all Z neurons in their column, making those neurons more likely to fire when the spatial signal arrives. Unlike TEMPORAL, apical signals are column-wide (not neuron-specific), functioning as a soft "attention" gate.
- **In the Vision Pipeline**: During Phase 2 (NARROW), the encoded SDR of each object is fired repeatedly as an apical signal (`SendApical(N)`), reinforcing the sequence memory connections.
- **Biological analogue**: Apical dendrite input from higher cortical layers or hippocampus (Layer 1 / distal dendrites).

**Typical firing sequence in biologically-faithful mode:**
```
TEMPORAL → APICAL → SPATIAL
(previous context)  (top-down expectation)  (sensory input)
```

In the current vision pipeline only SPATIAL and APICAL are used; TEMPORAL is reserved for future use.

---

### 2.4 Sequence Memory

**Sequence Memory** is the core learning mechanism of the SOM. It builds *temporal predictive associations* between consecutive spatial patterns using **distal dendritic connections**.

**How it works:**

1. A SPATIAL SDR fires at time *t*. The active neurons are stored in `NeuronsFiringThisCycle`.
2. After firing, `BuildSequenceMemory()` creates **distal dendritic synapses** from every neuron that fired at *t-1* (`NeuronsFiringLastCycle`) to every neuron that fired at *t* (`NeuronsFiringThisCycle`) — provided they are in different columns.
3. At the next timestep, when the *t* neurons fire, their axons send a spike through these distal synapses, raising the voltage of the neurons they connect to. Those neurons transition from RESTING → PREDICTED.
4. When the *t+1* pattern arrives and the predicted neurons fire (confirming the prediction), the synapse strength is increased.

**Higher-Order Sequencing** (`PerformHigherOrderSequencing`): Each synapse stores a `SupportedPredictions` list — a mapping from `ObjectLabel` to the set of neuron IDs predicted to fire next. This means the *same* column can predict different things depending on which object label the network is currently tracking, enabling disambiguation of sequences that share a common prefix.

---

### 2.5 Bursting

**Bursting** is the network's response to an unexpected or novel input — when a column fires but no neuron in it was predicted.

**Detection** (in `Column.GetPredictedNeuronsFromColumn()`):
```
predictedNeurons = neurons where CurrentState != RESTING

if predictedNeurons.Count == 0  →  BURST (return all Z neurons)
if predictedNeurons.Count == 1  →  normal fire (return that neuron)
if predictedNeurons.Count >  1  →  pick winner by highest voltage
```

**When bursting happens:**
- **First encounter**: The object has never been seen before, so no predictions exist.
- **Prediction error**: The predicted pattern did not match the arriving input.
- **Post-cleanup**: After `ChangeNetworkModeToPrediction()` flushes neuron states, all columns start in a "fresh" RESTING state.

**Why bursting is important:**
- Guarantees that at least *some* neuron fires for the new pattern, preserving temporal continuity.
- Triggers the network to *learn*: burst columns record distal connections to last-cycle neurons, bootstrapping the prediction machinery.
- Acts as the system's uncertainty signal: many burst columns = unfamiliar scene; few/no burst columns = familiar, well-predicted scene.

**Learning from bursts:** The Wiring module (Cases 1–5 inside `Wire()`) chooses a learning strategy based on burst/prediction ratios:
- **Case 4** (all burst): Wire all burst neurons to last-cycle neurons, initialise synapses.
- **Cases 2–3** (mixed): Strengthen already-correct predictions, optionally connect burst neurons.
- **Case 1** (no burst): Strengthen correctly-predicted neurons only.

---

### 2.6 Training Flow

Training is triggered by calling `Fire(sdr, cycle, CreateActiveSynapses: true)` with `NetWorkMode == TRAINING`.

```
Fire(SPATIAL SDR, cycle N)
│
├─ PreCyclePrep()         – reset counters, validate caches
├─ ValidateInput()        – check bounds, handle stale apical/temporal caches
│
├─ For each active (X, Y) in SDR:
│   GetPredictedNeuronsFromColumn()
│   → burst / predicted / winner fire
│   → NeuronsFiringThisCycle updated
│
├─ neuron.Fire()          – set voltage, add ObjectLabel to AllTimeSupportedLabels
├─ ProcessSpikes()        – axons fire: strengthen/create distal predictions
│
├─ Wire()                 – Cases 1–5: create/strengthen distal dendritic synapses
├─ PerformHigherOrderSequencing() – update per-label prediction tables in synapses
│
├─ PrepNetworkForNextCycle() – shift PredictedNeuronsForNextCycle → ThisCycle
└─ PostCycleCleanup()     – flush voltages, manage spiking neurons
```

`CreateActiveSynapses: true` means new distal synapses are **immediately active** and contribute to predictions in the very next cycle. With `false`, synapses require `DISTALNEUROPLASTICITY` (5) correct predictions before activating — modelling synaptic consolidation.

---

### 2.7 Prediction Flow

After training is complete, call `ChangeNetworkModeToPrediction()`. Subsequent `Fire()` calls run the same firing pipeline **but skip the Wiring step**, instead calling `UpdateCurrentPredictions()`.

```
UpdateCurrentPredictions():
│
├─ For each neuron in NeuronsFiringThisCycle:
│   GetCurrentPotentialMatchesForCurrentCycle()
│   → if contributing neurons exist: read SupportedPredictions from their synapses
│   → if not:                        fall back to AllTimeSupportedLabels
│   → union into cyclePredictions
│
├─ intersect = CurrentPredictions ∩ cyclePredictions
│   (first step: intersect = cyclePredictions, since CurrentPredictions is empty)
│
├─ if intersect.Count == 0 → ambiguous, do nothing
├─ if intersect.Count == 1 → classification complete:
│       NetWorkMode = DONE
│       CurrentObjectLabel = intersect[0]
└─ else                    → still ambiguous, CurrentPredictions = intersect
```

With each successive SPATIAL fire the prediction set narrows until exactly one object label remains. This is analogous to a sentence disambiguation: *"Bank"* alone is ambiguous, but *"bank" + "river"* narrows to one meaning.

---

### 2.8 Network Modes and Neuron States

#### Network Modes (`NetworkMode`)

| Mode | Description | Transition |
|---|---|---|
| `EXPLORE` | Initial state. No learning or prediction. | → TRAINING when `SetUpNewObjectLabel()` is called |
| `TRAINING` | Learns distal connections on each fire. | → PREDICTION when `ChangeNetworkModeToPrediction()` is called |
| `PREDICTION` | Classifies input, no new connections formed. | → DONE when single label remains in intersection |
| `DONE` | Classification complete. Further firing throws an exception. | Manual reset required |

#### Neuron States (`NeuronState`)

| State | Voltage | Description |
|---|---|---|
| `RESTING` | 0 | Neuron is inactive. Column will burst if no other neuron in it is predicted. |
| `PREDICTED` | 1–99 | Neuron has been depolarised (by temporal/apical/distal signals) and is primed to fire. |
| `FIRING` | 100–499 | Neuron is actively firing this cycle. Sends spikes to its axonal connections. |
| `SPIKING` | ≥ 500 | Neuron is in an ultra-high-activity state (spike train). Tracked separately for special learning rules. |

---

### 2.9 Schema-Based Connectivity

Schemas define the **fixed feedforward connectivity** of the network — which encoder positions connect to which SOM columns. They are pre-computed and stored as XML files, then cached as compact binary files for fast loading.

#### Schema Files

Located in `SecondOrderMemory/Schema Docs/1K Club/`:
- `DendriticSchemaSOM.xml` – proximal (feedforward) connections from encoder grid → columns
- `AxonalSchema-SOM.xml` – lateral axonal connections between columns

The network size determines which schema is loaded:

| Network Dimensions | Schema Variant |
|---|---|
| X=1200, Y=600, Z=5 (vision) | `1K Club/` (standard vision schemas) |
| X=200, Y=10, Z=5 (text) | `1K Club/Text/` (text schemas) |

#### Dendritic Schema

Maps encoder output positions `(eX, eY, eZ)` to dendritic connections on neurons `(nX, nY, nZ)`. This is the **feedforward path**: when a pixel region encodes to position `(eX, eY)`, the schema determines which column `(nX, nY)` "receives" that signal through its proximal dendrites. Each active proximal connection contributes **+100 mV** to its target neuron.

#### Axonal Schema

Maps neuron `(sX, sY, sZ)` to axonal connections at `(tX, tY, tZ)`. This defines **lateral inhibition and contextual spread** between columns. Active schema axonal connections contribute **+50 mV** to target neurons.

#### Loading Process

```
App startup
│
├─ ReadDendriticSchema()
│   ├─ Check for .bin cache file
│   ├─ If absent: parse .xml → write .bin (one-time, ~1 min for 1.4 GB XML)
│   └─ Load .bin: flat sequence of int[6] tuples → InitDendriticConnectionForConnector()
│
└─ ReadAxonalSchema()
    └─ Same process for axonal connections
```

The binary cache reduces startup from minutes to seconds on subsequent runs. The `.bin` files are gitignored and regenerated automatically if missing.

---

### 2.10 Label Tracking: AllTimeSupportedLabels vs SupportedLabels

Two distinct label tracking mechanisms coexist:

| | `AllTimeSupportedLabels` | `SupportedLabels` |
|---|---|---|
| **Scope** | Per neuron | Per SOM layer (BlockBehaviourManagerSOM) |
| **Type** | `List<string>` | `HashSet<string>` |
| **Updated** | When a neuron fires during TRAINING for a given object label | When `SetUpNewObjectLabel()` is called |
| **Purpose** | Fallback prediction source: "which objects has this neuron ever helped recognise?" | Validation gate: prevents prediction if no labels have been trained |
| **Used in** | `GetCurrentPotentialMatchesForCurrentCycle()` (line 212, Neuron.cs) | `ValidateInput()` guard (line 2158) |

During prediction, if a neuron fires but has no contributing neurons from the previous cycle (e.g., burst fire), it falls back to its `AllTimeSupportedLabels` as its contribution to `cyclePredictions`. This is why firing a column that was trained on exactly one label immediately narrows the prediction to that label.

---

### 2.11 Neuroplasticity (DISTALNEUROPLASTICITY)

```csharp
public static uint DISTALNEUROPLASTICITY = 5;
```

When `CreateActiveSynapses = false` in `Fire()`, new distal dendritic synapses are created in an **inactive** state. They must receive `DISTALNEUROPLASTICITY` (5) correct predictions before becoming active and contributing voltage to target neurons.

This models **synaptic consolidation**: connections are not trusted until they have been repeatedly confirmed. The constant can be adjusted to control how quickly the network generalises from new input.

---

## 3. Orchestrator

### 3.1 Role

`Hentul/Orchestrator.cs` is the **central singleton coordinator** for the entire system. It:
- Owns and initialises all subsystems: `VisionStreamProcessor`, `HippocampalComplex`, and `TextStreamProcessor`
- Captures visual input from the screen (via Win32 `CopyFromScreen`)
- Routes encoded visual data to both the SOM network (for pattern learning) and the HC (for spatial memory)
- Manages the global `NetworkMode` and `VisionScope` state
- Exposes a unified public API to the UI layer (`HentulWinforms`)

### 3.2 Vision Scopes

The `VisionScope` enum controls capture resolution and encoding strategy:

| Scope | Bitmap Size | Scale Factor | Use Case |
|---|---|---|---|
| `BROAD` | 3600 × 1800 px | Downsampled 6×3 → 600×600 grid | Phase 1: full-screen sweep to find objects |
| `NARROW` | 600 × 600 px | 1:1 | Phase 2: detailed capture of a single detected object |
| `OBJECT` | 600 × 600 px | 1:1 | Legacy / alternative to NARROW |

In `PixelEncoder`, BROAD scope uses the **screen cursor position** as the windowing centre, while NARROW/OBJECT use the **bitmap centre** (`bmp.Width/2, bmp.Height/2`). This corrects a potential off-centre encoding bug when the cursor is near a screen edge.

### 3.3 RecordPixels

```csharp
public void RecordPixels(VisionScope scope)   // Captures BROAD (3600×1800) or NARROW (600×600)
public void RecordPixels(int width, int height) // Captures custom-sized region
```

Both overloads:
1. Get the current cursor position via `GetCurrentPointerPosition()` (Win32 `GetCursorPos`)
2. Compute the top-left corner of the capture rectangle: `cursor - dimension/2`  (clamped to screen bounds)
3. Call `CaptureScreenRegion()` which uses `Graphics.CopyFromScreen()` to capture a live screenshot of that rectangle
4. Store the resulting bitmap in `orchestrator.bmp` for subsequent processing

### 3.4 Public API Summary

| Category | Method | Description |
|---|---|---|
| Initialisation | `GetInstance(isMock, shouldInit, nMode)` | Singleton accessor |
| Initialisation | `InitHC()` | Initialise HC Graph with primary screen dimensions |
| Input Capture | `RecordPixels(VisionScope)` | Cursor-centred screen capture |
| Training | `SetupLabel(Bitmap, string)` | Encode bitmap into SDR, register object label |
| Training | `TrainImage(offsetX, offsetY)` | Store visual location in HC Graph + fire SOM |
| Training | `DoneWithTraining(string)` | Finalise current object, transition HC to next training cycle |
| Mode Control | `ChangeNetworkModeToPrediction(isMock)` | Switch SOM to prediction mode |
| Mode Control | `SetVisionScope(scope, isMock)` | Switch vision scope |
| Recognition | `GetPredictedObject()` | Query HC for current recognised object |
| Recognition | `CheckIfObjectIsRecognised()` | Query HC recognition state |
| Persistence | `BackUp()` / `Restore()` | Serialise / deserialise HC state to XML |
| Utilities | `MoveCursorToSpecificPosition(x, y)` | Move cursor via Win32 `SetCursorPos` |

---

## 4. Hippocampal Complex

### 4.1 Role

`Hentul/Hippocampal_Entorinal_complex/HippocampalComplex.cs` models the **hippocampus and entorhinal cortex**: the region responsible for episodic and spatial memory. While the SOM learns *what* visual patterns look like and in what sequence they appear, the Hippocampal Complex learns *where* those objects are in the environment and stores object identities for later retrieval.

### 4.2 Object Lifecycle

```
SetupLabel("Apple")
     │
     ▼
UnrecognisedEntity { Label="Apple", ObjectSnapshot=[] }
     │
     │  (multiple TrainImage() calls accumulate Sensation_Location entries)
     │
     ▼
DoneWithTraining("Apple")
     │
     ▼
RecognisedVisualEntity { Label="Apple", FavouritePositions=[...], frame=RFrame }
     │   stored in Objects["Apple"]
     │   loaded into Graph.LoadObject()
     ▼
Graph (Quad-tree spatial index)
```

### 4.3 Core Data Structures

#### UnrecognisedEntity
The **in-progress training buffer**. Accumulates sensations and screen positions during the training phase:
```
UnrecognisedEntity
├── Label: string                           – object name ("Apple")
├── ObjectSnapshot: List<Sensation_Location> – visual feature snapshots
├── Sensations: List<Sensation>             – text/language sensations
└── sType: SenseType                        – SenseNLocation | SenseOnly
```

#### RecognisedVisualEntity
The **frozen trained snapshot**. Created from `UnrecognisedEntity` when `DoneWithTraining()` is called:
```
RecognisedVisualEntity
├── Label: string
├── ObjectSnapshot: List<Sensation_Location>  – learned visual patterns
├── FavouritePositions: List<Position2D>      – key screen locations
├── frame: RFrame                             – orientation reference
└── CenterPosition: Position2D               – object centroid
```

#### Sensation_Location
A single visual observation at a screen location. Maps a screen coordinate to the SOM neurons that fired when the camera/cursor was at that position:
```
Sensation_Location
└── sensLoc: SortedDictionary<
        string (screen location key),
        KeyValuePair<BBMID, List<Position2D>>  (which SOM neurons fired)
    >
```

#### Sensation
A language / text sensation. Associates typed or spoken characters with specific SOM firing patterns. Used by `AddNewCharacterSensationToHC()` to build multimodal associations.

### 4.4 The Graph (Spatial Index)

`Graph` is a **singleton quad-tree** that stores all known object positions in screen space:

```
Graph
├── Base: Node                         – root of the quad-tree
├── Environment: EnvironmentBounds     – primary screen dimensions
└── _objectBoundsCache: Dictionary<string, ObjectBounds>

Key operations:
  AddNewNode(Position2D)               – insert a position
  GetObjectsInRegion(x, y, w, h)      – bounding-box query
  GetObjectsAtPosition(Position2D)    – point query
  LoadObject(RecognisedVisualEntity)  – load all positions for an object
  CompareTwoObjects(first, second)    – spatial comparison for recognition
```

When `ConvertUnrecognisedObjectToRecognisedObject()` completes, the new object is immediately loaded into the Graph via `Graph.LoadObject(newObject)`, making it queryable from that point on.

### 4.5 Key Methods

| Method | Description |
|---|---|
| `InitialiseEnvironment(w, h)` | Sets `Graph.EnvironmentBounds` to primary screen dimensions |
| `StoreNewObjectLocationInGraph(sdr, offsetX, offsetY)` | Converts SDR positions to screen coords (pos + offset) and adds to Graph. **Only valid when VisionScope == BROAD.** |
| `DoneWithTraining(label)` | Converts `CurrentObject` (UnrecognisedEntity) → `RecognisedVisualEntity`, adds to `Objects`, loads into Graph |
| `AddNewSensationToObject(sensation)` | Appends a text sensation to the current training object |
| `GetEnvironmentBounds()` | Returns the screen dimensions registered at startup |
| `GetAllObjectsInEnvironment()` | Returns all currently loaded object bounding boxes from the Graph |
| `Backup()` / `Restore()` | Serialise/deserialise all `RecognisedVisualEntity` objects to/from XML |

---

## 5. Vision Stream Processor

`Hentul/VisionStream/VisionStreamProcessor.cs` is the **bridge between raw bitmap input and the SOM network**. It owns the `PixelEncoder` and the `BlockBehaviourManagerSOM` instance used for vision.

### 5.1 PixelEncoder

`Hentul/Encoders/PixelEncoder.cs` converts a captured bitmap into an `SDR_SOM` (sparse distributed representation) suitable for the SOM network.

**Input:** 600×600 or 3600×1800 bitmap (depending on scope)

**Algorithm:**

```
1. Determine scale factors:
      BROAD:        scaleX = 6,  scaleY = 3  (3600×1800 → 600×600 grid)
      NARROW/OBJECT: scaleX = 1, scaleY = 1  (600×600 → 600×600 grid, 1:1)

2. Determine encoding centre:
      NARROW/OBJECT: centre = (bmp.Width/2, bmp.Height/2)
      BROAD:         centre = screen cursor position

3. For each pixel (px, py) in the bitmap:
      a. Downsample: gridX = px / scaleX,  gridY = py / scaleY
      b. Colour filter: skip pixel if R < 240 || G < 240 || B < 240
         (only white / near-white pixels are encoded)
      c. Compute linear index:
            localX = gridX - centreX,  localY = gridY - centreY
            linearIndex = localY * EncoderWidth + localX
      d. Spread into SOM bit-space:
            step = (GridX * GridY) / (EncoderWidth * EncoderHeight)
            absoluteIndex = linearIndex * step
      e. Map to 2D SOM coordinates:
            somX = absoluteIndex % GridX
            somY = absoluteIndex / GridX
      f. Append Position_SOM(somX, somY) to ActiveBits

4. Return SDR_SOM(GridX=1200, GridY=600, ActiveBits, iType.SPATIAL)
```

**Key design choices:**
- Only **white pixels** are encoded: the input bitmap is assumed to be a binary edge/contour image (Canny-edge-detected).
- The encoding is **deterministic**: the same bitmap always produces the same SDR.
- **Sparsity** is maintained by the spreading step — not every pixel maps to a unique SOM bit.

### 5.2 SendApical and SendAPicalFeedback

| Method | Called from | Fires | Times | Purpose |
|---|---|---|---|---|
| `SendAPicalFeedback()` | `Orchestrator.TrainImage()` (Phase 1 BROAD) | Current `SomSDR` (SPATIAL type) | Once | Reinforce SOM on current chunk during BROAD sweep |
| `SendApical(int N)` | `Explore_Click` Phase 2 | `apical_SOM` (APICAL type) | N times | Sequence memory training on detected object |

`apical_SOM` is created in `SetupLabel()` as a copy of the encoded `SomSDR` but with `iType.APICAL`. Firing it N times (typically 5) in sequence allows the SOM's temporal machinery to strengthen the associations between consecutive apical steps, building a richer prediction model for that object.

---

## 6. Two-Phase Vision Pipeline

The complete vision pipeline is implemented in `HentulWinforms/Form1.cs` (`Explore_Click`).

### 6.1 Phase 1 – BROAD Scan

**Goal:** Sweep the full screen at low resolution to discover object locations.

```
VisionScope = BROAD (3600 × 1800)
NetworkMode = EXPLORE

1. Partition primary screen into chunks (3600 × 1800 each)

2. For each chunk:
   a. MoveCursor(chunk.CenterX, chunk.CenterY)
   b. orchestrator.RecordPixels(3600, 1800)   ← capture 3600×1800 bitmap centred on cursor
   c. GetEdgedMat(bmp)                         ← Canny edge detection (threshold 50 / 200)
   d. SegmentObjectsFromMat(edgedMat, chunk.StartX, chunk.StartY)
      → FindContours (external, simple approximation)
      → Filter contours by MinContourArea (500 px²)
      → Return List<DetectedRegion> { ScreenX, ScreenY, Width, Height }

3. Collect all DetectedRegion results into allRegions
```

`DetectedRegion` carries the screen-space bounding box of each discovered contour plus computed `CenterX` / `CenterY` for Phase 2 cursor targeting.

### 6.2 Phase 2 – NARROW Deep Learning

**Goal:** For each detected region, capture a high-resolution view and train the SOM sequence memory.

```
VisionScope = NARROW (600 × 600)
NetworkMode = TRAINING

For each DetectedRegion in allRegions:
   label = $"Object{index+1}"

   a. MoveCursor(region.CenterX, region.CenterY)     ← centre on discovered object
   b. orchestrator.RecordPixels(VisionScope.NARROW)   ← capture 600×600 bitmap
   c. edgedBmp = ConverToEdgedBitmap(bmp)             ← convert to edge image
   d. orchestrator.SetupLabel(edgedBmp, label)
      → PixelEncoder.EncodeBitmap()  → SomSDR (SPATIAL)
      → SomBBM.SetUpNewObjectLabel(label)
      → apical_SOM = copy of SomSDR with iType.APICAL
   e. (if SDR is empty, skip — no visible features)
   f. orchestrator.VisionProcessor.SendApical(5)
      → fires apical_SOM into SomBBM 5 times in sequence
      → CycleNum increments each fire
      → SOM builds temporal sequence memory for this object
   g. Update UI with active bit positions (visual feedback)
```

The 5 repetitions in `SendApical(5)` allow the SOM to see the same pattern multiple times in sequence. With `DISTALNEUROPLASTICITY = 5`, this is the minimum number of correct predictions needed for synapses to fully activate — so a single `SendApical(5)` pass bootstraps full temporal connectivity for each object.

---

## 7. Full Data-Flow Diagram

```
Screen
  │
  │ CopyFromScreen()
  ▼
Bitmap (3600×1800 BROAD  or  600×600 NARROW)
  │
  │ ConverToEdgedBitmap()   [Canny edge detection via OpenCvSharp]
  ▼
Edge Bitmap
  │
  ├──────────────────────────────────────────────────────────────────┐
  │  PHASE 1 (BROAD)                                                 │
  │  SegmentObjectsFromMat()                                         │
  │  → List<DetectedRegion>  ─────────────────────────────────────▶ │
  │                                         used to steer Phase 2   │
  └──────────────────────────────────────────────────────────────────┘
  │
  │  PHASE 2 (NARROW)
  │  PixelEncoder.EncodeBitmap()
  ▼
SDR_SOM (List<Position_SOM>, iType.SPATIAL)
  │
  ├──────────────────────────────────────────────────────┐
  │                                                      │
  │ VisionStreamProcessor.SendApical(5)                  │ Orchestrator.TrainImage()
  │    apical_SOM (iType.APICAL) × 5 firings             │    StoreNewObjectLocationInGraph()
  ▼                                                      ▼
BlockBehaviourManagerSOM                        HippocampalComplex
  │                                                      │
  │  Sequence Memory                                     │  Quad-tree Graph
  │  Higher-Order Sequencing                             │  Screen-space positions
  │  Temporal Distal Dendrites                           │  RecognisedVisualEntity
  ▼                                                      ▼
CurrentPredictions (List<string>)             Objects["label"] + Graph nodes
  │                                                      │
  └──────────────────────────────────────────────────────┘
                          │
                          ▼
               Orchestrator.GetPredictedObject()
               → recognised label + spatial position
```

---

*Document generated for Hentul project — `features/HCComplex` branch.*
