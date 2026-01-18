# Traditional GoGo Setup Guide

## Overview
This guide explains how to set up and use the **Traditional GoGo** interaction technique in your Unity VR project. You can use it standalone or toggle between Traditional GoGo and ReverseGoGo.

## Unity XR Hierarchy Reference

Your Unity scene should have an XR Rig structure like this:
```
XR Origin (or XR Rig)
├── Camera Offset
│   ├── Main Camera (HMD)
│   ├── LeftHand Controller
│   │   └── [Visual models/interactors]
│   └── RightHand Controller  ← THIS is what you need!
│       └── [Visual models/interactors]
```

**Important GameObject Names:**
- **Right Hand Controller**: `RightHand Controller`, `Right Controller`, or `RightHandController`
- **Main Camera (HMD)**: Usually named `Main Camera` with "MainCamera" tag
- Look for GameObjects with `XRController`, `ActionBasedController`, or `TrackedPoseDriver` components

## What is Traditional GoGo?

Traditional GoGo (Poupyrev et al., 1996) allows you to reach distant objects by extending your hand:

### How It Works:
1. **Extend your physical hand** beyond 0.3m from HMD (chest area)
2. **Virtual hand shoots outward** exponentially - extends much farther than your real hand
3. **Virtual hand physically touches** distant objects (no ray/laser)
4. **Press Grip** when touching object → grab it and attach to virtual hand
5. **Move controller** → object follows virtual hand position
6. **Exponential manipulation**: 10cm real hand movement = 1m+ virtual movement (rapid relocation)
7. **Release Grip** → drop object

### Key Features:
- **No visible ray or laser** - only the virtual hand itself
- **Collision-based selection** - virtual hand must physically touch objects
- **1:1 mapping inside threshold** - precise control near body
- **Exponential scaling beyond threshold** - reach and manipulate distant objects
- Small real movements create large virtual movements when holding objects far away

**Formula:** `virtual_distance = threshold + k × (real_distance - threshold)²`

**Example:** Extend hand 0.4m from HMD → virtual hand at 1.5m (3.75x amplification)
- While holding object: Move real hand 10cm → object moves 37.5cm in virtual space

---

## Setup Instructions

### Option 1: Traditional GoGo Only

#### 1. Create GameObject
1. In Unity Hierarchy, right-click → Create Empty
2. Rename to "TraditionalGoGoManager"

#### 2. Add Component
- Add Component → **Traditional Go Go Interaction**

#### 3. Configure Inspector
**References:**
- **Virtual Hand**: Drag your virtual hand GameObject (the visual representation)
  - **CRITICAL**: The script will automatically:
    - Add a SphereCollider (trigger) to the virtual hand
    - Add VirtualHandCollisionDetector component to the virtual hand
  - This is what physically "touches" objects
- **Controller Transform**: Drag your Right Hand Controller transform from XR Rig hierarchy:
  - Look in Hierarchy: `XR Origin` → `Camera Offset` → `RightHand Controller` (or `Right Controller`)
  - This is typically under the XR Origin/XR Rig GameObject
  - It's the GameObject that has `XRController` or `ActionBasedController` component
- **HMD Transform**: Leave empty (auto-finds Camera.main) or drag Main Camera
  - Main Camera is usually at: `XR Origin` → `Camera Offset` → `Main Camera`
- **Grip Action**: 
  - Click ➕ → **XRI RightHand Interaction** → **Select Value** (this is the grip button)
  - Alternative: Use **XRI RightHand/Select** or **XRI RightHand/Grip Press**
  - **Note**: Do NOT use "Activate Value" (that's the trigger button)
  
**GoGo Parameters:**
- **Threshold**: 0.3 (default - distance from HMD where scaling begins)
- **Scaling Factor**: 2.0 (default - controls exponential amplification)
- **Max Extension**: 5.0 (maximum virtual hand reach distance)

**Selectable Layers:**
- Set to layers containing grabbable objects (e.g., "Default" or "Grabbable")
- Objects MUST have Colliders to be touched by virtual hand

#### 4. Setup Grabbable Objects
For each object you want to grab:
1. **Must have a Collider** (BoxCollider, SphereCollider, etc.)
2. **Should have a Rigidbody** for physics (optional but recommended)
3. **Must be on the correct Layer** (matching Selectable Layers setting)

#### 5. CRITICAL - Disable ReverseGoGo
**If you have VirtualHandAttach (ReverseGoGo) in your scene:**
1. Find the GameObject with **VirtualHandAttach** component
2. **UNCHECK/DISABLE** the VirtualHandAttach component
3. This is REQUIRED - both systems cannot run simultaneously

#### 6. Verify Setup is Correct
Before testing in VR, check these in Unity Editor:
- [ ] TraditionalGoGoManager GameObject exists in Hierarchy
- [ ] Traditional Go Go Interaction component is **ENABLED** (checkbox checked)
- [ ] Virtual Hand, Controller Transform, Grip Action are all assigned
- [ ] VirtualHandAttach (ReverseGoGo) component is **DISABLED** (checkbox unchecked)
- [ ] Console shows on Play: "✅ Traditional GoGo Interaction initialized"
- [ ] Console shows: "✅ Added trigger collider to virtual hand"
- [ ] Console shows: "✅ Added collision detector to virtual hand"

#### 7. Test in VR
- Put on headset
- **Extend hand away from body beyond 0.3m** (away from chest)
- **Virtual hand shoots forward exponentially** (you'll see it extend much farther than your real hand)
- **Move extended hand near an object** - virtual hand physically touches it
- **Console shows**: "👆 Virtual hand touching: [object name]"
- **Press Grip** to grab when touching
- **Move controller slightly** → object moves dramatically in virtual space (10cm = 1m+)
- **Release Grip** → drop object

**Expected Console Messages:**
```
✅ Traditional GoGo Interaction initialized
✅ Added trigger collider to virtual hand
✅ Added collision detector to virtual hand
👆 Virtual hand touching: RedPhantom
✅ [GoGo] Grabbed: RedPhantom at virtual distance: 1.50m
🤚 GoGo Manipulation | Real hand: 0.45m | Virtual reach: 1.52m | Amplification: 3.4x | 10cm real = 0.34m virtual
```

**If you see "🎯 Exponential Pull" messages** → ReverseGoGo is still running! Go disable VirtualHandAttach component!

---

### Option 2: Toggle Between Both Techniques

#### 1. Setup Both Scripts
Follow **Option 1** setup for Traditional GoGo, and ensure your existing **VirtualHandAttach** (ReverseGoGo) is also set up.

#### 2. Create Toggle Manager
1. In Hierarchy, right-click → Create Empty
2. Rename to "GoGoModeToggle"
3. Add Component → **Go Go Mode Toggle**

#### 3. Configure Toggle Inspector
**Interaction Scripts:**
- **Traditional Go Go**: Drag your TraditionalGoGoManager GameObject
- **Reverse Go Go**: Drag your VirtualHandAttach GameObject

**Toggle Input:**
- **Toggle Action**: 
  - Click ➕ → XRI RightHand → Secondary Button (Y button on Quest)
  - Or choose any button you prefer

**Current Mode:**
- **Use Traditional Go Go**: ✓ Checked (starts with Traditional)
- Uncheck to start with ReverseGoGo

#### 4. Test Mode Switching
- In VR, press Y button (or your chosen button) to toggle
- Console will show: "🔄 Mode switched to: Traditional GoGo" or "ReverseGoGo"
- Try each mode to feel the difference!

---

## Comparison: Traditional GoGo vs ReverseGoGo

| Feature | Traditional GoGo | ReverseGoGo |
|---------|-----------------|-------------|
| **Hand Motion** | Extend forward | Retract backward |
| **Activation** | Beyond threshold enables reach | Beyond threshold enables pull |
| **Object Behavior** | Virtual hand reaches object | Object flies to you |
| **Selection Method** | Virtual hand physically touches object | Raycast with visual highlight |
| **Grab Method** | Grip when touching object | Grip to start pull, auto-attach when close |
| **Visual Feedback** | Only virtual hand (no ray/laser) | Ghost hand + visual ray |
| **Manipulation** | Exponential (10cm = 1m+) | Direct 1:1 when attached |
| **Best For** | Reaching and rapidly repositioning distant objects | Bringing distant objects close to you |

---

## Troubleshooting

### Virtual hand doesn't extend
- **Check**: Controller is beyond 0.3m from HMD (chest level)
- **Check**: Grip action is properly assigned
- **Check**: Virtual Hand GameObject is assigned and active

### Can't grab objects
- **Check**: Virtual hand has a trigger collider (auto-added by script)
- **Check**: Objects have Colliders
- **Check**: Objects are on the correct Layer (match Selectable Layers)
- **Check**: Virtual hand is actually touching the object (watch in Scene view)

### Virtual hand not touching objects
- **Check**: Virtual hand collider is a trigger (isTrigger = true)
- **Check**: Objects are within reach when hand is extended
- **Check**: Console shows "👆 Virtual hand touching: [object name]" message

### Can't see virtual hand
- **Check**: Virtual Hand GameObject has a visual mesh/renderer
- **Check**: Virtual hand is not positioned at (1000, 1000, 1000) initially
- **Check**: Virtual hand scale is appropriate (not too small)

### Objects fall through floor when released
- **Check**: Objects have Rigidbody component
- **Check**: Floor has Collider
- **Check**: Rigidbody gravity is enabled

---

## Advanced Configuration

### Adjust Amplification
To make virtual hand reach farther with less controller movement:
- **Increase Scaling Factor**: 2.0 → 3.0 or higher
- Higher = more aggressive scaling

Example:
- Scaling Factor = 2.0: Extend 0.5m → virtual hand at 0.8m (1.6x)
- Scaling Factor = 5.0: Extend 0.5m → virtual hand at 2.3m (4.6x)

### Change Threshold
- **Lower threshold** (0.2m): Scaling starts sooner, easier to activate
- **Higher threshold** (0.4m): Requires more extension, less accidental activation

### Virtual Hand Collider Size
- **Smaller collider** (0.05 radius): More precise touching, harder to select
- **Larger collider** (0.15 radius): Easier touching, less precise
- Adjust via virtual hand's SphereCollider component after script adds it

---

## User Study Integration

To use Traditional GoGo in user studies:

1. **Modify UserStudyManager.cs**:
   - Add reference to `TraditionalGoGoInteraction` instead of `VirtualHandAttach`
   - Change `handAttach.GetCurrentObject()` to `traditionalGoGo.GetCurrentObject()`

2. **Track Different Metrics**:
   - Extension distance (how far users extend)
   - Grab success rate at different distances
   - Time to reach distant vs close objects

3. **Create Comparison Study**:
   - Use `GoGoModeToggle` to switch between modes
   - Have participants try both techniques
   - Compare performance metrics

---

## Technical Details

### GoGo Formula Implementation
```csharp
if (realDistance <= threshold)
{
    virtualDistance = realDistance;  // 1:1 mapping
}
else
{
    // Exponential scaling beyond threshold
    float beyondThreshold = realDistance - threshold;
    float scaledDistance = scalingFactor × (beyondThreshold)²;
    virtualDistance = threshold + scaledDistance;
}
```

### Physics Integration
- Uses Rigidbody velocity for smooth movement
- Freezes rotation during grab
- Re-enables gravity on release

---

## References

- Poupyrev, I., Billinghurst, M., Weghorst, S., & Ichikawa, T. (1996). The go-go interaction technique: non-linear mapping for direct manipulation in VR. *Proceedings of UIST '96*, 79-80.
- Original ReverseGoGo implementation by this project team

---

## Quick Start Summary

**Traditional GoGo Setup Checklist:**
1. ✅ Create GameObject → Add `TraditionalGoGoInteraction` component
2. ✅ Assign: Virtual Hand, Controller, Grip Action, Selectable Layers
3. ✅ **DISABLE VirtualHandAttach (ReverseGoGo) component if it exists**
4. ✅ Press Play → Check console for "✅ Traditional GoGo Interaction initialized"
5. ✅ Test: Extend hand → Virtual hand shoots outward → Touch object → Grab with Grip!

**Behavior You Should See:**
- Extend hand forward → Virtual hand extends MUCH farther (exponential)
- Touch object with virtual hand → Console: "👆 Virtual hand touching: [name]"
- Press Grip → Grab object
- Move hand 10cm → Object moves 30cm-100cm+ (depending on distance from chest)
- Release Grip → Drop

**If it's pulling objects toward you instead** = ReverseGoGo is still running. DISABLE VirtualHandAttach component!

**Need Help?** Check console logs for debugging info (🤚 GoGo messages, not 🎯 Exponential Pull messages).
